using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using Helpers;
using UI;

namespace Menu
{
  public partial class LevelSelector : MarginContainer
  {
    [Export]
    public float levelsPerWorld = 16f;

    [GeneratedRegex(@"Level_\d")]
    private static partial Regex LevelRegex();
    [GeneratedRegex("\\d+")]
    private static partial Regex DigitRegex();
    private BoxContainer worldsContainer;
    private GridContainer levelsContainer;
    private StringName levelsFolder = "res://scenes/game/level";
    private string sceneExtension = ".tscn";
    private float currentWorld = 0f;
    private List<string> levels;
    private int? currentLevel = null;

    public override void _Ready()
    {
      levels = KamiFiles.GetFilesFromFolder(levelsFolder, LevelRegex())
       .ConvertAll(RemoveSceneExtension)
       .OrderBy(SortLevels).ToList();

      var levelsCount = (float)levels.Count;
      var worldsCount = Mathf.Ceil(levelsCount / levelsPerWorld);

      worldsContainer = GetNode<BoxContainer>("%WorldsContainer");
      levelsContainer = GetNode<GridContainer>("%LevelsContainer");

      // @Todo: load levels progress from Saves,
      // and set currentWorld to the last world the player has unlocked

      SetupWorldsButtons(worldsCount);
    }

    public void InitFromLevel(int currentLevel, Action ResumePlay)
    {
      this.currentLevel = currentLevel;
      currentWorld = Mathf.Ceil(currentLevel / levelsPerWorld) - 1;

      SetupLevelsButtons((KamiButton)worldsContainer.GetChild((int)currentWorld), currentWorld, ResumePlay);
    }

    private void SetupWorldsButtons(float worldsCount)
    {
      for (int world = 0; world < worldsCount; world++)
      {
        var worldButton = ResourceLoader.Load<PackedScene>("res://scenes/ui/Components/KamiButton.tscn").Instantiate<KamiButton>();
        worldButton.Flat = true;
        worldButton.Text = $"◇ {AddTrailingZero(levelsPerWorld * world + 1)}-{levelsPerWorld * (world + 1)}";
        worldButton.Name = worldButton.Text;
        worldButton.Theme = ResourceLoader.Load<Theme>("res://scenes/game/menus/LevelSelector/WorldButton.tres");
        worldButton.SetTextSize(KamiFonts.TextSize.SM);
        worldButton.SetColorType(KamiColors.ColorType.Light);

        // local copies for closure
        var _world = world;

        if (world == currentWorld)
        {
          SetupLevelsButtons(worldButton, world);
        }

        worldButton.ButtonUp += () =>
        {
          currentWorld = _world;
          SetupLevelsButtons(worldButton, _world);
        };

        worldsContainer.AddChild(worldButton);
      }
    }

    private void SetupLevelsButtons(KamiButton worldButton, float world, Action ResumePlay = null)
    {
      worldsContainer.GetChildren().ToList().ForEach(child =>
      {
        var button = (KamiButton)child;
        button.SetColorType(KamiColors.ColorType.Light);
        button.Text = button.Text.Replace("◈", "◇");
      });

      worldButton.SetColorType(KamiColors.ColorType.Primary);
      worldButton.Text = worldButton.Text.Replace("◇", "◈");

      var previousLevelsButtons = levelsContainer.GetChildren();
      foreach (var levelButton in previousLevelsButtons)
      {
        levelsContainer.RemoveChild(levelButton);
        levelButton.QueueFree();
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
        levelButton.SetColorType(KamiColors.ColorType.Light);

        if (level == currentLevel)
        {
          levelButton.SetColorType(KamiColors.ColorType.Primary);
          levelButton.Flat = true;

          levelButton.ButtonUp += () => ResumePlay();
        }
        else
        {
          // local copies for closure
          var _level = levelName;

          levelButton.ButtonUp += () =>
          {
            GoToLevel(level);
          };
        }

        levelsContainer.AddChild(levelButton);
      }
    }

    private void GoToLevel(int level)
    {
      var nextLevel = ResourceLoader.Load<PackedScene>($"res://scenes/game/level/Level_{level}{sceneExtension}");
      GetTree().ChangeSceneToPacked(nextLevel);
    }

    private int SortLevels(string a) => int.Parse(a.Split('_').Last());
    private string RemoveSceneExtension(string fileName) => fileName.Split(".").First();
    private string AddTrailingZero(float number)
    {
      if (number >= 10) return $"{number}";
      return $"0{number}";
    }
  }
}