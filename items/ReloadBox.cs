using Godot;
using System;

public partial class ReloadBox : Area2D
{
	private CharacterManager manager; 
	
	 public override void _Ready()
	{
		GD.Print("ReloadBox ready!");
		Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));
		manager = GetParent().GetNode<CharacterManager>("CharacterManager");

		if (manager == null)
			GD.PrintErr("CharacterManager not found!");
		else
			GD.Print("CharacterManager found successfully!");
	}

	 private void OnAreaEntered(Area2D area)
		{
			if (manager == null)
				return;
			Node current = area;
			while (current != null && !(current is MainCharacter))
				current = current.GetParent();
			if (current is MainCharacter)
			{
				GD.Print("MainCharacter entered ReloadBox!");
				manager.Reload(5);
				if (QuestManager.Instance != null)
				{
					QuestManager.Instance.UpdateProgress("collect_ammo", 1);
				}
				QueueFree();
			}
		}
}
