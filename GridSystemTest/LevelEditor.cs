using GridSystem;
using WeightedGraph;
using Raylib_cs;
using System.Numerics;
using System.Text.Json;
using System.Diagnostics;
using RaylibExt;

namespace GridSystemTest;

public class LevelEditor
{
  public bool Enabled { get; set; } = true;
  public string Filename { get; set; } = "grid.json";

  private Grid _grid;
  private WeightedGraph<GridTile> _graph;
  private GridRendererRaylib _gd;

  private bool displayGraph = false;

  GridTile? _hoveredTile;
  private Rectangle? _hoveredTileRect;
  Direction _localDirection;

  private GridRendererRaylib.GridTheme markedTheme = new()
  {
    Background = Color.Blank,
    Wall = Color.Blank,
    Tile = new(0, 0, 255, 16)
  };

  public LevelEditor(Grid grid, GridRendererRaylib gd)
  {
    _grid = grid;
    _graph = GridGraphConverter.ConvertGridToGraph(_grid);
    _gd = gd;
  }

  public void Update()
  {
    if (!Enabled) return;

    Vector2 mPos = Raylib.GetMousePosition();
    HandleHoveredTile(mPos);

    if (Raylib.IsKeyDown(KeyboardKey.LeftControl))
    {
      if (Raylib.IsKeyPressed(KeyboardKey.S))
      {
        File.WriteAllText(Filename, JsonSerializer.Serialize(_grid));
        Debug.Print("Saved");
      }

      if (Raylib.IsKeyPressed(KeyboardKey.L))
      {
        if (File.Exists("grid.json"))
        {
          Grid? newGrid = JsonSerializer.Deserialize<Grid>(File.ReadAllText(Filename));
          if (newGrid != null)
          {
            _grid = newGrid;
            _gd.Grid = _grid;
            _graph = GridGraphConverter.ConvertGridToGraph(_grid);
            Debug.Print("Loaded");
          }
        }
      }

      if (Raylib.IsKeyPressed(KeyboardKey.N))
      {
        _grid = _grid != null ?
          new(_grid.Width, _grid.Height) :
          new(10, 10);

        _gd.Grid = _grid;
        _graph = GridGraphConverter.ConvertGridToGraph(_grid);
        Debug.Print("Cleared");
      }
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Tab)) displayGraph = !displayGraph;
    
  }

  private void HandleHoveredTile(Vector2 mPos)
  {
    _hoveredTile = _gd.GetTileFromScreenPoint(mPos);
    if (_hoveredTile != null)
    {
      _hoveredTileRect = _gd.GetTileRect(_hoveredTile);
      if (_hoveredTileRect != null)
      _localDirection = _gd.GetLocalTileDirectionOfScreenPoint(_hoveredTileRect.Value, mPos);

      if (Raylib.IsMouseButtonPressed(MouseButton.Left))
      {
        _hoveredTile.Blocking[(int)_localDirection] = !_hoveredTile.Blocking[(int)_localDirection];
        _graph = GridGraphConverter.ConvertGridToGraph(_grid);
      }
    }
  }

  public void Draw()
  {
    if (!Enabled) return;
    DrawHovered();
    if (displayGraph) DisplayGraph();

    Raylib.DrawText("Edit mode on", 0, 0, Raylib.GetFontDefault().BaseSize*3, Color.Black);
  }

  private void DisplayGraph()
  {
    foreach (WeightedEdge<GridTile> edge in _graph.GetAllEdges())
    {
      (int x, int y)[] coords = [
        (edge.From.Value.X, edge.From.Value.Y),
        (edge.To.Value.X, edge.To.Value.Y),
      ];

      _gd.DrawLine(coords, Color.Blue, 3, (from, to, thickness, color) =>
      {
        Lines.DrawArrow([from, to], thickness, color, legLength: 5);
        Lines.DrawLineLabel(from, to, Color.Black, edge.Weight + "", Raylib.GetFontDefault().BaseSize);
      }, 15);
    }
  }

  private void DrawHovered()
  {
    if (_hoveredTile != null)
    {
      _gd.DrawTile(_hoveredTile, markedTheme);

      bool[] walls = new bool[4];
      walls[(int)_localDirection] = true;
      if (_hoveredTileRect != null)
      {
        _gd.DrawWalls(walls, _hoveredTileRect.Value, Color.Blue);
      }
    }
  }
}
