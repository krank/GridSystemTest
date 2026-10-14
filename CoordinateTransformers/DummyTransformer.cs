using System.Numerics;

namespace CoordinateTransformers;

public class DummyTransformer : CoordinateTransform
{
  public Vector2 Apply(Vector2 original) => original;
  public Vector2[] Apply(Vector2[] originals) => [.. originals];
  public Vector2 Reverse(Vector2 original) => original;
  public Vector2[] Reverse(Vector2[] originals) => [.. originals];
}
