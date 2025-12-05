using Godot;
using System;
using System.Collections.Generic;

public abstract partial class EnemyManager : Area2D
{
	protected bool facingRight = true;
	protected float bulletSpawnInterval = 3.0f;
	protected float timeSinceLastBullet = 0f;
	protected float activationDistance = 400f;
	
	public float HP { get; set; } = 30f;
	
	protected Sprite2D characterSprite;
	protected PackedScene bulletScene;
	protected Node2D player;

	private DistanceChecker distanceChecker;
	private BulletSpawner bulletSpawner;

	protected abstract string BulletScenePath { get; }

	private static Quadtree<EnemyManager> quadtree;
	private static Rect2 worldBounds = new Rect2(new Vector2(0, 0), new Vector2(2000, 2000));
	private static bool quadtreeInitialized = false;

	public override void _Ready()
	{
		characterSprite = GetNode<Sprite2D>("Sprite2D");
		bulletScene = ResourceLoader.Load<PackedScene>(BulletScenePath);
		AddToGroup("enemy");

		distanceChecker = new DistanceChecker(this);
		bulletSpawner = new BulletSpawner(this, bulletScene);

		if (!quadtreeInitialized)
		{
			quadtree = new Quadtree<EnemyManager>(worldBounds, 4, 8);
			quadtreeInitialized = true;
		}

		quadtree.Insert(this);

		OnEnemyReady();
	}

	public override void _Process(double delta)
	{
		player = GetNodeOrNull<Node2D>("/root/Node2D/CharacterManager/main_character/Character2D");
		if (player == null) return;

		quadtree.Clear();
		foreach (Node node in GetTree().GetNodesInGroup("enemy"))
		{
			if (node is EnemyManager e)
				quadtree.Insert(e);
		}

		Rect2 queryRect = new Rect2(player.GlobalPosition - new Vector2(activationDistance, activationDistance),
									 new Vector2(activationDistance * 2, activationDistance * 2));
		List<EnemyManager> nearby = quadtree.Query(queryRect);

		if (nearby.Contains(this))
		{
			distanceChecker.CheckDistance(player.GlobalPosition, characterSprite, ref facingRight);
			if (distanceChecker.IsActive)
			{
				timeSinceLastBullet += (float)delta;
				if (timeSinceLastBullet >= bulletSpawnInterval)
				{
					bulletSpawner.SpawnBullet(player, facingRight);
					timeSinceLastBullet = 0f;
				}
			}
		}
	}

	protected virtual void OnEnemyReady() { }

	public class DistanceChecker
	{
		private EnemyManager owner;
		private float distanceThreshold;
		private bool isActive = false;
		private ActivationHandler activationHandler = new ActivationHandler();

		public bool IsActive => isActive;

		public DistanceChecker(EnemyManager owner)
		{
			this.owner = owner;
			distanceThreshold = owner.activationDistance;
		}

		public void CheckDistance(Vector2 playerPosition, Sprite2D sprite, ref bool facingRight)
		{
			float distanceSqr = owner.GlobalPosition.DistanceSquaredTo(playerPosition);
			float thresholdSqr = distanceThreshold * distanceThreshold;

			if (distanceSqr < thresholdSqr && !isActive)
			{
				activationHandler.Activate(sprite, ref facingRight);
				isActive = true;
			}
			else if (distanceSqr >= thresholdSqr && isActive)
			{
				activationHandler.Deactivate(sprite, ref facingRight);
				isActive = false;
			}
		}
	}

	public void TakeDamage(float amount) // ADDED: Damage function
	{
		HP -= amount;
		if (HP <= 0)
		{
			Die();
		}
	}
	
	protected virtual void Die() // ADDED: Death function
	{
		// REPORT PROGRESS: Kill Enemies
		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.UpdateProgress("kill_enemies", 1);
		}
		
		QueueFree();
	}
	
	public class ActivationHandler
	{
		public void Activate(Sprite2D sprite, ref bool facingRight)
		{
			facingRight = false;
			sprite.FlipH = true;
		}

		public void Deactivate(Sprite2D sprite, ref bool facingRight)
		{
			facingRight = true;
			sprite.FlipH = false;
		}
	}

	public class BulletSpawner
	{
		private Node2D owner;
		private PackedScene bulletScene;

		public BulletSpawner(Node2D owner, PackedScene bulletScene)
		{
			this.owner = owner;
			this.bulletScene = bulletScene;
		}

		public void SpawnBullet(Node2D player, bool facingRight)
		{
			if (bulletScene == null) return;

			var bulletNode = bulletScene.Instantiate();
			if (bulletNode is EnemyBullet bullet)
			{
				bullet.Position = owner.GlobalPosition;
				Vector2 direction = (player.GlobalPosition - owner.GlobalPosition).Normalized();
				bullet.Init(direction, facingRight);
				owner.GetParent().AddChild(bullet);
			}
		}
	}

	public class Quadtree<T> where T : Node2D
	{
		private Rect2 bounds;
		private int capacity;
		private int maxDepth;
		private List<T> items;
		private bool subdivided;
		private Quadtree<T>[] children;

		public Quadtree(Rect2 bounds, int capacity, int maxDepth)
		{
			this.bounds = bounds;
			this.capacity = capacity;
			this.maxDepth = maxDepth;
			items = new List<T>();
			subdivided = false;
			children = null;
		}

		public void Clear()
		{
			items.Clear();
			if (subdivided && children != null)
			{
				for (int i = 0; i < 4; i++)
				{
					children[i].Clear();
				}
			}
			subdivided = false;
			children = null;
		}

		public bool Insert(T item)
		{
			if (!bounds.HasPoint(item.GlobalPosition))
				return false;

			if (items.Count < capacity || maxDepth == 0)
			{
				items.Add(item);
				return true;
			}
			else
			{
				if (!subdivided)
					Subdivide();

				for (int i = 0; i < 4; i++)
				{
					if (children[i].Insert(item))
						return true;
				}
			}

			return false;
		}

		private void Subdivide()
		{
			Vector2 size = bounds.Size / 2;
			Vector2 pos = bounds.Position;

			children = new Quadtree<T>[4];
			children[0] = new Quadtree<T>(new Rect2(pos, size), capacity, maxDepth - 1);
			children[1] = new Quadtree<T>(new Rect2(pos + new Vector2(size.X, 0), size), capacity, maxDepth - 1);
			children[2] = new Quadtree<T>(new Rect2(pos + new Vector2(0, size.Y), size), capacity, maxDepth - 1);
			children[3] = new Quadtree<T>(new Rect2(pos + size, size), capacity, maxDepth - 1);
			subdivided = true;

			foreach (var item in items)
			{
				for (int i = 0; i < 4; i++)
				{
					if (children[i].Insert(item))
						break;
				}
			}
			items.Clear();
		}

		public List<T> Query(Rect2 range)
		{
			List<T> found = new List<T>();
			Query(range, found);
			return found;
		}

		private void Query(Rect2 range, List<T> found)
		{
			if (!bounds.Intersects(range))
				return;

			foreach (var item in items)
			{
				if (range.HasPoint(item.GlobalPosition))
					found.Add(item);
			}

			if (subdivided && children != null)
			{
				for (int i = 0; i < 4; i++)
				{
					children[i].Query(range, found);
				}
			}
		}
	}
}
