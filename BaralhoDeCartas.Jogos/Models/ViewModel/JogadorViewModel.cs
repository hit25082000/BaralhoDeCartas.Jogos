using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models.ViewModel
{
    // Implementação de IJogador para uso no controller
    public class JogadorViewModel : IJogador
    {
        public int JogadorId { get; set; }
        public string Nome { get; set; }
        public List<ICarta> Cartas { get; private set; }

        public void AdicionarCarta(ICarta carta)
        {
            Cartas.Add(carta);
        }

        public ICarta ObterCartaDeMaiorValor()
        {
            return Cartas?.OrderByDescending(c => c.Valor).FirstOrDefault();
        }
    }
}
