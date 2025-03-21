using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models
{
    public class JogoMaiorCarta : IJogoMaiorCarta
    {
        public JogoMaiorCarta(List<IJogador> jogadores, IBaralho baralho)
        {
            Jogadores = jogadores;
            Baralho = baralho;
        }
        public List<IJogador> Jogadores { get; set; }
        public IBaralho Baralho { get; set; }
    }    
}
