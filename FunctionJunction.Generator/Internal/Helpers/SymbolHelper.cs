using FunctionJunction.Generator.Internal.Models;
using Microsoft.CodeAnalysis;
using Accessibility = FunctionJunction.Generator.Internal.Models.Accessibility;
using ExternalAccessibility = Microsoft.CodeAnalysis.Accessibility;

namespace FunctionJunction.Generator.Internal.Helpers;

internal static class SymbolHelper
{
    public static bool SymbolEquals(ISymbol? first, ISymbol? second) =>
        SymbolEqualityComparer.Default.Equals(first, second);

    public static Accessibility? GetAccessibility(this ISymbol symbol) => symbol.DeclaredAccessibility switch
    {
        ExternalAccessibility.Public => Accessibility.Public,
        ExternalAccessibility.Internal => Accessibility.Internal,
        _ => null
    };

    public static ObjectType? GetObjectType(this ITypeSymbol typeSymbol) => typeSymbol switch
    {
        { IsRecord: false, TypeKind: TypeKind.Class } => ObjectType.Class,
        { IsRecord: true, TypeKind: TypeKind.Class } => ObjectType.Record,
        _ => null
    };

    public static IEnumerable<INamedTypeSymbol> GetDerivedTypeSymbols(this ITypeSymbol unionSymbol) =>
        unionSymbol.GetTypeMembers()
            .Where(memberSymbol =>
                SymbolEqualityComparer.Default.Equals(memberSymbol.BaseType, unionSymbol)
            );

    public static string ToCamelCase(this string pascalCase) => pascalCase switch
    {
        [var first, .. var rest] => char.ToLowerInvariant(first) + rest,
        _ => pascalCase
    };
}
