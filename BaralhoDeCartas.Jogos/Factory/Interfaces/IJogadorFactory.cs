using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory.Interfaces
{
    public interface IJogadorFactory
    {
        IJogadorDeBlackjack CriarJogadorDeBlackJack(List<ICarta> cartas, int jogadorId, string nomeJogador);
        IJogadorDeBlackjack CriarJogadorDeBlackJack(JogadorBlackjackDTO jogadorBlackjackDTO);
        IJogador CriarJogador(List<ICarta> cartas, int jogadorId, string nomeJogador);
        IJogador CriarJogador(JogadorDTO jogadorDto);
    }
}
