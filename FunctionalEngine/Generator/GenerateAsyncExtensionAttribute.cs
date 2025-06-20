using System;

namespace FunctionalEngine.Generator;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
public class GenerateAsyncExtensionAttribute : Attribute
{
    public string? ExtensionClassName { get; set; }

    public string? ExtensionMethodName { get; set; }

    public string? Namespace { get; set; }
}
