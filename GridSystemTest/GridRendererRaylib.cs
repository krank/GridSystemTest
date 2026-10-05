using System.Numerics;
using GridSystem;
using Raylib_cs;

public class GridRendererRaylib
{
  public class GridTheme
  {
    public Color Background { get; set; } = new(240, 240, 240);
    public Color Wall { get; set; } = Color.Black;
    public Color Tile { get; set; } = Color.LightGray;
    public Color Text { get; set; } = Color.Black;
  }

  public float GridTileSize { get; set; } = 32;
  public float WallThickness = .1f;
  public bool IncludeWalls = true;

  public float GridSpacing { get; set; } = .1f;
  private float GridSpacingPx { get => GridSpacing * GridTileSize; }
  private float FontSize { get => GridTileSize / 2; }

  public GridTheme defaultTheme = new();

  private Vector2[] _innerRectCornerOffsets = [];

  public Vector2 GridOffset { get; set; }

  public Vector2 FirstTileOffset => GridOffset + Vector2.One * GridSpacingPx / 2;

  public Grid Grid { get; set; }

  public GridRendererRaylib(Grid grid)
  {
    Grid = grid;
  }

  // -- TILE DRAWING

  public void DrawTile(GridTile tile) => DrawTile(tile, defaultTheme);
  public void DrawTile(GridTile tile, string text) => DrawTile(tile, defaultTheme, text);

  public void DrawTile(GridTile tile, GridTheme theme, string text = "")
  {
    // TODO: Extract the rect-getting of a tile to new method
    Rectangle rect = GetTileRect(tile);

    Raylib.DrawRectangleRec(
      rect, theme.Tile
    );

    if (IncludeWalls)
    {
      DrawWalls(tile.Blocking, rect, Color.Black);
    }

    if (text.Length > 0)
    {
      Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, FontSize, FontSize / 16);
      Vector2 textPos = rect.Position + rect.Size / 2 - textSize / 2;

      Raylib.DrawTextEx(
        Raylib.GetFontDefault(),
        text,
        textPos,
        FontSize, FontSize / 16, theme.Text
      );
    }
  }

  public void DrawWalls(bool[] directions, Rectangle rect, Color color)
  {
    Vector2[] corners = [
      new(rect.X, rect.Y),
      new(rect.X + rect.Width, rect.Y),
      new(rect.X + rect.Width, rect.Y + rect.Height),
      new(rect.X, rect.Y + rect.Height)
    ];

    // TODO: Det MÅSTE finnas en bättre lösning
    Vector2[] innerCorners = [
      corners[0] + _innerRectCornerOffsets[0],
      corners[1] + _innerRectCornerOffsets[1],
      corners[2] + _innerRectCornerOffsets[2],
      corners[3] + _innerRectCornerOffsets[3],
    ];

    // -- Walls
    for (int i = 0; i < directions.Length; i++) // Go through all four directions
    {
      if (!directions[i]) continue;

      int nxt = (i + 1) % directions.Length;
      Raylib.DrawTriangleStrip([
        corners[i],
          innerCorners[i],
          corners[nxt],
          innerCorners[nxt]
      ], 4, color);
    }
  }

  // -- GRID DRAWING
  public void DrawGrid() => DrawGrid([], defaultTheme);
  public void DrawGrid(GridTheme theme) => DrawGrid([], theme);
  public void DrawGrid(List<(int, int)> includedCoords) => DrawGrid(includedCoords, defaultTheme);

  public void DrawGrid(List<(int, int)> includedCoords, GridTheme theme)
  {
    _innerRectCornerOffsets = [
        new Vector2(GridSpacingPx),
        new Vector2(-GridSpacingPx, GridSpacingPx),
        new Vector2(-GridSpacingPx, -GridSpacingPx),
        new Vector2(GridSpacingPx, -GridSpacingPx),
      ];

    Raylib.DrawRectangleV(
      GridOffset,
      GetPxSize(),
      theme.Background
    );

    for (int y = 0; y < Grid.Height; y++)
    {
      for (int x = 0; x < Grid.Width; x++)
      {
        if (!includedCoords.Contains((x, y)) && includedCoords.Count > 0) continue;

        GridTile tile = Grid.Get(x, y);
        DrawTile(tile, theme);
      }
    }
  }

  // -- PATHS AND LINE DRAWING

  //TODO: Draw line (Array of grid coords, arrow at the end yes/no, distance from tile center)
  //TODO: Draw line (Array of tile coords, arrow at the end yes/no, distance from tile center, text)
  //TODO: Draw line (Array of tile coords, Action<x, y>)


  public void DrawLine((int x, int y)[] coords, Color color, int thickness, float distanceFromCenter = 10)
  {
    Vector2[] pxCoords = GetTileCenterPxCoords(coords);

    for (int i = 0; i < pxCoords.Length - 1; i++)
    {
      Vector2 direction = Vector2.Normalize(pxCoords[i + 1] - pxCoords[i]);
      Vector2 start = pxCoords[i] + direction * distanceFromCenter;
      Vector2 end = pxCoords[i + 1] - direction * distanceFromCenter;

      Raylib.DrawLineEx(start, end, thickness, color);
    }
  }
  public void DrawLine((int x, int y)[] coords, Color color, int thickness, Action<Vector2, Vector2, float, Color> drawAction, float distanceFromCenter = 10)
  {
    Vector2[] pxCoords = GetTileCenterPxCoords(coords);

    for (int i = 0; i < pxCoords.Length - 1; i++)
    {
      Vector2 direction = Vector2.Normalize(pxCoords[i + 1] - pxCoords[i]);
      Vector2 start = pxCoords[i] + direction * distanceFromCenter;
      Vector2 end = pxCoords[i + 1] - direction * distanceFromCenter;

      drawAction(start, end, thickness, color);

      // Raylib.DrawLineEx(start, end, thickness, color);
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
    Vector2? localPxCoord = GetTileLocalPxCoords(screenPoint, false);
    if (localPxCoord == null) return null;

    screenPoint -= GridOffset;

    // TODO: Fix this
    screenPoint = screenPoint / (GridTileSize + GridSpacingPx);

    if (screenPoint.X >= Grid.Width || screenPoint.Y >= Grid.Height) return null;

    return ((int)screenPoint.X, (int)screenPoint.Y);
  }

  public GridTile? GetTileFromScreenPoint(Vector2 pos)
  {
    (int x, int y) coord = GetCoordFromScreenPoint(pos) ?? (-1, -1);
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

  public Rectangle GetTileRect(GridTile tile)
  {
    return new(
      tile.X * GridTileSize + tile.X * GridSpacingPx + FirstTileOffset.X,
      tile.Y * GridTileSize + tile.Y * GridSpacingPx + FirstTileOffset.Y,
      GridTileSize, GridTileSize
    );
  }

  // TODO Better name?
  public Direction GetLocalTileDirectionOfScreenPoint(Rectangle tileRect, Vector2 screenPoint)
  {
    Vector2 diff = tileRect.Center - screenPoint;

    // Horizontal or vertical
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

  public void FitToScreen()
  {
    GridTileSize = Math.Min(
      Raylib.GetScreenHeight() / (Grid.Height * GridSpacing + Grid.Height),
      Raylib.GetScreenWidth() / (Grid.Width * GridSpacing + Grid.Width)
    );
  }

  public void CenterOnScreen()
  {
    GridOffset = Raylib.GetScreenCenter() - GetPxSize() / 2;
  }
}