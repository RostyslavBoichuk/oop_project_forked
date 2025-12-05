using Godot;
using System;
using System.Threading.Tasks;

public partial class TraderMenu : Node2D
{
	public CharacterManager Manager;

	private Sprite2D _spinnerSprite;
	private Button _spinBtn;
	private Button _closeBtn;
	private Label _resultLabel;
	
	private Reward[] _rewards;
	private bool _spinning = false;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		GetTree().Paused = true;
		
		_spinnerSprite = GetNode<Sprite2D>("spiner");
		_spinBtn = GetNode<Button>("spin_btn");
		_closeBtn = GetNode<Button>("close_btn");
		_resultLabel = GetNodeOrNull<Label>("result_label");
		
		if (_spinnerSprite == null || _spinBtn == null || _closeBtn == null)
		{
			GD.PrintErr("ОШИБКА: Не найдены кнопки или спрайт! Проверьте имена нод в TraderMenu.");
			return;
		}

		_rewards = RewardManager.Instance.GetRandomShopSelection();
		
		_spinBtn.Pressed += async () => await Spin();
		_closeBtn.Pressed += CloseMenu;
	}

	private void CloseMenu()
	{
		GetTree().Paused = false;
		if (Manager != null) Manager.shop_exists = false;
		QueueFree();
	}

	private async Task Spin()
	{
		if (_spinning) return;
		_spinning = true;

		if (_resultLabel != null) _resultLabel.Text = "Spinning...";

		float target = _spinnerSprite.RotationDegrees + (float)GD.RandRange(720, 1440);
		
		Tween t = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		t.TweenProperty(_spinnerSprite, "rotation_degrees", target, 2.0f);
		
		await ToSignal(t, "finished");

		int idx = Mathf.Clamp(Mathf.FloorToInt((_spinnerSprite.RotationDegrees % 360) / 90f), 0, 3);
		
		if (idx < _rewards.Length) 
		{
			string winText = $"Won: {_rewards[idx].DisplayName}";
			GD.Print($"Won: {_rewards[idx].DisplayName}");
			_rewards[idx].Effect?.Invoke(Manager);
			
			if (_resultLabel != null) 
			{
				_resultLabel.Text = winText;
			}
		}
		else
		{
			string loseText = "You lose!";
			GD.Print(loseText);
			
			if (_resultLabel != null) 
			{
				_resultLabel.Text = loseText;
			}
		}

		_spinning = false;
	}
}
