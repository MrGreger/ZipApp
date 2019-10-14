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

            zipper.ProgressHelper.ProgressChanged += OnProgressChanged;

            Console.CursorVisible = false;

            zipper.Start();

            Console.CursorVisible = true;

            if (zipper.ResultCode == 0)
            {
                Console.WriteLine();           
                Console.WriteLine("Done!");
            }

            return zipper.ResultCode;
        }

        private static void OnProgressChanged(object sender, Progress.ProgressChangedEventArgs e)
        {
            UpdatePercents(e.CurrentProgress, e.TotalProgress);
        }

        private static void UpdatePercents(long currentProgress, long totalProgress)
        {
            for (int i = Console.BufferWidth - 1; i >= 0; i--)
            {
                Console.SetCursorPosition(i, Console.CursorTop);
                Console.Write("");
            }

            var progress = ((float)currentProgress / totalProgress) * 100;

            var progressRounded = Math.Round(progress);

            Console.Write($"  Progress: {progressRounded}%");
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
