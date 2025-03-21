namespace BaralhoDeCartas.Models.Interfaces
{
    public interface IJogadorDeBlackjack : IJogador
    {
        bool Parou { get; set; }
        bool Estourou { get; }
        bool TemBlackjack();
        int CalcularPontuacao();
    }
}
