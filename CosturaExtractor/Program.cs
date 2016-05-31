using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace CosturaExtractor
{
    internal class Program
    {
        // Represents the command line options
        private class CLIOptions
        {
            [Option("assembly", HelpText = "assembly to scan", Required = true)]
            public string AssemblyName { get; set; }

            [Option("output", HelpText = "output directory to save the extracted resources", Required = true)]
            public string OutputDirectory { get; set; }
        }

        private static void Main(string[] args)
        {
            var options = new CLIOptions();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ProcessAssembly(options.AssemblyName, options.OutputDirectory);
            }
        }

        private static void ProcessAssembly(string assemblyName, string outputDirectory)
        {
            var assembly = Assembly.LoadFile(assemblyName);
            var resourceNames = GetEmbeddedAssemblyNames(assembly);
            foreach (var name in resourceNames)
            {
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    using (var compressStream = new DeflateStream(stream, CompressionMode.Decompress))
                    {
                        using (var fileWriter = new FileStream(Path.Combine(outputDirectory, name.Replace(".zip", "")),
                                                        FileMode.Create))
                        {
                            CopyTo(compressStream, fileWriter);
                        }
                    }
                }
            }
        }

        private static void CopyTo(Stream source, Stream destination)
        {
            var array = new byte[81920];
            int count;
            while ((count = source.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }

        private static IEnumerable<string> GetEmbeddedAssemblyNames(Assembly assemblyToScan)
        {
            return assemblyToScan.GetManifestResourceNames().Where(name => name.EndsWith(".dll.zip"));
        }
    }
}