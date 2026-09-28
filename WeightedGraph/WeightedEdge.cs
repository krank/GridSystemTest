namespace WeightedGraph;

public class WeightedEdge<T> (Vertex<T> from, Vertex<T> to, int weight)
{
  public int Weight { get; set; } = weight;
  public Vertex<T> From { get; set; } = from;
  public Vertex<T> To { get; set; } = to;
}
