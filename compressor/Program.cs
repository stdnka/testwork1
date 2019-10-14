using System;
using System.Diagnostics;
using Compressor.Compression;
using Compressor.Exceptions;
using Compressor.Interfaces;
using Compressor.Readers;
using Compressor.Writers;

namespace Compressor
{
    class Program
    {
        class StartParams
        {
            public bool Compress { get; set; }

            public string SrcFilePath { get; set; }

            public string DstFilePath { get; set; }
        }

        static int Main(string[] args)
        {
            var result = 0;
            //args = new[] { "compress", @"D:\Data\data.dat", @"d:\Data\data.dat.compressed" };
            //args = new[] { "decompress", @"D:\Data\data.dat.compressed", @"D:\Data\data.dat.decompressed"};
            //args = new[] { "compress", @"D:\Data\data.dat.small", @"D:\Data\data.dat.small.compressed" };
            //args = new[] { "decompress", @"D:\Data\data.dat.small.compressed", @"D:\Data\data.dat.small.decompressed" };

            try
            {
                var startParams = ParseCommandLine(args);

                var readerFactory = new FileReaderFactory(startParams.SrcFilePath);
                var writerFactory = new FileWriterFactory(startParams.DstFilePath);
                
                IDataDispatcher dispatcher;

                if (startParams.Compress)
                {
                    var readThreadsCount = Math.Max(1, Environment.ProcessorCount / 4);
                    var writeThreadsCount = Math.Max(1, Environment.ProcessorCount - readThreadsCount);
                    dispatcher = new CompressDataDispatcher(readerFactory, writerFactory, readThreadsCount, writeThreadsCount);
                }
                else
                {
                    var writeThreadsCount = Math.Max(1, Environment.ProcessorCount / 4);
                    var readThreadsCount = Math.Max(1, Environment.ProcessorCount - writeThreadsCount);
                    dispatcher = new DecompressDataDispatcher(readerFactory, writerFactory, readThreadsCount, writeThreadsCount);
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                dispatcher.Start();
                var exception = dispatcher.WaitForCompletion();
                stopwatch.Stop();

                Console.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds}");

                if (exception != null)
                {
                    throw exception;
                }
            }
            catch (Exception ex)
            {
                PrintException(ex);
                result = 1;
            }

            //Console.ReadKey();

            return result;
        }

        static StartParams ParseCommandLine(string[] args)
        {
            StartParams startParams = new StartParams();

            if (args.Length == 0 || args.Length > 3)
            {
                throw new CommandLineException("The command line parameters are incorrect.");
            }

            switch (args[0].ToLower())
            {
                case "compress":
                    startParams.Compress = true;
                    break;
                case "decompress":
                    startParams.Compress = false;
                    break;
                default:
                {
                    throw new CommandLineException("The first parameter is incorrect.");
                }
            }

            startParams.SrcFilePath = args[1];
            startParams.DstFilePath = args[2];

            return startParams;
        }

        static void ShowHelp()
        {
            Console.WriteLine("Command line syntax:");
            Console.WriteLine("    compressor.exe compress/decompress \"source file path\" \"destination file path\"");
        }

        static void PrintException(Exception exception)
        {
            Console.WriteLine(exception.Message);

            if (exception is CommandLineException)
            {
                ShowHelp();
            }
            else if (exception is AggregateException aggregateException)
            {
                foreach (var ex in aggregateException.InnerExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
