using System.Numerics;
using Raylib_cs;

namespace GridSystemTest;

public class RaylibCameraControl()
{
  public float CameraSpeed { get; set; } = 100;

  public void Update(ref Camera2D camera, float deltaTime)
  {
    if (Raylib.IsKeyDown(KeyboardKey.Right)) camera.Offset.X -= CameraSpeed * deltaTime;
    if (Raylib.IsKeyDown(KeyboardKey.Left)) camera.Offset.X += CameraSpeed * deltaTime;
    if (Raylib.IsKeyDown(KeyboardKey.Up)) camera.Offset.Y += CameraSpeed * deltaTime;
    if (Raylib.IsKeyDown(KeyboardKey.Down)) camera.Offset.Y -= CameraSpeed * deltaTime;

    if (Raylib.IsMouseButtonDown(MouseButton.Middle))
    {
      Vector2 mDelta = Raylib.GetMouseDelta();
      camera.Offset += mDelta;
    }
  }
}
