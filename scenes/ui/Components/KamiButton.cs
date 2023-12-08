using Godot;
using Helpers;

namespace UI
{
  [Tool]
  public partial class KamiButton : Button
  {
    [Export]
    public KamiFonts.TextSize TextSize
    {
      get => textSize;
      private set
      {
        textSize = value;

        UpdateFontSize();
      }
    }

    [Export]
    public KamiColors.ColorType ColorType
    {
      get => colorType;
      private set
      {
        colorType = value;

        UpdateColor();
      }
    }

    private KamiFonts.TextSize textSize = KamiFonts.TextSize.MD;
    private KamiColors.ColorType colorType = KamiColors.ColorType.Primary;

    private UserSettings userSettings;

    public override void _Ready()
    {
      userSettings = GetNodeOrNull<UserSettings>("/root/UserSettings");

      if (userSettings != null) userSettings.UIScaleChange += OnUIScaleChange;

      UpdateFontSize();
      UpdateColor();
    }

    public override void _ExitTree()
    {
      if (userSettings != null) userSettings.UIScaleChange -= OnUIScaleChange;
    }

    public void SetTextSize(KamiFonts.TextSize textSize)
    {
      this.textSize = textSize;
      UpdateFontSize();
    }

    public void SetColorType(KamiColors.ColorType colorType)
    {
      this.colorType = colorType;
      UpdateColor();
    }

    private void UpdateFontSize()
    {
      var scale = userSettings == null ? 1f : userSettings.UIScale;
      Set("theme_override_font_sizes/font_size", KamiFonts.GetFontSizeByType(textSize, scale));
    }

    private void UpdateColor()
    {
      Set("theme_override_colors/font_color", KamiColors.GetColorByType(colorType));
    }

    private void OnUIScaleChange(float scale)
    {
      UpdateFontSize();
    }
  }
}