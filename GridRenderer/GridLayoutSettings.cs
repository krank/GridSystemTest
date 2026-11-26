using System.Numerics;
using GridSystem;

namespace GridRenderer;

public class GridLayoutSettings
{
  public float GridTileSize { get; set; } = 32;
  public Vector2 HalfSizeOffset => Vector2.One * GridTileSize / 2;

  public float WallThickness { set; get; } = .1f;
  public float WallThicknessPx { get => WallThickness * GridTileSize; }

  public float GridSpacing { get; set; } = .1f;
  public float GridSpacingPx => GridSpacing * GridTileSize;
  public Vector2 HalfSpacingOffset => Vector2.One * GridSpacingPx / 2;

  public float GridTileSizeWithSpacing => GridTileSize + GridSpacingPx;

  public float FontSize => GridTileSize / 2;
  public float FontSpacing => GridTileSize / 2;

  public Vector2 GetTileCenterPxCoords((int x, int y, int level) coord)
  {
    return new Vector2(
      coord.x * GridTileSizeWithSpacing,
      coord.y * GridTileSizeWithSpacing
    ) + HalfSpacingOffset + HalfSizeOffset;
  }

  public Vector2[] GetTileCorners(GridTile tile)
  {
    float left = tile.X * GridTileSizeWithSpacing + HalfSpacingOffset.X;
    float top = tile.Y * GridTileSizeWithSpacing + HalfSpacingOffset.Y;
    float right = left + GridTileSize;
    float bottom = top + GridTileSize;

    return [ // COUNTERCLOCKWISE
      new(left, top),
      new(right, top),
      new(right, bottom),
      new(left, bottom),
    ];
  }
}
