namespace BaralhoDeCartas.Models.Interfaces
{
    public interface IJogoMaiorCarta : IJogo
    {
        List<IJogador> Jogadores { get; }
    }
}
