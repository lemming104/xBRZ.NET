namespace xBRZTester
{
    using System;
    using System.IO;
    using System.Linq;

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: nBRZ.Shell.exe <inputFileOrDirectory> <outputFileOrDirectory> <scaleFactor>");
                return;
            }
            if (!int.TryParse(args[2], out int scale) || scale < 2 || scale > 5)
            {
                Console.WriteLine("Scale must be an integer between 2 and 5");
                return;
            }
            if (!Directory.Exists(args[0]) && !File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find input file(s).");
                return;
            }

            string[] inputFiles;
            string[] outputFiles;

            if (Directory.Exists(args[0]))
            {
                if (!Directory.Exists(args[1]))
                {
                    Directory.CreateDirectory(args[1]);
                }

                inputFiles = Directory.GetFiles(args[0]);
                outputFiles = inputFiles.Select(f => Path.Join(args[1], Path.GetFileName(f))).ToArray();
            }
            else
            {
                inputFiles = [args[0]];
                outputFiles = [args[1]];

                string? dirName = Path.GetDirectoryName(args[1]);
                if (dirName != null && !Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
            }

            for (uint i = 0; i < inputFiles.Length; i++)
            {
                ProcessImage(inputFiles[i], outputFiles[i], scale);
            }
        }

        private static void ProcessImage(string inputFile, string outputFile, int scale)
        {
            xBRZNet.xBRZScaler scaler = new xBRZNet.xBRZScaler(scale, xBRZNet.ScalerCfg.Default);

            using (xBRZNet.Image image = Converters.LoadImageArgb(inputFile, out int width, out int height))
            {
                int newWidth = width * scale;
                int newHeight = height * scale;

                using (xBRZNet.Image output = new xBRZNet.Image(newWidth, newHeight))
                {
                    scaler.ScaleImage(image, output);
                    Converters.WriteImageArgb(output.Data, newWidth, newHeight, outputFile);
                }
            }
        }
    }
}
