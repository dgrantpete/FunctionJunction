using System;

namespace FunctionalEngine.Generator;

[AttributeUsage(AttributeTargets.Method)]
public class GenerateAsyncExtensionAttribute : Attribute
{
    public string? ExtensionClassName { get; set; }
}
