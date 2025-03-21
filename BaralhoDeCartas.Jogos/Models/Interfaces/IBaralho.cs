namespace BaralhoDeCartas.Models.Interfaces
{
    public interface IBaralho
    {
        string BaralhoId { get; }
        bool EstaEmbaralhado { get; }
        int QuantidadeDeCartasRestantes { get; set; }
    }
}
