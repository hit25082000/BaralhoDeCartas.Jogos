using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Exceptions;
using BaralhoDeCartas.Common;

namespace BaralhoDeCartas.Services
{
    public class BlackjackService : IBlackjackService
    {
        private readonly IBaralhoApiClient _baralhoApiClient;
        private readonly IJogadorFactory _jogadorFactory;
        private readonly IJogoFactory _jogoFactory;
        private const int CartasIniciaisPorJogador = 2;
     
        public BlackjackService(IBaralhoApiClient baralhoApiClient, IJogadorFactory jogadorFactory, IJogoFactory jogoFactory)
        {
            _baralhoApiClient = baralhoApiClient;
            _jogadorFactory = jogadorFactory;
            _jogoFactory = jogoFactory;
        }

        public async Task<IJogoBlackJack> CriarJogoBlackJackAsync(int numeroJogadores)
        {
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                IBaralho baralho = await _baralhoApiClient.CriarNovoBaralhoAsync();
                List<IJogadorDeBlackjack> jogadores = await IniciarRodadaAsync(baralho.BaralhoId, numeroJogadores);

                // Validar a consistência do código de cada carta
                ValidacaoService.ValidarCodigoCartas(jogadores);

                baralho.QuantidadeDeCartasRestantes -= jogadores.Sum((jogador) => jogador.Cartas.Count());

                return _jogoFactory.CriarJogoBlackJack(jogadores, baralho);
            });
        }

        public async Task<IBaralho> CriarNovoBaralhoAsync()
        {
            try
            {
                return await _baralhoApiClient.CriarNovoBaralhoAsync();
            }
            catch (ExternalServiceUnavailableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao criar novo baralho", ex);
            }
        }

        public async Task<List<IJogadorDeBlackjack>> IniciarRodadaAsync(string baralhoId, int numeroJogadores)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            try
            {
                List<IJogadorDeBlackjack> jogadores = new List<IJogadorDeBlackjack>();
                int totalCartas = numeroJogadores * CartasIniciaisPorJogador;

                List<ICarta> todasAsCartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, totalCartas);
                
                for (int i = 0; i < numeroJogadores; i++)
                {
                    List<ICarta> cartasDoJogador = todasAsCartas.Skip(i * CartasIniciaisPorJogador)
                              .Take(CartasIniciaisPorJogador)
                              .ToList();

                    int jogadorId = i + 1;
                    string nomeJogador = $"Jogador {jogadorId}";

                    IJogadorDeBlackjack jogador = _jogadorFactory.CriarJogadorDeBlackJack(cartasDoJogador, jogadorId, nomeJogador);
                    jogadores.Add(jogador);
                }

                return jogadores;
            }
            catch (BaralhoNotFoundException)
            {
                // Propaga a exceção para o teste poder capturá-la
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao iniciar rodada", ex);
            }
        }

        public async Task<ICarta> ComprarCartaAsync(string baralhoId, IJogadorDeBlackjack jogador)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);
            ValidacaoService.ValidarJogadorDeBlackjack(jogador);

            try
            {
                var cartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, 1);
                var novaCarta = cartas.FirstOrDefault();
                
                if (novaCarta != null)
                {
                    // Validar a consistência do código da nova carta
                    ValidacaoService.ValidarCodigoCarta(novaCarta);
                    
                    jogador.Cartas.Add(novaCarta);
                }

                jogador.CalcularPontuacao();

                return novaCarta;
            }
            catch (ExternalServiceUnavailableException)
            {
                // Propaga a exceção para o teste poder capturá-la
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao comprar carta", ex);
            }
        }

        public List<IJogadorDeBlackjack> DeterminarVencedoresAsync(List<IJogadorDeBlackjack> jogadores)
        {
            ValidacaoService.ValidarListaJogadores(jogadores);
            
            return ServiceExceptionHandler.HandleServiceException(() =>
            {
                // Filtra os jogadores que não estouraram
                var jogadoresValidos = jogadores.Where(j => !j.Estourou).ToList();

                // Se não houver jogadores válidos (todos estouraram), retorna lista vazia
                if (!jogadoresValidos.Any())
                {
                    return new List<IJogadorDeBlackjack>();
                }

                // Verifica se algum jogador tem blackjack (21 com 2 cartas)
                var jogadoresComBlackjack = jogadoresValidos.Where(j => j.TemBlackjack()).ToList();
                if (jogadoresComBlackjack.Any())
                {
                    return jogadoresComBlackjack;
                }

                // Caso contrário, retorna os jogadores com maior pontuação
                var maiorPontuacao = jogadoresValidos.Max(j => j.CalcularPontuacao());
                return jogadoresValidos.Where(j => j.CalcularPontuacao() == maiorPontuacao).ToList();
            });
        }

        public async Task<IBaralho> RetornarCartasAoBaralhoAsync(string baralhoId)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);

            try
            {
                return await _baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId);
            }
            catch (BaralhoNotFoundException)
            {
                // Propaga a exceção para o teste poder capturá-la
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao retornar cartas ao baralho", ex);
            }
        }

        public async Task<IJogadorDeBlackjack> PararJogador(IJogadorDeBlackjack jogadorDeBlackJack)
        {
            ValidacaoService.ValidarJogadorDeBlackjack(jogadorDeBlackJack);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                jogadorDeBlackJack.Parou = true;

                return jogadorDeBlackJack;
            });
        }
    }
} 