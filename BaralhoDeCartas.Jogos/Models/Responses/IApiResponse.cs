namespace BaralhoDeCartas.Models.ApiResponses
{
    public interface IApiResponse
    {
        bool Success { get; set; }
        string Error { get; set; }
    }
} 