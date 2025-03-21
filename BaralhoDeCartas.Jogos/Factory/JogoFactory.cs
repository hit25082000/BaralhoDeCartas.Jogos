using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory
{
    public class JogoFactory : IJogoFactory
    {
        public IJogoMaiorCarta CriarJogoMaiorCarta(List<IJogador> jogadores,IBaralho baralho)
        {
            return new JogoMaiorCarta(jogadores, baralho);
        }

        public IJogoBlackJack CriarJogoBlackJack(List<IJogadorDeBlackjack> jogadores, IBaralho baralho)
        {
            return new JogoBlackJack(jogadores, baralho);            
        }
    }
}
