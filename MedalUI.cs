using Godot;

namespace UI
{
  public partial class MedalUI : TextureRect
  {
    private int value = 0;

    public void Init(int value)
    {
      if (value == 0)
      {
        Remove();
        return;
      }

      this.value = value;
      SetLabel();
    }

    public void SetLabel()
    {
      var label = GetNode<RichTextLabel>("LabelContainer/Label");
      label.Text = value.ToString();
    }

    public void Remove()
    {
      Visible = false;
      QueueFree();
    }

    public void Disable()
    {
      GetNode<TextureRect>("Cross").Visible = true;
      Modulate = new Color(0.5f, 0.5f, 0.5f);
    }

    public void OnPlayerMoved(int score)
    {
      if (score > value) Disable();
    }
  }
}