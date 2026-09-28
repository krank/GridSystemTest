using System;

namespace GridSystem;

public interface IGridRenderer
{
  public void DrawTile(GridTile tile);
  public void DrawGrid();
}
