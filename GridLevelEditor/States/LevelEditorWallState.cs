using System.Numerics;
using GridSystem;
using Raylib_cs;

namespace GridLevelEditor.States;
public class LevelEditorWallState(LevelEditor levelEditor) : LevelEditorState(levelEditor, "Wall edit")
{
  private Vector2[]? _hoveredTileCorners = null;

  Direction _localDirection;

  public override void Draw() { }

  public override void DrawUI() { }

  public void DrawHoveredWalls(GridLevel level)
  {
    if (_levelEditor.HoveredTile == null) return;
    if (_levelEditor.HoveredTile.GridLevel != level) return;

    bool[] walls = new bool[4];
    walls[(int)_localDirection] = true;
    if (_hoveredTileCorners != null)
    {
      // TODO: Get rid of specifying color as raylib format
      _levelEditor.Gd.DrawWalls(walls, _hoveredTileCorners, Color.Blue);
    }
  }

  public override void EnterState()
  {
    _levelEditor.Gd.DrawLevelExtras += _levelEditor.DrawHovered;
    _levelEditor.Gd.DrawLevelExtras += DrawHoveredWalls;
  }

  public override void ExitState()
  {
    _levelEditor.Gd.DrawLevelExtras -= _levelEditor.DrawHovered;
    _levelEditor.Gd.DrawLevelExtras -= DrawHoveredWalls;
  }

  public override void HandleHoveredTile()
  {
    if (_levelEditor.HoveredTile == null) return;
    _hoveredTileCorners = _levelEditor.Gd.GetTileCorners(_levelEditor.HoveredTile);

    _hoveredTileCorners = _levelEditor.Gd.ApplyTransformers(_hoveredTileCorners, _levelEditor.HoveredTile.GridLevel.LevelNum);

    if (_hoveredTileCorners != null)
    {
      // Get raw (untransformed) center-coords
      Vector2 centerPxCoords = _levelEditor.Gd.GetTileCenterPxCoords(
          (_levelEditor.HoveredTile.X, _levelEditor.HoveredTile.Y, 0)
        );
      // Untransform mouse position to get it into the same frame of reference
      Vector2 mPos = _levelEditor.Gd.ReverseTransformers(_levelEditor.MousePosition, _levelEditor.HoveredTile.GridLevel.LevelNum);

      _localDirection = _levelEditor.Gd.GetLocalTileDirectionOfScreenPoint(
        centerPxCoords,
        mPos
      );
    }
  }

  public override void HandleKeys()
  {

  }

  public override void HandleMouseClicks(bool left, bool right)
  {
    if (left)
    {
      _levelEditor.HoveredTile?.Blocking[(int)_localDirection] = !_levelEditor.HoveredTile.Blocking[(int)_localDirection];
      _levelEditor.Dirty = true;
    }
  }
}
