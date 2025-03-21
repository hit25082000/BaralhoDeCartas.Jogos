using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Api.Interfaces
{
    public interface IBaralhoApiClient
    {
        Task<IBaralho> CriarNovoBaralhoAsync();
        Task<IBaralho> EmbaralharBaralhoAsync(string baralhoId, bool embaralharSomenteCartasRestantes);
        Task<List<ICarta>> ComprarCartasAsync(string baralhoId, int quantidade);
        Task<IBaralho> RetornarCartasAoBaralhoAsync(string baralhoId);
    }
} 