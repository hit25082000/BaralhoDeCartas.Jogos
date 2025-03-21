using System.Text.Json.Serialization;

namespace BaralhoDeCartas.Models.ApiResponses
{
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        public static ErrorResponse Create(string message, string details, int statusCode)
        {
            return new ErrorResponse
            {
                Message = message,
                Details = details,
                StatusCode = statusCode
            };
        }
    }
} 