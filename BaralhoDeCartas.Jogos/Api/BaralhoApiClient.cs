using System.Text.Json;
using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Exceptions;

namespace BaralhoDeCartas.Api
{
    public class BaralhoApiClient : IBaralhoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IBaralhoFactory _baralhoFactory;
        private readonly ICartaFactory _cartaFactory;

        private const string BaseUrl = "https://deckofcardsapi.com/api/deck";

        public BaralhoApiClient(HttpClient httpClient, IBaralhoFactory baralhoFactory, ICartaFactory cartaFactory)
        {
            _httpClient = httpClient;
            _baralhoFactory = baralhoFactory;
            _cartaFactory = cartaFactory;
        }

        public async Task<IBaralho> CriarNovoBaralhoAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/new/shuffle/");
                response.EnsureSuccessStatusCode();

                var baralhoResponse = await DeserializeResponseAsync<BaralhoResponse>(response);
                ValidateResponse(baralhoResponse);

                return _baralhoFactory.CriarBaralho(baralhoResponse);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new BaralhoNotFoundException("Baralho não encontrado", ex);
            }
        }

        public async Task<IBaralho> EmbaralharBaralhoAsync(string baralhoId, bool embaralharSomenteCartasRestantes = true)
        {
            if (string.IsNullOrEmpty(baralhoId))
            {
                throw new ArgumentException("ID do baralho não pode ser nulo ou vazio", nameof(baralhoId));
            }

            try
            {
                string url = $"{BaseUrl}/{baralhoId}/shuffle/";

                if (embaralharSomenteCartasRestantes)
                {
                    url = $"{BaseUrl}/{baralhoId}/shuffle/?remaining=true";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var baralhoResponse = await DeserializeResponseAsync<BaralhoResponse>(response);
                ValidateResponse(baralhoResponse);

                return _baralhoFactory.CriarBaralho(baralhoResponse);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new BaralhoNotFoundException($"Baralho {baralhoId} não encontrado", ex);
            }
        }

        public async Task<List<ICarta>> ComprarCartasAsync(string baralhoId, int quantidade)
        {
            if (string.IsNullOrEmpty(baralhoId))
            {
                throw new ArgumentException("ID do baralho não pode ser nulo ou vazio", nameof(baralhoId));
            }

            if (quantidade <= 0 )
            {
                throw new ArgumentException("Quantidade de cartas deve ser maior que zero", nameof(quantidade));
            }

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{baralhoId}/draw/?count={quantidade}");
                response.EnsureSuccessStatusCode();

                var cartasResponse = await DeserializeResponseAsync<CartasResponse>(response);
                ValidateResponse(cartasResponse);

                return _cartaFactory.CriarCartas(cartasResponse);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new BaralhoNotFoundException($"Baralho {baralhoId} não encontrado", ex);
            }
        }

        public async Task<IBaralho> RetornarCartasAoBaralhoAsync(string baralhoId)
        {
            if (string.IsNullOrEmpty(baralhoId))
            {
                throw new ArgumentException("ID do baralho não pode ser nulo ou vazio", nameof(baralhoId));
            }

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{baralhoId}/return/");
                response.EnsureSuccessStatusCode();

                var baralhoResponse = await DeserializeResponseAsync<BaralhoResponse>(response);
                ValidateResponse(baralhoResponse);

                return _baralhoFactory.CriarBaralho(baralhoResponse);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new BaralhoNotFoundException($"Baralho {baralhoId} não encontrado", ex);
            }
        }

        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content);
        }

        private void ValidateResponse<T>(T response) where T : IApiResponse
        {
            if (!response.Success)
            {
                if (response is CartasResponse cartasResponse && 
                    cartasResponse.Error?.Contains("deck_id") == true)
                {
                    throw new BaralhoNotFoundException("Baralho não encontrado");
                }
                throw new HttpRequestException($"Falha na operação: {response.Error}");
            }
        }
    }
}