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

  public Grid Grid
  {
    get => field;
    set
    {
      field = value;
    }
  }

  public float GridTileSize { get; set; } = 32;

  private int _levelsToDisplay;
  public int LevelsToDisplay
  {
    get => _levelsToDisplay;
    set => _levelsToDisplay = Math.Clamp(value, 1, Grid.Levels.Length);
  }

  private float _floorColorLevelDifference;

  public Action<GridLevel> DrawLevelExtras = i => { };

  // TODO: Extract visual stuff like sizes etc into a config class
  // -- Wall properties
  public float WallThickness { set; get; } = .1f;

  private float WallThicknessPx { get => WallThickness * GridTileSize; }
  public bool IncludeWalls = true;
  private Vector2[] _innerRectCornerOffsets = [];
  private Vector2 _halfSizeOffset = Vector2.Zero;

  // -- Grid spacing properties
  public float GridSpacing { get; set; } = .1f;
  private float GridSpacingPx { get => GridSpacing * GridTileSize; }
  public Vector2 FirstTileOffset => Vector2.One * GridSpacingPx / 2;

  // -- Font & theme properties
  private float FontSize { get => GridTileSize / 2; }
  public GridTheme DefaultTheme { get; set; } = new();

  // -- Coordinate transformers
  public ICoordinateTransform Transformer { get; set; } = new DummyTransformer();
  public ICoordinateTransform OffsetTransformer { get; set; } = new OffsetTransform();
  public ICoordinateTransform LevelOffsetTransformer { get; set; } = new OffsetTransform() { Offset = new Vector2(0, -100) };

  // -- Cached vectors
  private readonly List<Vector2[]> _cacheBackgroundSpacingCorners = [];

  public GridRendererRaylib(Grid grid)
  {
    Grid = grid;
    LevelsToDisplay = grid.Levels.Length;
    _floorColorLevelDifference = 0.25f / grid.Levels.Length;
  }


  public void Recalculate()
  {
    CalculateBackgroundSpacingCorners();

    _innerRectCornerOffsets = [
      new Vector2(WallThicknessPx),
      new Vector2(-WallThicknessPx, WallThicknessPx),
      new Vector2(-WallThicknessPx, -WallThicknessPx),
      new Vector2(WallThicknessPx, -WallThicknessPx),
    ];

    _innerRectCornerOffsets = Transformer.Apply(_innerRectCornerOffsets);

    _halfSizeOffset = Vector2.One * GridTileSize / 2;

  }

  // -- TILE DRAWING
  // TODO: Extract tile drawing into its own module

  #region tile drawing

  public void DrawTile(GridTile tile) => DrawTile(tile, DefaultTheme);
  public void DrawTile(GridTile tile, string text) => DrawTile(tile, DefaultTheme, text);

  public void DrawTile(GridTile tile, GridTheme theme, string text = "", bool alwaysDrawFloor = false)
  {
    Vector2[] corners = GetTileCorners(tile);
    corners = ApplyTransformers(corners, tile.GridLevel.LevelNum);

    // TODO: Change order of blocker/directions to be counterclockwise, to avoid needing to reverse?
    if (tile.HasFloor || alwaysDrawFloor)
    {
      Raylib.DrawTriangleFan([.. corners.Reverse()], 4, ColorUtil.Brighten(theme.Tile, 1 + _floorColorLevelDifference * tile.GridLevel.LevelNum));
    }

    if (IncludeWalls)
    {
      DrawWalls(tile.Blocking, corners, Color.Black);
    }

    // -- TEXT?
    if (text.Length > 0)
    {
      Rectangle rect = Rect.GetRectangleFromPoints(corners);
      Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, FontSize, FontSize / 16);
      Vector2 textPos = rect.Center - textSize / 2;

      Raylib.DrawTextEx(
        Raylib.GetFontDefault(),
        text,
        textPos,
        FontSize, FontSize / 16, theme.Text
      );
    }
  }

  public void DrawWalls(bool[] directions, Vector2[] corners, Color color)
  {
    Vector2[] innerCorners = new Vector2[corners.Length];
    for (int i = 0; i < corners.Length; i++)
    {
      innerCorners[i] = corners[i] + _innerRectCornerOffsets[i];
    }

    // -- Walls
    for (int i = 0; i < directions.Length; i++) // Go through all four directions
    {
      if (!directions[i]) continue;

      int nxt = (i + 1) % directions.Length;
      Raylib.DrawTriangleFan([
        innerCorners[nxt],
        corners[nxt],
        corners[i],
        innerCorners[i],
      ], 4, color);
    }
  }

  #endregion

  // -- GRID DRAWING
  // TODO: Extract grid drawing into its own module

  #region grid drawing

  public void DrawGrid() => DrawGrid([], DefaultTheme);
  public void DrawGrid(GridTheme theme) => DrawGrid([], theme);
  public void DrawGrid(List<(int x, int y, int level)> includedCoords) => DrawGrid(includedCoords, DefaultTheme);

  public void DrawGrid(List<(int x, int y, int level)> includedCoords, GridTheme theme)
  {
    for (int levelNum = 0; levelNum < LevelsToDisplay; levelNum++)
    {
      DrawBackgroundMesh(theme.Background, levelNum);
      GridLevel currentLevel = Grid.GetLevel(levelNum);

      for (int y = 0; y < Grid.Height; y++)
      {
        for (int x = 0; x < Grid.Width; x++)
        {
          // TODO: IncludeCoords needs level awareness
          if (!includedCoords.Contains((x, y, levelNum)) && includedCoords.Count > 0) continue;

          GridTile? tile = currentLevel.Get(x, y);
          if (tile != null) DrawTile(tile, theme);
        }
      }

      DrawLevelExtras(currentLevel);

    }
  }

  // TODO: Move background mesh drawing to new module?
  private void CalculateBackgroundSpacingCorners()
  {
    // TODO: Add level awareness; different caches for each level
    Vector2[] corners = ApplyTransformers(GetCorners(0), 0);
    _cacheBackgroundSpacingCorners.Clear();

    Vector2 spacingOffsetV = Transformer.Apply(new Vector2(0, GridSpacingPx));
    Vector2 tileOffsetV = Transformer.Apply(new Vector2(0, GridTileSize));
    Vector2 spacingOffsetH = Transformer.Apply(new Vector2(GridSpacingPx, 0));
    Vector2 tileOffsetH = Transformer.Apply(new Vector2(GridTileSize, 0));

    // Initial corners for Vertical
    Vector2[] crn = [corners[0], corners[1],
      corners[1] + spacingOffsetV / 2,
      corners[0] + spacingOffsetV / 2
    ];

    _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);

    for (int i = 0; i < 2; i++) crn[i] -= spacingOffsetV / 2;

    for (int y = 0; y < Grid.Height; y++)
    {
      // Move all corners one step
      for (int i = 0; i < 4; i++) crn[i] += tileOffsetV + spacingOffsetV;
      // Decrease the distance between corners for the last line
      if (y == Grid.Height - 1)
      {
        for (int i = 2; i < 4; i++) crn[i] -= spacingOffsetV / 2;
      }

      _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);
    }

    crn = [
      corners[0],
      corners[0] + spacingOffsetH / 2,
      corners[3] + spacingOffsetH / 2,
      corners[3]
    ];

    _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);
    crn[0] -= spacingOffsetH / 2;
    crn[3] -= spacingOffsetH / 2;

    for (int x = 0; x < Grid.Width; x++)
    {
      // Move all corners one step
      for (int i = 0; i < 4; i++) crn[i] += tileOffsetH + spacingOffsetH;
      // Decrease the distance between corners for the last line
      if (x == Grid.Width - 1)
      {
        crn[1] -= spacingOffsetH / 2;
        crn[2] -= spacingOffsetH / 2;
      }

      _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);
    }

  }

  private void DrawBackgroundMesh(Color color, int level)
  {
    color = ColorUtil.Brighten(color, 1 + (level * 0.1f));
    List<Vector2[]> localCornerGroups = [.. _cacheBackgroundSpacingCorners];

    for (int i = 0; i < localCornerGroups.Count; i++)
    {
      localCornerGroups[i] = LevelOffsetTransformer.Apply(localCornerGroups[i], level);
    }

    foreach (Vector2[] crn in localCornerGroups)
    {
      Raylib.DrawTriangleFan(crn, crn.Length, color);
    }
  }

  public Vector2[] GetCorners(int levelNum)
  {
    Vector2 size = GetPxSize();

    Vector2[] corners = [ // clockwise
      Vector2.Zero,
      new(size.X, 0),
      new(size.X, size.Y),
      new(0, size.Y)
    ];

    // corners = ApplyTransformers(corners, levelNum);
    return corners;
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
      pxCoords[i] = ApplyTransformers(GetTileCenterPxCoords(coords[i]), coords[i].level);
    }

    //  GetTileCenterPxCoords(coords);

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

  public Vector2 GetTileCenterPxCoords((int x, int y, int level) coord)
  {
    return new Vector2(
      coord.x * (GridTileSize + GridSpacingPx),
      coord.y * (GridTileSize + GridSpacingPx)
    ) + FirstTileOffset + _halfSizeOffset;
  }

  public (int x, int y)? GetCoordFromScreenPoint(Vector2 screenPoint, int level)
  {
    screenPoint = ReverseTransformers(screenPoint, level);

    screenPoint /= GridTileSize + GridSpacingPx;

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

  public Vector2? GetTileLocalPxCoords(Vector2 screenPoint, bool constrain = true)
  {
    screenPoint -= FirstTileOffset;
    if (screenPoint.X < 0 || screenPoint.Y < 0) return constrain ? Vector2.Zero : null;

    float localX = screenPoint.X % (GridTileSize + GridSpacingPx);
    float localY = screenPoint.Y % (GridTileSize + GridSpacingPx);

    if (localY > GridTileSize || localX > GridTileSize) return constrain ? new(GridTileSize) : null;

    return new(localX, localY);
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
  public Vector2 GetPxSize() => new(
    Grid.Width * (GridTileSize + GridSpacingPx),
    Grid.Height * (GridTileSize + GridSpacingPx)
  );

  public Vector2[] GetTileCorners(GridTile tile)
  {
    float left = tile.X * GridTileSize + tile.X * GridSpacingPx + FirstTileOffset.X;
    float top = tile.Y * GridTileSize + tile.Y * GridSpacingPx + FirstTileOffset.Y;
    float right = left + GridTileSize;
    float bottom = top + GridTileSize;

    return [ // COUNTERCLOCKWISE
      new(left, top),
      new(right, top),
      new(right, bottom),
      new(left, bottom),
    ];
  }

  // TODO Better name?
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
    // TODO: Get a big rectangle that contains all floors

    List<Vector2> allCorners = [];

    for (int i = 0; i < Grid.Levels.Length; i++)
    {
      allCorners.Append(GetCorners(0));
    }

    Rectangle r = Rect.GetRectangleFromPoints(
      ApplyTransformers(GetCorners(0), 0)
      );
    if (OffsetTransformer is not OffsetTransform) return;

    ((OffsetTransform)OffsetTransformer).Offset = centerpoint - r.Center;
  }

  // -- NESTED CLASSES

  public class GridTheme
  {
    public Color Background { get; set; } = new(210, 210, 210);
    public Color Wall { get; set; } = Color.Black;
    public Color Tile { get; set; } = Color.LightGray;
    public Color Text { get; set; } = Color.Black;
  }
}