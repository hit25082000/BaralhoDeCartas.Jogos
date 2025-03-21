using Microsoft.AspNetCore.Mvc;
using BaralhoDeCartas.Exceptions;

namespace BaralhoDeCartas.Common
{
    public static class ExceptionHandler
    {
        public static ActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                BaralhoNotFoundException => new NotFoundObjectResult(new { success = false, message = "Baralho não encontrado", details = ex.Message }),
                ExternalServiceUnavailableException => new ObjectResult(new { success = false, message = "Serviço temporariamente indisponível", details = ex.Message }) { StatusCode = 503 },
                InvalidOperationException => new BadRequestObjectResult(new { success = false, message = "Operação inválida", details = ex.Message }),
                ArgumentException => new BadRequestObjectResult(new { success = false, message = ex.Message }),
                _ => new ObjectResult(new { success = false, message = "Ocorreu um erro interno no servidor" }) { StatusCode = 500 }
            };
        }
    }
} 