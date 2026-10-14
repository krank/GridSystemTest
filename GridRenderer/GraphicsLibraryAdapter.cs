using System.Numerics;
using Raylib_cs;

namespace GridRenderer;

public interface GraphicsLibraryAdapter
{
  public void DrawLine(Vector2 from, Vector2 to, float thickness);
  public void DrawQuad(Vector2 corners);
  public void DrawText();
  public void DrawCircle();
}
