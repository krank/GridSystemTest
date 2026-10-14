using GridSystem;
using WeightedGraph;
using Raylib_cs;
using System.Numerics;
using System.Text.Json;
using System.Diagnostics;
using RaylibExt;
using GridRenderer;

namespace GridSystemTest.LevelEditor;

public class LevelEditor
{
  enum State { Walls, Tiles }

  public bool Enabled { get; set; } = false;
  public string Filename { get; set; } = "grid.json";

  public bool Dirty { get; set; } = false;

  private Grid _grid;
  private WeightedGraph<GridTile> _graph;
  public IsoGridRendererRaylib Gd { get; private set; }

  private bool displayGraph = false;

  public GridTile? HoveredTile { get; private set; }

  public Vector2 MousePosition { get; private set; }

  private Dictionary<State, LevelEditorState> _states;

  private LevelEditorState _state;

  public HashSet<GridTile> SelectedTiles { get; } = [];

  public IsoGridRendererRaylib.GridTheme ThemeHovered { get; private set; } = new()
  {
    Background = Color.Blank,
    Wall = Color.Blank,
    Tile = new(0, 0, 255, 16)
  };
  public IsoGridRendererRaylib.GridTheme ThemeMarked { get; private set; } = new()
  {
    Background = Color.Blank,
    Wall = Color.Blank,
    Tile = new(255, 0, 0, 100)
  };

  public LevelEditor(Grid grid, IsoGridRendererRaylib gd)
  {
    _grid = grid;
    _graph = GridGraphConverter.ConvertGridToGraph(_grid);
    Gd = gd;

    _states = new()
    {
      {State.Walls, new LevelEditorWallState(this)},
      {State.Tiles, new LevelEditorTilesState(this)}
    };

    _state = _states[State.Walls];
  }

  public void Update(Vector2 mPos)
  {
    if (!Enabled) return;

    MousePosition = mPos;

    HoveredTile = Gd.GetTileFromScreenPoint(mPos);
    if (HoveredTile != null) _state.HandleHoveredTile();

    _state.HandleMouseClicks(
      Raylib.IsMouseButtonPressed(MouseButton.Left),
      Raylib.IsMouseButtonPressed(MouseButton.Right)
    );

    if (Raylib.IsKeyDown(KeyboardKey.LeftControl))
    {
      if (Raylib.IsKeyPressed(KeyboardKey.S)) SaveToFile();
      if (Raylib.IsKeyPressed(KeyboardKey.L)) LoadFromFile();
      if (Raylib.IsKeyPressed(KeyboardKey.N)) ClearGrid();
      if (Raylib.IsKeyPressed(KeyboardKey.W)) _state = _states[State.Walls];
      if (Raylib.IsKeyPressed(KeyboardKey.T)) _state = _states[State.Tiles];
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Tab)) displayGraph = !displayGraph;

    if (Dirty) _graph = GridGraphConverter.ConvertGridToGraph(_grid);
  }

  private void ClearGrid()
  {
    _grid = _grid != null ?
      new(_grid.Width, _grid.Height) :
      new(10, 10);

    Gd.Grid = _grid;
    _graph = GridGraphConverter.ConvertGridToGraph(_grid);
    Debug.Print("Cleared");
  }

  private void SaveToFile()
  {
    File.WriteAllText(Filename, JsonSerializer.Serialize(_grid));
    Debug.Print("Saved");
  }

  private void LoadFromFile()
  {
    if (File.Exists("grid.json"))
    {
      Grid? newGrid = JsonSerializer.Deserialize<Grid>(File.ReadAllText(Filename));
      if (newGrid != null)
      {
        _grid = newGrid;
        Gd.Grid = _grid;
        _graph = GridGraphConverter.ConvertGridToGraph(_grid);
        Debug.Print("Loaded");
      }
    }
  }

  public void DrawUI()
  {
    if (!Enabled) return;
    Raylib.DrawText("Edit mode on", 10, 0, Raylib.GetFontDefault().BaseSize * 3, Color.Black);
    Raylib.DrawText($"{_state.Name}", 10, 40, Raylib.GetFontDefault().BaseSize * 3, Color.Gray);
    _state.DrawUI();
  }

  public void Draw()
  {
    if (!Enabled) return;

    _state.Draw();

    if (displayGraph) DisplayGraph();
  }

  public void DrawSelected()
  {
    foreach (GridTile tile in SelectedTiles)
    {
      Gd.DrawTile(tile, ThemeMarked);
    }
  }

  public void DrawHovered()
  {
    if (HoveredTile == null) return;

    Gd.DrawTile(HoveredTile, ThemeHovered);
  }

  private void DisplayGraph()
  {
    foreach (WeightedEdge<GridTile> edge in _graph.GetAllEdges())
    {
      (int x, int y)[] coords = [
        (edge.From.Value.X, edge.From.Value.Y),
        (edge.To.Value.X, edge.To.Value.Y),
      ];

      Gd.DrawLine(coords, Color.Blue, 3, (from, to, thickness, color) =>
      {
        Lines.DrawArrow([from, to], thickness, color, legLength: 5);
        Lines.DrawLineLabel(from, to, Color.Black, edge.Weight + "", Raylib.GetFontDefault().BaseSize);
      }, 15);
    }
  }
}
