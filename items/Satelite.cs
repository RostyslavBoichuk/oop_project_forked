using Godot;
using System;
using System.Threading.Tasks;

public partial class Satelite : Area2D
{
	private CharacterManager manager;
	private Vector2 origin;
	private Vector2 target;
	private RandomNumberGenerator rng = new RandomNumberGenerator();

	public override void _Ready()
	{
		GD.Print("Satellite ready!");
		Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));
		manager = GetParent().GetNode<CharacterManager>("CharacterManager");

		origin = Position;
		SetNewTarget();

		_ = FlyRandomly();
	}

	private async Task FlyRandomly()
	{
		float speed = 40f;

		while (IsInstanceValid(this))
		{
			Vector2 direction = (target - Position).Normalized();
			Position += direction * speed * (float)GetProcessDeltaTime();

			if (Position.DistanceTo(target) < 5f)
				SetNewTarget();

			await ToSignal(GetTree(), "physics_frame");
		}
	}

	private void SetNewTarget()
	{
		float offsetX = rng.RandfRange(-50f, 50f);
		float offsetY = rng.RandfRange(-50f, 50f);
		target = origin + new Vector2(offsetX, offsetY);
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
			GD.Print("MainCharacter entered Satellite!");
			manager.AddMoney(5);
			QuestManager.Instance.UpdateProgress("collect_satellites", 1);
			QueueFree();
		}
	}
}
