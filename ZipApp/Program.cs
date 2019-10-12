using System;
using System.Collections.Generic;
using ZipApp.Validation;
using ZipApp.Zipper;

namespace ZipApp
{
    class Program
    {
        static ZipperBase zipper;

        private static Dictionary<string, Action<string, string>> _actions = new Dictionary<string, Action<string, string>> { ["decompress"] = DecompressFile, ["compress"] = CompressFile };


        static int Main(string[] args)
        {
            var validator = new ArgumentsCountValidator();
            var operationValidator = new OperationValidator();
            var sourceFileValidator = new SourceFileValidator(new DestinationFileValidator(null));

            validator.SetChildValidator(operationValidator);
            operationValidator.SetChildValidator(sourceFileValidator);

            var validationResult = validator.Validate(args);

            if (validationResult.Succeeded == false)
            {
                Console.WriteLine(validationResult.ErrorMessage);
                return 1;
            }

            _actions[args[0].ToLower()](args[1], args[2]);

            zipper.Start();

            if (zipper.ResultCode == 0)
            {
                Console.WriteLine("Done!");
            }

            return zipper.ResultCode;
        }

        static void CompressFile(string sourcePath, string destinationPath)
        {
            Console.WriteLine("Compressing started...");
            zipper = new Compressor(sourcePath, destinationPath);
        }

        static void DecompressFile(string sourcePath, string destinationPath)
        {
            Console.WriteLine("Decompressing started...");
            zipper = new Decompressor(sourcePath, destinationPath);          
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
                zipper.Stop();

            }
        }

    }
}
