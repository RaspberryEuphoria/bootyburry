using Game;
using Godot;

namespace InputHandler
{
  public partial class TouchScreenHandler : Node2D
  {
    private Level level;
    private InputEventScreenDrag startingDragEvent;
    private readonly int threeshold = 64 * 2;
    private bool isInputAllowed = true;

    public override void _Ready()
    {
      level = GetParent<Level>();
    }

    public override void _Input(InputEvent @event)
    {
      if (!level.IsInputAllowed()) return;

      if (@event is InputEventScreenDrag dragEvent && isInputAllowed)
      {
        if (startingDragEvent == null)
        {
          startingDragEvent = dragEvent;
          return;
        }

        var currentPosition = dragEvent.Position;
        var startingPosition = startingDragEvent.Position;
        var delta = currentPosition - startingPosition;

        Direction? direction = delta switch
        {
          var d when d.X >= threeshold => Direction.Right,
          var d when d.X <= -threeshold => Direction.Left,
          var d when d.Y >= threeshold => Direction.Down,
          var d when d.Y <= -threeshold => Direction.Up,
          _ => null
        };

        if (direction is not null)
        {
          level.TriggerInputInDirection((Direction)direction);
          startingDragEvent = null;
          isInputAllowed = false;
        }
      }

      if (@event is InputEventScreenTouch && !isInputAllowed)
      {
        startingDragEvent = null;
        isInputAllowed = true;
      }
    }
  }
}