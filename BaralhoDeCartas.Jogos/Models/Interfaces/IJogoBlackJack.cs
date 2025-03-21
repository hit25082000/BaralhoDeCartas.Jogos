namespace BaralhoDeCartas.Models.Interfaces
{
    public interface IJogoBlackJack : IJogo
    { 
        IJogadorDeBlackjack JogadorAtual { get; }
        List<IJogadorDeBlackjack> Jogadores { get; }
        void PassarRodada();
    }
}
