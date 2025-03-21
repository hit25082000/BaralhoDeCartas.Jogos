using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Services.Interfaces
{
    public interface IBlackjackService
    {
        Task<IJogoBlackJack> CriarJogoBlackJackAsync(int numeroJogadores);
        Task<IBaralho> CriarNovoBaralhoAsync();
        Task<List<IJogadorDeBlackjack>> IniciarRodadaAsync(string baralhoId, int numeroJogadores);
        Task<ICarta> ComprarCartaAsync(string baralhoId, IJogadorDeBlackjack jogador);
        List<IJogadorDeBlackjack> DeterminarVencedoresAsync(List<IJogadorDeBlackjack> jogadores);
        Task<IBaralho> RetornarCartasAoBaralhoAsync(string baralhoId);
        Task<IJogadorDeBlackjack> PararJogador(IJogadorDeBlackjack jogadorDeBlackJack);
        Task<IJogadorDeBlackjack> JogarDealer(string baralhoId, List<IJogadorDeBlackjack> jogadores);
    }
} 