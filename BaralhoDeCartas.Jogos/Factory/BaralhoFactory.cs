using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory
{
    public class BaralhoFactory : IBaralhoFactory
    {
        public IBaralho CriarBaralho(BaralhoResponse response)
        {
            return new Baralho(response.DeckId)
            {
                EstaEmbaralhado = response.Shuffled,
                QuantidadeDeCartasRestantes = response.Remaining
            };
        }
    }
}
