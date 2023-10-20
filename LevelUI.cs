using Godot;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    private Game.Board board;
    private Label scoreLabel;
    private int score = 0;

    public override void _Ready()
    {
      scoreLabel = GetNode<Label>("%ScoreLabel");

      board = GetParent<Game.Board>();
      board.PlayerMoved += OnPlayerMoved;
    }

    public void OnPlayerMoved()
    {
      score++;
      scoreLabel.Text = score.ToString();
    }
  }
}