using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public partial class LosingScreen : Node2D
  {
    private IEnumerable<Direction> moves;
    private PackedScene nextLevel;
    private RichTextLabel label;

    public override void _Ready()
    {
      label = GetNode("CanvasLayer").GetNode("Control").GetNode<RichTextLabel>("RichTextLabel");
      label.Text = GetVictoryText();
    }

    public void Init(IEnumerable<Direction> moves, PackedScene nextLevel)
    {
      this.moves = moves;
      this.nextLevel = nextLevel;
    }

    private string GetVictoryText()
    {
      var report = $"The sea is harsh and unforgiving. Your ship sank after {moves.Count()} moves...\n";
      report += "Press \"R\" to retry!";

      return report;
    }
  }
}