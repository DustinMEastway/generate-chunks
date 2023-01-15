using Godot;
using System;

public class World : Node2D {
	public static readonly PackedScene ChunkScene = ResourceLoader.Load<PackedScene>("res://World/Chunk.tscn");
	public static readonly PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://Player/Player.tscn");
	private Camera2D _Camera;
	private Player _Player = null;
	private Vector2 _SpawnPoint = new Vector2(
		(Chunk.ChunkBlockWidth / 2) * Block.Width,
		(Chunk.ChunkBlockHeight - Chunk.DefaultGroundLevel - Player.BlockHeight) * Block.Height
	);

	public override void _Ready() {
		_Camera = GetNode<Camera2D>("Camera");
		_SpawnChunk();
		_SpawnPlayer();
	}

	private void _SpawnChunk() {
		AddChild(World.ChunkScene.Instance<Chunk>());
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
}
