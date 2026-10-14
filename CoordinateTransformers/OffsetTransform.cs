using System.Numerics;

namespace CoordinateTransformers;

public class OffsetTransform : CoordinateTransform
{
  public Vector2 Offset { get; set; }

  public Vector2 Apply(Vector2 original)
  {
    return original += Offset;
  }

  public Vector2[] Apply(Vector2[] originals)
  {
    Vector2[] modified = new Vector2[originals.Length];
    for (int i = 0; i < originals.Length; i++)
    {
      modified[i] = originals[i] + Offset;
    }
    return modified;
  }

  public Vector2 Reverse(Vector2 original)
  {
    return original -= Offset;
  }

  public Vector2[] Reverse(Vector2[] originals)
  {
    Vector2[] modified = new Vector2[originals.Length];
    for (int i = 0; i < originals.Length; i++)
    {
      modified[i] = originals[i] + Offset;
    }
    return modified;
  }
}
