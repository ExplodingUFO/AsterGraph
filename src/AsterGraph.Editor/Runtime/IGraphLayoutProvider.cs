namespace AsterGraph.Editor.Runtime;

public interface IGraphLayoutProvider
{
    GraphLayoutPlan CreateLayoutPlan(GraphLayoutRequest request);
}
