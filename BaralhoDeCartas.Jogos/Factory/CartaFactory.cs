using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory
{
    public class CartaFactory : ICartaFactory
    {
        public List<ICarta> CriarCartas(CartasResponse response)
        {
            return response.Cards
            .Select(c => (ICarta)new Carta(c.Code,c.Image,c.Value,c.Suit))
            .ToList();
        }
        public List<ICarta> CriarCartas(List<CartaDTO> cartasDto)
        {
            return cartasDto
            .Select(c => (ICarta)new Carta(c.Codigo, c.ImagemUrl,c.ValorSimbolico, c.Naipe))
            .ToList();
        }
    }
}
