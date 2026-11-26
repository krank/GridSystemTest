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

// Grid grid = new(5, 6, 6);
Grid grid = new(2, 2, 1);

Camera2D camera = new()
{
  Zoom = 1f,
  Target = Raylib.GetScreenCenter(),
  Offset = Raylib.GetScreenCenter()
};

RaylibCameraControl cameraControl = new();

GridRendererRaylib gd = new(grid)
{
  LayoutSettings = new()
  {
    GridTileSize = 64,
    GridSpacing = .5f
  },
  // Transformer = new IsoTransform() { Ratio = .5f },
  Transformer = new DummyTransformer(),
  LevelOffsetTransformer = new OffsetTransform() { Offset = new(0, -96f) }
};

// gd.CenterOnScreen();
gd.Recalculate();

LevelEditor levelEditor = new(grid, gd);

Color bkg = Color.White;

Texture2D bkgTexture = GenerateBackground(screenWidth, screenHeight);

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
  Raylib.DrawTexture(bkgTexture, 0, 0, Color.White);



  Raylib.BeginMode2D(camera);
  gd.DrawGrid();

  levelEditor.Draw();

  Raylib.EndMode2D();
  levelEditor.DrawUI();

  Raylib.EndDrawing();
}







static Texture2D GenerateBackground(int screenWidth, int screenHeight)
{
  Color bkgColor = new(250, 250, 250);
  Color pen = new(230, 230, 230);
  Image tile = Raylib.GenImageColor(64, 64, bkgColor);
  Raylib.ImageDrawLine(ref tile, 0, 8, 17, 8, pen);
  Raylib.ImageDrawLine(ref tile, 8, 0, 8, 17, pen);



  Image bkg = Raylib.GenImageColor(screenWidth, screenHeight, Color.RayWhite);
  Rectangle src = new(Vector2.Zero, tile.Dimensions);
  Rectangle dst = new(Vector2.Zero, tile.Dimensions);

  for (dst.X = 0; dst.X < bkg.Width; dst.X += dst.Width)
  {
    for (dst.Y = 0; dst.Y < bkg.Height; dst.Y += dst.Height)
    {
      Raylib.ImageDraw(ref bkg, tile, src, dst, Color.White);
    }
  }

  return Raylib.LoadTextureFromImage(bkg);
}