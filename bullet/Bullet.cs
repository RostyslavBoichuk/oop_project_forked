using Godot;

public partial class Bullet : Area2D
{
	public int atk = RewardManager.Instance.GlobalATK;
	public int def = RewardManager.Instance.GlobalDEF;
	
	protected BulletMovement movementLogic;
	protected BulletCollisionHandler collisionHandler;
	protected bool facingRight;
	protected float speed = 400f;
	protected string selfGroup = "";
	protected string oppositeGroup = "";
	protected float spinSpeed = 720f;
	protected Node2D visual;

	public Bullet()
	{
		movementLogic = new BulletMovement(this, speed);
		collisionHandler = new BulletCollisionHandler(this);
	}

	public virtual void Init(Vector2 direction, bool facingRight)
	{
		this.facingRight = facingRight;
		movementLogic.SetDirection(direction, facingRight);
	}

	public override void _Process(double delta)
	{
		movementLogic.Move(delta);

		if (visual != null)
			visual.RotationDegrees += spinSpeed * (float)delta;
	}

	public override void _Ready()
	{
		Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

		if (!string.IsNullOrEmpty(selfGroup))
			AddToGroup(selfGroup);

		visual = FindVisualNode();
	}

	private Node2D FindVisualNode()
	{
		Node2D node = GetNodeOrNull<Node2D>("Sprite2D");
		if (node != null) return node;

		node = GetNodeOrNull<Node2D>("AnimatedSprite2D");
		if (node != null) return node;

		foreach (Node child in GetChildren())
		{
			if (child is Node2D nd)
				return nd;
		}

		return null;
	}

	protected virtual void OnAreaEntered(Area2D area)
	{
		collisionHandler.HandleCollision(area, oppositeGroup);
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
		movementLogic.SetSpeed(newSpeed);
	}
}

public class BulletMovement
{
	private Area2D owner;
	private Vector2 direction;
	private float speed;

	public BulletMovement(Area2D owner, float speed)
	{
		this.owner = owner;
		this.speed = speed;
	}

	public void SetDirection(Vector2 dir, bool facingRight)
	{
		direction = dir.Normalized();
		if (direction == Vector2.Zero)
			direction.X = facingRight ? 1 : -1;
	}

	public void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
	}

	public void Move(double delta)
	{
		owner.Position += direction * speed * (float)delta;
		owner.Rotation = direction.Angle();
	}
}

public class BulletCollisionHandler
{
	private Area2D owner;

	public BulletCollisionHandler(Area2D owner)
	{
		this.owner = owner;
	}

	public void HandleCollision(Area2D area, string oppositeGroup)
	{
		if (area.IsInGroup(oppositeGroup))
		{
			area.QueueFree();
			owner.QueueFree();
		}
		else if (area.IsInGroup("player") && owner.IsInGroup("enemy_bullet"))
		{
			var manager = area.GetParent<CharacterManager>();
			if (manager != null) {
				
				manager.TakeDamage(RewardManager.Instance.GlobalATK * (10f - 10f / (10f + RewardManager.Instance.GlobalDEF)));
			}
			owner.QueueFree();
		}
		else if (area.IsInGroup("enemy") && owner.IsInGroup("player_bullet"))
		{
			area.QueueFree();
			owner.QueueFree();
		}
	}
}
