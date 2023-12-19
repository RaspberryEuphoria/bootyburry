using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Helpers;
using UI;

namespace Menu
{
  public partial class LevelSelector : BoxContainer
  {
    [Export]
    public float levelsPerWorld = 16f;

    [GeneratedRegex(@"Level_\d")]
    private static partial Regex LevelRegex();
    [GeneratedRegex("\\d+")]
    private static partial Regex DigitRegex();
    private GridContainer levelsContainer;
    private BoxContainer worldsContainer;
    private StringName levelsFolder = "res://scenes/game/level";
    private string sceneExtension = ".tscn";
    private float currentWorld = 0f;
    private List<string> levels;

    public override void _Ready()
    {
      levels = KamiFiles.GetFilesFromFolder(levelsFolder, LevelRegex())
        .Select(RemoveSceneExtension)
        .OrderBy(SortLevels).ToList();

      var levelsCount = (float)levels.Count;
      var worldsCount = Mathf.Ceil(levelsCount / levelsPerWorld);

      levelsContainer = GetNode<GridContainer>("LevelsContainer");
      worldsContainer = GetNode<BoxContainer>("WorldsContainer");

      // @Todo: load levels progress from Saves,
      // and set currentWorld to the last world the player has unlocked

      SetupWorldsButtons(worldsCount);
    }

    private void SetupWorldsButtons(float worldsCount)
    {
      for (int world = 0; world < worldsCount; world++)
      {
        var worldButton = ResourceLoader.Load<PackedScene>("res://scenes/ui/Components/KamiButton.tscn").Instantiate<KamiButton>();
        worldButton.Flat = false;
        worldButton.Text = $"{AddTrailingZero(levelsPerWorld * world + 1)}-{levelsPerWorld * (world + 1)}";
        worldButton.Name = worldButton.Text;
        worldButton.Theme = ResourceLoader.Load<Theme>("res://scenes/game/menus/LevelSelector/WorldButton.tres");
        worldButton.SetTextSize(KamiFonts.TextSize.SM);

        // local copies for closure
        var _world = world;

        if (world == currentWorld)
        {
          SetupLevelsButtons(world);
        }

        worldButton.ButtonUp += () =>
        {
          currentWorld = _world;
          SetupLevelsButtons(_world);
        };

        worldsContainer.AddChild(worldButton);
      }
    }

    private void SetupLevelsButtons(float world)
    {
      var previousLevelsButtons = levelsContainer.GetChildren();
      foreach (var button in previousLevelsButtons)
      {
        levelsContainer.RemoveChild(button);
        button.QueueFree();
      }

      var levelsInWorld = levels.Skip((int)(world * levelsPerWorld)).Take((int)levelsPerWorld);

      foreach (var levelName in levelsInWorld)
      {
        var level = int.Parse(levelName.Split('_')[1]);
        var levelButton = ResourceLoader.Load<PackedScene>("res://scenes/ui/Components/KamiButton.tscn").Instantiate<KamiButton>();
        levelButton.Flat = false;
        levelButton.Text = $"{level}";
        levelButton.Name = levelName;
        levelButton.Theme = ResourceLoader.Load<Theme>("res://scenes/game/menus/LevelSelector/LevelButton.tres");
        levelButton.SetTextSize(KamiFonts.TextSize.SM);

        // local copies for closure
        var _level = levelName;

        levelButton.ButtonUp += () =>
        {
          GoToLevel(level);
        };

        levelsContainer.AddChild(levelButton);
      }
    }

    private void GoToLevel(int level)
    {
      var nextLevel = ResourceLoader.Load<PackedScene>($"res://scenes/game/level/Level_{level}{sceneExtension}");
      GetTree().ChangeSceneToPacked(nextLevel);
    }

    private int SortLevels(string a) => int.Parse(a.Split('_')[1]);
    private string RemoveSceneExtension(string fileName) => fileName.Remove(fileName.Length - sceneExtension.Length);
    private string AddTrailingZero(float number)
    {
      if (number >= 10) return $"{number}";
      return $"0{number}";
    }
  }
}