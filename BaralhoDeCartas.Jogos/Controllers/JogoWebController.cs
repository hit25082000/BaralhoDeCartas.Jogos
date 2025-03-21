using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Models.ViewModel;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Factory;

namespace BaralhoDeCartas.Controllers   
{    
    public class JogoWebController : Controller
    {
        private readonly IMaiorCartaService _maiorCartaService;
        private readonly IBlackjackService _blackjackService;
        private readonly IJogadorFactory _jogadorFactory;

        public JogoWebController(IMaiorCartaService jogoService, IBlackjackService blackjackService,IJogadorFactory jogadorFactory)
        {
            _maiorCartaService = jogoService;
            _blackjackService = blackjackService;
            _jogadorFactory = jogadorFactory;
        }

        public async Task<IActionResult> Index(string jogo, int numeroJogadores)
        {
            if (numeroJogadores < 2)
            {
                numeroJogadores = 2;
            }

            if (numeroJogadores > 6)
            {
                numeroJogadores = 6;
            }

            if (string.IsNullOrEmpty(jogo))
            {
                return RedirectToAction("Index", "Jogos");
            }

            switch (jogo.ToLower())
            {
                case "maiorcarta":
                    var jogoMaiorCarta = await _maiorCartaService.CriarJogoMaiorCartaAsync(numeroJogadores);
                    return View("MaiorCarta", jogoMaiorCarta);

                case "blackjack":
                    var jogoBlackjack = await _blackjackService.CriarJogoBlackJackAsync(numeroJogadores);
                    return View("Blackjack", jogoBlackjack);

                default:
                    return RedirectToAction("Index", "Jogos");
            }
        }

        public IActionResult EscolherJogador()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> IniciarMaiorCarta(int numeroJogadores)
        {
            try
            {
                if (numeroJogadores < 2)
                {
                    numeroJogadores = 2;
                }
                
                if (numeroJogadores > 6)
                {
                    numeroJogadores = 6;
                }

                var jogoMaiorCarta = await _maiorCartaService.CriarJogoMaiorCartaAsync(numeroJogadores);
                
                return Json(new { 
                    success = true, 
                    baralho = new {
                        baralhoId = jogoMaiorCarta.Baralho.BaralhoId,
                        quantidadeDeCartasRestantes = jogoMaiorCarta.Baralho.QuantidadeDeCartasRestantes,
                        estaEmbaralhado = jogoMaiorCarta.Baralho.EstaEmbaralhado
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DistribuirCartas(string baralhoId, int numeroJogadores)
        {
            IBaralho baralho = null;
            List<IJogador> jogadores = null;
            int tentativas = 0;
            const int maxTentativas = 3;

            while (tentativas < maxTentativas)
            {
                try
                {
                    baralho = await _maiorCartaService.VerificarBaralhoAsync(baralhoId);
                    
                    if (baralho.QuantidadeDeCartasRestantes < numeroJogadores * 5) // 5 cartas por jogador
                    {
                        baralho = await _maiorCartaService.FinalizarJogoAsync(baralhoId);
                        
                        baralho = await _maiorCartaService.EmbaralharBaralhoAsync(baralhoId, false);
                    }
                    
                    jogadores = await _maiorCartaService.DistribuirCartasAsync(baralhoId, numeroJogadores);
                    
                    return Json(new { 
                        success = true, 
                        baralho = new {
                            quantidadeDeCartasRestantes = baralho.QuantidadeDeCartasRestantes,
                            estaEmbaralhado = baralho.EstaEmbaralhado
                        },
                        data = jogadores.Select(j => new {
                            id = j.JogadorId,
                            nome = j.Nome,
                            cartas = j.Cartas.Select(c => new {
                                valor = c.Valor,
                                valorSimbolico = c.ValorSimbolico,
                                naipe = c.Naipe,
                                imagem = c.ImagemUrl,
                                codigo = c.Codigo
                            })
                        })
                    });
                }
                catch (Exception ex)
                {
                    tentativas++;
                    
                    if (ex.Message.Contains("Falha ao comprar cartas") || tentativas >= maxTentativas)
                    {
                        try
                        {
                            baralho = await _maiorCartaService.CriarNovoBaralhoAsync();
                            baralhoId = baralho.BaralhoId; 
                            
                            jogadores = await _maiorCartaService.DistribuirCartasAsync(baralhoId, numeroJogadores);
                            
                            return Json(new { 
                                success = true,
                                novoBaralho = true,
                                baralhoId = baralhoId,
                                baralho = new {
                                    quantidadeDeCartasRestantes = baralho.QuantidadeDeCartasRestantes,
                                    estaEmbaralhado = baralho.EstaEmbaralhado
                                },
                                data = jogadores.Select(j => new {
                                    id = j.JogadorId,
                                    nome = j.Nome,
                                    cartas = j.Cartas.Select(c => new {
                                        valor = c.Valor,
                                        valorSimbolico = c.ValorSimbolico,
                                        naipe = c.Naipe,
                                        imagem = c.ImagemUrl,
                                        codigo = c.Codigo
                                    })
                                })
                            });
                        }
                        catch (Exception innerEx)
                        {
                            return Json(new { success = false, error = $"Erro após criar novo baralho: {innerEx.Message}" });
                        }
                    }
                    
                    continue;
                }
            }
            
            return Json(new { success = false, error = "Não foi possível distribuir as cartas após várias tentativas." });
        }

        [HttpPost]
        public IActionResult RenderizarCarta([FromBody] CartaViewModel carta)
        {
            return PartialView("_CartaPartial", carta);
        }

        [HttpGet]
        public async Task<IActionResult> IniciarRodada(string baralhoId, int numeroJogadores)
        {
            var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, numeroJogadores);
            return Json(jogadores);
        }

        [HttpGet]
        public async Task<IActionResult> ComprarCarta(string baralhoId, IJogadorDeBlackjack jogadorId)
        {
            var jogador = await _blackjackService.ComprarCartaAsync(baralhoId, jogadorId);
            return Json(jogador);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarJogo(string baralhoId)
        {
            await _maiorCartaService.FinalizarJogoAsync(baralhoId);
            return RedirectToAction("Index", "Jogos");
        }

        [HttpPost]
        public async Task<IActionResult> DeterminarVencedor([FromBody] List<JogadorDTO> jogadoresDto)
        {
            try
            {
                List<IJogador> jogadores = new List<IJogador>();

                foreach (var jogador in jogadoresDto)
                {
                    jogadores.Add(_jogadorFactory.CriarJogador(jogador));
                }

                IJogador vencedor = await _maiorCartaService.DeterminarVencedorAsync(jogadores);

                return Json(new
                {
                    success = true,
                    vencedor = new
                    {
                        id = vencedor.JogadorId,
                        nome = vencedor.JogadorId == 1 ? "Computador" : "Jogador " + (vencedor.JogadorId - 1),
                        isComputador = vencedor.JogadorId == 1
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
