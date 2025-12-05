using Godot;
using System;

public class Reward
{
	public string Id { get; set; }
	public string DisplayName { get; set; }
	public int Cost { get; set; }
	public Action<CharacterManager> Effect { get; set; }
	public bool IsUnlocked { get; set; }

	public Reward(string id, string name, int cost, Action<CharacterManager> effect, bool defaultUnlocked = false)
	{
		Id = id;
		DisplayName = name;
		Cost = cost;
		Effect = effect;
		IsUnlocked = defaultUnlocked;
	}
}
