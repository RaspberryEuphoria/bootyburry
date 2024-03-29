using System;
using Game;
using Godot;

namespace UI
{
  public partial class VictoryUI : CanvasLayer
  {
    private Button retryButton;
    private KamiButton nextButton;

    private Level level;
    private int movesCount = 0;
    private PackedScene nextLevel;

    public override void _Ready()
    {
      var completedLabel = GetNode<KamiLabel>("%CompletedLabel");
      completedLabel.Text = completedLabel.Text.Replace("{{movesCount}}", movesCount.ToString());

      retryButton = GetNode<Button>("%RetryButton");
      nextButton = GetNode<KamiButton>("%NextButton");

      retryButton.Pressed += OnRetry;
      nextButton.Pressed += OnNext;

      level = GetParent<Level>();
    }

    public void Init(PackedScene nextLevel, int movesCount)
    {
      this.movesCount = movesCount;
      this.nextLevel = nextLevel;
    }

    private void OnRetry()
    {
      GetTree().ReloadCurrentScene();
    }

    private void OnNext()
    {
      try
      {
        GetTree().ChangeSceneToPacked(nextLevel);
      }
      catch (Exception e)
      {
        GD.PrintErr(e.Message);
      }
    }
  }
}