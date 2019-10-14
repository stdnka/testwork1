using System;

namespace Compressor.Interfaces
{
    interface IDataDispatcher
    {
        void Start();

        Exception WaitForCompletion();
    }
}
