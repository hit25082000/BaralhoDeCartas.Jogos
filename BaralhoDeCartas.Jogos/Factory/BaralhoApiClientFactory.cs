
using System.Net.Http.Headers;
using System;

namespace BaralhoDeCartas.Factory
{
    public class BaralhoApiClientFactory : IHttpClientFactory
    {
        private string url;

        public BaralhoApiClientFactory(string url)
        {
            this.url = url;
        }

        public HttpClient CreateClient(string name)
        {        
            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _client.BaseAddress = new Uri(url);
            return _client;
        }    
    }
}
