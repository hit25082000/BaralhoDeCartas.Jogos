using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory
{
    public interface IJogoFactory 
    {
        IJogoMaiorCarta CriarJogoMaiorCarta(List<IJogador> jogadores, IBaralho baralho);
        IJogoBlackJack CriarJogoBlackJack(List<IJogadorDeBlackjack> jogadores, IBaralho baralho);
    }
}
