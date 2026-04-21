using System.Text.Json.Serialization;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted bend-point route for one connection.
/// </summary>
public sealed class GraphConnectionRoute : IEquatable<GraphConnectionRoute>
{
    private readonly GraphPoint[] _vertices;

    public static GraphConnectionRoute Empty { get; } = new();

    public GraphConnectionRoute()
        : this(null)
    {
    }

    [JsonConstructor]
    public GraphConnectionRoute(IReadOnlyList<GraphPoint>? vertices)
    {
        _vertices = vertices is null || vertices.Count == 0
            ? []
            : vertices.ToArray();
    }

    public IReadOnlyList<GraphPoint> Vertices => _vertices;

    public bool IsEmpty => _vertices.Length == 0;

    public bool Equals(GraphConnectionRoute? other)
        => other is not null
           && _vertices.SequenceEqual(other._vertices);

    public override bool Equals(object? obj)
        => obj is GraphConnectionRoute other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var vertex in _vertices)
        {
            hash.Add(vertex);
        }

        return hash.ToHashCode();
    }
}
