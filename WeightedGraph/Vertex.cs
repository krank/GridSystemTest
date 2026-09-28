namespace WeightedGraph;

public class Vertex<T>(T value)
{
  public T Value { get; set; } = value;

  public List<WeightedEdge<T>> Edges { get; } = [];
  public bool IsVisited { get; set; }
  public float Cost { get; set; }

  public override string ToString()
  {
    return $"{Value}: " + string.Join(", ",
      Edges.Select(edge => $"{edge.To.Value} [{edge.Weight}]"));
  }
}
