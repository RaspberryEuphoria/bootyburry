using Godot;

namespace Helpers
{
  public partial class Device : Node
  {
    public static bool IsMobile()
    {
      return OS.GetName() == "Android" || OS.GetName() == "iOS";
    }
  }
}