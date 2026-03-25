using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorInitializationTests
{
    private const string SkipReason = "Planned for 01-02: public initialization helpers and options/factory entry points are not implemented yet.";

    [Fact(Skip = SkipReason)]
    public void CreateEditorFactory_UsesDocumentCatalogAndCompatibilityOptions()
    {
    }

    [Fact(Skip = SkipReason)]
    public void AddAsterGraphEditor_RegistersComposableInitializationServices()
    {
    }

    [Fact(Skip = SkipReason)]
    public void AddAsterGraphAvalonia_RegistersDefaultGraphEditorViewComposition()
    {
    }

    [Fact(Skip = SkipReason)]
    public void DefaultGraphEditorViewComposition_PreservesExpectedChromeBehavior()
    {
    }
}
