using Godot;
using System;

public partial class UnlockShopMenu : Control
{
	public CharacterManager Manager;

	private VBoxContainer _itemContainer;
	private Label _currencyLabel;
	private TextureButton _closeButton;

	private const string MAIN_MENU_PATH = "res://scenes/game_menu.tscn";
	
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		GetTree().Paused = true;

		ZIndex = 100;
		
		_currencyLabel = GetNodeOrNull<Label>("CurrencyLabel");
		_itemContainer = GetNodeOrNull<VBoxContainer>("ScrollContainer/ItemContainer");
		if (_itemContainer == null) _itemContainer = GetNodeOrNull<VBoxContainer>("ItemContainer");
		_closeButton = GetNodeOrNull<TextureButton>("CloseButton");

		if (_closeButton == null)
		{
			_closeButton = GetNodeOrNull<TextureButton>("close_btn");
		}

		if (_currencyLabel == null || _closeButton == null || _itemContainer == null)
		{
			GD.PrintErr("ERROR: Missing nodes in UnlockShopMenu.");
			return;
		}
		_closeButton.Pressed += CloseMenu;
		Refresh();
	}

	private void CloseMenu()
	{
		GetTree().Paused = false;
		if (Manager != null) 
		{
			Manager.shop_exists = false;
			QueueFree();
		}
		else
		{
			if (ResourceLoader.Exists(MAIN_MENU_PATH))
			{
				GetTree().ChangeSceneToFile(MAIN_MENU_PATH);
			}
			else
			{
				GD.PrintErr($"Main Menu scene not found at: {MAIN_MENU_PATH}");
				QueueFree(); 
			}
		}
	}

	private void Refresh()
	{
		if (_itemContainer == null) return;
		
		int cash = (Manager != null) ? (int)Manager.currency : RewardManager.Instance.GlobalCurrency;
		
		_currencyLabel.Text = $"Cash: {cash}";
		foreach (Node n in _itemContainer.GetChildren())
		{
			n.QueueFree();
		}

		var lockedItems = RewardManager.Instance.GetLockedRewards();

		if (lockedItems.Count == 0)
		{
			Label empty = new Label();
			empty.Text = "All items unlocked!";
			_itemContainer.AddChild(empty);
			return;
		}

		foreach (var r in lockedItems)
		{
			Button b = new Button();
			b.Text = $"{r.DisplayName} (${r.Cost})";
			b.Alignment = HorizontalAlignment.Left;

			if (cash < r.Cost) 
			{
				b.Disabled = true;
				b.Modulate = new Color(1, 0.5f, 0.5f);
			}
			
			var rewardRef = r;

			b.Pressed += () => 
			{
				if (RewardManager.Instance.TrySpendGlobalCurrency(rewardRef.Cost)) 
				{
					RewardManager.Instance.UnlockReward(rewardRef.Id);
					if (Manager != null) 
					{
						Manager.currency = RewardManager.Instance.GlobalCurrency;
					}

					Refresh();
				}
			};

			_itemContainer.AddChild(b);
		}
	}
}
