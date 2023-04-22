using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using VerifyXunit;

namespace DTO.Generator.Tests
{
    public static class TestHelper
    {
        private static readonly IConfiguration configuration =
            new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static Task Verify(string source, IConfiguration configuration)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: "Tests",
                syntaxTrees: new[] { syntaxTree });

            var generator = new DTOGenerator(configuration);
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);

            return Verifier.Verify(driver);
        }

        public static Task Verify(string source)
        {
            return Verify(source, configuration);
        }
    }
}