using System;

namespace BaralhoDeCartas.Exceptions
{
    public class BaralhoNotFoundException : Exception
    {
        public BaralhoNotFoundException(string message) : base(message)
        {
        }

        public BaralhoNotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
} 