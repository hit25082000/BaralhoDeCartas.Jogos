using System.Linq;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Models.DTOs
{
    public class JogadorBlackjackDTO : JogadorDTO
    {
        public bool Parou { get; set; }
        public bool Estourou { get; set; }
        public int Pontuacao { get; set; }
        public bool TemBlackjack { get; set; }

        public JogadorBlackjackDTO() : base()
        {
        }

        public JogadorBlackjackDTO(IJogadorDeBlackjack jogador) : base(jogador)
        {
            Parou = jogador.Parou;
            Estourou = jogador.Estourou;
            Pontuacao = jogador.CalcularPontuacao();
            TemBlackjack = jogador.TemBlackjack();
        }

        public static List<JogadorBlackjackDTO> FromJogadores(List<IJogadorDeBlackjack> jogadores)
        {
            return jogadores.Select(j => new JogadorBlackjackDTO(j)).ToList();
        }

        public static List<IJogadorDeBlackjack> ToJogadores(List<JogadorBlackjackDTO> jogadoresDTO)
        {
            return jogadoresDTO.Select(dto =>
            {
                var cartas = dto.Cartas.Select(c =>
                {
                    ICarta carta = new Carta(c.Codigo, c.ImagemUrl, c.ValorSimbolico, c.Naipe);
                    return carta;
                }).ToList();

                IJogadorDeBlackjack jogador = new JogadorDeBlackjack(cartas, dto.JogadorId, dto.Nome);
                return jogador;
            }).ToList();
        }
    }
} 