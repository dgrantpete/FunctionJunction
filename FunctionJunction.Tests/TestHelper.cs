using FunctionJunction.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace FunctionJunction.Tests;

internal static class TestHelper
{
    public static TError UnwrapErrorOr<TOk, TError>(this Result<TOk, TError> result, Func<TOk, TError> defaultProvider) =>
        result.Match(
            defaultProvider,
            error => error
        );

    public static string GetEmbeddedSource(string sourceName)
    {
        const string prefix = "FunctionJunction.Tests.TestSources.{0}.cs";

        var assembly = typeof(TestHelper).Assembly;

        using var stream = assembly.GetManifestResourceStream(string.Format(prefix, sourceName)) 
            ?? throw new InvalidOperationException($"Source name '{sourceName}' could not be found.");
        
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static GeneratorDriver CreateDriver(IIncrementalGenerator generator)
    {
        var options = new GeneratorDriverOptions(
            disabledOutputs: IncrementalGeneratorOutputKind.None,
            trackIncrementalGeneratorSteps: true
        );

        return CSharpGeneratorDriver.Create([generator.AsSourceGenerator()], driverOptions: options);
    }

    public static async Task<Compilation> CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var frameworkReference = await ReferenceAssemblies.Net.Net80
            .ResolveAsync(LanguageNames.CSharp, default);

        var mainProjectReference = MetadataReference.CreateFromFile(typeof(DiscriminatedUnionAttribute).Assembly.Location);

        var options = new CSharpCompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);

        return CSharpCompilation.Create(
            "TestAssembly", 
            [syntaxTree],
            frameworkReference.Add(mainProjectReference),
            options
        );
    }

    public static async Task<Compilation> CreateCompilationFromSourceName(string sourceName)
    {
        var source = GetEmbeddedSource(sourceName);

        var compilation = await CreateCompilation(source);

        if (
            compilation
                .GetDiagnostics()
                .FirstOrDefault(diagnostic => diagnostic is { Severity: DiagnosticSeverity.Error })
            is { } error
        )
        {
            throw new InvalidOperationException($"Provided source did not compile: {error}");
        }

        return compilation;
    }
}
