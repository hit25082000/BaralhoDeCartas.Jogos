using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models
{
    public class JogoBlackJack : IJogoBlackJack
    {
        public JogoBlackJack(List<IJogadorDeBlackjack> jogadores, IBaralho baralho)
        {
            Jogadores = jogadores;
            Baralho = baralho;
            
            Random rnd = new Random();
            int primeiroAJogar = rnd.Next(1, jogadores.Count());

            JogadorAtual = jogadores[primeiroAJogar];
        }
        public List<IJogadorDeBlackjack> Jogadores { get; }
        public IBaralho Baralho { get; }
        public IJogadorDeBlackjack JogadorAtual { get; private set; }

        public void PassarRodada()
        {
            int jogadorAtualId = JogadorAtual.JogadorId;

            if (jogadorAtualId == Jogadores.Count)
            {
                JogadorAtual = Jogadores[0];
            }
            else
            {
                JogadorAtual = Jogadores[jogadorAtualId + 1];
            }
        }
    }
}
