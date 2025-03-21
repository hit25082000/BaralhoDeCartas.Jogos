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
            // Forçar exatamente 2 jogadores (dealer e player)
            numeroJogadores = 2;
            
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                IBaralho baralho = await _baralhoApiClient.CriarNovoBaralhoAsync();
                List<IJogadorDeBlackjack> jogadores = await CriarJogadoresParaBlackjackAsync(baralho.BaralhoId);

                // Validar a consistência do código de cada carta
                ValidacaoService.ValidarCodigoCartas(jogadores);

                baralho.QuantidadeDeCartasRestantes -= jogadores.Sum((jogador) => jogador.Cartas.Count());

                return _jogoFactory.CriarJogoBlackJack(jogadores, baralho);
            });
        }

        /// <summary>
        /// Método específico para criar jogadores para Blackjack (dealer e jogador)
        /// </summary>
        private async Task<List<IJogadorDeBlackjack>> CriarJogadoresParaBlackjackAsync(string baralhoId)
        {
            // Total de cartas para os dois jogadores
            int totalCartas = 2 * CartasIniciaisPorJogador;
            
            // Comprar todas as cartas de uma vez
            List<ICarta> todasAsCartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, totalCartas);
            
            // Separar as cartas para dealer e jogador
            List<ICarta> cartasDealer = todasAsCartas.Take(CartasIniciaisPorJogador).ToList();
            List<ICarta> cartasJogador = todasAsCartas.Skip(CartasIniciaisPorJogador).Take(CartasIniciaisPorJogador).ToList();
            
            // Criar os jogadores com os nomes adequados
            IJogadorDeBlackjack dealer = _jogadorFactory.CriarJogadorDeBlackJack(cartasDealer, 1, "Dealer");
            IJogadorDeBlackjack jogador = _jogadorFactory.CriarJogadorDeBlackJack(cartasJogador, 2, "Jogador");
            
            // Retornar a lista de jogadores
            return new List<IJogadorDeBlackjack> { dealer, jogador };
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
            
            // Forçar exatamente 2 jogadores (dealer e player)
            numeroJogadores = 2;
            
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            try
            {
                // Usar o método específico para criar jogadores de Blackjack
                return await CriarJogadoresParaBlackjackAsync(baralhoId);
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

                // Calcular pontuação após adicionar carta
                int pontuacao = jogador.CalcularPontuacao();
                
                // Verificar se é a vez do dealer e se ele deve continuar comprando cartas
                if (jogador.Nome == "Dealer" && pontuacao < 17)
                {
                    // Dealer deve comprar até ter pelo menos 17 pontos
                    return await ComprarCartaAsync(baralhoId, jogador);
                }

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
                var dealer = jogadores.FirstOrDefault(j => j.Nome == "Dealer");
                var jogador = jogadores.FirstOrDefault(j => j.Nome == "Jogador");
                
                if (dealer == null || jogador == null)
                {
                    throw new Exception("Dealer ou jogador não encontrado");
                }
                
                int pontosDealer = dealer.CalcularPontuacao();
                int pontosJogador = jogador.CalcularPontuacao();
                
                // Se o jogador estourou, o dealer vence
                if (jogador.Estourou)
                {
                    return new List<IJogadorDeBlackjack> { dealer };
                }
                
                // Se o dealer estourou, o jogador vence
                if (dealer.Estourou)
                {
                    return new List<IJogadorDeBlackjack> { jogador };
                }
                
                // Jogador tem blackjack
                if (jogador.TemBlackjack())
                {
                    // Se dealer também tem blackjack, é empate
                    if (dealer.TemBlackjack())
                    {
                        return new List<IJogadorDeBlackjack> { dealer, jogador };
                    }
                    
                    // Senão, jogador vence
                    return new List<IJogadorDeBlackjack> { jogador };
                }
                
                // Dealer tem blackjack (e jogador não tem)
                if (dealer.TemBlackjack())
                {
                    return new List<IJogadorDeBlackjack> { dealer };
                }
                
                // Ambos não estouraram e não têm blackjack, compara pontuação
                if (pontosDealer > pontosJogador)
                {
                    return new List<IJogadorDeBlackjack> { dealer };
                }
                else if (pontosJogador > pontosDealer)
                {
                    return new List<IJogadorDeBlackjack> { jogador };
                }
                else
                {
                    // Empate
                    return new List<IJogadorDeBlackjack> { dealer, jogador };
                }
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
        
        /// <summary>
        /// Executa a jogada do dealer após o jogador parar
        /// </summary>
        public async Task<IJogadorDeBlackjack> JogarDealer(string baralhoId, List<IJogadorDeBlackjack> jogadores)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);
            ValidacaoService.ValidarListaJogadores(jogadores);
            
            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                // Encontrar o dealer
                var dealer = jogadores.FirstOrDefault(j => j.Nome == "Dealer");
                if (dealer == null)
                {
                    throw new Exception("Dealer não encontrado");
                }
                
                // Verificar se o jogador estourou (dealer não precisa jogar)
                var jogador = jogadores.FirstOrDefault(j => j.Nome == "Jogador");
                if (jogador != null && jogador.Estourou)
                {
                    return dealer;
                }
                
                // Calcular pontuação atual do dealer
                int pontuacaoDealer = dealer.CalcularPontuacao();
                
                // Dealer deve comprar cartas até ter pelo menos 17 pontos
                while (pontuacaoDealer < 17)
                {
                    await ComprarCartaAsync(baralhoId, dealer);
                    pontuacaoDealer = dealer.CalcularPontuacao();
                }
                
                return dealer;
            });
        }
    }
} 