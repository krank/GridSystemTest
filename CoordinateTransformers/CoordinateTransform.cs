using System.Numerics;

namespace CoordinateTransformers;

public interface CoordinateTransform
{
  public Vector2 Apply(Vector2 original);
  public Vector2[] Apply(Vector2[] originals);

  public Vector2 Reverse(Vector2 original);
  public Vector2[] Reverse(Vector2[] originals);
}
