namespace GridSystem;

public class GridTile(
  int x, int y, int level,
  bool blockN = false,
  bool blockE = false,
  bool blockS = false,
  bool blockW = false
  )
{
  public int X { get; set; } = x;
  public int Y { get; set; } = y;
  public int Level { get; set; } = level;

  public bool[] Blocking { get; set; } = [blockN, blockE, blockS, blockW];
}