using GridSystem;
using GridSystemTest;
using Raylib_cs;

Raylib.InitWindow(1024, 768, "Title");
Raylib.SetTargetFPS(60);

Grid grid = new(10, 7);

GridRendererRaylib gd = new(grid);
gd.FitToScreen();
gd.CenterOnScreen();

LevelEditor levelEditor = new(grid, gd);

while (!Raylib.WindowShouldClose())
{
  levelEditor.Update();

  if (Raylib.IsKeyPressed(KeyboardKey.E))
    levelEditor.Enabled = !levelEditor.Enabled;

  Raylib.BeginDrawing();
  Raylib.ClearBackground(Color.White);

  gd.DrawGrid();
  levelEditor.Draw();

  Raylib.EndDrawing();
}