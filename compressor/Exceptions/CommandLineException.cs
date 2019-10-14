using System;

namespace Compressor.Exceptions
{
    public class CommandLineException : Exception
    {
        public CommandLineException(string message) : base(message)
        {
        }
    }
}
