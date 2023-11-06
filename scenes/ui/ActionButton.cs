using Godot;

namespace UI
{
  public partial class ActionButton : Button
  {
    [Export]
    private bool autoFocus;

    public override void _Ready()
    {
      if (autoFocus) GrabFocus();
    }
  }
}