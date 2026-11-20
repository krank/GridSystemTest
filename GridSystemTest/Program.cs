using System.Numerics;
using GridSystem;
using CoordinateTransformers;
using Raylib_cs;
using GridRenderer;
using GridLevelEditor;



// https://www.youtube.com/watch?v=04oQ2jOUjkU
// https://medium.com/@kavierim/demystifying-isometric-projection-in-2d-games-with-python-bbcc2038a620

/*
NEXT STEPS
Renderer
 - Completely separate out the Raylib stuff into a RaylibAdapter (that can be switched out)
 - Rendering Graph connections between levels

Grid
 - Graphing connections between levels 
   (Add Actor interface, where Actor decides if it's able to climb a certain height.
    Evaluate(Vertex from, Vertex to) -> Actor does type checking and checks heights etc
    To be decided when pathfinding, not when graphing! The connection's there!)

Systems
 - Pathfinders

Problems for Future-me
 - Doors; walls that are special (not just blocker)
*/


// TODO: Caching of all the various calculated points...?

int screenWidth = 1024;
int screenHeight = 768;
float cameraSpeed = 150;

Raylib.InitWindow(screenWidth, screenHeight, "Title");
Raylib.SetTargetFPS(60);

Grid grid = new(5, 6, 5);

Camera2D camera = new()
{
  Zoom = 1f,
  Target = Raylib.GetScreenCenter(),
  Offset = Raylib.GetScreenCenter()
};


GridRendererRaylib gd = new(grid)
{
  GridTileSize = 64,
  GridSpacing = .05f,
  Transformer = new IsoTransform() { Ratio = .5f }
};

gd.CenterOnScreen();
gd.Recalculate();

((OffsetTransform)gd.LevelOffsetTransformer).Offset = new(0, -96);

LevelEditor levelEditor = new(grid.Levels[0], gd);

Color bkg = Color.White;

while (!Raylib.WindowShouldClose())
{
  float deltaTime = Raylib.GetFrameTime();

  // TODO: Refactor into actions?
  if (Raylib.IsKeyDown(KeyboardKey.Right)) camera.Offset.X -= cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Left)) camera.Offset.X += cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Up)) camera.Offset.Y += cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Down)) camera.Offset.Y -= cameraSpeed * deltaTime;

  if (Raylib.GetMouseWheelMove() > 0) gd.LevelsToDisplay++;
  if (Raylib.GetMouseWheelMove() < 0) gd.LevelsToDisplay--;

  if (Raylib.IsMouseButtonDown(MouseButton.Middle))
  {
    Vector2 mDelta = Raylib.GetMouseDelta();
    camera.Offset += mDelta;
  }

  Vector2 mPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

  if (Raylib.IsKeyPressed(KeyboardKey.E))
    levelEditor.Enabled = !levelEditor.Enabled;

  levelEditor.Update(mPos);

  // -- DRAWING

  Raylib.BeginDrawing();
  Raylib.ClearBackground(bkg);

  Raylib.BeginMode2D(camera);
  gd.DrawGrid();

  levelEditor.Draw();

  Raylib.EndMode2D();
  levelEditor.DrawUI();

  Raylib.EndDrawing();
}