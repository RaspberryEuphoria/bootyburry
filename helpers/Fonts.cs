namespace Helpers
{
  public class KamiFonts
  {
    public enum TextSize { SM, MD, LG, XL }

    private static readonly float default_sm = 28f;
    private static readonly float default_md = 48f;
    private static readonly float default_lg = 72f;
    private static readonly float default_xl = 96f;

    public static float GetFontSizeByType(TextSize textSize, float scale = 1f)
    {
      return textSize switch
      {
        TextSize.SM => default_sm * scale,
        TextSize.MD => default_md * scale,
        TextSize.LG => default_lg * scale,
        TextSize.XL => default_xl * scale,
        _ => throw new System.NotImplementedException(),
      };
    }
  }
}