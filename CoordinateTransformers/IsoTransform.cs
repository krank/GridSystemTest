using System.Numerics;

namespace CoordinateTransformers;

public class IsoTransform : CoordinateTransform
{
  public float Ratio { get; set; }

  public Vector2 Apply(Vector2 original)
  {
    return new(
      original.X - original.Y,
      (original.X + original.Y) * Ratio
    );
  }

  public Vector2[] Apply(Vector2[] originals)
  {
    Vector2[] modified = new Vector2[originals.Length];
    for (int i = 0; i < originals.Length; i++)
    {
      modified[i] = Apply(originals[i]);
    }
    return modified;
  }

  public Vector2 Reverse(Vector2 original)
  {
    return new(
      (original.X + original.Y / Ratio) / 2,
      (original.Y / Ratio - original.X) / 2
    );
  }

  public Vector2[] Reverse(Vector2[] originals)
  {
    Vector2[] modified = new Vector2[originals.Length];
    for (int i = 0; i < originals.Length; i++)
    {
      modified[i] = Reverse(originals[i]);
    }
    return modified;
  }
}
