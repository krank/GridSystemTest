using WeightedGraph;

namespace GridSystem;

public class Grid
{
  private readonly GridTile[,] _tiles;

  public int Width { get => _tiles.GetLength(0); }
  public int Height { get => _tiles.GetLength(1); }

  public Grid(int width, int height)
  {
    _tiles = new GridTile[width, height];
    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        _tiles[x, y] = new GridTile(x, y, 1);
      }
    }
  }

  public GridTile Get(int x, int y) => _tiles[x, y];
  public void Set(int x, int y, GridTile tile)
  {
    x = Math.Clamp(x, 0, Width - 1);
    y = Math.Clamp(y, 0, Height - 1);
    tile.X = x;
    tile.Y = y;
    _tiles[x, y] = tile;
  }

  public (int x, int y)[] GetAllCoords()
  {
    (int x, int y)[] coords = new (int x, int y)[Width * Height];

    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        int i = (Width * y) + x;
        coords[i] = (x, y);
      }
    }

    return coords;
  }

  public WeightedGraph<GridTile> GetGraph()
  {
    WeightedGraph<GridTile> graph = new();

    List<(int from, int to)> potentialEdges = [];

    // Add all vertices
    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        Vertex<GridTile> thisVertex = new(_tiles[x, y]);
        graph.Vertices.Add(thisVertex);
      }
    }

    Dictionary<Direction, (int x, int y)> offsets = new() {
      {Direction.North, (0,-1) },
      {Direction.East,  (1,0)  },
      {Direction.South, (0,1)  },
      {Direction.West,  (-1,0) }
    };

    int numberOfDirections = Enum.GetNames(typeof(Direction)).Length;

    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        int thisIndex = y * Width + x;
        Vertex<GridTile> thisVertex = graph.Vertices[thisIndex];

        foreach (var (direction, offset) in offsets)
        {
          (int x, int y) neighborCoord = (x + offset.x, y + offset.y);
          int neighborIndex = neighborCoord.y * Width + neighborCoord.x;

          if (neighborCoord.x < 0 || neighborCoord.x > Width - 1
            || neighborCoord.y < 0 || neighborCoord.y > Height - 1
          ) continue;

          Vertex<GridTile> neighborVertex = graph.Vertices[neighborIndex];
          int oppositeDirection = ((int)direction + numberOfDirections / 2) % numberOfDirections;

          if (thisVertex.Value.Blocking[(int)direction]
            || neighborVertex.Value.Blocking[oppositeDirection]
          ) continue;

          graph.AddEdge(thisVertex, neighborVertex, 1);

        }
      }
    }

    return graph;
  }
}