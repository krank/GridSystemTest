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

  public override string ToString()
  {
    return $"Tile at [x:{X} y:{Y}], " +
      "walls [" +
        $"{(Blocking[(int)Direction.North] ? "N" : "")}" +
        $"{(Blocking[(int)Direction.East] ? "E" : "")}" +
        $"{(Blocking[(int)Direction.South] ? "S" : "")}" +
        $"{(Blocking[(int)Direction.West] ? "W" : "")}" +
      "]";
  }
}