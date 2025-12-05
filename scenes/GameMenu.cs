using Godot;
using System;

public partial class GameMenu : Node2D 
{
	private AnimatedSprite2D _backgroundAnim;
	private BaseButton _levelMapButton;
	private BaseButton _charSelectButton;
	private BaseButton _casinoButton;

	private const string LEVEL_MAP_PATH = "res://levels/level_1.tscn"; 
	private const string CHAR_SELECT_PATH = "res://scenes/character_select.tscn"; 
	private const string CASINO_PATH = "res://characters/trader/UnlockShopMenu.tscn";

	public override void _Ready()
	{
		_backgroundAnim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_levelMapButton = GetNode<BaseButton>("LevelMapButton");
		_charSelectButton = GetNode<BaseButton>("CharSelectButton");
		_casinoButton = GetNode<BaseButton>("CasinoButton");
		if (_backgroundAnim != null)
		{
			_backgroundAnim.Play();
			GD.Print("Menu Animation Started");
		}

		if (_levelMapButton != null)
		{
			_levelMapButton.Pressed += OnLevelMapPressed;
			GD.Print("Level Map Button Connected");
		}

		if (_charSelectButton != null)
		{
			_charSelectButton.Pressed += OnCharSelectPressed;
			GD.Print("Character Select Button Connected");
		}

		if (_casinoButton != null)
		{
			_casinoButton.Pressed += OnCasinoPressed;
			GD.Print("Casino Button Connected");
		}
	}

	private void OnLevelMapPressed()
	{
		GD.Print(" [BUTTON CLICKED] Level Map Button was pressed!");
		
		if (ResourceLoader.Exists(LEVEL_MAP_PATH))
			GetTree().ChangeSceneToFile(LEVEL_MAP_PATH);
		else
			GD.PrintErr($"Scene not found: {LEVEL_MAP_PATH}");
	}

	private void OnCharSelectPressed()
	{
		GD.Print(" [BUTTON CLICKED] Character Selection Button was pressed!");

		if (ResourceLoader.Exists(CHAR_SELECT_PATH))
			GetTree().ChangeSceneToFile(CHAR_SELECT_PATH);
		else
			GD.PrintErr($"Scene not found: {CHAR_SELECT_PATH}");
	}

	private void OnCasinoPressed()
	{
		GD.Print(" [BUTTON CLICKED] Casino Button was pressed!");

		if (ResourceLoader.Exists(CASINO_PATH))
			GetTree().ChangeSceneToFile(CASINO_PATH);
		else
			GD.PrintErr($"Scene not found: {CASINO_PATH}");
	}
}
