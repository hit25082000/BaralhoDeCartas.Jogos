//using Microsoft.AspNetCore.Mvc;
//using BaralhoDeCartas.Services.Interfaces;
//using BaralhoDeCartas.Models.Interfaces;
//using BaralhoDeCartas.Models.ViewModel;
//using BaralhoDeCartas.Models.DTOs;
//using System.Text.Json;
//using BaralhoDeCartas.Factory.Interfaces;

//namespace BaralhoDeCartas.Controllers
//{
//    public class BlackjackWebController : Controller
//    {
//        private readonly IBlackjackService _blackjackService;

//        public BlackjackWebController(IBlackjackService blackjackService)
//        {
//            _blackjackService = blackjackService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> Index(int numeroJogadores = 2)
//        {
//            try
//            {
//                // Limitar o número de jogadores
//                if (numeroJogadores < 2)
//                {
//                    numeroJogadores = 2;
//                }

//                if (numeroJogadores > 6)
//                {
//                    numeroJogadores = 6;
//                }

//                var jogoBlackjack = await _blackjackService.CriarJogoBlackJackAsync(numeroJogadores);
//                return View("Blackjack", jogoBlackjack);
//            }
//            catch (Exception ex)
//            {
//                // Log de erro
//                return RedirectToAction("Index", "Jogos");
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> IniciarRodada(string baralhoId, int numeroJogadores)
//        {
//            try
//            {
//                var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, numeroJogadores);

//                return Json(jogadores.Select(j => new
//                {
//                    id = j.JogadorId,
//                    nome = j.Nome,
//                    cartas = j.Cartas.Select(c => new
//                    {
//                        valor = c.Valor,
//                        valorSimbolico = c.ValorSimbolico,
//                        naipe = c.Naipe,
//                        imagem = c.ImagemUrl,
//                        codigo = c.Codigo
//                    })
//                }));
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, error = ex.Message });
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> ComprarCarta(string baralhoId, [FromBody] JogadorBlackjackDTO jogadorBlackJackDto)
//        {
//            try
//            {
//                IJogadorDeBlackjack jogadorDeBlackJack = JogadorBlackjackDTO.ToJogadores(new List<JogadorBlackjackDTO> { jogadorBlackJackDto });

//                var carta = await _blackjackService.ComprarCartaAsync(baralhoId, jogadorDeBlackJack);

//                return Json(new
//                {
//                    id = jogadorDeBlackJack.JogadorId,
//                    nome = jogadorDeBlackJack.Nome,
//                    cartas = jogadorDeBlackJack.Cartas.Select(c => new
//                    {
//                        valor = c.Valor,
//                        valorSimbolico = c.ValorSimbolico,
//                        naipe = c.Naipe,
//                        imagemUrl = c.ImagemUrl,
//                        codigo = c.Codigo
//                    }),
//                    pontos = jogadorDeBlackJack.CalcularPontuacao()
//                });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, error = ex.Message });
//            }
//        }

//        [HttpGet]
//        public async Task<IActionResult> Parar(JogadorBlackjackDTO jogadorBlackJackDto)
//        {
//            try
//            {
//                IJogadorDeBlackjack jogadorDeBlackJack = _jogadorFactory.CriarJogadorDeBlackJack(jogadorBlackJackDto);

//                var carta = await _blackjackService.Para(baralhoId, jogadorDeBlackJack);

//                return Json(new
//                {
//                    id = jogadorDeBlackJack.JogadorId,
//                    nome = jogadorDeBlackJack.Nome,
//                    cartas = jogadorDeBlackJack.Cartas.Select(c => new
//                    {
//                        valor = c.Valor,
//                        valorSimbolico = c.ValorSimbolico,
//                        naipe = c.Naipe,
//                        imagemUrl = c.ImagemUrl,
//                        codigo = c.Codigo
//                    }),
//                    pontos = jogadorDeBlackJack.CalcularPontuacao()
//                });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { success = false, error = ex.Message });
//            }
//        }

//        [HttpPost]
//        public IActionResult RenderizarCarta([FromBody] CartaViewModel carta)
//        {
//            return PartialView("_CartaPartial", carta);
//        }
//    }
//}