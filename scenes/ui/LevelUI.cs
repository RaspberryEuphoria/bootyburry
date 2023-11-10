
using Game;
using Godot;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    [Export(PropertyHint.MultilineText)]
    private string helperText;

    private Control helperControl;
    private Level level;
    private Label levelTitleLabel;
    private Label synchronizationLabel;
    private Label activeCoresLabel;
    private Label remainingCoresLabel;
    private Label actualComputationsLabel;
    private Label optimalComputationsLabel;

    public override void _Ready()
    {
      // var medalsContainer = GetNode<BoxContainer>("%MedalsContainer");
      // medals = medalsContainer.GetChildren().OfType<MedalWithScore>();

      // helperControl = GetNode<Control>("%HelperControl");
      // scoreLabel = GetNode<Label>("%ScoreLabel");
      // shipWheel = GetNode<Sprite2D>("%ShipWheel");
      // navigationPath = GetNode<NavigationPath>("%NavigationPath");

      level = GetParent<Level>();
      level.PlayerMoved += OnPlayerMoved;

      levelTitleLabel = GetNode<Label>("%LevelTitleLabel");
      synchronizationLabel = GetNode<Label>("%SynchronizationLabel");
      activeCoresLabel = GetNode<Label>("%ActiveCoresLabel");
      remainingCoresLabel = GetNode<Label>("%RemainingCoresLabel");
      actualComputationsLabel = GetNode<Label>("%ActualComputationsLabel");
      optimalComputationsLabel = GetNode<Label>("%OptimalComputationsLabel");

      // SetupHelperText();
      // SetupMedals();
    }

    public override void _Process(double delta)
    {

    }

    private void SetupHelperText()
    {
      var helpLabel = helperControl.GetNode<RichTextLabel>("%HelpLabel");
      if (helperText == null || helperText.Trim() == "")
      {
        helpLabel.QueueFree();
        return;
      }

      helpLabel.Visible = true;
      helpLabel.Text = helperText;
    }

    private void OnPlayerMoved(int score)
    {
      scoreLabel.Text = score.ToString();
    }
  }
}