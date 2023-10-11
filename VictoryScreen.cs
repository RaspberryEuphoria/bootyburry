using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public partial class VictoryScreen : Node2D
  {
    private IEnumerable<Direction> moves;
    private PackedScene nextLevel;
    private RichTextLabel label;

    public override void _Ready()
    {
      label = GetNode("CanvasLayer").GetNode("Control").GetNode<RichTextLabel>("RichTextLabel");
      label.Text = GetVictoryText();
    }

    public override void _Process(double delta)
    {
      if (Input.IsActionJustPressed("next_level") && nextLevel != null)
      {
        GetTree().ChangeSceneToPacked(nextLevel);
      }
    }

    public void Init(IEnumerable<Direction> moves, PackedScene nextLevel)
    {
      this.moves = moves;
      this.nextLevel = nextLevel;
    }

    private string GetVictoryText()
    {
      var report = $"You succesfuly burried your booty with {moves.Count()} moves!\n";

      if (nextLevel != null)
      {
        report += "Press \"Enter\" to chart the next sea. Yarrr!";
      }
      else
      {
        report += "This is the last sea (for now). Congratulations!";
      }

      return report;
    }
  }
}