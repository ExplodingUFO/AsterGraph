using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public void RunAiPipelineMockRunner(bool forceError = false)
    {
        if (forceError)
        {
            _aiPipelineMockRuntimeProvider.RunError();
            Session.Commands.SetSelection(["llm"], "llm", updateStatus: false);
        }
        else
        {
            _aiPipelineMockRuntimeProvider.RunSuccess();
            Session.Commands.SetSelection(["output"], "output", updateStatus: false);
        }

        RefreshRuntimeProjection();
        RefreshScenarioTourProjection();
    }

    public GraphEditorRuntimeOverlaySnapshot GetAiPipelineRuntimeOverlay()
        => Session.Queries.GetRuntimeOverlaySnapshot();
}
