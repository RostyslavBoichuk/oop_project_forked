using Godot;
using System;

public partial class UnlockShopMenu : Control
{
	public CharacterManager Manager;

	private VBoxContainer _itemContainer;
	private Label _currencyLabel;
	private Button _closeButton;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		GetTree().Paused = true;

		ZIndex = 100;
		
		_currencyLabel = GetNode<Label>("CurrencyLabel");
		_closeButton = GetNode<Button>("CloseButton");
		_itemContainer = GetNode<VBoxContainer>("ScrollContainer/ItemContainer");

		_closeButton.Pressed += CloseMenu;
		Refresh();
	}

	private void CloseMenu()
	{
		GetTree().Paused = false;

		if (Manager != null) 
		{
			Manager.shop_exists = false;
		}

		QueueFree();
	}

	private void Refresh()
	{

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
