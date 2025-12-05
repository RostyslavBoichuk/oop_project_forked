using Godot;

public partial class QuestMenu : CanvasLayer
{
	private Control _container;
	private bool _isOpen = false;
	private const string LEVEL_MENU_PATH = "res://scenes/levels/LevelSelectMenu.tscn";
	public Texture2D CompletedIcon;
	public Texture2D OngoingIcon; 
	
	public Vector2 IconSize = new Vector2(100, 100); 

	public override void _Ready()
	{

		CompletedIcon = GD.Load<Texture2D>("res://items/textures/Quest_Complete.png");
		OngoingIcon = GD.Load<Texture2D>("res://items/textures/Quest_Uncomplete.png");

		_container = GetNodeOrNull<Control>("Panel/VBoxContainer");

		if (_container == null) 
			GD.PrintErr("QuestMenu: Could not find Panel/VBoxContainer! Quests won't display.");
		Visible = false;

		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.LevelComplete += OnLevelComplete;
		}
	}

	public override void _ExitTree()
	{
		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.LevelComplete -= OnLevelComplete;
		}
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.J)
		{
			ToggleMenu();
		}
	}

	private void OnLevelComplete()
	{
		_isOpen = true;
		Visible = true;
		RefreshUI(true);
	}
	
	public void ToggleMenu()
	{
		_isOpen = !_isOpen;
		Visible = _isOpen;

		if (_isOpen)
		{
			bool alreadyComplete = false;
			if (QuestManager.Instance != null)
			{
				alreadyComplete = QuestManager.Instance.IsLevelComplete();
			}
			RefreshUI(alreadyComplete);
		}
	}

	private void RefreshUI(bool isVictory = false)
	{
		if (_container == null) return;

		foreach (Node n in _container.GetChildren()) n.QueueFree();

		if (isVictory)
		{
			Label header = new Label();
			header.Text = "--- LEVEL COMPLETED! ---";
			header.HorizontalAlignment = HorizontalAlignment.Center;
			header.Modulate = Colors.Yellow;
			header.AddThemeFontSizeOverride("font_size", 24); 
			_container.AddChild(header);

			HBoxContainer buttons = new HBoxContainer();
			buttons.Alignment = BoxContainer.AlignmentMode.Center;
			buttons.AddThemeConstantOverride("separation", 20);

			Button btnLeave = new Button();
			btnLeave.Text = "Leave Level";
			btnLeave.Pressed += () => 
			{
				if (ResourceLoader.Exists(LEVEL_MENU_PATH))
					GetTree().ChangeSceneToFile(LEVEL_MENU_PATH);
				else
					GD.PrintErr($"Scene not found: {LEVEL_MENU_PATH}");
			};
			buttons.AddChild(btnLeave);

			Button btnContinue = new Button();
			btnContinue.Text = "Continue Playing";
			btnContinue.Pressed += () => 
			{
				ToggleMenu(); 
			};
			buttons.AddChild(btnContinue);

			_container.AddChild(buttons);
		}

		if (QuestManager.Instance != null)
		{
			foreach (var q in QuestManager.Instance.Quests)
			{
				HBoxContainer row = new HBoxContainer();
				
				Label descLabel = new Label();
				descLabel.Text = q.Description;
				descLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
				
				if (q.IsCompleted) 
				{
					descLabel.Modulate = Colors.Green;
					
					if (CompletedIcon != null)
					{
						TextureRect icon = new TextureRect();
						icon.Texture = CompletedIcon;

						icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
						icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
						
						icon.CustomMinimumSize = IconSize;
						row.AddChild(descLabel);
						row.AddChild(icon);
					}
					else
					{
						Label doneText = new Label();
						doneText.Text = "[DONE]";
						doneText.Modulate = Colors.Green;
						row.AddChild(descLabel);
						row.AddChild(doneText);
					}
				}
				else 
				{
					descLabel.Modulate = Colors.White;
					
					Label progressLabel = new Label();
					progressLabel.Text = $"[{q.CurrentAmount}/{q.RequiredAmount}]";
					
					row.AddChild(descLabel);
					row.AddChild(progressLabel);

					if (OngoingIcon != null)
					{
						TextureRect icon = new TextureRect();
						icon.Texture = OngoingIcon;
						icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
						icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
						
						icon.CustomMinimumSize = IconSize;
						row.AddChild(icon);
					}
				}

				_container.AddChild(row);
			}
		}
	}
}
