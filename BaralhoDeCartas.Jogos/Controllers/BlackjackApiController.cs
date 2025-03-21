using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Exceptions;
using BaralhoDeCartas.Common;

namespace BaralhoDeCartas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlackjackApiController : ControllerBase
    {
        private readonly IBlackjackService _jogoService;

        public BlackjackApiController(IBlackjackService jogoService)
        {
            _jogoService = jogoService;
        }

        [HttpGet("iniciar")]
        public async Task<ActionResult<IBaralho>> IniciarJogoAsync()
        {
            try
            {
                var baralho = await _jogoService.CriarNovoBaralhoAsync();
                return Ok(baralho);
            }
            catch (Exception ex)
            {
                return new ActionResult<IBaralho>(ExceptionHandler.HandleException(ex));
            }
        }

        [HttpPost("{baralhoId}/iniciar-rodada/{numeroJogadores}")]
        public async Task<ActionResult<List<JogadorBlackjackDTO>>> IniciarRodadaAsync(string baralhoId, int numeroJogadores)
        {
            try
            {
                var jogadores = await _jogoService.IniciarRodadaAsync(baralhoId, numeroJogadores);
                var jogadoresDTO = JogadorBlackjackDTO.FromJogadores(jogadores);
                return Ok(jogadoresDTO);
            }
            catch (Exception ex)
            {
                return new ActionResult<List<JogadorBlackjackDTO>>(ExceptionHandler.HandleException(ex));
            }
        }

        [HttpPost("{baralhoId}/comprar-carta")]
        public async Task<ActionResult<CartaDTO>> ComprarCarta(string baralhoId, [FromBody] JogadorBlackjackDTO jogadorDTO)
        {
            try
            {
                var jogadores = JogadorBlackjackDTO.ToJogadores(new List<JogadorBlackjackDTO> { jogadorDTO });
                var jogador = jogadores.First();
                var novaCarta = await _jogoService.ComprarCartaAsync(baralhoId, jogador);
                return Ok(novaCarta);
            }
            catch (Exception ex)
            {
                return new ActionResult<CartaDTO>(ExceptionHandler.HandleException(ex));
            }
        }

        [HttpPost("parar")]
        public ActionResult<JogadorBlackjackDTO> PararJogador([FromBody] JogadorBlackjackDTO jogadorDTO)
        {
            try
            {
                var jogadores = JogadorBlackjackDTO.ToJogadores(new List<JogadorBlackjackDTO> { jogadorDTO });
                var jogador = jogadores.First();

                _jogoService.PararJogador(jogador);

                return Ok(jogador);
            }
            catch (Exception ex)
            {
                return new ActionResult<JogadorBlackjackDTO>(ExceptionHandler.HandleException(ex));
            }
        }

        [HttpPost("{baralhoId}/finalizar")]
        public async Task<ActionResult<ResultadoRodadaBlackjackDTO>> FinalizarRodadaAsync(string baralhoId, [FromBody] List<JogadorBlackjackDTO> jogadoresDTO)
        {
            try
            {
                var jogadores = JogadorBlackjackDTO.ToJogadores(jogadoresDTO);
                var vencedores = _jogoService.DeterminarVencedoresAsync(jogadores);
                await _jogoService.RetornarCartasAoBaralhoAsync(baralhoId);

                var resultado = new ResultadoRodadaBlackjackDTO
                {
                    Vencedores = JogadorBlackjackDTO.FromJogadores(vencedores),
                    JogadoresFinais = jogadoresDTO
                };

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return new ActionResult<ResultadoRodadaBlackjackDTO>(ExceptionHandler.HandleException(ex));
            }
        }
    }
} 