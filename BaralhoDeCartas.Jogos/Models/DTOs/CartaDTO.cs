using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models.DTOs
{
    public class CartaDTO
    {
        public string Codigo { get; set; }
        public string ImagemUrl { get; set; }
        public string ValorSimbolico { get; set; }
        public string Naipe { get; set; }
        public int Valor { get; set; }

        public CartaDTO()
        {
        }

        public CartaDTO(ICarta carta)
        {
            Codigo = carta.Codigo;
            ImagemUrl = carta.ImagemUrl;
            ValorSimbolico = carta.ValorSimbolico;
            Naipe = carta.Naipe;
            Valor = carta.Valor;
        }
    }
} 