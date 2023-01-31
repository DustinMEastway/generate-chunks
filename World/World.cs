using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class World : Node2D {
	public static readonly PackedScene ChunkScene = ResourceLoader.Load<PackedScene>("res://World/Chunk.tscn");
	public static readonly OpenSimplexNoise Noise = new OpenSimplexNoise();
	public static readonly PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Player/Player.tscn");
	public static readonly Random Random = new Random();
	public static int Seed { get; private set; } = Random.Next();
	public static int WorldChunkWidth = 3;
	private Camera2D _Camera;
	private Player _Player = null;
	private Dictionary<int, Chunk> _RenderedChunks = new Dictionary<int, Chunk>();
	private Vector2 _SpawnPoint = new Vector2(
		(Chunk.ChunkBlockWidth / 2) * Chunk.BlockWidth,
		(Chunk.ChunkBlockHeight / 4) * Chunk.BlockHeight
	);

	public override void _Ready() {
		World.Noise.Octaves = 4;
		World.Noise.Period = 20.0f;
		World.Noise.Persistence = 0.8f;
		_Camera = GetNode<Camera2D>("Camera");
		_SpawnChunk(-1);
		_SpawnChunk(0);
		_SpawnChunk(1);
		_SpawnPlayer();
	}

	private void _OnPlayerEnteredChunk(Player player, Chunk chunk) {
		// Render chunks around the player
		var maxChunk = chunk.ChunkOffset + 2;
		var minChunk = chunk.ChunkOffset - 2;
		for (int i = minChunk; i <= maxChunk; ++i) {
			CallDeferred(nameof(_SpawnChunk), i);
		}

		// Remove chunks far from the player
		var farAwayChunkOffsets = _RenderedChunks.Keys.Where((offset) => {
			return offset > maxChunk || offset < minChunk;
		}).ToArray();
		foreach (var chunkOffset in farAwayChunkOffsets) {
			RemoveChild(_RenderedChunks[chunkOffset]);
			_RenderedChunks.Remove(chunkOffset);
		}
	}

	private void _SpawnChunk(int chunkOffset) {
		if (_RenderedChunks.ContainsKey(chunkOffset)) {
			return;
		}

		var chunk = World.ChunkScene.Instance<Chunk>();
		var id = chunkOffset % World.WorldChunkWidth;
		chunk.ChunkId = (id < 0) ? id + World.WorldChunkWidth : id;
		chunk.ChunkOffset = chunkOffset;
		chunk.Position = new Vector2(chunkOffset * Chunk.ChunkBlockWidth * Chunk.BlockWidth, 0);
		chunk.Connect(nameof(Chunk.PlayerEntered), this, nameof(_OnPlayerEnteredChunk));
		_RenderedChunks.Add(chunkOffset, chunk);
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
		World.Seed = Random.Next();
		World.Noise.Seed = Seed;
		foreach (var renderedChunk in _RenderedChunks.Values) {
			renderedChunk.RenderBlocks();
		}
	}
}
