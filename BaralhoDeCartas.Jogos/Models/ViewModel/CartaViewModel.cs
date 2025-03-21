using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models.ViewModel
{
    public class CartaViewModel : ICarta
    {
        public int Valor { get; set; }
        public string ValorSimbolico { get; set; }
        public string Naipe { get; set; }
        public string ImagemUrl { get; set; }

        public string Codigo { get; set; }
    }
}
