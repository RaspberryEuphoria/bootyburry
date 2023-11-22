using System.Linq;
using Godot;

namespace Game
{
  public partial class Bot : TextEdit
  {
    [Export]
    public int maxTries = 50;
    [Export]
    public float delay = 0.5f;
    [Export]
    public bool disabled = true;

    private Level level;
    private string lastAction;
    private Timer timer = null;
    private string thinking = ".";

    public override void _Ready()
    {
      if (disabled)
      {
        Visible = false;
        return;
      }

      var viewportWidth = GetViewportRect().Size.X;
      var viewportHeight = GetViewportRect().Size.Y;
      GlobalPosition = new Vector2(viewportWidth / 2 - Size.X / 2, viewportHeight - Size.Y * 2);

      level = GetParent<Level>();
      timer = new Timer
      {
        OneShot = true
      };

      AddChild(timer);
      timer.Start(delay);
    }

    public override void _Process(double delta)
    {
      if (disabled) return;
      if (!level.IsReady || (delay > 0 && !timer.IsStopped())) return;

      if (level.GameState == GameState.Won)
      {
        Text = "Level complete! Moves (" + level.Moves.Count() + "): \n";

        for (int i = 0; i < level.Moves.Count(); i++)
        {
          Text += MoveToArrowEmoji(level.Moves.ElementAt(i)) + " ";
        }

        return;
      }

      if (level.GameState == GameState.Lost)
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      var availableActions = level.GetAvailableActions();
      var randomAction = availableActions.ElementAt((System.Index)(GD.Randi() % availableActions.Count));

      timer.Start(delay);

      var press = new InputEventAction
      {
        Action = randomAction,
        Pressed = true
      };
      Input.ParseInputEvent(press);

      Text = "Last action: " + lastAction;
      Text += "\nCurrent action: " + randomAction;
      Text += "\n" + thinking;

      thinking += ".";
      if (thinking == "......") thinking = ".";
      var release = new InputEventAction
      {
        Action = randomAction,
        Pressed = false
      };
      Input.ParseInputEvent(release);

      lastAction = randomAction;

      if (level.Moves.Count() >= maxTries)
      {
        GetTree().ReloadCurrentScene();
        return;
      }
    }

    public static string MoveToArrowEmoji(Direction direction)
    {
      return direction switch
      {
        Direction.Up => "⬆️",
        Direction.Down => "⬇️",
        Direction.Left => "⬅️",
        Direction.Right => "➡️",
        _ => "Unsupported direction: " + direction.ToString()
      };
    }
  }
}