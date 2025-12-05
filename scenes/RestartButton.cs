using Godot;
using System;

public partial class RestartButton : Node2D
{
	private Area2D area;
	[Export]
	private string sceneToLoad = "res://scenes/levels/level_1.tscn";

	public override void _Ready()
	{
		area = GetChild<Area2D>(0);

		if (area == null)
		{
			GD.PrintErr("No Area2D found for this button!");
			return;
		}

		area.InputEvent += OnButtonClicked;
	}

	private void OnButtonClicked(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GD.Print($"{Name} clicked! Loading scene: {sceneToLoad}");
			GetTree().ChangeSceneToFile(sceneToLoad);
		}
	}
}
