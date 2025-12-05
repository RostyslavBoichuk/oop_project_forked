using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class QuestManager : Node
{
	public static QuestManager Instance { get; private set; }
	[Signal] public delegate void LevelCompleteEventHandler();

	private const string SAVE_PATH = "user://quest_progress.json";

	public class Quest
	{
		public string Id;
		public string Description;
		public int CurrentAmount;
		public int RequiredAmount;
		public bool IsCompleted;

		public Quest(string id, string desc, int required)
		{
			Id = id;
			Description = desc;
			RequiredAmount = required;
			CurrentAmount = 0;
			IsCompleted = false;
		}
	}

	public List<Quest> Quests = new List<Quest>();

	public override void _Ready()
	{
		Instance = this;
		InitializeQuests();
		LoadQuests();
	}

	private void InitializeQuests()
	{
		Quests.Clear();
		Quests.Add(new Quest("kill_enemies", "Kill 2 Enemies", 6));
		Quests.Add(new Quest("collect_ammo", "Collect 5 Reload Boxes", 7));
		Quests.Add(new Quest("collect_satellites", "Collect 2 Satellites", 4));
	}

	public void UpdateProgress(string questId, int amount = 1)
	{
		bool anyUpdate = false;
		bool allFinished = true;

		foreach (var q in Quests)
		{
			if (q.Id == questId && !q.IsCompleted)
			{
				q.CurrentAmount += amount;
				
				if (q.CurrentAmount >= q.RequiredAmount)
				{
					q.CurrentAmount = q.RequiredAmount;
					q.IsCompleted = true;
					GD.Print($"Quest Completed: {q.Description}");
				}
				anyUpdate = true;
			}
			
			if (!q.IsCompleted) allFinished = false;
		}

		if (anyUpdate)
		{
			SaveQuests();
			
			if (allFinished)
			{
				_ = HandleLevelCompletion();
			}
		}
	}

	public bool IsLevelComplete()
	{
		if (Quests.Count == 0) return false;
		
		foreach (var q in Quests)
		{
			if (!q.IsCompleted) return false;
		}
		return true;
	}

	private async Task HandleLevelCompletion()
	{
		if (LevelManager.Instance != null)
		{
			LevelManager.Instance.UnlockLevel(2);
		}
		GD.Print("All quests finished! Emitting signal...");
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		EmitSignal(SignalName.LevelComplete);
		
	}

	public void SaveQuests()
	{
		var saveArray = new Godot.Collections.Array();
		foreach (var q in Quests)
		{
			var qData = new Dictionary();
			qData["id"] = q.Id;
			qData["current"] = q.CurrentAmount;
			qData["completed"] = q.IsCompleted;
			saveArray.Add(qData);
		}

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Write);
		file.StoreString(Json.Stringify(saveArray));
	}

	public void LoadQuests()
	{
		if (!FileAccess.FileExists(SAVE_PATH)) return;

		using var file = FileAccess.Open(SAVE_PATH, FileAccess.ModeFlags.Read);
		var json = new Json();
		if (json.Parse(file.GetAsText()) == Error.Ok)
		{
			var saveArray = json.Data.AsGodotArray();
			foreach (Godot.Collections.Dictionary qData in saveArray)
			{
				string id = (string)qData["id"];
				foreach (var localQuest in Quests)
				{
					if (localQuest.Id == id)
					{
						localQuest.CurrentAmount = (int)qData["current"];
						localQuest.IsCompleted = (bool)qData["completed"];
					}
				}
			}
		}
	}

	public void ResetData()
	{
		InitializeQuests();
		SaveQuests();
	}
}
