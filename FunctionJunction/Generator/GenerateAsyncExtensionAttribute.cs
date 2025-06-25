using System;

#if MAIN_PROJECT
namespace FunctionJunction.Generator;
#else
namespace FunctionJunction.Generator.Internal.Attributes;
#endif

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
internal class GenerateAsyncExtensionAttribute : Attribute
{
    public string ExtensionClassName { get; set; } = "{0}ExtensionsAsync";

    public string ExtensionMethodName { get; set; } = "Await{0}";

    public string Namespace { get; set; } = "{0}";
}
