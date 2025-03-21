using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models
{
    public class Carta : ICarta
    {
        public Carta(string codigo, string imagemUrl, string valorSimbolico, string naipe)
        {
            Codigo = codigo;
            ImagemUrl = imagemUrl;
            ValorSimbolico = valorSimbolico;
            Naipe = naipe;
        }

        public string Codigo { get; }
        public string ImagemUrl { get; }
        public string ValorSimbolico { get; }
        public string Naipe { get; }

        public virtual int Valor
        {
            get
            {
                return ValorSimbolico switch
                {
                    "ACE" => 14,
                    "KING" => 13,
                    "QUEEN" => 12,
                    "JACK" => 11,
                    _ => int.TryParse(ValorSimbolico, out int valor) ? valor : 0
                };
            }
        }        
    }
}
