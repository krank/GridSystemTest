using System.Numerics;
using Raylib_cs;
namespace RaylibExt;

public class Lines
{
  public static void DrawRoundedLine(Vector2[] path, float thickness, Color color)
  {
    for (int i = 0; i < path.Length - 1; i++)
    {
      DrawRoundedLine(path[i], path[i + 1], thickness, color);
    }
  }

  public static void DrawRoundedLine(Vector2 from, Vector2 to, float thickness, Color color)
  {
    Raylib.DrawCircleV(from, thickness / 2, color);
    Raylib.DrawCircleV(to, thickness / 2, color);
    Raylib.DrawLineEx(from, to, thickness, color);
  }

  public static void DrawArrow(Vector2[] path,
    float thickness, Color color,
    bool triangleHead = false,
    float legLength = 20,
    float legAngle = .8f
  )
  {
    DrawRoundedLine(path[..^1], thickness, color);
    if (path.Length >= 2)
    {
      DrawArrow(path[^2], path[^1], thickness, color, triangleHead, legLength, legAngle);
    }
  }

  public static void DrawArrow(
    Vector2 from, Vector2 to,
    float thickness, Color color,
    bool triangleHead = false,
    float legLength = 20,
    float legAngle = .8f
  )
  {
    Vector2 diff = to - from;
    float radAngle = MathF.Atan2(diff.Y, diff.X) + MathF.PI;

    float leftAngle = radAngle + legAngle;
    float rightAngle = radAngle - legAngle;

    Vector2 leftPoint = new Vector2(
      MathF.Cos(leftAngle),
      MathF.Sin(leftAngle)
    ) * legLength;

    Vector2 rightPoint = new Vector2(
      MathF.Cos(rightAngle),
      MathF.Sin(rightAngle)
    ) * legLength;

    DrawRoundedLine(from, to, thickness, color);
    DrawRoundedLine(to, to + leftPoint, thickness, color);
    DrawRoundedLine(to, to + rightPoint, thickness, color);
    if (triangleHead)
    {
      DrawRoundedLine(to + leftPoint, to + rightPoint, thickness, color);
      Raylib.DrawTriangle(to, to + leftPoint, to + rightPoint, color);
    }

  }

  public static void DrawLineLabeled(Vector2 from, Vector2 to, float thickness, Color color, string label, float fontSize)
  {
    Raylib.DrawLineEx(from, to, thickness, color);
    Vector2 midPoint = from + ((to - from) / 2);
    Raylib.DrawCircleV(midPoint, 10, Color.Blue);

    Vector2 textSize = Raylib.MeasureTextEx(
      Raylib.GetFontDefault(),
      label, fontSize, fontSize / 10.5f
    );

    Rectangle textRect = new(midPoint - textSize / 2, textSize);
    Rectangle expandedRect = ExpandRectangle(textRect, new(5, 5));

    Raylib.DrawRectangleRec(expandedRect, Color.White);

    Raylib.DrawTextEx(
      Raylib.GetFontDefault(),
      label, textRect.Position, fontSize, fontSize / 10.5f, color
    );
  }

  public static Rectangle ExpandRectangle(Rectangle rect, Vector2 amount)
  {
    rect.Position -= amount;
    rect.Size += amount * 2;

    return rect;
  }

}
