using Microsoft.CodeAnalysis;

namespace FunctionJunction.Generator.Internal.Models;

internal readonly record struct SymbolId<TSymbol>(string Id, SymbolId.Type Type) where TSymbol : class, ISymbol
{
    public string? ForeignId { get; init; }

    public TSymbol Resolve(Compilation compilation)
    {
        if (
            ForeignId is { } foreignId
                && typeof(TSymbol) == typeof(IParameterSymbol)
                && GetSymbolInternal<IMethodSymbol>(Id, Type, compilation) is { } methodSymbol
        )
        {
            return methodSymbol.Parameters.FirstOrDefault(parameter => parameter.Name == foreignId) as TSymbol
                ?? throw new InvalidOperationException("Symbol could not be resolved");
        }

        return GetSymbolInternal<TSymbol>(Id, Type, compilation)
            ?? throw new InvalidOperationException("Symbol could not be resolved");
    }

    public string RenderType(Compilation compilation, SymbolDisplayFormat? format = null) =>
        Resolve(compilation).ToDisplayString(format);

    private static TInternalSymbol? GetSymbolInternal<TInternalSymbol>(
        string id,
        SymbolId.Type type,
        Compilation compilation
    )
        where TInternalSymbol : class, ISymbol
    =>
        type switch
        {
            SymbolId.Type.Declaration => DocumentationCommentId.GetFirstSymbolForDeclarationId(id, compilation) as TInternalSymbol,
            _ => DocumentationCommentId.GetFirstSymbolForReferenceId(id, compilation) as TInternalSymbol
        };
}

internal static class SymbolId
{
    internal enum Type
    {
        Reference,
        Declaration
    }

    public static SymbolId<TSymbol> Create<TSymbol>(TSymbol symbol) where TSymbol : class, ISymbol
    {
        var symbolId = DocumentationCommentId.CreateReferenceId(symbol);

        if (
            symbolId is ""
                && symbol is IParameterSymbol { ContainingSymbol: IMethodSymbol methodSymbol } parameterSymbol
                && DocumentationCommentId.CreateDeclarationId(methodSymbol) is { } methodSymbolId
        )
        {
            return new(methodSymbolId, Type.Declaration)
            {
                ForeignId = parameterSymbol.Name
            };
        }

        return new(symbolId, Type.Reference);
    }
}
