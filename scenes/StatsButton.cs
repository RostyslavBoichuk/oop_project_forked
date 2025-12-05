using Godot;
using System;

public partial class StatsButton : Node2D
{
	private Area2D area;
	[Export]
	private string sceneToLoad = "res://scenes/Statistical_Upgrade.tscn";

	public override void _Ready()
	{
		area = GetNode<Area2D>("Area2D_StatsButton");
		if (area == null)
		{
			GD.PrintErr("Area2D node not found! Check the child node name.");
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
