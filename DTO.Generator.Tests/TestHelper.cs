using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using VerifyXunit;

namespace DTO.Generator.Tests
{
    internal static class TestHelper
    {
        private static readonly IConfiguration _configuration =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static Task Verify(string source, DTOGeneratorSettings? settings = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree });

            settings ??= _configuration.Get<DTOGeneratorSettings>();
            _configuration.Bind(settings);

            var generator = new DTOGenerator(settings);
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            return Verifier.Verify(driver);
        }

        public static Task Verify(string source)
        {
            return Verify(source, null);
        }
    }
}