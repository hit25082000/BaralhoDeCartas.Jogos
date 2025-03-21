using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.ApiResponses;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Factory
{
    public class JogadorFactory : IJogadorFactory
    {
        private readonly ICartaFactory _cartaFactory;        
        public JogadorFactory(ICartaFactory cartaFactory)
        {
            _cartaFactory = cartaFactory;    
        }

        public IJogador CriarJogador(List<ICarta> cartas, int jogadorId, string nomeJogador)
        {
            return new Jogador(cartas, jogadorId, nomeJogador);
        }
         public IJogador CriarJogador(JogadorDTO jogadorDto)
        {
            List<ICarta> cartas = _cartaFactory.CriarCartas(jogadorDto.Cartas);

            return new Jogador(cartas, jogadorDto.JogadorId, jogadorDto.Nome);
        }

        public IJogadorDeBlackjack CriarJogadorDeBlackJack(List<ICarta> cartas, int jogadorId, string nomeJogador)
        {     
            return new JogadorDeBlackjack(cartas, jogadorId, nomeJogador);
        } 
        
        public IJogadorDeBlackjack CriarJogadorDeBlackJack(JogadorBlackjackDTO jogadorBlackjackDTO)
        {
            List<ICarta> cartas = _cartaFactory.CriarCartas(jogadorBlackjackDTO.Cartas);

            return new JogadorDeBlackjack(cartas, jogadorBlackjackDTO.JogadorId, jogadorBlackjackDTO.Nome);
        }
    }
}
