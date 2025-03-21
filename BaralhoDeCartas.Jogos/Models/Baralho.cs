using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models
{
    public class Baralho : IBaralho
    {
        public Baralho(string baralhoId)
        {
            BaralhoId = baralhoId;
            EstaEmbaralhado = true;
        }

        public string BaralhoId { get; }
        public bool EstaEmbaralhado { get; set; }
        public int QuantidadeDeCartasRestantes { get; set; }
    }
}
