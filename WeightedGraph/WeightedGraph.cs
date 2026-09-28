namespace WeightedGraph;

public class WeightedGraph<T>
{
  public List<Vertex<T>> Vertices { get; } = [];

  public void AddEdge(Vertex<T> from, Vertex<T> to, int weight)
  {
    WeightedEdge<T> edge = new(from, to, weight);
    from.Edges.Add(edge);
  }

  public override string ToString()
  {
    return string.Join("\n", Vertices);
  }

  public List<WeightedEdge<T>> GetAllEdges()
  {
    return [.. Vertices.SelectMany(vertex => vertex.Edges)];
  }
}
