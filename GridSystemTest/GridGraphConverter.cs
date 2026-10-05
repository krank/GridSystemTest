using WeightedGraph;
using GridSystem;

public class GridGraphConverter
{
  //TODO: Optimize
  public static WeightedGraph<GridTile> ConvertGridToGraph(Grid grid)
  {
    WeightedGraph<GridTile> graph = new();

    // Add all vertices
    for (int y = 0; y < grid.Height; y++)
    {
      for (int x = 0; x < grid.Width; x++)
      {
        graph.Vertices.Add(new(grid.Get(x, y)));
      }
    }

    // Go through all tiles, add edges where appropriate
    for (int y = 0; y < grid.Height; y++)
    {
      for (int x = 0; x < grid.Width; x++)
      {
        GridTile tile = grid.Get(x, y);

        // Find the right source vertex
        int thisIndex = tile.Y * grid.Width + tile.X;
        Vertex<GridTile> thisVertex = graph.Vertices[thisIndex];

        foreach ((int x, int y) offset in Grid.Offsets)
        {
          // Make sure there's no blocking walls
          if (grid.IsBlockedInDirection(tile, offset)) continue;

          // Find the target vertex
          int targetIndex = (tile.Y + offset.y) * grid.Width + tile.X + offset.x;
          Vertex<GridTile> targetVertex = graph.Vertices[targetIndex];

          // TODO: Extract cost calculation to its own module
          int cost = offset.x == 0 || offset.y == 0 ? grid.CostStraight : grid.CostDiagonal;
          // TODO: We should be able to make this both ways at the same time, to save some iterations
          graph.AddEdge(thisVertex, targetVertex, cost);
        }
      }
    }

    return graph;
  }
}
