using System.Reflection;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorArchitectureBoundaryTests
{
    [Fact]
    public void CoreAssembly_DoesNotReferenceEditorOrAvaloniaLayers()
    {
        var referencedAssemblyNames = typeof(GraphDocument).Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AsterGraph.Editor", referencedAssemblyNames);
        Assert.DoesNotContain("AsterGraph.Avalonia", referencedAssemblyNames);
        Assert.DoesNotContain(referencedAssemblyNames, name => name is not null && name.StartsWith("Avalonia.", StringComparison.Ordinal));
    }

    [Fact]
    public void EditorAssembly_DoesNotReferenceAvaloniaAdapterLayer()
    {
        var referencedAssemblyNames = typeof(IGraphEditorSession).Assembly.GetReferencedAssemblies()
            .Select(assembly => assembly.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("AsterGraph.Avalonia", referencedAssemblyNames);
        Assert.DoesNotContain(referencedAssemblyNames, name => name is not null && name.StartsWith("Avalonia.", StringComparison.Ordinal));
    }

    [Fact]
    public void PublicRuntimeContracts_DoNotExposeAvaloniaOrRetainedViewModelTypesBeyondExplicitCompatibilityExceptions()
    {
        var runtimeAssembly = typeof(IGraphEditorSession).Assembly;
        var publicRuntimeTypes = runtimeAssembly.GetExportedTypes()
            .Where(type => type.Namespace is not null &&
                type.Namespace.StartsWith("AsterGraph.Editor.Runtime", StringComparison.Ordinal))
            .Append(typeof(IGraphEditorSession))
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();

        var violations = publicRuntimeTypes
            .SelectMany(GetPubliclyReferencedTypeGraph)
            .Where(reference => IsDisallowedPublicContractType(reference.ReferencedType))
            .Where(reference => !IsAllowedCompatibilityException(reference))
            .Select(reference => $"{reference.Path} -> {reference.ReferencedType.FullName}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(violation => violation, StringComparer.Ordinal)
            .ToList();

        Assert.True(
            violations.Count == 0,
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void ArchitectureDocs_PublishExecutableLayerBoundaryContract()
    {
        var english = ReadRepoFile("docs/en/architecture.md");
        var chinese = ReadRepoFile("docs/zh-CN/architecture.md");

        Assert.Contains("## Layer Boundary Contract", english, StringComparison.Ordinal);
        Assert.Contains("## Layer Boundary Contract", chinese, StringComparison.Ordinal);

        foreach (var required in RequiredBoundaryTerms)
        {
            Assert.Contains(required, english, StringComparison.Ordinal);
            Assert.Contains(required, chinese, StringComparison.Ordinal);
        }

        Assert.Contains("Core -> Abstractions", english, StringComparison.Ordinal);
        Assert.Contains("Editor -> Core + Abstractions", english, StringComparison.Ordinal);
        Assert.Contains("Avalonia -> Editor + Core", english, StringComparison.Ordinal);
        Assert.Contains("Compatibility exceptions", english, StringComparison.Ordinal);
        Assert.Contains("GetCompatiblePortTargets", english, StringComparison.Ordinal);
        Assert.DoesNotContain("GetCompatibleTargets", english, StringComparison.Ordinal);
        Assert.DoesNotContain("`CompatiblePortTarget`", english, StringComparison.Ordinal);

        Assert.Contains("Core -> Abstractions", chinese, StringComparison.Ordinal);
        Assert.Contains("Editor -> Core + Abstractions", chinese, StringComparison.Ordinal);
        Assert.Contains("Avalonia -> Editor + Core", chinese, StringComparison.Ordinal);
        Assert.Contains("compatibility exception", chinese, StringComparison.Ordinal);
        Assert.Contains("GetCompatiblePortTargets", chinese, StringComparison.Ordinal);
        Assert.DoesNotContain("GetCompatibleTargets", chinese, StringComparison.Ordinal);
        Assert.DoesNotContain("`CompatiblePortTarget`", chinese, StringComparison.Ordinal);
    }

    private static IReadOnlyList<PublicTypeReference> GetPubliclyReferencedTypeGraph(Type type)
    {
        var references = new List<PublicTypeReference>();
        var visitedTypes = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Queue<PublicTypeReference>(GetDirectPubliclyReferencedTypes(type));

        while (pending.Count > 0)
        {
            var reference = pending.Dequeue();
            references.Add(reference);

            if (IsAllowedCompatibilityException(reference))
            {
                continue;
            }

            if (!ShouldInspectReferencedType(reference.ReferencedType))
            {
                continue;
            }

            var referenceTypeName = reference.ReferencedType.FullName ?? reference.ReferencedType.Name;
            if (!visitedTypes.Add(referenceTypeName))
            {
                continue;
            }

            foreach (var childReference in GetDirectPubliclyReferencedTypes(reference.ReferencedType))
            {
                pending.Enqueue(childReference with
                {
                    Path = $"{reference.Path} -> {childReference.OwnerType}.{childReference.MemberName}",
                });
            }
        }

        return references
            .DistinctBy(reference => $"{reference.Path}|{reference.ReferencedType.FullName}")
            .ToList();
    }

    private static IReadOnlyList<PublicTypeReference> GetDirectPubliclyReferencedTypes(Type type)
    {
        var referencedTypes = new List<PublicTypeReference>();

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            switch (member)
            {
                case PropertyInfo property:
                    AddReference(referencedTypes, type, property.Name, property.PropertyType);
                    break;
                case MethodInfo method when !method.IsSpecialName:
                    AddReference(referencedTypes, type, method.Name, method.ReturnType);

                    foreach (var parameter in method.GetParameters())
                    {
                        AddReference(referencedTypes, type, method.Name, parameter.ParameterType);
                    }

                    break;
                case ConstructorInfo constructor:
                    foreach (var parameter in constructor.GetParameters())
                    {
                        AddReference(referencedTypes, type, ".ctor", parameter.ParameterType);
                    }

                    break;
                case EventInfo eventInfo when eventInfo.EventHandlerType is not null:
                    AddReference(referencedTypes, type, eventInfo.Name, eventInfo.EventHandlerType);
                    break;
            }
        }

        return referencedTypes
            .DistinctBy(reference => $"{reference.OwnerType}|{reference.MemberName}|{reference.ReferencedType.FullName}")
            .ToList();
    }

    private static void AddReference(List<PublicTypeReference> references, Type ownerType, string memberName, Type referencedType)
    {
        if (referencedType == typeof(void))
        {
            return;
        }

        foreach (var expanded in ExpandType(referencedType))
        {
            var ownerName = ownerType.FullName ?? ownerType.Name;
            references.Add(new PublicTypeReference(ownerName, memberName, expanded, $"{ownerName}.{memberName}"));
        }
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var expanded in ExpandType(elementType))
            {
                yield return expanded;
            }

            yield break;
        }

        if (type.IsGenericType)
        {
            yield return type.GetGenericTypeDefinition();

            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var expanded in ExpandType(argument))
                {
                    yield return expanded;
                }
            }

            yield break;
        }

        yield return type;
    }

    private static bool IsDisallowedPublicContractType(Type type)
    {
        var fullName = type.FullName ?? string.Empty;
        return fullName.StartsWith("Avalonia.", StringComparison.Ordinal)
            || fullName.Contains("GraphEditorViewModel", StringComparison.Ordinal)
            || fullName.Contains("NodeViewModel", StringComparison.Ordinal)
            || fullName.Contains("ConnectionViewModel", StringComparison.Ordinal)
            || fullName.Contains("PortViewModel", StringComparison.Ordinal);
    }

    private static bool ShouldInspectReferencedType(Type type)
    {
        var fullName = type.FullName ?? string.Empty;
        return fullName.StartsWith("AsterGraph.Abstractions.", StringComparison.Ordinal)
            || fullName.StartsWith("AsterGraph.Core.", StringComparison.Ordinal)
            || fullName.StartsWith("AsterGraph.Editor.", StringComparison.Ordinal);
    }

    private static bool IsAllowedCompatibilityException(PublicTypeReference reference)
    {
        if (!IsDisallowedPublicContractType(reference.ReferencedType))
        {
            return false;
        }

        return reference.Path == "AsterGraph.Editor.Runtime.GraphEditorSession..ctor";
    }

    private static string ReadRepoFile(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../", relativePath));
        return File.ReadAllText(fullPath);
    }

    private static readonly string[] RequiredBoundaryTerms =
    [
        "AsterGraph.Abstractions",
        "AsterGraph.Core",
        "AsterGraph.Editor",
        "AsterGraph.Avalonia",
        "IGraphEditorSession",
        "GraphEditorViewModel",
        "NodeCanvas",
    ];

    private sealed record PublicTypeReference(string OwnerType, string MemberName, Type ReferencedType, string Path);
}
