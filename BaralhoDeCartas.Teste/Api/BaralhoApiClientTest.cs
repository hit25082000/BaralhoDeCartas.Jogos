using Moq.Protected;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using BaralhoDeCartas.Api;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Exceptions;

namespace BaralhoDeCartas.Teste.Api
{
    public class BaralhoApiClientTest
    {
        private readonly Mock<IBaralhoFactory> _mockBaralhoFactory;
        private readonly Mock<ICartaFactory> _mockCartaFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public BaralhoApiClientTest()
        {
            _mockBaralhoFactory = new Mock<IBaralhoFactory>();
            _mockCartaFactory = new Mock<ICartaFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://deckofcardsapi.com/api/deck")
            };
        }

        #region Criar Baralho

        [Fact]
        public async Task CriarNovoBaralhoDeveRetornarUmBaralho()
        {
            // Arrange
            var baralhoResponse = new BaralhoResponse
            {
                Success = true,
                DeckId = "abc123",
                Shuffled = true,
                Remaining = 52
            };

            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoResponse.DeckId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(baralhoResponse.Shuffled);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(baralhoResponse.Remaining);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _mockBaralhoFactory
                .Setup(f => f.CriarBaralho(It.Is<BaralhoResponse>(r => 
                    r.DeckId == baralhoResponse.DeckId && 
                    r.Shuffled == baralhoResponse.Shuffled &&
                    r.Remaining == baralhoResponse.Remaining)))
                .Returns(baralhoMock.Object);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act
            var resultado = await baralhoApiClient.CriarNovoBaralhoAsync();

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoResponse.DeckId, resultado.BaralhoId);
            Assert.Equal(baralhoResponse.Shuffled, resultado.EstaEmbaralhado);
            Assert.Equal(baralhoResponse.Remaining, resultado.QuantidadeDeCartasRestantes);

            _mockBaralhoFactory.Verify(f => f.CriarBaralho(It.IsAny<BaralhoResponse>()), Times.Once);
        }

        [Fact]
        public async Task CriarNovoBaralhoDeveRetornarExcecaoQuandoApiRetornaError()
        {
            // Arrange
            var baralhoResponse = new BaralhoResponse
            {
                Success = false,
                Error = "Erro ao criar baralho"
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => baralhoApiClient.CriarNovoBaralhoAsync());
            Assert.Contains("Falha na operação", exception.Message);
        }

        [Fact]
        public async Task CriarNovoBaralhoDeveRetornarExcecaoQuandoApiNãoResponde()
        {
            // Arrange
            _mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new SocketException());

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => baralhoApiClient.CriarNovoBaralhoAsync());
        }

        [Fact]
        public async Task CriarNovoBaralhoDeveRetornarBaralhoNotFoundExceptionQuandoApiRetorna404()
        {
            // Arrange
            _mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BaralhoNotFoundException>(() => baralhoApiClient.CriarNovoBaralhoAsync());
        }

        #endregion

        #region Embaralhar Baralho

        [Fact]
        public async Task EmbaralharBaralhoDeveRetornarBaralhoEmbaralhado()
        {
            // Arrange
            string baralhoId = "abc123";
            bool embaralharSomenteCartasRestantes = false;
            
            var baralhoResponse = new BaralhoResponse
            {
                Success = true,
                DeckId = baralhoId,
                Shuffled = true,
                Remaining = 52
            };

            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoResponse.DeckId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(baralhoResponse.Shuffled);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(baralhoResponse.Remaining);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains($"{baralhoId}/shuffle/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _mockBaralhoFactory
                .Setup(f => f.CriarBaralho(It.IsAny<BaralhoResponse>()))
                .Returns(baralhoMock.Object);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act
            var resultado = await baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.True(resultado.EstaEmbaralhado);
            
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"{baralhoId}/shuffle/")),
                ItExpr.IsAny<CancellationToken>());
        }
        
        [Fact]
        public async Task EmbaralharBaralhoComCartasRestantesDeveUsarParametroRemaining()
        {
            // Arrange
            string baralhoId = "abc123";
            bool embaralharSomenteCartasRestantes = true;
            
            var baralhoResponse = new BaralhoResponse
            {
                Success = true,
                DeckId = baralhoId,
                Shuffled = true,
                Remaining = 40
            };

            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoResponse.DeckId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(baralhoResponse.Shuffled);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(baralhoResponse.Remaining);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains($"{baralhoId}/shuffle/") &&
                        req.RequestUri.ToString().Contains("remaining=true")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _mockBaralhoFactory
                .Setup(f => f.CriarBaralho(It.IsAny<BaralhoResponse>()))
                .Returns(baralhoMock.Object);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act
            var resultado = await baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.True(resultado.EstaEmbaralhado);
            
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"{baralhoId}/shuffle/") &&
                    req.RequestUri.ToString().Contains("remaining=true")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task EmbaralharBaralhoDeveLancarExcecaoQuandoBaralhoIdNuloOuVazio()
        {
            // Arrange
            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.EmbaralharBaralhoAsync("", true));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
            
            exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.EmbaralharBaralhoAsync(null, true));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
        }

        [Fact]
        public async Task EmbaralharBaralhoDeveLancarBaralhoNotFoundExceptionQuandoApiRetorna404()
        {
            // Arrange
            string baralhoId = "abc123";
            
            _mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BaralhoNotFoundException>(() => baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, true));
            Assert.Contains(baralhoId, exception.Message);
        }

        [Fact]
        public async Task EmbaralharBaralhoDeveLancarExcecaoQuandoApiRetornaError()
        {
            // Arrange
            string baralhoId = "abc123";
            
            var baralhoResponse = new BaralhoResponse
            {
                Success = false,
                Error = "Erro ao embaralhar"
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, true));
            Assert.Contains("Falha na operação", exception.Message);
        }

        #endregion

        #region Comprar Cartas

        [Fact]
        public async Task ComprarCartasDeveRetornarListaDeCartas()
        {
            // Arrange
            string baralhoId = "abc123";
            int quantidade = 3;
            
            var cartasResponse = new CartasResponse
            {
                Success = true,
                Deck_id = baralhoId,
                Remaining = 49,
                Cards = new List<CartaListItemResponse>
                {
                    new CartaListItemResponse { Code = "AS", Value = "ACE", Suit = "SPADES", Image = "url1" },
                    new CartaListItemResponse { Code = "KH", Value = "KING", Suit = "HEARTS", Image = "url2" },
                    new CartaListItemResponse { Code = "QD", Value = "QUEEN", Suit = "DIAMONDS", Image = "url3" }
                }
            };

            var cartasMock = new List<ICarta>
            {
                Mock.Of<ICarta>(c => c.Codigo == "AS" && c.ValorSimbolico == "ACE" && c.Naipe == "SPADES"),
                Mock.Of<ICarta>(c => c.Codigo == "KH" && c.ValorSimbolico == "KING" && c.Naipe == "HEARTS"),
                Mock.Of<ICarta>(c => c.Codigo == "QD" && c.ValorSimbolico == "QUEEN" && c.Naipe == "DIAMONDS")
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(cartasResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains($"{baralhoId}/draw/") &&
                        req.RequestUri.ToString().Contains($"count={quantidade}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _mockCartaFactory
                .Setup(f => f.CriarCartas(It.IsAny<CartasResponse>()))
                .Returns(cartasMock);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act
            var resultado = await baralhoApiClient.ComprarCartasAsync(baralhoId, quantidade);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Count);
            Assert.Equal("AS", resultado[0].Codigo);
            Assert.Equal("KH", resultado[1].Codigo);
            Assert.Equal("QD", resultado[2].Codigo);
            
            _mockCartaFactory.Verify(f => f.CriarCartas(It.IsAny<CartasResponse>()), Times.Once);
        }

        [Fact]
        public async Task ComprarCartasDeveLancarExcecaoQuandoBaralhoIdNuloOuVazio()
        {
            // Arrange
            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.ComprarCartasAsync("", 5));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
            
            exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.ComprarCartasAsync(null, 5));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
        }

        [Fact]
        public async Task ComprarCartasDeveLancarExcecaoQuandoQuantidadeInvalida()
        {
            // Arrange
            string baralhoId = "abc123";
            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.ComprarCartasAsync(baralhoId, 0));
            Assert.Contains("Quantidade de cartas deve ser maior que zero", exception.Message);
            
            exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.ComprarCartasAsync(baralhoId, -5));
            Assert.Contains("Quantidade de cartas deve ser maior que zero", exception.Message);
        }

        [Fact]
        public async Task ComprarCartasDeveLancarBaralhoNotFoundExceptionQuandoApiRetorna404()
        {
            // Arrange
            string baralhoId = "abc123";
            int quantidade = 5;
            
            _mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BaralhoNotFoundException>(() => baralhoApiClient.ComprarCartasAsync(baralhoId, quantidade));
            Assert.Contains(baralhoId, exception.Message);
        }

        [Fact]
        public async Task ComprarCartasDeveLancarBaralhoNotFoundExceptionQuandoApiRetornaErrorComDeckId()
        {
            // Arrange
            string baralhoId = "abc123";
            int quantidade = 5;
            
            var cartasResponse = new CartasResponse
            {
                Success = false,
                Error = "Invalid deck_id specified",
                Deck_id = baralhoId
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(cartasResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BaralhoNotFoundException>(() => baralhoApiClient.ComprarCartasAsync(baralhoId, quantidade));
        }

        [Fact]
        public async Task ComprarCartasDeveLancarExcecaoQuandoApiRetornaError()
        {
            // Arrange
            string baralhoId = "abc123";
            int quantidade = 5;
            
            var cartasResponse = new CartasResponse
            {
                Success = false,
                Error = "Erro ao comprar cartas"
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(cartasResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => baralhoApiClient.ComprarCartasAsync(baralhoId, quantidade));
            Assert.Contains("Falha na operação", exception.Message);
        }

        #endregion

        #region Retornar Cartas ao Baralho

        [Fact]
        public async Task RetornarCartasAoBaralhoDeveRetornarBaralho()
        {
            // Arrange
            string baralhoId = "abc123";
            
            var baralhoResponse = new BaralhoResponse
            {
                Success = true,
                DeckId = baralhoId,
                Shuffled = true,
                Remaining = 52
            };

            var baralhoMock = new Mock<IBaralho>();
            baralhoMock.Setup(b => b.BaralhoId).Returns(baralhoResponse.DeckId);
            baralhoMock.Setup(b => b.EstaEmbaralhado).Returns(baralhoResponse.Shuffled);
            baralhoMock.Setup(b => b.QuantidadeDeCartasRestantes).Returns(baralhoResponse.Remaining);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString().Contains($"{baralhoId}/return/")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            _mockBaralhoFactory
                .Setup(f => f.CriarBaralho(It.IsAny<BaralhoResponse>()))
                .Returns(baralhoMock.Object);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act
            var resultado = await baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(baralhoId, resultado.BaralhoId);
            Assert.Equal(52, resultado.QuantidadeDeCartasRestantes);
            
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Get && 
                    req.RequestUri.ToString().Contains($"{baralhoId}/return/")),
                ItExpr.IsAny<CancellationToken>());
        }
        
        [Fact]
        public async Task RetornarCartasAoBaralhoDeveLancarExcecaoQuandoBaralhoIdNuloOuVazio()
        {
            // Arrange
            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.RetornarCartasAoBaralhoAsync(""));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
            
            exception = await Assert.ThrowsAsync<ArgumentException>(() => baralhoApiClient.RetornarCartasAoBaralhoAsync(null));
            Assert.Contains("ID do baralho não pode ser nulo ou vazio", exception.Message);
        }

        [Fact]
        public async Task RetornarCartasAoBaralhoDeveLancarBaralhoNotFoundExceptionQuandoApiRetorna404()
        {
            // Arrange
            string baralhoId = "abc123";
            
            _mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
               .ThrowsAsync(new HttpRequestException("Not Found", null, HttpStatusCode.NotFound));

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BaralhoNotFoundException>(() => baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId));
            Assert.Contains(baralhoId, exception.Message);
        }

        [Fact]
        public async Task RetornarCartasAoBaralhoDeveLancarExcecaoQuandoApiRetornaError()
        {
            // Arrange
            string baralhoId = "abc123";
            
            var baralhoResponse = new BaralhoResponse
            {
                Success = false,
                Error = "Erro ao retornar cartas"
            };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(baralhoResponse), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var baralhoApiClient = new BaralhoApiClient(_httpClient, _mockBaralhoFactory.Object, _mockCartaFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId));
            Assert.Contains("Falha na operação", exception.Message);
        }

        #endregion
    }
}
