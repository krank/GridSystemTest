using GridSystem;
using WeightedGraph;
using Raylib_cs;
using System.Numerics;
using System.Diagnostics;
using RaylibExt;
using GridRenderer;
using GGC = GridGraphConverter;

namespace GridLevelEditor;

public class LevelEditor
{
  enum State { Walls, Tiles }

  private bool _enabled = false;

  public bool Enabled
  {
    get => _enabled;
    set
    {
      _enabled = value;
      if (_enabled) Gd.DrawLevelExtras += DrawGraph;
      else Gd.DrawLevelExtras -= DrawGraph;
    }
  }

  private readonly LevelEditorData levelEditorData;

  public bool Dirty { get; set; } = false;

  public Grid Grid
  {
    get; set
    {
      field = value;
      _graph = GGC.GridGraphConverter.ConvertGridToGraph(value); // TODO: Multilevel
    }
  }
  private WeightedGraph<GridTile>? _graph;
  public GridRendererRaylib Gd { get; private set; }

  public bool DisplayGraph { get; private set; } = false;

  public GridTile? HoveredTile { get; private set; }

  public Vector2 MousePosition { get; private set; }

  private readonly Dictionary<State, LevelEditorState> _states;

  public HashSet<GridTile> SelectedTiles { get; } = [];

  public GridRendererRaylib.GridTheme ThemeHovered { get; private set; } = new()
  {
    Background = Color.Blank,
    Wall = Color.Blank,
    Tile = new(0, 0, 255, 16)
  };
  public GridRendererRaylib.GridTheme ThemeMarked { get; private set; } = new()
  {
    Background = Color.Blank,
    Wall = Color.Blank,
    Tile = new(255, 0, 0, 100)
  };

  public LevelEditorState CurrentState
  {
    private get => field;
    set
    {
      if (field != null) field.ExitState();
      field = value;
      field.EnterState();
    }
  }

  public LevelEditor(Grid grid, GridRendererRaylib gd)
  {
    Grid = grid;
    Gd = gd;
    levelEditorData = new(this);

    _states = new()
    {
      {State.Walls, new LevelEditorWallState(this)},
      {State.Tiles, new LevelEditorTilesState(this)}
    };

    CurrentState = _states[State.Tiles];
  }

  public void Update(Vector2 mPos)
  {
    if (!Enabled) return;

    MousePosition = mPos;

    HoveredTile = Gd.GetTileFromScreenPoint(mPos);
    if (HoveredTile != null) CurrentState.HandleHoveredTile();

    CurrentState.HandleMouseClicks(
      Raylib.IsMouseButtonPressed(MouseButton.Left),
      Raylib.IsMouseButtonPressed(MouseButton.Right)
    );

    CurrentState.HandleKeys();

    if (Raylib.IsKeyDown(KeyboardKey.LeftControl))
    {
      if (Raylib.IsKeyPressed(KeyboardKey.S)) levelEditorData.SaveToFile();
      if (Raylib.IsKeyPressed(KeyboardKey.L)) levelEditorData.LoadFromFile();
      if (Raylib.IsKeyPressed(KeyboardKey.N)) ClearGrid();
      if (Raylib.IsKeyPressed(KeyboardKey.W)) CurrentState = _states[State.Walls];
      if (Raylib.IsKeyPressed(KeyboardKey.T)) CurrentState = _states[State.Tiles];
    }

    if (Raylib.IsKeyPressed(KeyboardKey.Tab)) DisplayGraph = !DisplayGraph;

    if (Dirty) _graph = GGC.GridGraphConverter.ConvertGridToGraph(Grid);
  }

  private void ClearGrid()
  {
    Grid = Grid != null ?
      new(Grid.Width, Grid.Height, Grid.Levels.Length) :
      new(10, 10, 0);

    Debug.Print("Cleared");
  }

  public void DrawUI()
  {
    if (!Enabled) return;
    Raylib.DrawText("Edit mode on", 10, 0, Raylib.GetFontDefault().BaseSize * 3, Color.Black);
    Raylib.DrawText($"{CurrentState.Name}", 10, 40, Raylib.GetFontDefault().BaseSize * 3, Color.Gray);
    CurrentState.DrawUI();
  }

  public void Draw()
  {
    Console.WriteLine("-----");
    if (!Enabled) return;

    CurrentState.Draw();
  }

  public void DrawSelected(GridLevel level)
  {
    foreach (GridTile tile in SelectedTiles)
    {
      if (tile.GridLevel != level) continue;
      Gd.DrawTile(tile, ThemeMarked, alwaysDrawFloor: true);
    }
  }

  public void DrawHovered(GridLevel level)
  {
    if (HoveredTile == null) return;
    if (HoveredTile.GridLevel != level) return;

    Gd.DrawTile(HoveredTile, ThemeHovered, alwaysDrawFloor: true);
  }

  private void DrawGraph(GridLevel level)
  {
    if (!DisplayGraph || _graph == null) return;
    Console.WriteLine(level.LevelNum);

    List<WeightedEdge<GridTile>> edges = _graph.GetAllEdges();

    // var r = edges.Where(V => V.From.Value.GridLevel != V.To.Value.GridLevel);
    // Console.WriteLine(r.Count());

    foreach (WeightedEdge<GridTile> edge in edges)
    {
      if (edge.From.Value.GridLevel.LevelNum != level.LevelNum 
        && edge.To.Value.GridLevel.LevelNum != level.LevelNum
      )
      {
        continue;
      }

      (int x, int y, int level)[] coords = [
        (edge.From.Value.X, edge.From.Value.Y, edge.From.Value.GridLevel.LevelNum),
        (edge.To.Value.X, edge.To.Value.Y, edge.From.Value.GridLevel.LevelNum)
      ];

      Gd.DrawLine(coords, Color.Blue, 3, (from, to, thickness, color) =>
      {
        Lines.DrawArrow([from, to], thickness, color, legLength: 5);
        Lines.DrawLineLabel(from, to, Color.Black, edge.Weight + "", Raylib.GetFontDefault().BaseSize);
      }, 15);
    }
  }
}
