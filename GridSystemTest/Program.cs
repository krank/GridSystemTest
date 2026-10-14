using System.Numerics;
using GridSystem;
using GridSystemTest.LevelEditor;
using CoordinateTransformers;
using Raylib_cs;
using GridRenderer;



// https://www.youtube.com/watch?v=04oQ2jOUjkU
// https://medium.com/@kavierim/demystifying-isometric-projection-in-2d-games-with-python-bbcc2038a620

/*
NEXT STEPS
Renderer
 - Completely separate out the Raylib stuff into a RaylibAdapter (that can be switched out)
 - Rendering Graph connections between levels

Editor
 - Allow deletion (nulling)

Grid
 - Multiple levels
 - Graphing connections between levels 
   (Add Actor interface, where Actor decides if it's able to climb a certain height.
    Evaluate(Vertex from, Vertex to) -> Actor does type checking and checks heights etc
    To be decided when pathfinding, not when graphing! The connection's there!)

Systems
 - Pathfinders

Problems for Future-me
 - Doors; walls that are special (not just blocker)
*/


// TODO: Caching of all the various calculated points...


Vector2[] test = [
  new(200,200),
  new(200,100),
  new(100,200),
  new(100,100),
];

int screenWidth = 1024;
int screenHeight = 768;
float cameraSpeed = 150;

Raylib.InitWindow(screenWidth, screenHeight, "Title");
Raylib.SetTargetFPS(60);

Grid grid = new(5, 6);

Camera2D camera = new()
{
  Zoom = 1f,
  Target = Raylib.GetScreenCenter(),
  Offset = Raylib.GetScreenCenter()
};


IsoGridRendererRaylib gd = new(grid) { GridTileSize = 64, GridSpacing = .25f };

gd.Transformer = new IsoTransform() { Ratio = .5f };
// gd.Transformer = new DummyTransformer();

// gd.FitToScreen(); // TODO: Find rectangle of a list of points
gd.CenterOnScreen();

LevelEditor levelEditor = new(grid, gd);

while (!Raylib.WindowShouldClose())
{
  float deltaTime = Raylib.GetFrameTime();

  if (Raylib.IsKeyDown(KeyboardKey.Right)) camera.Offset.X -= cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Left)) camera.Offset.X += cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Up)) camera.Offset.Y += cameraSpeed * deltaTime;
  if (Raylib.IsKeyDown(KeyboardKey.Down)) camera.Offset.Y -= cameraSpeed * deltaTime;

  Vector2 mPos = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), camera);

  if (Raylib.IsKeyPressed(KeyboardKey.E))
    levelEditor.Enabled = !levelEditor.Enabled;

  levelEditor.Update(mPos);

  // -- DRAWING

  Raylib.BeginDrawing();
  Raylib.ClearBackground(Color.White);

  Raylib.BeginMode2D(camera);
  gd.DrawGrid();

  levelEditor.Draw();


  Raylib.EndMode2D();
  levelEditor.DrawUI();

  Raylib.EndDrawing();
}