using Godot;
using Godot.Collections;

public partial class LevelManager : Node
{
	public static LevelManager Instance { get; private set; }
	private const string SAVE_PATH = "user://level_progress.json";

	public int MaxUnlockedLevel { get; private set; } = 1;

	public override void _Ready()
	{
		if (Instance != null)
		{
			QueueFree();
			return;
		}
		Instance = this;
		LoadProgress();
	}

	public void UnlockLevel(int level)
	{
		if (level > MaxUnlockedLevel)
		{
			MaxUnlockedLevel = level;
			SaveProgress();
			GD.Print($"Unlocked Level {level}");
		}
	}

	public void ResetProgress()
	{
		MaxUnlockedLevel = 1;
		SaveProgress();
		
		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.ResetData();
		}
	}

	public void LockProgress()
	{
		ResetProgress();
		GD.Print("Progress Locked/Reset.");
	}

	private void SaveProgress()
	{
		var data = new Dictionary();
		data["max_level"] = MaxUnlockedLevel;
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(data));
	}

	public void LoadProgress()
	{
		if (!FileAccess.FileExists(SAVE_PATH)) return;
		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		var data = Json.ParseString(file.GetAsText()).AsGodotDictionary();
		if (data.ContainsKey("max_level")) MaxUnlockedLevel = data["max_level"].AsInt32();
	}
}
