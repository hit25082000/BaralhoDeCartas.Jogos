using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory.Interfaces
{
    public interface IBaralhoFactory
    {
        IBaralho CriarBaralho(BaralhoResponse response);
    }
}
