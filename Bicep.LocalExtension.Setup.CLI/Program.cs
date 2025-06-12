using Azure.Bicep.Types.Concrete;
using Bicep.LocalExtension.Setup.Generator;
using Bicep.LocalExtension.Setup.Shared.Models;
using CommandLine;

namespace Bicep.LocalExtension.Setup.CLI
{

    public static class Program
    {
        internal class Options
        {
            [Option("outdir", Required = true, HelpText = "The path to output types to")]
            public string? Outdir { get; set; }
        }
        public static void Main(string[] args)
            => Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(Main);
        
        private static void Main(Options options)
        {
            
            var config = new ExtensionConfiguration();
            config.Add(ObjectTypePropertyFlags.None, new ExtensionConfigurationStringProperty("token", ObjectTypePropertyFlags.None, "Access token for the api", IsSecure: true));
            var types = TypeGenerator.GenerateTypes("test", "1.0.0", config,typeof(Class1));
            var outdir = Path.GetFullPath(options.Outdir!);

            Directory.CreateDirectory(outdir);
            foreach (var kvp in types)
            {
                var filePath = Path.Combine(outdir, kvp.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                File.WriteAllText(filePath, kvp.Value);
            }
            
        }
    }
}


