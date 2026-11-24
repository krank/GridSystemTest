using WeightedGraph;
using GridSystem;
using System.Diagnostics;
namespace GridGraphConverter;

public class GridGraphConverter
{
  public static WeightedGraph<GridTile> ConvertGridToGraph(Grid grid)
  {
    WeightedGraph<GridTile> graph = new();

    // Add all vertices
    foreach (GridLevel level in grid.Levels)
    {
      for (int y = 0; y < grid.Height; y++)
      {
        for (int x = 0; x < grid.Width; x++)
        {
          graph.Vertices.Add(new(level.Get(x, y)));
        }
      }
    }

    // Go through all tiles, add edges where appropriate

    int floorSize = grid.Width * grid.Height;
    int index = 0;

    foreach (GridLevel level in grid.Levels)
    {
      for (int y = 0; y < grid.Height; y++)
      {
        for (int x = 0; x < grid.Width; x++)
        {
          Vertex<GridTile> thisVertex = graph.Vertices[index];
          GridTile tile = thisVertex.Value;

          foreach ((int x, int y) offset in GridLevel.Offsets)
          {
            // Make sure there's no blocking walls
            if (level.IsBlockedInDirection(tile, offset)) continue;

            // Find the target vertex
            int targetIndex = index + offset.x + offset.y * grid.Width;

            Vertex<GridTile> targetVertex = graph.Vertices[targetIndex];

            // TODO: Extract cost calculation to its own module
            int cost = offset.x == 0 || offset.y == 0 ? level.CostStraight : level.CostDiagonal;
            // TODO: We should be able to make this both ways at the same time, to save some iterations
            graph.AddEdge(thisVertex, targetVertex, cost);
          }

          // Check for floor
          if (!tile.HasFloor && tile.GridLevel.LevelNum > 0)
          {
            int targetIndex = index - floorSize; // straight below
            Vertex<GridTile> vertexBelow = graph.Vertices[targetIndex];
            graph.AddEdge(thisVertex, vertexBelow, 0);
            graph.AddEdge(vertexBelow, thisVertex, 50);
          }


          index++;
        }
      }
    }

    return graph;
  }
}
