using BaralhoDeCartas.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaralhoDeCartas.Common
{
    public static class ValidacaoService
    {
        public static void ValidarNumeroJogadores(int numeroJogadores, int maximo = 10)
        {
            if (numeroJogadores <= 0)
            {
                throw new ArgumentException("O número de jogadores deve ser maior que zero");
            }

            if (numeroJogadores >= maximo)
            {
                throw new ArgumentException($"O número de jogadores deve ser menor que {maximo}");
            }
        }

        public static void ValidarBaralhoId(string baralhoId)
        {
            if (string.IsNullOrEmpty(baralhoId))
            {
                throw new ArgumentException("O ID do baralho não pode ser nulo ou vazio");
            }
        }

        public static void ValidarListaJogadores<T>(List<T> jogadores) where T : IJogador
        {
            if (jogadores == null || !jogadores.Any())
            {
                throw new ArgumentException("A lista de jogadores não pode estar vazia");
            }
        }

        public static void ValidarJogadoresDuplicados<T>(List<T> jogadores) where T : IJogador
        {
            var jogadoresAgrupados = jogadores.GroupBy(j => j.JogadorId);
            var jogadoresDuplicados = jogadoresAgrupados.Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            
            if (jogadoresDuplicados.Any())
            {
                throw new ArgumentException($"A lista de jogadores não pode ter jogadores com o mesmo Id. IDs duplicados: {string.Join(", ", jogadoresDuplicados)}");
            }
        }

        public static void ValidarCartasDuplicadas<T>(List<T> jogadores) where T : IJogador
        {
            var cartasRegistradas = new Dictionary<string, int>();
            
            foreach (var jogador in jogadores)
            {
                foreach (var carta in jogador.Cartas)
                {
                    string codigoCarta = carta.Codigo;
                    
                    if (cartasRegistradas.ContainsKey(codigoCarta))
                    {
                        int jogadorIdDuplicado = cartasRegistradas[codigoCarta];
                        throw new InvalidOperationException(
                            $"Carta duplicada encontrada: {codigoCarta}. " +
                            $"A carta está com o Jogador {jogadorIdDuplicado} e Jogador {jogador.JogadorId}");
                    }
                    
                    cartasRegistradas.Add(codigoCarta, jogador.JogadorId);
                }
            }
        }

        public static void ValidarQuantidadeCartasBaralho(IBaralho baralho, int minimoCartas)
        {
            if (baralho.QuantidadeDeCartasRestantes < minimoCartas)
            {
                throw new InvalidOperationException("Quantidade insuficiente de cartas no baralho");
            }
        }

        public static void ValidarJogadorDeBlackjack(IJogadorDeBlackjack jogador)
        {
            if (jogador == null)
            {
                throw new ArgumentNullException(nameof(jogador), "O jogador não pode ser nulo");
            }

            if (jogador.Parou || jogador.Estourou)
            {
                throw new InvalidOperationException($"O jogador {jogador.Nome} não pode comprar mais cartas.");
            }
        }

        public static void ValidarCodigoCarta(ICarta carta)
        {
            if (carta == null)
            {
                throw new ArgumentNullException(nameof(carta), "A carta não pode ser nula");
            }

            if (string.IsNullOrEmpty(carta.Codigo))
            {
                throw new ArgumentException("O código da carta não pode ser nulo ou vazio");
            }

            if (string.IsNullOrEmpty(carta.ValorSimbolico))
            {
                throw new ArgumentException("O valor simbólico da carta não pode ser nulo ou vazio");
            }

            if (string.IsNullOrEmpty(carta.Naipe))
            {
                throw new ArgumentException("O naipe da carta não pode ser nulo ou vazio");
            }

            string valorSimbolico = carta.ValorSimbolico;
            string naipe = carta.Naipe;
            string codigo = carta.Codigo;

            // Obtém a primeira letra do naipe (H para HEARTS, S para SPADES, etc.)
            char letraNaipe = ObterLetraNaipe(naipe);
            char letraValorSimbolico = ObterLetraValorSimbolico(valorSimbolico);

            // O código esperado deve ser o valor simbólico seguido da letra do naipe
            string codigoEsperado = letraValorSimbolico.ToString() + letraNaipe;

            if (codigo != codigoEsperado)
            {
                throw new ArgumentException(
                    $"O código da carta '{codigo}' não corresponde ao valor simbólico '{valorSimbolico}' e naipe '{naipe}'. " +
                    $"O código esperado seria '{codigoEsperado}'");
            }
        }

        public static void ValidarCodigoCartas<T>(List<T> jogadores) where T : IJogador
        {
            if (jogadores == null)
            {
                throw new ArgumentNullException(nameof(jogadores), "A lista de jogadores não pode ser nula");
            }

            foreach (var jogador in jogadores)
            {
                foreach (var carta in jogador.Cartas)
                {
                    ValidarCodigoCarta(carta);
                }
            }
        }

        private static char ObterLetraNaipe(string naipe)
        {
            return naipe.ToUpper() switch
            {
                "HEARTS" => 'H',
                "SPADES" => 'S',
                "CLUBS" => 'C',
                "DIAMONDS" => 'D',
                _ => throw new ArgumentException($"Naipe desconhecido: {naipe}")
            };
        }

        private static char ObterLetraValorSimbolico(string valorSimbolico)
        {
            return valorSimbolico.ToUpper() switch
            {
                "ACE" => 'A',
                "2" => '2',
                "3" => '3',
                "4" => '4',
                "5" => '5',
                "6" => '6',
                "7" => '7',
                "8" => '8',
                "9" => '9',
                "10" => '0',
                "JACK" => 'J',
                "QUEEN" => 'Q',
                "KING" => 'K',
                _ => throw new ArgumentException($"Valor simbólico desconhecido: {valorSimbolico}")
            };
        }
    }
} 