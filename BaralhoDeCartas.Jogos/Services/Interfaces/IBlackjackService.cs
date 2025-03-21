using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Services.Interfaces
{
    public interface IBlackjackService
    {
        Task<IBaralho> CriarNovoBaralhoAsync();
        Task<List<IJogadorDeBlackjack>> IniciarRodadaAsync(string baralhoId, int numeroJogadores);
        Task<ICarta> ComprarCartaAsync(string baralhoId, IJogadorDeBlackjack jogador);
        Task<IBaralho> RetornarCartasAoBaralhoAsync(string baralhoId);
        List<IJogadorDeBlackjack> DeterminarVencedoresAsync(List<IJogadorDeBlackjack> jogadores);
        Task<IJogoBlackJack> CriarJogoBlackJackAsync(int numeroJogadores);
        Task<IJogadorDeBlackjack> PararJogador(IJogadorDeBlackjack jogadorDeBlackjack);
    }
} 