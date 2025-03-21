using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Models.ViewModel;
using BaralhoDeCartas.Models.DTOs;
using System.Text.Json;
using BaralhoDeCartas.Factory.Interfaces;

namespace BaralhoDeCartas.Controllers
{
    public class BlackjackWebController : Controller
    {
        private readonly IBlackjackService _blackjackService;
        private readonly IJogadorFactory _jogadorFactory;

        public BlackjackWebController(IBlackjackService blackjackService, IJogadorFactory jogadorFactory)
        {
            _blackjackService = blackjackService;
            _jogadorFactory = jogadorFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int numeroJogadores = 2)
        {
            try
            {
                // O serviço agora já força exatamente 2 jogadores (dealer e jogador)
                var jogoBlackjack = await _blackjackService.CriarJogoBlackJackAsync(numeroJogadores);
                return View("Blackjack", jogoBlackjack);
            }
            catch (Exception ex)
            {
                // Log de erro
                return RedirectToAction("Index", "Jogos");
            }
        }

        [HttpGet]
        public async Task<IActionResult> IniciarRodada(string baralhoId, int numeroJogadores)
        {
            try
            {
                // O serviço agora já força exatamente 2 jogadores (dealer e jogador)
                var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, numeroJogadores);
                
                return Json(jogadores.Select(j => new
                {
                    id = j.JogadorId,
                    nome = j.Nome,
                    cartas = j.Cartas.Select(c => new
                    {
                        valor = c.ValorSimbolico,
                        naipe = ObterSimbolo(c.Naipe),
                        imagem = c.ImagemUrl,
                        codigo = c.Codigo
                    })
                }));
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ComprarCarta(string baralhoId, int jogadorId)
        {
            try
            {
                // Recuperar informações dos jogadores
                var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, 2);
                var jogadorAtual = jogadores.FirstOrDefault(j => j.JogadorId == jogadorId);
                
                if (jogadorAtual == null)
                {
                    return Json(new { success = false, error = "Jogador não encontrado" });
                }

                var carta = await _blackjackService.ComprarCartaAsync(baralhoId, jogadorAtual);
                
                // Calcular a pontuação atualizada (não precisamos fazer manualmente, o serviço já faz)
                int pontuacao = jogadorAtual.CalcularPontuacao();
                bool estourou = pontuacao > 21;

                return Json(new
                {
                    id = jogadorAtual.JogadorId,
                    nome = jogadorAtual.Nome,
                    cartas = jogadorAtual.Cartas.Select(c => new
                    {
                        valor = c.ValorSimbolico,
                        naipe = ObterSimbolo(c.Naipe),
                        imagem = c.ImagemUrl,
                        codigo = c.Codigo
                    }),
                    pontos = pontuacao,
                    estourou = estourou
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DealerJogar(string baralhoId)
        {
            try
            {
                var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, 2);
                
                // Usar o novo método para fazer o dealer jogar
                var dealer = await _blackjackService.JogarDealer(baralhoId, jogadores);
                
                // Buscar o jogador para determinar o vencedor
                var jogador = jogadores.FirstOrDefault(j => j.Nome == "Jogador");
                
                if (jogador == null)
                {
                    return Json(new { success = false, error = "Jogador não encontrado" });
                }
                
                // Determinar o vencedor
                string vencedor = "empate";
                int pontosJogador = jogador.CalcularPontuacao();
                int pontosDealer = dealer.CalcularPontuacao();

                if (pontosDealer > 21 || (pontosJogador <= 21 && pontosJogador > pontosDealer))
                {
                    vencedor = "player";
                }
                else if (pontosJogador > 21 || (pontosDealer <= 21 && pontosDealer > pontosJogador))
                {
                    vencedor = "dealer";
                }
                
                return Json(new 
                { 
                    success = true,
                    dealer = new {
                        pontos = pontosDealer,
                        cartas = dealer.Cartas.Select(c => new
                        {
                            valor = c.ValorSimbolico,
                            naipe = ObterSimbolo(c.Naipe),
                            imagem = c.ImagemUrl,
                            codigo = c.Codigo
                        })
                    },
                    vencedor = vencedor
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Parar(string baralhoId, int jogadorId)
        {
            try
            {
                var jogadores = await _blackjackService.IniciarRodadaAsync(baralhoId, 2);
                var jogadorAtual = jogadores.FirstOrDefault(j => j.JogadorId == jogadorId);
                
                if (jogadorAtual == null)
                {
                    return Json(new { success = false, error = "Jogador não encontrado" });
                }

                await _blackjackService.PararJogador(jogadorAtual);

                return Json(new
                {
                    success = true,
                    id = jogadorAtual.JogadorId,
                    nome = jogadorAtual.Nome,
                    cartas = jogadorAtual.Cartas.Select(c => new
                    {
                        valor = c.ValorSimbolico,
                        naipe = ObterSimbolo(c.Naipe),
                        imagem = c.ImagemUrl,
                        codigo = c.Codigo
                    }),
                    pontos = jogadorAtual.CalcularPontuacao(),
                    parou = jogadorAtual.Parou
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult RenderizarCarta([FromBody] CartaViewModel carta)
        {
            return PartialView("_CartaPartial", carta);
        }
        
        [HttpPost]
        public async Task<IActionResult> FinalizarJogo(string baralhoId)
        {
            try
            {
                await _blackjackService.RetornarCartasAoBaralhoAsync(baralhoId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
        
        // Função auxiliar para obter o símbolo do naipe
        private string ObterSimbolo(string naipe)
        {
            return naipe.ToLower() switch
            {
                "hearts" => "♥",
                "diamonds" => "♦",
                "clubs" => "♣",
                "spades" => "♠",
                _ => naipe
            };
        }
    }
}