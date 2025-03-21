using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Models.ViewModel;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Factory.Interfaces;
using BaralhoDeCartas.Factory;

namespace BaralhoDeCartas.Controllers   
{    
    public class MaiorCartaWebController : Controller
    {
        private readonly IMaiorCartaService _maiorCartaService;
        private readonly IJogadorFactory _jogadorFactory;

        public MaiorCartaWebController(IMaiorCartaService jogoService, IJogadorFactory jogadorFactory)
        {
            _maiorCartaService = jogoService;
            _jogadorFactory = jogadorFactory;
        }

        public async Task<IActionResult> Index(int numeroJogadores)
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
            return View("MaiorCarta", jogoMaiorCarta);
        }

        public IActionResult EscolherJogadores()
        {
            return View();
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
                    
                    if (baralho.QuantidadeDeCartasRestantes < numeroJogadores * 5)
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
