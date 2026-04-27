using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public void RunAiPipelineMockRunner(bool forceError = false)
    {
        if (forceError)
        {
            _aiPipelineMockRuntimeProvider.RunError();
        }
        else
        {
            _aiPipelineMockRuntimeProvider.RunSuccess();
        }

        RefreshRuntimeProjection();
        RefreshScenarioTourProjection();
    }

    public GraphEditorRuntimeOverlaySnapshot GetAiPipelineRuntimeOverlay()
        => Session.Queries.GetRuntimeOverlaySnapshot();
}
