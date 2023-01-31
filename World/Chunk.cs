using Godot;
using System;

public class BlockId {
	public int Dirt = -1;
	public int Grass = -1;
	public int Stone = -1;

	public BlockId(
		TileMap tileMap
	) {
		Dirt = _GetByName(tileMap, "dirt");
		Grass = _GetByName(tileMap, "grass");
		Stone = _GetByName(tileMap, "stone");
	}

	private int _GetByName(TileMap tileMap, string name) {
		int id = tileMap.TileSet.FindTileByName(name);
		if (id == -1) {
			throw new Exception($"TileMap does not contain tile with name '{name}'.");
		}
		return id;
	}
}

public class Chunk : Area2D {
	public static readonly int BlockHeight = 6;
	public static readonly int BlockWidth = 6;
	public static readonly Vector2 BlockSize = new Vector2(
		Chunk.BlockWidth,
		Chunk.BlockHeight
	);
	public static readonly int ChunkBlockHeight = 64;
	public static readonly int ChunkBlockWidth = 64;
	public static readonly Vector2 ChunkBlockSize = new Vector2(
		Chunk.ChunkBlockWidth,
		Chunk.ChunkBlockHeight
	);
	public static readonly int DefaultGroundLevel = 32;
	public int ChunkId;
	public bool IsReady { get; private set; } = false;
	public int ChunkOffset;
	[Signal]
	public delegate void PlayerEntered(Player player, Chunk chunk);
	public float Smoothness = 4;
	public TileMap TileMap;
	private BlockId _BlockId;
	private int _BlockOffset;

	public override void _Ready() {
		base._Ready();
		TileMap = GetNode<TileMap>("TileMap");
		var collisionShape = GetNode<CollisionShape2D>("CollisionShape");
		var halfChunkSize = Chunk.ChunkBlockSize * Chunk.BlockSize / 2;
		collisionShape.Position = halfChunkSize;
		(collisionShape.Shape as RectangleShape2D).Extents = halfChunkSize;
		_BlockId = new BlockId(TileMap);
		_BlockOffset = ChunkId * Chunk.ChunkBlockWidth;
		IsReady = true;
		RenderBlocks();
	}

	public void _OnChunkBodyEntered(Node node) {
		if (node is Player) {
			EmitSignal(nameof(PlayerEntered), node, this);
		}
	}

	public void RenderBlocks() {
		if (!IsReady) {
			return;
		}

		TileMap.Clear();
		for (var columnI = 0; columnI < Chunk.ChunkBlockWidth; columnI++) {
			var groundLevel = Chunk.DefaultGroundLevel + Mathf.RoundToInt(
				World.Noise.GetNoise2d(
					(_BlockOffset + columnI) / Smoothness,
					World.Seed
				) * Chunk.DefaultGroundLevel
			);

			for (var blockI = 0; blockI < groundLevel; blockI++) {
				var x = columnI;
				var y = Chunk.ChunkBlockHeight - blockI - 1;
				TileMap.SetCell(
					x,
					y,
					_GetCellId(columnI, blockI, groundLevel)
				);
			}
		}
	}

	private int _GetCellId(
		int columnI,
		int blockI,
		int groundLevel
	) {
		if (blockI == groundLevel - 1) {
			return _BlockId.Grass;
		}

		var noise = World.Noise.GetNoise2d(
			_BlockOffset + columnI,
			blockI
		);
		if (noise < -0.1) {
			return _BlockId.Stone;
		}

		return _BlockId.Dirt;
	}
}
