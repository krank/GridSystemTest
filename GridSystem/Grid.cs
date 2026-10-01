using WeightedGraph;
using System.Diagnostics;

namespace GridSystem;

public class Grid
{
  private readonly GridTile[,] _tiles;

  public int Width { get => _tiles.GetLength(0); }
  public int Height { get => _tiles.GetLength(1); }

  public int CostStraight { get; } = 12;
  public int CostDiagonal { get; } = 17;



  private readonly static (int x, int y)[] offsets = [
      (0,-1), (0,1), (1,0), (-1,0),  // N-S-E-W
      (1,-1), (1,1), (-1,1), (-1,-1), // NE, SE, SW, NW
  ];

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

  private bool IsBlockedInDirection(GridTile tile, (int x, int y) offset)
  {
    int vertical = offset.y + 1;
    int horizontal = Math.Abs(offset.x - 1) + 1;

    GridTile? target = GetNeighbor(tile, offset);
    if (target == null) return true;

    GridTile? VerticalNeighbor = null;
    GridTile? horizontalNeighbor = null;

    // Offset has verticality
    if (offset.y != 0)
    {
      // Check ↕ of source to target
      VerticalNeighbor = GetNeighbor(tile, (0, offset.y));

      if (VerticalNeighbor != null)
      {
        if (IsEitherTileBlocking(tile, VerticalNeighbor, (Direction)vertical)) return true;
      }
    }

    // Offset has horizontality
    if (offset.x != 0)
    {
      // Check ↔ of source to target
      horizontalNeighbor = GetNeighbor(tile, (offset.x, 0));
      if (horizontalNeighbor != null)
      {
        if (IsEitherTileBlocking(tile, horizontalNeighbor, (Direction)horizontal)) return true;
      }
    }

    // Offset has both - is diagonal
    if (offset.x != 0 && offset.y != 0)
    {
      // Check ⤱ of source to target
      if (IsEitherTileBlocking(tile, target, (Direction)horizontal)) return true;
      if (IsEitherTileBlocking(tile, target, (Direction)vertical)) return true;

      // Check ↔ and ↕ of neighbors to target
      if (VerticalNeighbor != null && IsEitherTileBlocking(VerticalNeighbor, target, (Direction)horizontal)) return true;
      if (horizontalNeighbor != null && IsEitherTileBlocking(horizontalNeighbor, target, (Direction)vertical)) return true;
    }

    return false;
  }

  private bool IsEitherTileBlocking(GridTile from, GridTile to, Direction direction)
  {
    int numberOfDirections = Enum.GetNames<Direction>().Length;
    int oppositeDirection = ((int)direction + numberOfDirections / 2) % numberOfDirections;

    return from.Blocking[(int)direction]
      || to.Blocking[oppositeDirection];
  }

  private GridTile? GetNeighbor(GridTile tile, (int x, int y) offset)
  {
    // Get the target neighbor coord
    (int x, int y) = (tile.X + offset.x, tile.Y + offset.y);

    Debug.Print($"Getting neighbor at {x},{y}");

    // Exit if coord is outside bounds
    if (x < 0 || x > Width - 1
      || y < 0 || y > Height - 1
    ) return null;

    return _tiles[x, y];
  }

  public WeightedGraph<GridTile> GetGraph()
  {
    WeightedGraph<GridTile> graph = new();

    // Add all vertices
    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        graph.Vertices.Add(new(_tiles[x, y]));
      }
    }

    // Go through all tiles, add edges where appropriate
    for (int y = 0; y < Height; y++)
    {
      for (int x = 0; x < Width; x++)
      {
        GridTile tile = _tiles[x, y];

        // Find the right source vertex
        int thisIndex = tile.Y * Width + tile.X;
        Vertex<GridTile> thisVertex = graph.Vertices[thisIndex];

        foreach ((int x, int y) offset in offsets)
        {
          // Make sure there's no blocking walls
          if (IsBlockedInDirection(tile, offset)) continue;

          // Find the target vertex
          int targetIndex = (tile.Y + offset.y) * Width + tile.X + offset.x;
          Vertex<GridTile> targetVertex = graph.Vertices[targetIndex];

          int cost = offset.x == 0 || offset.y == 0 ? CostStraight : CostDiagonal;
          graph.AddEdge(thisVertex, targetVertex, cost);

        }
      }
    }

    return graph;
  }
}