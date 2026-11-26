using System.Numerics;
using GridSystem;
using Raylib_cs;
using RaylibExt;
using CoordinateTransformers;
using System.Diagnostics;

namespace GridRenderer;
// TODO: isometric Wall rendering (will require Z-ordering)
// TODO: Extract all the Raylib/RaylibExt stuff
public class GridRendererRaylib
{
  public Grid Grid { get; set; }

  public int LevelsToDisplay
  {
    get; set => field = Math.Clamp(value, 1, Grid.Levels.Length);
  }

  public float FloorColorLevelDifference;

  public Action<GridLevel> DrawLevelExtras = i => { };

  public TileRenderer TileRenderer { get; private set; }
  public LevelMeshRenderer LevelMeshRenderer { get; private set; }

  // -- Wall properties
  public bool IncludeWalls = true;

  // -- Font & theme properties
  public GridColorTheme DefaultTheme { get; set; } = new();
  public GridLayoutSettings LayoutSettings { get; set; } = new();

  // -- Coordinate transformers
  public ICoordinateTransform Transformer { get; set; } = new DummyTransformer();
  public ICoordinateTransform OffsetTransformer { get; set; } = new OffsetTransform();
  public ICoordinateTransform LevelOffsetTransformer { get; set; } = new OffsetTransform() { Offset = new Vector2(0, -100) };

  // -- Cached vectors
  public readonly List<Vector2[]> CacheBackgroundSpacingCorners = [];

  public GridRendererRaylib(Grid grid)
  {
    Grid = grid;
    LevelsToDisplay = grid.Levels.Length;
    FloorColorLevelDifference = 0.25f / grid.Levels.Length;

    TileRenderer = new(this);
    LevelMeshRenderer = new(this);
  }


  public void Recalculate()
  {
    LevelMeshRenderer.Recalculate();
    TileRenderer.Recalculate();
  }

  // -- GRID DRAWING
  // TODO: Extract grid drawing into its own module

  #region grid drawing

  public void DrawGrid() => DrawGrid([], DefaultTheme);
  public void DrawGrid(GridColorTheme theme) => DrawGrid([], theme);
  public void DrawGrid(List<(int x, int y, int level)> includedCoords) => DrawGrid(includedCoords, DefaultTheme);

  public void DrawGrid(List<(int x, int y, int level)> includedCoords, GridColorTheme theme)
  {
    for (int levelNum = 0; levelNum < LevelsToDisplay; levelNum++)
    {
      LevelMeshRenderer.Draw(Color.Green, levelNum);
      GridLevel currentLevel = Grid.GetLevel(levelNum);

      for (int y = 0; y < Grid.Height; y++)
      {
        for (int x = 0; x < Grid.Width; x++)
        {
          if (!includedCoords.Contains((x, y, levelNum)) && includedCoords.Count > 0) continue;

          GridTile? tile = currentLevel.Get(x, y);
          // if (tile != null) TileRenderer.DrawTile(tile, theme);
        }
      }

      DrawLevelExtras(currentLevel);

    }
  }

  #endregion

  // -- PATHS AND LINE DRAWING
  // TODO: Extract paths & lines drawing into its own module

  #region paths and lines

  public void DrawLine(
    (int x, int y, int level)[] coords,
    Color color, int thickness,
    Action<Vector2, Vector2, float, Color> drawAction,
    float distanceFromCenter = 10)
  {
    Vector2[] pxCoords = new Vector2[coords.Length];
    for (int i = 0; i < coords.Length; i++)
    {
      pxCoords[i] = ApplyTransformers(LayoutSettings.GetTileCenterPxCoords(coords[i]), coords[i].level);
    }

    for (int i = 0; i < pxCoords.Length - 1; i++)
    {
      // Offset the lines a little, to allow for differentiation?
      Vector2 direction = Vector2.Normalize(pxCoords[i + 1] - pxCoords[i]);
      Vector2 start = pxCoords[i] + direction * distanceFromCenter;
      Vector2 end = pxCoords[i + 1] - direction * distanceFromCenter;

      drawAction(start, end, thickness, color);
    }
  }

  #endregion

  // -- COORDINATE MANAGEMENT
  // TODO: Extract coordinate management into its own module

  #region coordinates



  public (int x, int y)? GetCoordFromScreenPoint(Vector2 screenPoint, int level)
  {
    screenPoint = ReverseTransformers(screenPoint, level);

    screenPoint /= LayoutSettings.GridTileSizeWithSpacing;

    if (
      screenPoint.X >= Grid.Width || screenPoint.Y >= Grid.Height
      || screenPoint.X < 0 || screenPoint.Y < 0
      ) return null;

    return ((int)screenPoint.X, (int)screenPoint.Y);
  }

  public GridTile? GetTileFromScreenPoint(Vector2 screenPoint)
  {
    for (int levelNum = LevelsToDisplay - 1; levelNum >= 0; levelNum--)
    {
      (int x, int y)? coord = GetCoordFromScreenPoint(screenPoint, levelNum);
      if (coord == null) continue;

      return Grid.GetLevel(levelNum).Get(coord.Value.x, coord.Value.y);
    }

    return null;
  }

  #endregion

  // -- TRANSFORMERS
  // TODO: Extract transformers into its own module
  #region transformers

  public Vector2 ApplyTransformers(Vector2 point, int levelNum)
  {
    point = Transformer.Apply(point);
    point = LevelOffsetTransformer.Apply(point, levelNum);
    point = OffsetTransformer.Apply(point);
    return point;
  }

  public Vector2[] ApplyTransformers(Vector2[] points, int levelNum)
  {
    Vector2[] modified = Transformer.Apply(points);
    modified = LevelOffsetTransformer.Apply(modified, levelNum);
    modified = OffsetTransformer.Apply(modified);
    return modified;
  }

  public Vector2 ReverseTransformers(Vector2 point, int levelNum)
  {
    point = OffsetTransformer.Reverse(point);
    point = LevelOffsetTransformer.Reverse(point, levelNum);
    point = Transformer.Reverse(point);
    return point;
  }

  public Vector2[] ReverseTransformers(Vector2[] points, int levelNum)
  {
    Vector2[] modified = OffsetTransformer.Reverse(points);
    modified = LevelOffsetTransformer.Reverse(modified, levelNum);
    modified = Transformer.Reverse(modified);
    return modified;
  }

  #endregion

  // -- UTIL

  #region util

  public Vector2[] GetCorners(int levelNum)
  {
    Vector2 size = GetPxSize();

    Vector2[] corners = [ // clockwise
      Vector2.Zero,
      new(size.X, 0),
      new(size.X, size.Y),
      new(0, size.Y)
    ];

    return corners;
  }

  public Vector2 GetPxSize() => new(
    Grid.Width * LayoutSettings.GridTileSizeWithSpacing,
    Grid.Height * LayoutSettings.GridTileSizeWithSpacing
  );

  // TODO Better name?
  // TODO Also maybe extract to Direction module?
  public Direction GetLocalTileDirectionOfScreenPoint(Vector2 tileCenterPoint, Vector2 screenPoint)
  {
    Vector2 diff = tileCenterPoint - screenPoint;

    // Horizontal or vertical first
    if (Math.Abs(diff.X) > MathF.Abs(diff.Y))
    {
      return diff.X < 0 ? Direction.East : Direction.West;
    }
    else
    {
      return diff.Y < 0 ? Direction.South : Direction.North;
    }
  }

  #endregion

  // -- CHANGE VISUALS

  public void CenterOnScreen()
  {
    Vector2 centerpoint = Raylib.GetScreenCenter();

    Vector2[] allCorners = [];

    for (int i = 0; i < Grid.Levels.Length; i++)
    {
      allCorners = [.. allCorners, .. ApplyTransformers(GetCorners(i), i)];
    }

    Rectangle r = Rect.GetRectangleFromPoints(allCorners);

    if (OffsetTransformer is not OffsetTransform) return;

    ((OffsetTransform)OffsetTransformer).Offset = centerpoint - r.Center;
  }

}