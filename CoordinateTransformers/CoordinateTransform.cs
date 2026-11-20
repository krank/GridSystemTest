using System.Numerics;

namespace CoordinateTransformers;

public interface ICoordinateTransform
{
  public Vector2 Apply(Vector2 original);
  public Vector2 Apply(Vector2 original, int times)
  {
    for (int i = 0; i < times; i++)
    {
      original = Apply(original);
    }
    return original;
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
  public Vector2[] Apply(Vector2[] originals, int times)
  {
    for (int i = 0; i < times; i++)
    {
      originals = Apply(originals);
    }
    return originals;
  }

  public Vector2 Reverse(Vector2 original);
  public Vector2 Reverse(Vector2 original, int times)
  {
    for (int i = 0; i < times; i++)
    {
      original = Reverse(original);
    }
    return original;
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
  public Vector2[] Reverse(Vector2[] originals, int times)
  {
    for (int i = 0; i < times; i++) originals = Reverse(originals);
    return originals;
  }
}
