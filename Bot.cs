using System.Collections.Generic;
using System.Linq;
using Godot;
using Helpers;

namespace Game
{
  public partial class Bot : TextEdit
  {
    [Export]
    public int maxTries = 50;
    [Export]
    public bool disabled = true;

    private Level level;

    public override void _Ready()
    {
      GD.Print("Bot ready");
      if (disabled)
      {
        Visible = false;
        QueueFree();
        return;
      }

      RenderingServer.RenderLoopEnabled = false;
      AudioServer.SetBusMute(AudioServer.GetBusIndex("Master"), true);

      level = GetParent<Level>();
    }

    public override void _Process(double delta)
    {
      if (disabled) return;
      if (!level.IsReady) return;
      if (level.GameState == GameState.Won) return;

      if (level.GameState == GameState.Lost)
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      var randomAction = GetRandomAction();
      if (randomAction == null)
      {
        GetTree().ReloadCurrentScene();
        return;
      }

      level.TriggerInputInDirection(level.ActionToDirection[randomAction]);

      if (level.Moves.Count() >= maxTries)
      {
        GetTree().ReloadCurrentScene();
        return;
      }
    }

    private string GetRandomAction()
    {
      var availableActions = level.GetAvailableActions();
      if (availableActions.Count == 0) return null;
      var randomAction = availableActions.ElementAt((System.Index)(GD.Randi() % availableActions.Count));
      return randomAction;
    }
  }
}