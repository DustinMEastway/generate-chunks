using Godot;
using System;
using System.Collections.Generic;

public class World : Node2D {
	public static readonly PackedScene ChunkScene = ResourceLoader.Load<PackedScene>("res://World/Chunk.tscn");
	public static readonly OpenSimplexNoise Noise = new OpenSimplexNoise();
	public static readonly PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Player/Player.tscn");
	public static readonly Random _Random = new Random();
	public static int Seed { get; private set; } = _Random.Next();
	private Camera2D _Camera;
	private Player _Player = null;
	private Dictionary<int, Chunk> _RenderedChunks = new Dictionary<int, Chunk>();
	private Vector2 _SpawnPoint = new Vector2(
		(Chunk.ChunkBlockWidth / 2) * Chunk.BlockWidth,
		(Chunk.ChunkBlockHeight / 4) * Chunk.BlockHeight
	);

	public override void _Ready() {
		_ChangeSeed();
		_Camera = GetNode<Camera2D>("Camera");
		_SpawnChunk(-1);
		_SpawnChunk(0);
		_SpawnChunk(1);
		_SpawnPlayer();
	}

	private void _SpawnChunk(int i) {
		var chunk = World.ChunkScene.Instance<Chunk>();
		chunk.ChunkId = i;
		_RenderedChunks.Add(i, chunk);
		AddChild(chunk);
	}

	private void _SpawnPlayer() {
		_Player = World.PlayerScene.Instance<Player>();
		_Player.Position = _SpawnPoint;
		// Attach camera to the player.
		var cameraTransform = new RemoteTransform2D();
		cameraTransform.RemotePath = _Camera.GetPath();
		_Player.AddChild(cameraTransform);
		AddChild(_Player);
	}

	public override void _UnhandledInput(InputEvent @event) {
		base._UnhandledInput(@event);
		if (@event is InputEventKey eventKey) {
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space) {
				_ChangeSeed();
			}
		}
	}

	private void _ChangeSeed() {
		World.Seed = _Random.Next();
		World.Noise.Seed = Seed;
		World.Noise.Octaves = 4;
		World.Noise.Period = 20.0f;
		World.Noise.Persistence = 0.8f;
		foreach (var renderedChunk in _RenderedChunks.Values) {
			renderedChunk.RenderBlocks();
		}
	}
}
