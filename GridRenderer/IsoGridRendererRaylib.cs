using System.Numerics;
using GridSystem;
using Raylib_cs;
using RaylibExt;
using CoordinateTransformers;

namespace GridRenderer;

public class IsoGridRendererRaylib
{
  public Grid Grid { get; set; }
  public float GridTileSize { get; set; } = 32;

  // -- Wall properties
  private float _wallThickness = .1f;
  public float WallThickness
  {
    set
    {
      _wallThickness = value;

      _innerRectCornerOffsets = [
        new Vector2(WallThicknessPx),
        new Vector2(-WallThicknessPx, WallThicknessPx),
        new Vector2(-WallThicknessPx, -WallThicknessPx),
        new Vector2(WallThicknessPx, -WallThicknessPx),
      ];

      _innerRectCornerOffsets = Transformer.Apply(_innerRectCornerOffsets);

    }
    get => _wallThickness;
  }
  private float WallThicknessPx { get => _wallThickness * GridTileSize; }
  public bool IncludeWalls = true;
  private Vector2[] _innerRectCornerOffsets = [];

  // -- Grid spacing properties
  public float GridSpacing { get; set; } = .1f;
  private float GridSpacingPx { get => GridSpacing * GridTileSize; }
  public Vector2 FirstTileOffset => Vector2.One * GridSpacingPx / 2;

  // -- Font & theme properties
  private float FontSize { get => GridTileSize / 2; }
  public GridTheme DefaultTheme { get; set; } = new();

  // -- Coordinate transformers
  public CoordinateTransform Transformer { get; set; } = new IsoTransform() { Ratio = .5f };
  public OffsetTransform OffsetTransformer { get; set; } = new OffsetTransform();

  public IsoGridRendererRaylib(Grid grid)
  {
    Grid = grid;
    WallThickness = _wallThickness;
  }

  // -- TILE DRAWING

  public void DrawTile(GridTile tile) => DrawTile(tile, DefaultTheme);
  public void DrawTile(GridTile tile, string text) => DrawTile(tile, DefaultTheme, text);

  public void DrawTile(GridTile tile, GridTheme theme, string text = "")
  {
    // TODO: Extract the rect-getting of a tile to new method

    Vector2[] corners = GetTileCorners(tile);
    corners = Transformer.Apply(corners);
    corners = OffsetTransformer.Apply(corners);

    // TODO: Change order of blocker/directions to be counterclockwise, to avoid needing to reverse?
    if (tile.Floor)
    {
      Raylib.DrawTriangleFan([.. corners.Reverse()], 4, theme.Tile);
    }

    if (IncludeWalls)
    {
      DrawWalls(tile.Blocking, corners, Color.Black);
    }

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

  // -- GRID DRAWING
  public void DrawGrid() => DrawGrid([], DefaultTheme);
  public void DrawGrid(GridTheme theme) => DrawGrid([], theme);
  public void DrawGrid(List<(int, int)> includedCoords) => DrawGrid(includedCoords, DefaultTheme);

  public void DrawGrid(List<(int, int)> includedCoords, GridTheme theme)
  {
    // This setup feels like it shouldn't need to be done each frame
    // Just each time WallSpacingPx is recalculated
    _innerRectCornerOffsets = [
        new Vector2(WallThicknessPx),
        new Vector2(-WallThicknessPx, WallThicknessPx),
        new Vector2(-WallThicknessPx, -WallThicknessPx),
        new Vector2(WallThicknessPx, -WallThicknessPx),
      ];

    _innerRectCornerOffsets = Transformer.Apply(_innerRectCornerOffsets);

    Vector2 size = GetPxSize();

    Vector2[] corners = [ // clockwise
      Vector2.Zero,
      new(size.X, 0),
      new(size.X, size.Y),
      new(0, size.Y)
    ];

    corners = Transformer.Apply(corners);
    corners = OffsetTransformer.Apply(corners);

    Raylib.DrawTriangleFan([.. corners.Reverse()], 4, theme.Background);

    for (int y = 0; y < Grid.Height; y++)
    {
      for (int x = 0; x < Grid.Width; x++)
      {
        if (!includedCoords.Contains((x, y)) && includedCoords.Count > 0) continue;

        GridTile? tile = Grid.Get(x, y);
        if (tile != null) DrawTile(tile, theme);
      }
    }
  }

  // -- PATHS AND LINE DRAWING

  public void DrawLine((int x, int y)[] coords, Color color, int thickness, Action<Vector2, Vector2, float, Color> drawAction, float distanceFromCenter = 10)
  {
    Vector2[] pxCoords = GetTileCenterPxCoords(coords);

    for (int i = 0; i < pxCoords.Length - 1; i++)
    {
      Vector2 direction = Vector2.Normalize(pxCoords[i + 1] - pxCoords[i]);
      Vector2 start = pxCoords[i] + direction * distanceFromCenter;
      Vector2 end = pxCoords[i + 1] - direction * distanceFromCenter;

      start = Transformer.Apply(start);
      end = Transformer.Apply(end);
      start = OffsetTransformer.Apply(start);
      end = OffsetTransformer.Apply(end);

      drawAction(start, end, thickness, color);
    }
  }

  // -- COORDINATE MANAGEMENT
  public Vector2[] GetTileCenterPxCoords((int, int)[] coords)
  {
    Vector2[] pxCoords = new Vector2[coords.Length];
    Vector2 halfSizeOffset = Vector2.One * GridTileSize / 2;

    for (int i = 0; i < coords.Length; i++)
    {
      (int x, int y) = coords[i];

      pxCoords[i] = new Vector2(
          x * (GridTileSize + GridSpacingPx),
          y * (GridTileSize + GridSpacingPx)
        ) + FirstTileOffset + halfSizeOffset;
    }

    return pxCoords;
  }

  public Vector2 GetTileCenterPxCoords((int, int) coord) => GetTileCenterPxCoords([coord])[0];

  public Vector2[] GetTileCenterPxCoords() => GetTileCenterPxCoords(Grid.GetAllCoords());

  public (int x, int y)? GetCoordFromScreenPoint(Vector2 screenPoint)
  {
    screenPoint = OffsetTransformer.Reverse(screenPoint);
    screenPoint = Transformer.Reverse(screenPoint);

    screenPoint /= GridTileSize + GridSpacingPx;

    if (screenPoint.X >= Grid.Width || screenPoint.Y >= Grid.Height) return null;

    return ((int)screenPoint.X, (int)screenPoint.Y);
  }

  public GridTile? GetTileFromScreenPoint(Vector2 screenPoint)
  {
    (int x, int y) coord = GetCoordFromScreenPoint(screenPoint) ?? (-1, -1);
    if (coord.x < 0 || coord.y < 0) return null;

    return Grid.Get(coord.x, coord.y);
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

  // -- UTIL

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

  // -- CHANGE VISUALS
  // TODO: Make these workregardless of transformations?

  public void FitToScreen()
  {
    GridTileSize = Math.Min(
      Raylib.GetScreenHeight() / (Grid.Height * GridSpacing + Grid.Height),
      Raylib.GetScreenWidth() / (Grid.Width * GridSpacing + Grid.Width)
    );
  }

  public void CenterOnScreen()
  {
    OffsetTransformer.Offset = Raylib.GetScreenCenter();
  }

  // -- NESTED CLASSES

  public class GridTheme
  {
    public Color Background { get; set; } = new(240, 240, 240);
    public Color Wall { get; set; } = Color.Black;
    public Color Tile { get; set; } = Color.LightGray;
    public Color Text { get; set; } = Color.Black;
  }
}