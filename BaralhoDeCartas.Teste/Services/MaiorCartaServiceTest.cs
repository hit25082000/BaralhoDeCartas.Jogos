using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services;
using BaralhoDeCartas.Services.Interfaces;
using Moq;
using BaralhoDeCartas.Factory;

namespace BaralhoDeCartas.Test.Services
{
    public class MaiorCartaServiceTest
    {
        private readonly Mock<IBaralhoApiClient> _mockBaralhoApiClient;
        private readonly Mock<IJogadorFactory> _mockJogadorFactory;
        private readonly Mock<IJogoFactory> _mockJogoFactory;
        private readonly IMaiorCartaService _service;
        private const int CARTAS_POR_JOGADOR = 5;

        public MaiorCartaServiceTest()
        {
            _mockBaralhoApiClient = new Mock<IBaralhoApiClient>();
            _mockJogadorFactory = new Mock<IJogadorFactory>();
            _mockJogoFactory = new Mock<IJogoFactory>();
            _service = new MaiorCartaService(_mockBaralhoApiClient.Object, _mockJogadorFactory.Object, _mockJogoFactory.Object);
        }

        [Fact]
        public async Task CriarNovoBaralhoAsync_QuandoChamado_ChamaApiClientERetornaBaralho()
        {
            // Arrange
            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns("abc123");
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(true);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(52);

            _mockBaralhoApiClient.Setup(c => c.CriarNovoBaralhoAsync())
                .ReturnsAsync(baralhoMock.Object);

            // Act
            var resultado = await _service.CriarNovoBaralhoAsync();

            // Assert
            Assert.Equal("abc123", resultado.BaralhoId);
            Assert.True(resultado.EstaEmbaralhado);
            Assert.Equal(52, resultado.QuantidadeDeCartasRestantes);

            _mockBaralhoApiClient.Verify(c => c.CriarNovoBaralhoAsync(), Times.Once);
        }

        [Fact]
        public async Task DistribuirCartasAsync_QuandoChamadoComParametrosValidos_RetornaListaDeJogadores()
        {
            // Arrange
            string baralhoId = "abc123";
            int numeroJogadores = 3;
            int totalCartas = numeroJogadores * CARTAS_POR_JOGADOR;

            var cartas = new List<ICarta>();
            for (int i = 0; i < totalCartas; i++)
            {
                var cartaMock = new Mock<ICarta>();
                cartaMock.Setup(c => c.Codigo).Returns($"Carta{i+1}");
                cartas.Add(cartaMock.Object);
            }

            _mockBaralhoApiClient.Setup(c => c.ComprarCartasAsync(baralhoId, totalCartas))
                .ReturnsAsync(cartas);

            for (int i = 0; i < numeroJogadores; i++)
            {
                var jogadorMock = new Mock<IJogador>();
                jogadorMock.Setup(j => j.JogadorId).Returns(i + 1);
                jogadorMock.Setup(j => j.Nome).Returns($"Jogador {i+1}");

                List<ICarta> cartasDoJogador = cartas.Skip(i * CARTAS_POR_JOGADOR).Take(CARTAS_POR_JOGADOR).ToList();
                _mockJogadorFactory.Setup(f => f.CriarJogador(
                    It.Is<List<ICarta>>(c => c.Count == CARTAS_POR_JOGADOR), 
                    i + 1, 
                    $"Jogador {i+1}"))
                    .Returns(jogadorMock.Object);
            }

            // Act
            var jogadores = await _service.DistribuirCartasAsync(baralhoId, numeroJogadores);

            // Assert
            Assert.Equal(numeroJogadores, jogadores.Count);
            _mockBaralhoApiClient.Verify(c => c.ComprarCartasAsync(baralhoId, totalCartas), Times.Once);
            
            for (int i = 0; i < numeroJogadores; i++)
            {
                _mockJogadorFactory.Verify(f => f.CriarJogador(
                    It.IsAny<List<ICarta>>(),
                    i + 1, 
                    $"Jogador {i+1}"), 
                    Times.Once);
            }
        }

        [Fact]
        public async Task DistribuirCartasAsync_QuandoBaralhoIdInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DistribuirCartasAsync("", 3));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DistribuirCartasAsync(null, 3));
        }

        [Fact]
        public async Task DistribuirCartasAsync_QuandoNumeroJogadoresInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DistribuirCartasAsync("abc123", 0));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DistribuirCartasAsync("abc123", -1));
        }

        [Fact]
        public async Task DeterminarVencedorAsync_QuandoHaJogadores_RetornaJogadorComMaiorCarta()
        {
            // Arrange
            var jogadores = new List<IJogador>();
            for (int i = 0; i < 3; i++)
            {
                var jogadorMock = new Mock<IJogador>();
                jogadorMock.Setup(j => j.JogadorId).Returns(i + 1);
                jogadorMock.Setup(j => j.Nome).Returns($"Jogador {i+1}");
                
                var cartaMaiorValor = new Mock<ICarta>();
                cartaMaiorValor.Setup(c => c.Valor).Returns(i + 10); // Jogador 3 terá a carta de maior valor (12)
                
                jogadorMock.Setup(j => j.ObterCartaDeMaiorValor()).Returns(cartaMaiorValor.Object);
                jogadorMock.Setup(j => j.Cartas).Returns(new List<ICarta> { cartaMaiorValor.Object });
                
                jogadores.Add(jogadorMock.Object);
            }

            // Act
            var vencedor = await _service.DeterminarVencedorAsync(jogadores);

            // Assert
            Assert.NotNull(vencedor);
            Assert.Equal(3, vencedor.JogadorId); // Jogador 3 deve ser o vencedor
        }

        [Fact]
        public async Task DeterminarVencedorAsync_QuandoListaVazia_LancaArgumentException()
        {
            // Arrange
            var jogadores = new List<IJogador>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeterminarVencedorAsync(jogadores));
        }

        [Fact]
        public async Task DeterminarVencedorAsync_QuandoJogadoresSemCartas_LancaArgumentException()
        {
            // Arrange
            var jogadores = new List<IJogador>
            {
                Mock.Of<IJogador>(j => j.Cartas == new List<ICarta>() && j.JogadorId == 1 && j.Nome == "Jogador 1")
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeterminarVencedorAsync(jogadores));
        }

        [Fact]
        public async Task FinalizarJogoAsync_QuandoChamado_RetornaBaralho()
        {
            // Arrange
            string baralhoId = "abc123";
            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(52);

            _mockBaralhoApiClient.Setup(c => c.RetornarCartasAoBaralhoAsync(baralhoId))
                .ReturnsAsync(baralhoMock.Object);

            // Act
            var resultado = await _service.FinalizarJogoAsync(baralhoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.Equal(52, resultado.QuantidadeDeCartasRestantes);
            _mockBaralhoApiClient.Verify(c => c.RetornarCartasAoBaralhoAsync(baralhoId), Times.Once);
        }

        [Fact]
        public async Task FinalizarJogoAsync_QuandoBaralhoIdInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.FinalizarJogoAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.FinalizarJogoAsync(null));
        }

        [Fact]
        public async Task CriarJogoMaiorCartaAsync_QuandoChamado_CriaJogoComBaralhoEJogadores()
        {
            // Arrange
            int numeroJogadores = 3;
            string baralhoId = "abc123";
            int totalCartas = numeroJogadores * CARTAS_POR_JOGADOR;

            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(true);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(52);

            _mockBaralhoApiClient.Setup(c => c.CriarNovoBaralhoAsync())
                .ReturnsAsync(baralhoMock.Object);

            var cartas = new List<ICarta>();
            for (int i = 0; i < totalCartas; i++)
            {
                var cartaMock = new Mock<ICarta>();
                cartaMock.Setup(c => c.Codigo).Returns($"Carta{i+1}");
                cartas.Add(cartaMock.Object);
            }

            _mockBaralhoApiClient.Setup(c => c.ComprarCartasAsync(baralhoId, totalCartas))
                .ReturnsAsync(cartas);

            var jogadores = new List<IJogador>();
            for (int i = 0; i < numeroJogadores; i++)
            {
                var jogadorMock = new Mock<IJogador>();
                jogadorMock.Setup(j => j.JogadorId).Returns(i + 1);
                jogadorMock.Setup(j => j.Nome).Returns($"Jogador {i+1}");
                
                List<ICarta> cartasDoJogador = cartas.Skip(i * CARTAS_POR_JOGADOR).Take(CARTAS_POR_JOGADOR).ToList();
                jogadorMock.Setup(j => j.Cartas).Returns(cartasDoJogador);
                
                jogadores.Add(jogadorMock.Object);

                _mockJogadorFactory.Setup(f => f.CriarJogador(
                    It.Is<List<ICarta>>(c => c.Count == CARTAS_POR_JOGADOR), 
                    i + 1, 
                    $"Jogador {i+1}"))
                    .Returns(jogadorMock.Object);
            }

            var jogoMock = new Mock<IJogoMaiorCarta>();
            jogoMock.Setup(j => j.Jogadores).Returns(jogadores);
            jogoMock.Setup(j => j.Baralho).Returns(baralhoMock.Object);

            _mockJogoFactory.Setup(f => f.CriarJogoMaiorCarta(
                It.IsAny<List<IJogador>>(),
                It.IsAny<IBaralho>()))
                .Returns(jogoMock.Object);

            // Act
            var resultado = await _service.CriarJogoMaiorCartaAsync(numeroJogadores);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(jogadores, resultado.Jogadores);
            Assert.Equal(baralhoMock.Object, resultado.Baralho);
            
            _mockBaralhoApiClient.Verify(c => c.CriarNovoBaralhoAsync(), Times.Once);
            _mockBaralhoApiClient.Verify(c => c.ComprarCartasAsync(baralhoId, totalCartas), Times.Once);
            _mockJogoFactory.Verify(f => f.CriarJogoMaiorCarta(
                It.IsAny<List<IJogador>>(),
                It.IsAny<IBaralho>()), 
                Times.Once);
        }

        [Fact]
        public async Task CriarJogoMaiorCartaAsync_QuandoNumeroJogadoresInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarJogoMaiorCartaAsync(0));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarJogoMaiorCartaAsync(-1));
        }

        [Fact]
        public async Task VerificarBaralhoAsync_QuandoCartasSuficientes_RetornaBaralhoEmbaralhado()
        {
            // Arrange
            string baralhoId = "abc123";
            
            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(15); // > 10 cartas
            
            _mockBaralhoApiClient.Setup(c => c.EmbaralharBaralhoAsync(baralhoId, true))
                .ReturnsAsync(baralhoMock.Object);
            
            // Act
            var resultado = await _service.VerificarBaralhoAsync(baralhoId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.Equal(15, resultado.QuantidadeDeCartasRestantes);
            
            _mockBaralhoApiClient.Verify(c => c.EmbaralharBaralhoAsync(baralhoId, true), Times.Once);
            _mockBaralhoApiClient.Verify(c => c.RetornarCartasAoBaralhoAsync(baralhoId), Times.Never);
        }
        
        [Fact]
        public async Task VerificarBaralhoAsync_QuandoCartasInsuficientes_RetornaTodasCartasEEmbaralha()
        {
            string baralhoId = "abc123";
            
            var baralhoInsuficienteMock = new Mock<IBaralho>();
            baralhoInsuficienteMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoInsuficienteMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(5);
            
            var baralhoCompletoMock = new Mock<IBaralho>();
            baralhoCompletoMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoCompletoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(52);
            
            _mockBaralhoApiClient.Setup(c => c.EmbaralharBaralhoAsync(baralhoId, true))
                .ReturnsAsync(baralhoInsuficienteMock.Object);
                
            _mockBaralhoApiClient.Setup(c => c.RetornarCartasAoBaralhoAsync(baralhoId))
                .ReturnsAsync(baralhoCompletoMock.Object);
                
            _mockBaralhoApiClient.Setup(c => c.EmbaralharBaralhoAsync(baralhoId, false))
                .ReturnsAsync(baralhoCompletoMock.Object);
            
            var resultado = await _service.VerificarBaralhoAsync(baralhoId);
            
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.Equal(52, resultado.QuantidadeDeCartasRestantes);
            
            _mockBaralhoApiClient.Verify(c => c.EmbaralharBaralhoAsync(baralhoId, true), Times.Once);
            _mockBaralhoApiClient.Verify(c => c.RetornarCartasAoBaralhoAsync(baralhoId), Times.Once);
            _mockBaralhoApiClient.Verify(c => c.EmbaralharBaralhoAsync(baralhoId, false), Times.Once);
        }
        
        [Fact]
        public async Task VerificarBaralhoAsync_QuandoBaralhoIdInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.VerificarBaralhoAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.VerificarBaralhoAsync(null));
        }
        
        [Fact]
        public async Task EmbaralharBaralhoAsync_QuandoChamado_RetornaBaralhoEmbaralhado()
        {
            // Arrange
            string baralhoId = "abc123";
            bool embaralharSomenteCartasRestantes = true;
            
            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(true);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(52);
            
            _mockBaralhoApiClient.Setup(c => c.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes))
                .ReturnsAsync(baralhoMock.Object);
            
            // Act
            var resultado = await _service.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.True(resultado.EstaEmbaralhado);
            Assert.Equal(52, resultado.QuantidadeDeCartasRestantes);
            
            _mockBaralhoApiClient.Verify(c => c.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes), Times.Once);
        }
        
        [Fact]
        public async Task EmbaralharBaralhoAsync_QuandoBaralhoIdInvalido_LancaArgumentException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.EmbaralharBaralhoAsync("", true));
            await Assert.ThrowsAsync<ArgumentException>(() => _service.EmbaralharBaralhoAsync(null, true));
        }
    }
} 