using System;

namespace SellAvi.Classes.Exceptions
{
    internal class TorBinariesNotFound : Exception
    {
        public TorBinariesNotFound()
        {
        }

        public TorBinariesNotFound(string message)
            : base(message)
        {
        }

        public TorBinariesNotFound(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public override string Message => "Запуск анонимного соединения требует дополнительной установки tor";
    }
}