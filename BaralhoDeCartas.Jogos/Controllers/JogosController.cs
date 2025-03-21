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