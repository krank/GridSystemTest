using System.Numerics;
using GridSystem;
using Raylib_cs;

public class GridRendererRaylib : IGridRenderer
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

  private Vector2 _innerRectOffset;
  private Vector2 _innerRectSize;

  public Vector2 GridOffset { get; set; }

  public Vector2 FirstTileOffset => GridOffset + Vector2.One * GridSpacingPx / 2;

  private Grid _grid;

  public GridRendererRaylib(Grid grid)
  {
    _grid = grid;
  }

  public void DrawTile(GridTile tile) => DrawTile(tile, defaultTheme);
  public void DrawTile(GridTile tile, string text) => DrawTile(tile, defaultTheme, text);

  public void DrawTile(GridTile tile, GridTheme theme, string text = "")
  {
    Rectangle rect = new(
      tile.X * GridTileSize + tile.X * GridSpacingPx,
      tile.Y * GridTileSize + tile.Y * GridSpacingPx,
      GridTileSize, GridTileSize
    );
    rect.Position += FirstTileOffset;

    Raylib.DrawRectangleRec(
      rect, theme.Tile
    );

    if (IncludeWalls)
    {
      Vector2[] corners = [
        new(rect.X, rect.Y),
        new(rect.X + rect.Width, rect.Y),
        new(rect.X + rect.Width, rect.Y + rect.Height),
        new(rect.X, rect.Y + rect.Height)
      ];

      // -- Walls
      for (int i = 0; i < tile.Blocking.Length; i++)
      {
        if (!tile.Blocking[i]) continue;

        Raylib.DrawTriangle(
                  corners[i],
                  rect.Center,
                  corners[(i + 1) % tile.Blocking.Length],
                  theme.Wall
        );
      }

      // -- Inner rectangle
      Raylib.DrawRectangleV(
        corners[0] + _innerRectOffset,
        _innerRectSize,
        theme.Tile);
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

  public Vector2 GetPxSize() => new(
      _grid.Width * (GridTileSize + GridSpacingPx),
      _grid.Height * (GridTileSize + GridSpacingPx)
    );

  public void DrawGrid() => DrawGrid([], defaultTheme);
  public void DrawGrid(GridTheme theme) => DrawGrid([], theme);
  public void DrawGrid(List<(int, int)> includedCoords) => DrawGrid(includedCoords, defaultTheme);

  public void DrawGrid(List<(int, int)> includedCoords, GridTheme theme)
  {
    _innerRectSize = Vector2.One * GridTileSize * (1 - WallThickness * 2);
    _innerRectOffset = Vector2.One * GridTileSize * WallThickness;

    Raylib.DrawRectangleV(
      GridOffset,
      GetPxSize(),
      theme.Background
    );

    for (int y = 0; y < _grid.Height; y++)
    {
      for (int x = 0; x < _grid.Width; x++)
      {
        if (!includedCoords.Contains((x, y)) && includedCoords.Count > 0) continue;

        GridTile tile = _grid.Get(x, y);
        DrawTile(tile, theme);
      }
    }
  }

  public Vector2[] GetTileCenterCoords((int, int)[] coords)
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

  public Vector2 GetTileCenterCoords((int, int) coord) => GetTileCenterCoords([coord])[0];

  public Vector2[] GetTileCenterCoords() => GetTileCenterCoords(_grid.GetAllCoords());

  public (int x, int y) GetCoordFromScreenPoint(Vector2 pos)
  {
    pos -= FirstTileOffset;
    if (pos.X < 0 || pos.Y < 0) return (-1, -1);

    float localX = pos.X % (GridTileSize + GridSpacingPx);
    float localY = pos.Y % (GridTileSize + GridSpacingPx);

    if (localY > GridTileSize || localX > GridTileSize) return (-1, -1);

    pos /= GridTileSize + GridSpacingPx;
    if (pos.X >= _grid.Width || pos.Y >= _grid.Height) return (-1, -1);

    return ((int)pos.X, (int)pos.Y);
  }

  public GridTile? GetTileFromScreenPoint(Vector2 pos)
  {
    (int x, int y) coord = GetCoordFromScreenPoint(pos);
    if (coord.x < 0 || coord.y < 0) return null;

    return _grid.Get(coord.x, coord.y);
  }
}