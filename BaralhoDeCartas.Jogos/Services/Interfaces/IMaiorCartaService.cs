using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Services.Interfaces
{
    public interface IMaiorCartaService
    {
        Task<IBaralho> CriarNovoBaralhoAsync();
        Task<List<IJogador>> DistribuirCartasAsync(string baralhoId, int numeroJogadores);
        Task<IJogador> DeterminarVencedorAsync(List<IJogador> jogadores);
        Task<IBaralho> FinalizarJogoAsync(string baralhoId);
        Task<IJogoMaiorCarta> CriarJogoMaiorCartaAsync(int numeroJogadores);
        Task<IBaralho> VerificarBaralhoAsync(string baralhoId);
        Task<IBaralho> EmbaralharBaralhoAsync(string baralhoId, bool embaralharSomenteCartasRestantes);
    }
} 