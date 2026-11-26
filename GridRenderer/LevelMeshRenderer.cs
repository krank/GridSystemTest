using System.Numerics;
using Raylib_cs;
using RaylibExt;

namespace GridRenderer;

public class LevelMeshRenderer(GridRendererRaylib gridRenderer)
{
  private readonly GridRendererRaylib _gridRenderer = gridRenderer;

  private readonly List<Vector2[]> _cacheBackgroundSpacingCorners = [];

  public void Recalculate()
  {
    // TODO: Moar comments here bitte
    // TODO: Add level awareness; different caches for each level
    Vector2[] corners = _gridRenderer.ApplyTransformers(_gridRenderer.GetCorners(0), 0);
    _cacheBackgroundSpacingCorners.Clear();

    Vector2 spacingOffsetV = _gridRenderer.Transformer.Apply(new Vector2(0, _gridRenderer.LayoutSettings.GridSpacingPx));
    Vector2 tileOffsetV = _gridRenderer.Transformer.Apply(new Vector2(0, _gridRenderer.LayoutSettings.GridTileSize));
    Vector2 spacingOffsetH = _gridRenderer.Transformer.Apply(new Vector2(_gridRenderer.LayoutSettings.GridSpacingPx, 0));
    Vector2 tileOffsetH = _gridRenderer.Transformer.Apply(new Vector2(_gridRenderer.LayoutSettings.GridTileSize, 0));

    // Initial corners for Vertical
    Vector2[] crn = [corners[0], corners[1],
      corners[1] + spacingOffsetV / 2,
      corners[0] + spacingOffsetV / 2
    ];

    _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);

    for (int i = 0; i < 2; i++) crn[i] -= spacingOffsetV / 2;

    for (int y = 0; y < _gridRenderer.Grid.Height; y++)
    {
      // Move all corners one step
      for (int i = 0; i < 4; i++) crn[i] += tileOffsetV + spacingOffsetV;
      // Decrease the distance between corners for the last line
      if (y == _gridRenderer.Grid.Height - 1)
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

    for (int x = 0; x < _gridRenderer.Grid.Width; x++)
    {
      // Move all corners one step
      for (int i = 0; i < 4; i++) crn[i] += tileOffsetH + spacingOffsetH;
      // Decrease the distance between corners for the last line
      if (x == _gridRenderer.Grid.Width - 1)
      {
        crn[1] -= spacingOffsetH / 2;
        crn[2] -= spacingOffsetH / 2;
      }

      _cacheBackgroundSpacingCorners.Add([.. crn.Reverse()]);
    }
  }

  public void Draw(Color color, int level)
  {
    color = ColorUtil.Brighten(color, 1 + (level * 0.1f));
    List<Vector2[]> localCornerGroups = [.. _cacheBackgroundSpacingCorners];

    for (int i = 0; i < localCornerGroups.Count; i++)
    {
      localCornerGroups[i] = _gridRenderer.LevelOffsetTransformer.Apply(localCornerGroups[i], level);
    }

    foreach (Vector2[] crn in localCornerGroups)
    {
      Raylib.DrawTriangleFan(crn, crn.Length, color);
      // Raylib.DrawCircleV(crn[0], 5, Color.Green);
    }
  }

}
