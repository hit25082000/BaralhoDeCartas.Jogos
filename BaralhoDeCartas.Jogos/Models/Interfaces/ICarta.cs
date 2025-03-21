namespace BaralhoDeCartas.Models.Interfaces
{
    public interface ICarta
    {
        string Codigo { get; }
        
        string ImagemUrl { get; }
        
        string ValorSimbolico { get; }
        
        string Naipe { get; }  

        int Valor { get; }
    }
}
