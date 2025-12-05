using Godot;
using System;

public partial class StatisticalUpgrade : Node2D
{
	private Area2D area;
	private const int HP_INCREASE = 10;
	private const int DEF_INCREASE = 2;
	
	public override void _Ready()
	{
		area = GetNode<Area2D>("Area2D");
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
			if (RewardManager.Instance.GlobalUpgradePoints > 0)
			{
				RewardManager.Instance.GlobalHP += HP_INCREASE;
				RewardManager.Instance.GlobalDEF += DEF_INCREASE;
				RewardManager.Instance.GlobalUpgradePoints--;
				RewardManager.Instance.SaveGame();

				GD.Print($"Stats Upgraded! HP: {RewardManager.Instance.GlobalHP}, DEF: {RewardManager.Instance.GlobalDEF}, Remaining Upgrade Points: {RewardManager.Instance.GlobalUpgradePoints}");
			}
			else
			{
				GD.Print("No upgrade points available!");
			}
		}
	}
}
