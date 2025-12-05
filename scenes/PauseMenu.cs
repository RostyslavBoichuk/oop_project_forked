using Godot;
using System;

public partial class PauseMenu : Node2D
{
	public bool isPaused = false;

	public override void _Ready()
	{
		Visible = false;
		ProcessMode = Node.ProcessModeEnum.Always;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
		{
			TogglePause();
		}
	}

	public void TogglePause()
	{
		isPaused = !isPaused;
		Visible = isPaused;
		GetTree().Paused = isPaused;
	}
}
