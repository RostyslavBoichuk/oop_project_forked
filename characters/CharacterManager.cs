using Godot;
using System;

public partial class CharacterManager : Node2D
{
	private const string Pause_Menu = "res://scenes/Pause_Menu.tscn";
	private const string SHOP_PATH = "res://characters/trader/trader_menu.tscn";
	private const string UNLOCK_PATH = "res://characters/trader/UnlockShopMenu.tscn";
	private const string SCOOTER_PATH = "res://items/scooter.tscn";
	private const string CHARACTER_PATH = "res://characters/Atlas/main_character.tscn";
	private const string TOMBSTONE_PATH = "res://characters/tombstone.tscn";
	
	private PackedScene characterBase;
	private PackedScene characterTombstoneBase;
	private MainCharacter character;
	private Tombstone characterTombstone;
	public float hp;
	private float bullet_counter = 5;
	private bool exists = false;
	public bool allow_shop = false;
  	public bool shop_exists = false;
	
	public float currency 
	{
		get => RewardManager.Instance.GlobalCurrency;
		set => RewardManager.Instance.GlobalCurrency = (int)value;
	}
	
	public int UpgradePoints { get; set; } = 0;
	

	
	public override void _Ready()
	{
		hp = RewardManager.Instance.GlobalHP;
	}
	
	public override void _Process(double delta)
	{
			
		if (Input.IsKeyPressed(Key.Q))
		{
			if (characterBase == null)
			{
				characterBase = GD.Load<PackedScene>(CHARACTER_PATH);
			}
			SpawnCharacter();
		}

		if (Input.IsKeyPressed(Key.M) && allow_shop && !shop_exists)
		{
			OpenMenu(SHOP_PATH);
		}
		
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			OpenMenu(Pause_Menu);
		}
		
		if (Input.IsKeyPressed(Key.U) && allow_shop && !shop_exists)
		{
			OpenMenu(UNLOCK_PATH);
		}

		if (Input.IsKeyPressed(Key.R))
		{
			RewardManager.Instance.ResetData();
			if (QuestManager.Instance != null)
			{
				QuestManager.Instance.ResetData();
			}
			GD.Print($"Data Reset. Currency: {currency}");
		}
	}
	
	private void OpenMenu(string path)
	{
		if (ResourceLoader.Exists(path))
		{
			var scene = GD.Load<PackedScene>(path);
			Node menu = scene.Instantiate();
			if (menu is TraderMenu tm) tm.Manager = this;
			if (menu is UnlockShopMenu us) us.Manager = this;

			AddChild(menu);
			shop_exists = true;
		}
		else
		{
			GD.PrintErr($"Scene not found at: {path}");
		}
	}
		
	private void SpawnCharacter()
	{
		if(!exists){
			character = (MainCharacter)characterBase.Instantiate();
			hp = 100;
			character.GlobalPosition = GlobalPosition;
			character.init(200f,400f,0.5f,0.4f,0.5f);
			AddChild(character);
			exists = true;
		}
	}

	public void TakeDamage(float damage)
	{
		hp -= damage;
		GD.Print(hp);
		if (hp <= 0)
		{
			GD.Print("Character died!");
			if (character != null && IsInstanceValid(character)){
				characterTombstoneBase = ResourceLoader.Load<PackedScene>("res://characters/tombstone.tscn");
				characterTombstone = (Tombstone)characterTombstoneBase.Instantiate();
				characterTombstone.GlobalPosition = character.GlobalPosition;
				AddChild(characterTombstone);
				character.QueueFree();
				character = null;
				exists = false;
			}
		}
	}
	
	public void Reload(float bullets){
		bullet_counter += bullets;
		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.UpdateProgress("collect_ammo", 1);
		}
	}
	
	public bool bullet_manage(){
		if (bullet_counter > 0){
			bullet_counter -= 1;
			GD.Print(bullet_counter);
			return true;
		}
		return false;
	}

	public void AddMoney(float money)
	{
		RewardManager.Instance.AddCurrency((int)money);
		GD.Print($"Money Collected! Total: {currency}");
	}

	public void Heal(float amount)
	{
		hp += amount;
		if (hp > 100) hp = 100;
		GD.Print($"Healed! HP is now {hp}");
	}

	public void EquipScooter()
	{
		GD.Print("Scooter added:");
		if (character == null || !IsInstanceValid(character))
		{
			GD.PrintErr("No valid character to attach scooter to!");
			return;
		}

		if (ResourceLoader.Exists(SCOOTER_PATH))
		{
			var scooterBase = GD.Load<PackedScene>(SCOOTER_PATH);
			var scooter = scooterBase.Instantiate(); 
			
			if (scooter is Scooter s) s.manager = this; 

			character.AddChild(scooter);
			GD.Print("Scooter equipped successfully.");
		}
	}
	
	public void AddUpgradePoint(int amount)
	{
		UpgradePoints += amount;
		GD.Print($"Upgrade Point Acquired! Total Points: {UpgradePoints}");
	}
}
