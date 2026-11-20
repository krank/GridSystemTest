using System.Numerics;
using GridSystem;
using Raylib_cs;

namespace GridLevelEditor;

public class LevelEditorWallState(LevelEditor levelEditor) : LevelEditorState(levelEditor, "Wall edit")
{
  private Vector2[]? _hoveredTileCorners = null;

  Direction _localDirection;

  public override void Draw()
  {
    if (_levelEditor.HoveredTile == null) return;

    _levelEditor.DrawHovered();

    bool[] walls = new bool[4];
    walls[(int)_localDirection] = true;
    if (_hoveredTileCorners != null)
    {
      // TODO: Get rid of specifying color as raylib format
      _levelEditor.Gd.DrawWalls(walls, _hoveredTileCorners, Color.Blue);
    }
  }

  public override void DrawUI()
  {

  }

  public override void HandleHoveredTile()
  {
    if (_levelEditor.HoveredTile == null) return;
    _hoveredTileCorners = _levelEditor.Gd.GetTileCorners(_levelEditor.HoveredTile);

    _hoveredTileCorners = _levelEditor.Gd.ApplyTransformers(_hoveredTileCorners, _levelEditor.HoveredTile.GridLevel.LevelNum);

    if (_hoveredTileCorners != null)
    {
      Vector2 mPos = _levelEditor.Gd.ReverseTransformers(_levelEditor.MousePosition, _levelEditor.HoveredTile.GridLevel.LevelNum);

      // TODO: For the love of god clean this up
      _localDirection = _levelEditor.Gd.GetLocalTileDirectionOfScreenPoint(
        _levelEditor.Gd.GetTileCenterPxCoords(
          (_levelEditor.HoveredTile.X, _levelEditor.HoveredTile.Y, 0)
        ),
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
