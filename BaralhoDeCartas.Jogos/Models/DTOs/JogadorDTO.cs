using BaralhoDeCartas.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace BaralhoDeCartas.Models.DTOs
{
    public class JogadorDTO
    {
        public int JogadorId { get; set; }
        public string Nome { get; set; }
        public List<CartaDTO> Cartas { get; set; }

        public JogadorDTO()
        {
            Cartas = new List<CartaDTO>();
        }

        public JogadorDTO(IJogador jogador)
        {
            JogadorId = jogador.JogadorId;
            Nome = jogador.Nome;
            Cartas = jogador.Cartas.Select(c => new CartaDTO(c)).ToList();
        }

        public static List<JogadorDTO> FromJogadores(List<IJogador> jogadores)
        {
            return jogadores.Select(j => new JogadorDTO(j)).ToList();
        }

        public static List<IJogador> ToJogadores(List<JogadorDTO> jogadoresDTO)
        {
            return jogadoresDTO.Select(dto =>
            {
                var cartas = dto.Cartas.Select(c => 
                {
                    ICarta carta = new Carta(c.Codigo, c.ImagemUrl, c.ValorSimbolico, c.Naipe);
                    return carta;
                }).ToList();

                IJogador jogador = new Jogador(cartas, dto.JogadorId, dto.Nome);
                return jogador;
            }).ToList();
        }
    }
} 