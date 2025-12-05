using Godot;
using System;

public partial class LevelSelectScreen : Control
{
	private TextureButton _resetButton; 
	private TextureButton[] _levelButtons;
	
	private const int TOTAL_LEVELS = 4;

	private const string GAME_MENU_PATH = "res://scenes/game_menu.tscn";
	private const string LEVEL_1_PATH = "res://scenes/levels/level_1.tscn";
	private const string LEVEL_3_PATH = "res://scenes/levels/level_3.tscn";
	private const string LEVEL_4_PATH = "res://scenes/levels/level_4.tscn";

	public override void _Ready()
	{
		_resetButton = GetNodeOrNull<TextureButton>("ResetButton");

		_levelButtons = new TextureButton[TOTAL_LEVELS];

		for (int i = 0; i < TOTAL_LEVELS; i++)
		{
			int levelNum = i + 1;
			string path = $"BtnLevel{levelNum}";
			_levelButtons[i] = GetNodeOrNull<TextureButton>(path);
		}

		if (_resetButton != null)
		{
			_resetButton.Pressed += OnResetPressed;
		}

		if (_levelButtons[0] != null) _levelButtons[0].Pressed += LoadLevel1;
		if (_levelButtons[1] != null) _levelButtons[1].Pressed += LoadLevel2;
		if (_levelButtons[2] != null) _levelButtons[2].Pressed += LoadLevel3;
		if (_levelButtons[3] != null) _levelButtons[3].Pressed += LoadLevel4;

		RefreshButtons();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			if (keyEvent.Keycode == Key.Key2) UnlockAndRefresh(2);
			if (keyEvent.Keycode == Key.Key3) UnlockAndRefresh(3);
			if (keyEvent.Keycode == Key.Key4) UnlockAndRefresh(4);
		}
	}

	private void UnlockAndRefresh(int level)
	{
		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.UnlockLevel(level);
			RefreshButtons();
		}
	}

	private void RefreshButtons()
	{
		if (LevelManager.Instance == null) return;

		int unlocked = LevelManager.Instance.MaxUnlockedLevel;

		for (int i = 0; i < TOTAL_LEVELS; i++)
		{
			int levelNum = i + 1;
			TextureButton btn = _levelButtons[i];

			if (btn == null) continue;

			if (levelNum <= unlocked)
			{
				btn.Disabled = false;
				btn.Modulate = new Color(1, 1, 1);
				btn.MouseDefaultCursorShape = CursorShape.PointingHand;
			}
			else
			{
				btn.Disabled = true;
				btn.Modulate = new Color(0.3f, 0.3f, 0.3f);
				btn.MouseDefaultCursorShape = CursorShape.Arrow;
			}
		}
	}


	private void OnResetPressed()
	{
		GD.Print("Returning to Game Menu...");
		
		if (ResourceLoader.Exists(GAME_MENU_PATH))
			GetTree().ChangeSceneToFile(GAME_MENU_PATH);
		else
			GD.PrintErr($"Scene not found: {GAME_MENU_PATH}");
	}

	private void LoadLevel1()
	{
		GD.Print("Loading Level 1...");
		
		if (ResourceLoader.Exists(LEVEL_1_PATH))
			GetTree().ChangeSceneToFile(LEVEL_1_PATH);
		else
			GD.PrintErr($"Scene not found: {LEVEL_1_PATH}");
	}

	private void LoadLevel2()
	{
		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.ResetProgress();
			RefreshButtons();
		}
		else
		{
			GD.PrintErr("LevelManager Instance is null! Check Autoloads.");
		}
	}

	private void LoadLevel3()
	{
		GD.Print("Loading Level 3...");
		
		if (ResourceLoader.Exists(LEVEL_3_PATH))
			GetTree().ChangeSceneToFile(LEVEL_3_PATH);
		else
			GD.PrintErr($"Scene not found: {LEVEL_3_PATH}");
	}

	private void LoadLevel4()
	{
		GD.Print("Loading Level 4...");
		
		if (ResourceLoader.Exists(LEVEL_4_PATH))
			GetTree().ChangeSceneToFile(LEVEL_4_PATH);
		else
			GD.PrintErr($"Scene not found: {LEVEL_4_PATH}");
	}
}
