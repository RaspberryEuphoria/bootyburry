using System.Collections.Generic;
using System.Linq;
using Game;
using Godot;

namespace UI
{
  public partial class VictoryUI : CanvasLayer
  {
    [Export]
    private Button retryButton;
    [Export]
    private Button nextButton;
    private IEnumerable<MedalWithScore> medals;

    private Board board;
    private int movesCount = 0;
    private PackedScene nextLevel;

    public override void _Ready()
    {
      var completedLabel = GetNode<RichTextLabel>("%CompletedLabel");
      completedLabel.Text = completedLabel.Text.Replace("{{movesCount}}", movesCount.ToString());

      retryButton.Pressed += OnRetry;
      nextButton.Pressed += OnNext;

      var medalsContainer = GetNode<BoxContainer>("%MedalsContainer");
      medals = medalsContainer.GetChildren().OfType<MedalWithScore>();

      board = GetParent<Board>();

      SetupMedals();
    }

    public void Init(PackedScene nextLevel, int movesCount)
    {
      this.movesCount = movesCount;
      this.nextLevel = nextLevel;
    }

    private void SetupMedals()
    {

    }

    private void OnRetry()
    {
      GetTree().ReloadCurrentScene();
    }

    private void OnNext()
    {
      GetTree().ChangeSceneToPacked(nextLevel);
    }
  }
}