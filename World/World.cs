using Godot;
using System;

public class World : Node2D {
	public static readonly PackedScene ChunkScene = ResourceLoader.Load<PackedScene>("res://World/Chunk.tscn");
	public static readonly PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Player/Player.tscn");
	private Player _Player = null;
	private Vector2 _SpawnPoint = new Vector2(170, 130);

	public override void _Ready() {
		_SpawnChunk();
		_SpawnPlayer();
	}

	private void _SpawnChunk() {
		AddChild(World.ChunkScene.Instance<Chunk>());
	}

	private void _SpawnPlayer() {
		_Player = World.PlayerScene.Instance<Player>();
		_Player.Position = _SpawnPoint;
		AddChild(_Player);
	}
}
