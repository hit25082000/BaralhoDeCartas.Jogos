using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models
{
    public class CartaBlackjack : ICartaBlackjack
    {
        private readonly ICarta _cartaBase;

        public CartaBlackjack(ICarta cartaBase)
        {
            _cartaBase = cartaBase;
        }

        public string Codigo => _cartaBase.Codigo;
        public string ImagemUrl => _cartaBase.ImagemUrl;
        public string ValorSimbolico => _cartaBase.ValorSimbolico;
        public string Naipe => _cartaBase.Naipe;
        public int Valor => _cartaBase.Valor;

        public int ValorBlackjack => ValorSimbolico switch
        {
            "ACE" => 11, // Ás pode valer 1 ou 11, depende da lógica do jogo
            "KING" => 10,
            "QUEEN" => 10,
            "JACK" => 10,
            _ => int.TryParse(ValorSimbolico, out int valor) ? valor : 0
        };
    }
}
