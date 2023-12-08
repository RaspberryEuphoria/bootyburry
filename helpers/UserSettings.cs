using Godot;

namespace Helpers
{
  public partial class UserSettings : Node
  {
    [Signal]
    public delegate void UIScaleChangeEventHandler(float scale);

    public float UIScale { get; private set; } = 1f;
    private ConfigFile config = new();

    public override void _Ready()
    {
      config.Load("user://settings.cfg");
      UIScale = (float)config.GetValue("settings", "ui_scale");
    }

    public void SetUIScale(float scale)
    {
      if (Device.IsMobile()) scale *= 1.5f;

      UIScale = scale;

      config.SetValue("settings", "ui_scale", scale);
      config.Save("user://settings.cfg");

      EmitSignal(SignalName.UIScaleChange, scale);
    }
  }
}