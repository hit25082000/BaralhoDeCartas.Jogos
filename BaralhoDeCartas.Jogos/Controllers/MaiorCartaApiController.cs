using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Models.Interfaces;
using BaralhoDeCartas.Services.Interfaces;
using BaralhoDeCartas.Models.DTOs;
using BaralhoDeCartas.Common;

namespace BaralhoDeCartas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaiorCartaApiController : ControllerBase
    {
        private readonly IMaiorCartaService _maiorCartaService;

        public MaiorCartaApiController(IMaiorCartaService jogoService)
        {
            _maiorCartaService = jogoService;
        }

        [HttpGet("iniciar")]
        public async Task<ActionResult<IBaralho>> CriarNovoBaralhoAsync()
        {
            try
            {
                var baralho = await _maiorCartaService.CriarNovoBaralhoAsync();
                return Ok(baralho);
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        [HttpPost("{baralhoId}/distribuir/{numeroJogadores}")]
        public async Task<ActionResult<List<JogadorDTO>>> DistribuirCartasAsync(string baralhoId, int numeroJogadores)
        {
            try
            {
                var jogadores = await _maiorCartaService.DistribuirCartasAsync(baralhoId, numeroJogadores);
                var jogadoresDTO = JogadorDTO.FromJogadores(jogadores);
                return Ok(jogadoresDTO); 
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        [HttpPost("vencedor")]
        public async Task<ActionResult<JogadorDTO>> ObterVencedorAsync([FromBody] List<JogadorDTO> jogadoresDTO)
        {
            try
            {
                var jogadores = JogadorDTO.ToJogadores(jogadoresDTO);
                var vencedor = await _maiorCartaService.DeterminarVencedorAsync(jogadores);
                return Ok(vencedor);
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }

        [HttpPost("{baralhoId}/finalizar")]
        public async Task<ActionResult<bool>> FinalizarJogoAsync(string baralhoId)
        {
            try
            {
                var baralho = await _maiorCartaService.FinalizarJogoAsync(baralhoId);
                return Ok(baralho);
            }
            catch (Exception ex)
            {
                return ExceptionHandler.HandleException(ex);
            }
        }
    }
} 