using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.Interfaces;

namespace BaralhoDeCartas.Controllers
{
    public class JogosController : Controller
    {
        private readonly IBlackjackService _blackjackService;

        public JogosController(IBlackjackService blackjackService)
        {
            _blackjackService = blackjackService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SelecionarJogo(string jogo, int numeroJogadores = 2)
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
                return RedirectToAction("Index");
            }

            switch (jogo.ToLower())
            {
                case "maiorcarta":
                    return RedirectToAction("EscolherJogadores", "MaiorCartaWeb");

                case "blackjack":
                    return RedirectToAction("Index", "BlackjackWeb");

                default:
                    return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Blackjack(int numeroJogadores = 2)
        {
            // Validação do número de jogadores
            if (numeroJogadores < 2 || numeroJogadores > 5)
            {
                return RedirectToAction("Index");
            }

            var jogo = await _blackjackService.CriarJogoBlackJackAsync(numeroJogadores);
            return View(jogo);
        }
    }
} 