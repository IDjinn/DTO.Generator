#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DTO.Generator
{
    [Generator]
    public class DTOGenerator : IIncrementalGenerator
    {
        private readonly DTOGeneratorSettings _settings;

        public DTOGenerator(DTOGeneratorSettings settings)
        {
            _settings = settings;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var typeDeclarationSyntaxProvider = context.SyntaxProvider.CreateSyntaxProvider(
                (sn, _) => sn is TypeDeclarationSyntax,
                (c, _) => (TypeDeclarationSyntax)c.Node);

            var compilationAndClasses = context.CompilationProvider.Combine(typeDeclarationSyntaxProvider.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                (spc, source) => Execute(source.Left, source.Right, spc));
        }

        private void Execute(Compilation compilation,
            ImmutableArray<TypeDeclarationSyntax> typeDeclarationSyntaxProvider,
            SourceProductionContext spc)
        {
            var sb = new StringBuilder(1024);
            var namespaces = new Dictionary<string, string>();
            foreach (var typeDeclaration in typeDeclarationSyntaxProvider.Where(typeDeclaration =>
                         typeDeclaration.Kind() == SyntaxKind.InterfaceDeclaration))
            {
                var interfaceDeclaration = (InterfaceDeclarationSyntax)typeDeclaration;
                if (_settings.Generate.Equals(GenerateMode.Attribute))
                {
                    foreach (var attributeList in interfaceDeclaration.AttributeLists)
                    {
                        foreach (var attributeSyntax in attributeList.Attributes)
                        {
                            var name = (IdentifierNameSyntax)attributeSyntax.Name;
                            if (name.Identifier.ValueText.Equals("DTO"))
                                goto generate;
                        }
                    }

                    continue;
                }

                generate:
                var justValueTypes = !typeDeclaration.Members
                    .Where(member => member is PropertyDeclarationSyntax)
                    .Where(member => member.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
                    .Any(member => ((PropertyDeclarationSyntax)member).Type is IdentifierNameSyntax);

                var interfaceNamespaceIdentifier =
                    (IdentifierNameSyntax?)interfaceDeclaration.Ancestors()
                        .OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault()?.Name
                    ?? (IdentifierNameSyntax?)interfaceDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>()
                        .FirstOrDefault()?.Name;
                var @namespace = interfaceNamespaceIdentifier?.Identifier.ValueText;
                var interfaceName = typeDeclaration.Identifier.ValueText;
                var generatedRecordName = generateRecordName(interfaceName);
                if (justValueTypes && !_settings.TypeGenerated.Equals(TypeGenerated.Record))
                {
                    sb.Append("public readonly record struct ");
                }
                else
                {
                    sb.Append("public record ");
                }

                sb.Append(generatedRecordName).Append('(');

                var hasMoreThanOneProperty = false;
                foreach (var declarationMember in typeDeclaration.Members)
                {
                    if (declarationMember.IsKind(SyntaxKind.PropertyDeclaration))
                    {
                        var propertyTree = (PropertyDeclarationSyntax)declarationMember;
                        if (!declarationMember.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)))
                            continue;

                        if (hasMoreThanOneProperty)
                            sb.Append(", ");

                        hasMoreThanOneProperty = true;
                        var typeAnnotation = GetTypeToRawString(propertyTree.Type);
                        var identifierText = GetIdentifierName(propertyTree);
                        sb.Append(typeAnnotation);
                        sb.Append(" ");
                        sb.Append(identifierText);
                    }
                }

                sb.Append(");\n");
                if (@namespace is not null)
                {
                    var source = sb.ToString();
                    if (namespaces.TryGetValue(@namespace, out var value))
                    {
                        source += value;
                        namespaces.Remove(@namespace);
                    }

                    namespaces.Add(@namespace, source);
                }
                else
                {
                    spc.AddSource(@namespace + ".generated.cs", sb.ToString());
                }

                sb.Clear();
            }

            foreach (var entry in namespaces)
            {
                var source = "namespace " + entry.Key + ";\n" + entry.Value;
                spc.AddSource(entry.Key + ".generated.cs", source);
            }
        }

        private string generateRecordName(string interfaceName)
        {
            var match = _settings.NamingConvention.Interface.Match(interfaceName);
            if (match.Success)
            {
                return _settings.NamingConvention.Generated.Replace("{Name}", match.Groups[1].Value);
            }

            return interfaceName + "Generated";
        }

        private string GetTypeToRawString(SyntaxNode syntaxNode)
        {
            return syntaxNode switch
            {
                PredefinedTypeSyntax predefinedTypeSyntax => predefinedTypeSyntax.Keyword.ValueText,
                NullableTypeSyntax nullableTypeSyntax =>
                    nullableTypeSyntax.ElementType is IdentifierNameSyntax identifierName
                        ? identifierName.Identifier.ValueText + "?"
                        : ((PredefinedTypeSyntax)nullableTypeSyntax.ElementType).Keyword.ValueText + "?",
                PropertyDeclarationSyntax propertyDeclarationSyntax => propertyDeclarationSyntax.Identifier.ValueText,
                IdentifierNameSyntax identifierNameSyntax => identifierNameSyntax.Identifier.ValueText,
                _ => throw new ArgumentOutOfRangeException(nameof(syntaxNode))
            };
        }

        private static string GetIdentifierName(PropertyDeclarationSyntax declarationSyntax)
        {
            return declarationSyntax switch
            {
                PropertyDeclarationSyntax propertyDeclarationSyntax => propertyDeclarationSyntax.Identifier.ValueText,
                _ => throw new ArgumentOutOfRangeException(nameof(declarationSyntax))
            };
        }
    }
}

#nullable restore