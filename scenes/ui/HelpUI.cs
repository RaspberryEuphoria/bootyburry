using Godot;

namespace UI
{
  public partial class HelpUI : CanvasLayer
  {
    private KamiButton closeButton;

    public override void _Ready()
    {
      closeButton = GetNode<KamiButton>("%CloseButton");
      closeButton.ButtonUp += Close;
    }

    private void Close()
    {
      QueueFree();
    }
  }
}