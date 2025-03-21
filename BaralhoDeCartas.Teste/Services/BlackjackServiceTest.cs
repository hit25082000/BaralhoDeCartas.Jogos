using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Exceptions;
using Moq;
using Xunit;
using BaralhoDeCartas.Factory;

namespace BaralhoDeCartas.Test.Services
{
    public class BlackjackServiceTest
    {
        private readonly Mock<IBaralhoApiClient> _mockBaralhoApiClient;
        private readonly Mock<IJogadorFactory> _mockJogadorFactory;
        private readonly Mock<IJogoFactory> _mockJogoFactory;
        private readonly IBlackjackService _service;

        public BlackjackServiceTest()
        {
            _mockBaralhoApiClient = new Mock<IBaralhoApiClient>();
            _mockJogadorFactory = new Mock<IJogadorFactory>();
            _mockJogoFactory = new Mock<IJogoFactory>();
            _service = new BlackjackService(_mockBaralhoApiClient.Object, _mockJogadorFactory.Object, _mockJogoFactory.Object);
        }

        [Fact]
        public async Task CriarNovoBaralho_QuandoServicoExternoIndisponivel_LancaExcecao()
        {
            // Arrange
            _mockBaralhoApiClient.Setup(c => c.CriarNovoBaralhoAsync())
                .ThrowsAsync(new ExternalServiceUnavailableException("Serviço indisponível"));

            // Act & Assert
            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(() => 
                _service.CriarNovoBaralhoAsync());
        }

        [Fact]
        public async Task IniciarRodada_QuandoBaralhoNaoEncontrado_LancaExcecao()
        {
            // Arrange
            string baralhoId = "abc123";
            _mockBaralhoApiClient.Setup(c => c.ComprarCartasAsync(baralhoId, It.IsAny<int>()))
                .ThrowsAsync(new BaralhoNotFoundException("Baralho não encontrado"));

            // Act & Assert
            await Assert.ThrowsAsync<BaralhoNotFoundException>(() => 
                _service.IniciarRodadaAsync(baralhoId, 2));
        }

        [Fact]
        public async Task ComprarCarta_QuandoJogadorParou_LancaExcecao()
        {
            // Arrange
            string baralhoId = "abc123";
            var jogadorMock = new Mock<IJogadorDeBlackjack>();
            jogadorMock.Setup(j => j.Nome).Returns("Jogador 1");
            jogadorMock.Setup(j => j.Parou).Returns(true);
            jogadorMock.Setup(j => j.Estourou).Returns(false);

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.ComprarCartaAsync(baralhoId, jogadorMock.Object));
            
            Assert.Equal("O jogador Jogador 1 não pode comprar mais cartas.", excecao.Message);
            _mockBaralhoApiClient.Verify(c => c.ComprarCartasAsync(baralhoId, 1), Times.Never);
        }

        [Fact]
        public async Task ComprarCarta_QuandoServicoExternoIndisponivel_LancaExcecao()
        {
            // Arrange
            string baralhoId = "abc123";
            var jogadorMock = new Mock<IJogadorDeBlackjack>();
            jogadorMock.Setup(j => j.Parou).Returns(false);
            jogadorMock.Setup(j => j.Estourou).Returns(false);

            _mockBaralhoApiClient.Setup(c => c.ComprarCartasAsync(baralhoId, 1))
                .ThrowsAsync(new ExternalServiceUnavailableException("Serviço indisponível"));

            // Act & Assert
            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(() => 
                _service.ComprarCartaAsync(baralhoId, jogadorMock.Object));
        }

        [Fact]
        public void DeterminarVencedores_QuandoTodosJogadoresEstouram_RetornaListaVazia()
        {
            // Arrange
            var jogadores = new List<IJogadorDeBlackjack>();
            for (int i = 0; i < 3; i++)
            {
                var jogadorMock = new Mock<IJogadorDeBlackjack>();
                jogadorMock.Setup(j => j.Estourou).Returns(true);
                jogadores.Add(jogadorMock.Object);
            }

            // Act
            var vencedores = _service.DeterminarVencedoresAsync(jogadores);

            // Assert
            Assert.Empty(vencedores);
        }

        [Fact]
        public async Task RetornarCartasAoBaralho_QuandoBaralhoNaoEncontrado_LancaExcecao()
        {
            // Arrange
            string baralhoId = "abc123";
            _mockBaralhoApiClient.Setup(c => c.RetornarCartasAoBaralhoAsync(baralhoId))
                .ThrowsAsync(new BaralhoNotFoundException("Baralho não encontrado"));

            // Act & Assert
            await Assert.ThrowsAsync<BaralhoNotFoundException>(() => 
                _service.RetornarCartasAoBaralhoAsync(baralhoId));
        }
    }
} 