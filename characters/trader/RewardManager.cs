using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class RewardManager : Node
{
	public static RewardManager Instance { get; private set; }
	private const string SAVE_PATH = "user://shop_data.json";

	public int GlobalCurrency { get; set; } = 1000;
	public int GlobalUpgradePoints { get; set; } = 0;
	
	public int GlobalHP { get; set; } = 100;
	public int GlobalATK { get; set; } = 10;
	public int GlobalDEF { get; set; } = 10;
	
	private List<Reward> _allRewards = new List<Reward>();

	public override void _Ready()
	{
		Instance = this;
		InitializeRewards();
		LoadGame();
	}

	private void InitializeRewards()
	{
		_allRewards.Clear();
		_allRewards.Add(new Reward("ammo_small", "2 Bullets", 0, (cm) => cm.Reload(2), true));
		_allRewards.Add(new Reward("gold_small", "50 Gold", 0, (cm) => cm.AddMoney(50), true));
		_allRewards.Add(new Reward("ammo_box", "Ammo Box (10)", 150, (cm) => cm.Reload(10)));
		_allRewards.Add(new Reward("max_heal", "Full Restore", 300, (cm) => cm.Heal(100)));	
		_allRewards.Add(new Reward("scooter", "Combat Scooter", 500, (cm) => cm.EquipScooter()));
		_allRewards.Add(new Reward("upgrade_point", "+1 Upgrade Point", 200, (cm) => AddUpgradePoints(1)));
	}

	public void AddUpgradePoints(int amount)
	{
		GlobalUpgradePoints += amount;
		SaveGame();
		GD.Print($"Upgrade Points: {GlobalUpgradePoints}");
	}

	public void ResetData()
	{
		if (FileAccess.FileExists(SAVE_PATH)) DirAccess.RemoveAbsolute(SAVE_PATH);
		GlobalCurrency = 1000;
		GlobalHP = 100;
		GlobalATK = 10;
		GlobalDEF = 10;
		GlobalUpgradePoints = 0;
		InitializeRewards();
		SaveGame();
		GD.Print("Save Data Reset!");
	}

	public bool TrySpendGlobalCurrency(int cost)
	{
		if (GlobalCurrency >= cost) { GlobalCurrency -= cost; SaveGame(); return true; }
		return false;
	}

	public void AddCurrency(int amount) { GlobalCurrency += amount; SaveGame(); }
	
	public void UnlockReward(string id) {
		var r = _allRewards.FirstOrDefault(x => x.Id == id);
		if (r != null) { r.IsUnlocked = true; SaveGame(); }
	}

	public List<Reward> GetLockedRewards() => _allRewards.Where(r => !r.IsUnlocked).ToList();

	public Reward[] GetRandomShopSelection() {
		var unlocked = _allRewards.Where(r => r.IsUnlocked).ToList();
		int n = unlocked.Count;
		while (n > 1) { n--; int k = GD.RandRange(0, n); (unlocked[k], unlocked[n]) = (unlocked[n], unlocked[k]); }
		return unlocked.Take(4).ToArray();
	}

	private void SaveGame() {
		var data = new Godot.Collections.Dictionary();
		data["currency"] = GlobalCurrency;
		data["upgrade_points"] = GlobalUpgradePoints;
		
		data["atk"] = GlobalATK;
		data["def"] = GlobalDEF;
		data["hp"] = GlobalHP;
		
		data["unlocked"] = _allRewards.Where(r => r.IsUnlocked).Select(r => r.Id).ToArray();
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(data));
	}

	private void LoadGame() {
		if (!FileAccess.FileExists(SAVE_PATH)) return;
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		var data = Json.ParseString(file.GetAsText()).AsGodotDictionary();
		
		if (data.ContainsKey("currency")) GlobalCurrency = (int)data["currency"];
		
		if (data.ContainsKey("upgrade_points")) GlobalUpgradePoints = (int)data["upgrade_points"];

		if (data.ContainsKey("atk")) GlobalATK = (int)data["atk"];
		if (data.ContainsKey("def")) GlobalDEF = (int)data["def"];
		if (data.ContainsKey("hp")) GlobalHP = (int)data["hp"];

		if (data.ContainsKey("unlocked")) {
			foreach(string id in data["unlocked"].AsStringArray()) {
				var r = _allRewards.FirstOrDefault(x => x.Id == id);
				if (r != null) r.IsUnlocked = true;
			}
		}
	}
}
