using System.Numerics;
using GridSystem;
using CoordinateTransformers;
using Raylib_cs;
using GridRenderer;
using GridLevelEditor;
using GridSystemTest;
using Wacton.Unicolour;


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

// TODO: Implement Unicolours everywhere, as part of moving from hardcoded Raylib stuff in the renderer


int screenWidth = 1280;
int screenHeight = 900;

Raylib.InitWindow(screenWidth, screenHeight, "Title");
Raylib.SetTargetFPS(60);

// Grid grid = new(5, 6, 5);
Grid grid = new(2, 2, 2);
var graph = GridGraphConverter.GridGraphConverter.ConvertGridToGraph(grid);

Camera2D camera = new()
{
  Zoom = 1f,
  Target = Raylib.GetScreenCenter(),
  Offset = Raylib.GetScreenCenter()
};

RaylibCameraControl cameraControl = new();

GridRendererRaylib gd = new(grid)
{
  GridTileSize = 64,
  GridSpacing = .05f,
  Transformer = new IsoTransform() { Ratio = .5f }
};

gd.CenterOnScreen();
gd.Recalculate();

((OffsetTransform)gd.LevelOffsetTransformer).Offset = new(0, -96);

LevelEditor levelEditor = new(grid, gd);

Color bkg = Color.White;

while (!Raylib.WindowShouldClose())
{
  float deltaTime = Raylib.GetFrameTime();

  cameraControl.Update(ref camera, deltaTime);

  if (Raylib.GetMouseWheelMove() > 0) gd.LevelsToDisplay++;
  if (Raylib.GetMouseWheelMove() < 0) gd.LevelsToDisplay--;

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