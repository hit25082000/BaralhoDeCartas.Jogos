using BaralhoDeCartas.Api.Interfaces;
using BaralhoDeCartas.Factory;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Exceptions;
using BaralhoDeCartas.Common;

namespace BaralhoDeCartas.Services
{
    public class MaiorCartaService : IMaiorCartaService
    {
        private readonly IBaralhoApiClient _baralhoApiClient;
        private readonly IJogadorFactory _jogadorFactory;
        private readonly IJogoFactory _jogoFactory;
        private const int CARTAS_POR_JOGADOR = 5;
        private const int MINIMO_CARTAS_BARALHO = 10;

        public MaiorCartaService(IBaralhoApiClient baralhoApiClient, IJogadorFactory jogadorFactory, IJogoFactory jogoFactory)
        {
            _baralhoApiClient = baralhoApiClient;
            _jogadorFactory = jogadorFactory;
            _jogoFactory = jogoFactory;
        }

        private void ValidarListaJogadores(List<IJogador> jogadores)
        {
            ValidacaoService.ValidarListaJogadores(jogadores);
            

            foreach (var jogador in jogadores)
            {
                if (jogador == null)
                {
                    throw new ArgumentException("A lista não pode conter jogadores nulos");
                }
                
                if (string.IsNullOrEmpty(jogador.Nome))
                {
                    throw new ArgumentException("Todos os jogadores devem ter um nome válido");
                }
            }
        }

        public async Task<IJogoMaiorCarta> CriarJogoMaiorCartaAsync(int numeroJogadores)
        {
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                var baralho = await _baralhoApiClient.CriarNovoBaralhoAsync();
                var jogadores = await DistribuirCartasAsync(baralho.BaralhoId, numeroJogadores);
                return _jogoFactory.CriarJogoMaiorCarta(jogadores, baralho);
            });
        }

        public async Task<IBaralho> CriarNovoBaralhoAsync()
        {
            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                return await _baralhoApiClient.CriarNovoBaralhoAsync();
            });
        }

        public async Task<List<IJogador>> DistribuirCartasAsync(string baralhoId, int numeroJogadores)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);
            ValidacaoService.ValidarNumeroJogadores(numeroJogadores);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                List<IJogador> jogadores = new List<IJogador>();
                int totalCartas = numeroJogadores * CARTAS_POR_JOGADOR;

                List<ICarta> todasAsCartas = await _baralhoApiClient.ComprarCartasAsync(baralhoId, totalCartas);
                
                for (int i = 0; i < numeroJogadores; i++)
                {
                    List<ICarta> cartasDoJogador = todasAsCartas.Skip(i * CARTAS_POR_JOGADOR).Take(CARTAS_POR_JOGADOR).ToList();

                    int jogadorId = i + 1;
                    string nomeJogador = $"Jogador {jogadorId}";

                    IJogador jogador = _jogadorFactory.CriarJogador(cartasDoJogador, jogadorId, nomeJogador);               
                    jogadores.Add(jogador);
                }

                return jogadores;
            });
        }

        public async Task<IJogador> DeterminarVencedorAsync(List<IJogador> jogadores)
        {
            ValidarListaJogadores(jogadores);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                // Verificar se todos os jogadores possuem cartas
                if (jogadores.Any(j => j.Cartas == null || !j.Cartas.Any()))
                {
                    throw new ArgumentException("Todos os jogadores devem ter pelo menos uma carta");
                }
                
                return jogadores
                    .OrderByDescending(j => j.ObterCartaDeMaiorValor()?.Valor ?? 0)
                    .FirstOrDefault();
            });
        }

        public async Task<IBaralho> FinalizarJogoAsync(string baralhoId)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                return await _baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId);
            });
        }
        
        public async Task<IBaralho> VerificarBaralhoAsync(string baralhoId)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                var baralho = await _baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, true);
                
                if (baralho.QuantidadeDeCartasRestantes < MINIMO_CARTAS_BARALHO)
                {
                    baralho = await _baralhoApiClient.RetornarCartasAoBaralhoAsync(baralhoId);
                    baralho = await _baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, false);
                }
                
                return baralho;
            });
        }
        
        public async Task<IBaralho> EmbaralharBaralhoAsync(string baralhoId, bool embaralharSomenteCartasRestantes)
        {
            ValidacaoService.ValidarBaralhoId(baralhoId);

            return await ServiceExceptionHandler.HandleServiceExceptionAsync(async () =>
            {
                return await _baralhoApiClient.EmbaralharBaralhoAsync(baralhoId, embaralharSomenteCartasRestantes);
            });
        }
    }
} 