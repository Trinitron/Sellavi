using System;

namespace SellAvi.Classes.Exceptions
{
    public class LicenceException : Exception
    {
        public LicenceException()
        {
        }

        public LicenceException(string message)
            : base(message)
        {
        }

        public LicenceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}