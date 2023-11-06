
using System.Collections.Generic;
using System.Linq;
using Game;
using Godot;

namespace UI
{
  public partial class LevelUI : CanvasLayer
  {
    [Export(PropertyHint.MultilineText)]
    private string helperText;

    private Board board;
    private NavigationPath navigationPath;
    private Control helperControl;
    private IEnumerable<MedalWithScore> medals;
    private Label scoreLabel;
    private Sprite2D shipWheel;
    private float rotationSpeed = 2f;
    private bool isRotating = false;

    public override void _Ready()
    {
      var medalsContainer = GetNode<BoxContainer>("%MedalsContainer");
      medals = medalsContainer.GetChildren().OfType<MedalWithScore>();

      helperControl = GetNode<Control>("%HelperControl");
      scoreLabel = GetNode<Label>("%ScoreLabel");
      shipWheel = GetNode<Sprite2D>("%ShipWheel");
      navigationPath = GetNode<NavigationPath>("%NavigationPath");

      board = GetParent<Board>();
      board.PlayerMoved += OnPlayerMoved;

      SetupHelperText();
      SetupMedals();
    }

    public override void _Process(double delta)
    {
      if (!isRotating) return;

      shipWheel.RotationDegrees += 360 * (float)delta * rotationSpeed;

      if (shipWheel.RotationDegrees >= 360)
      {
        isRotating = false;
        shipWheel.RotationDegrees = 0;
      }
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

    private void SetupMedals()
    {
      if (board.medals.Length != medals.Count())
      {
        GD.PrintErr($"Medals count ({medals.Count()}) in LevelUI does not match the medals count ({board.medals.Length}) on Board.");
        return;
      }

      for (int i = 0; i < medals.Count(); i++)
      {
        var medal = medals.ElementAt(i);

        if (board.medals[i] == 0)
        {
          medal.Remove();
          continue;
        }

        medal.Init(board.medals[i]);
        board.PlayerMoved += medal.OnPlayerMoved;
      }
    }

    private void OnPlayerMoved(int score)
    {
      scoreLabel.Text = score.ToString();
      isRotating = true;
      navigationPath.Points = new Vector2[] { navigationPath.Points[1], navigationPath.Points[0] };
    }
  }
}