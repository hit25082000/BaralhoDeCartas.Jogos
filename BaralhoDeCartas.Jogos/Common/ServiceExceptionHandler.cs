using BaralhoDeCartas.Exceptions;

namespace BaralhoDeCartas.Common
{
    public static class ServiceExceptionHandler
    {
        public static async Task<T> HandleServiceExceptionAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (BaralhoNotFoundException ex)
            {
                throw new InvalidOperationException("O baralho não foi encontrado", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("A operação não é valida: ", ex);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException("Não foi possivel conectar ao host, verifique a conexão com a internet ", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalServiceUnavailableException("Não foi possivel conectar ao host, verifique a conexão com a internet");
            }
            catch (Exception ex)
            {
                throw new Exception("Erro interno no serviço", ex);
            }
        }

        public static T HandleServiceException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (BaralhoNotFoundException)
            {
                throw;
            }
            catch (ExternalServiceUnavailableException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro interno no serviço", ex);
            }
        }
    }
} 