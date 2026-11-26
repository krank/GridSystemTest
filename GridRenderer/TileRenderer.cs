using System.Numerics;
using GridSystem;
using Raylib_cs;
using RaylibExt;

namespace GridRenderer;

public class TileRenderer(GridRendererRaylib gridRenderer)
{
  private readonly GridRendererRaylib _gridRenderer = gridRenderer;
  private Vector2[] _innerRectCornerOffsets = [];

  public void DrawTile(GridTile tile) => DrawTile(tile, _gridRenderer.DefaultTheme);
  public void DrawTile(GridTile tile, string text) => DrawTile(tile, _gridRenderer.DefaultTheme, text);

  public void DrawTile(GridTile tile, GridColorTheme theme, string text = "", bool alwaysDrawFloor = false)
  {
    Vector2[] corners = _gridRenderer.LayoutSettings.GetTileCorners(tile);
    corners = _gridRenderer.ApplyTransformers(corners, tile.GridLevel.LevelNum);

    // TODO: Change order of blocker/directions to be counterclockwise, to avoid needing to reverse?
    if (tile.HasFloor || alwaysDrawFloor)
    {
      Raylib.DrawTriangleFan([.. corners.Reverse()], 4, ColorUtil.Brighten(theme.Tile, 1 + _gridRenderer.FloorColorLevelDifference * tile.GridLevel.LevelNum));
    }

    if (_gridRenderer.IncludeWalls)
    {
      DrawWalls(tile.Blocking, corners, Color.Black);
    }

    // -- TEXT?
    if (text.Length > 0)
    {
      Rectangle rect = Rect.GetRectangleFromPoints(corners);
      Vector2 textSize = Raylib.MeasureTextEx(Raylib.GetFontDefault(), text,
        _gridRenderer.LayoutSettings.FontSize, _gridRenderer.LayoutSettings.FontSpacing);
      Vector2 textPos = rect.Center - textSize / 2;

      Raylib.DrawTextEx(
        Raylib.GetFontDefault(),
        text,
        textPos,
        _gridRenderer.LayoutSettings.FontSize, _gridRenderer.LayoutSettings.FontSpacing, theme.Text
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

  public void Recalculate()
  {
    float unit = _gridRenderer.LayoutSettings.WallThicknessPx;

    _innerRectCornerOffsets = [
      new Vector2(unit),
      new Vector2(-unit, unit),
      new Vector2(-unit, -unit),
      new Vector2(unit, -unit),
    ];

    _innerRectCornerOffsets = _gridRenderer.Transformer.Apply(_innerRectCornerOffsets);
  }
}
