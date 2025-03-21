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
        private const int NumeroExatoDeJogadores = 2;
        private const int PontuacaoMinimaDealer = 17;
        private const string NomeDealer = "Dealer";
        private const string NomeJogador = "Jogador";
        private const int IdDealer = 1;
        private const int IdJogador = 2;
     
        public BlackjackService(IBaralhoApiClient baralhoApiClient, IJogadorFactory jogadorFactory, IJogoFactory jogoFactory)
        {
            _baralhoApiClient = baralhoApiClient;
            _jogadorFactory = jogadorFactory;
            _jogoFactory = jogoFactory;
        }

        public async Task<IJogoBlackJack> CriarJogoBlackJackAsync(int numeroJogadores)
        {
            numeroJogadores = NumeroExatoDeJogadores;
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                IBaralho baralho = await _baralhoApiClient.CriarNovoBaralhoAsync();
                List<IJogadorDeBlackjack> jogadores = await CriarJogadoresParaBlackjackAsync(baralho.BaralhoId);

                ValidacaoService.ValidarCodigoCartas(jogadores);
                baralho.QuantidadeDeCartasRestantes -= jogadores.Sum((jogador) => jogador.Cartas.Count());

                return _jogoFactory.CriarJogoBlackJack(jogadores, baralho);
            });
        }

        private async Task<List<IJogadorDeBlackjack>> CriarJogadoresParaBlackjackAsync(string baralhoId)
        {
            int totalCartas = NumeroExatoDeJogadores * CartasIniciaisPorJogador;
            List<ICarta> todasAsCartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, totalCartas);
            
            List<ICarta> cartasDealer = todasAsCartas.Take(CartasIniciaisPorJogador).ToList();
            List<ICarta> cartasJogador = todasAsCartas.Skip(CartasIniciaisPorJogador).Take(CartasIniciaisPorJogador).ToList();
            
            IJogadorDeBlackjack dealer = _jogadorFactory.CriarJogadorDeBlackJack(cartasDealer, IdDealer, NomeDealer);
            IJogadorDeBlackjack jogador = _jogadorFactory.CriarJogadorDeBlackJack(cartasJogador, IdJogador, NomeJogador);
            
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
            
            numeroJogadores = NumeroExatoDeJogadores;
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores, int.MaxValue);

            try
            {
                return await CriarJogadoresParaBlackjackAsync(baralhoId);
            }
            catch (BaralhoNotFoundException)
            {
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
                var novaCarta = await ComprarEAdicionarCartaAoJogador(baralhoId, jogador);
                int pontuacao = jogador.CalcularPontuacao();
                
                if (EhDealer(jogador) && DeveComprarMaisCartas(pontuacao))
                {
                    return await ComprarCartasAtePontuacaoMinima(baralhoId, jogador);
                }

                return novaCarta;
            }
            catch (ExternalServiceUnavailableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao comprar carta", ex);
            }
        }

        private async Task<ICarta> ComprarEAdicionarCartaAoJogador(string baralhoId, IJogadorDeBlackjack jogador)
        {
            var cartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, 1);
            var novaCarta = cartas.FirstOrDefault();
            
            if (novaCarta != null)
            {
                ValidacaoService.ValidarCodigoCarta(novaCarta);
                jogador.Cartas.Add(novaCarta);
            }

            return novaCarta;
        }

        private bool EhDealer(IJogadorDeBlackjack jogador) => jogador.Nome == NomeDealer;

        private bool DeveComprarMaisCartas(int pontuacao) => pontuacao < PontuacaoMinimaDealer;

        private async Task<ICarta> ComprarCartasAtePontuacaoMinima(string baralhoId, IJogadorDeBlackjack dealer)
        {
            ICarta ultimaCarta = null;
            
            while (dealer.CalcularPontuacao() < PontuacaoMinimaDealer)
            {
                ultimaCarta = await ComprarEAdicionarCartaAoJogador(baralhoId, dealer);
            }
            
            return ultimaCarta;
        }

        public List<IJogadorDeBlackjack> DeterminarVencedoresAsync(List<IJogadorDeBlackjack> jogadores)
        {
            ValidacaoService.ValidarListaJogadores(jogadores);
            
            return ServiceExceptionHandler.HandleServiceException(() =>
            {
                var dealer = EncontrarJogadorPorNome(jogadores, NomeDealer);
                var jogador = EncontrarJogadorPorNome(jogadores, NomeJogador);
                
                if (dealer == null || jogador == null)
                {
                    throw new Exception("Dealer ou jogador não encontrado");
                }
                
                if (dealer.Estourou && jogador.Estourou)
                {
                    return new List<IJogadorDeBlackjack>();
                }
                
                if (jogador.Estourou) return new List<IJogadorDeBlackjack> { dealer };
                if (dealer.Estourou) return new List<IJogadorDeBlackjack> { jogador };
                
                if (jogador.TemBlackjack())
                {
                    return dealer.TemBlackjack() 
                        ? new List<IJogadorDeBlackjack> { dealer, jogador } // Empate
                        : new List<IJogadorDeBlackjack> { jogador };
                }
                
                if (dealer.TemBlackjack()) return new List<IJogadorDeBlackjack> { dealer };
                
                return DeterminarVencedorPorPontuacao(dealer, jogador);
            });
        }

        private IJogadorDeBlackjack EncontrarJogadorPorNome(List<IJogadorDeBlackjack> jogadores, string nome)
        {
            return jogadores.FirstOrDefault(j => j.Nome == nome);
        }

        private List<IJogadorDeBlackjack> DeterminarVencedorPorPontuacao(IJogadorDeBlackjack dealer, IJogadorDeBlackjack jogador)
        {
            int pontosDealer = dealer.CalcularPontuacao();
            int pontosJogador = jogador.CalcularPontuacao();
            
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
                return new List<IJogadorDeBlackjack> { dealer, jogador }; // Empate
            }
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
        
        public async Task<IJogadorDeBlackjack> JogarDealer(string baralhoId, List<IJogadorDeBlackjack> jogadores)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);
            ValidacaoService.ValidarListaJogadores(jogadores);
            
            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                var dealer = EncontrarJogadorPorNome(jogadores, NomeDealer);
                if (dealer == null) throw new Exception("Dealer não encontrado");
                
                var jogador = EncontrarJogadorPorNome(jogadores, NomeJogador);
                if (jogador != null && jogador.Estourou) return dealer;
                
                await ComprarCartasAtePontuacaoMinima(baralhoId, dealer);
                return dealer;
            });
        }
    }
} 