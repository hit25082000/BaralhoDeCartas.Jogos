using System.Collections.Generic;

namespace BaralhoDeCartas.Models.Interfaces
{
    public interface IJogador
    {
        int JogadorId { get; }
        string Nome { get; }
        List<ICarta> Cartas { get; }

        void AdicionarCarta(ICarta carta);
        ICarta ObterCartaDeMaiorValor();
    }
}
