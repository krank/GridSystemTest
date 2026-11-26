using Raylib_cs;
namespace GridRenderer;

public class GridColorTheme
{
  public Color Background { get; set; } = new(210, 210, 210);
  public Color Wall { get; set; } = Color.Black;
  public Color Tile { get; set; } = Color.LightGray;
  public Color Text { get; set; } = Color.Black;
}
