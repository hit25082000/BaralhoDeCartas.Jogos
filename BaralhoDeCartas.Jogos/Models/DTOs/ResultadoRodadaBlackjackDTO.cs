namespace BaralhoDeCartas.Models.DTOs
{
    public class ResultadoRodadaBlackjackDTO
    {
        public List<JogadorBlackjackDTO> Vencedores { get; set; }
        public List<JogadorBlackjackDTO> JogadoresFinais { get; set; }

        public ResultadoRodadaBlackjackDTO()
        {
            Vencedores = new List<JogadorBlackjackDTO>();
            JogadoresFinais = new List<JogadorBlackjackDTO>();
        }
    }
} 