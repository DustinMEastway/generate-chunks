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

public class Chunk : Node2D {
	public static Random Ran = new Random();
	public static readonly int BlockHeight = 6;
	public static readonly int BlockWidth = 6;
	public static readonly int ChunkBlockHeight = 64;
	public static readonly int ChunkBlockWidth = 64;
	public static readonly int DefaultGroundLevel = 32;
	public OpenSimplexNoise Noise = new OpenSimplexNoise();
	public int Seed = Ran.Next();
	public float Smoothness = 4;
	public TileMap TileMap;
	private BlockId _BlockId;

	public override void _Ready() {
		TileMap = GetNode<TileMap>("TileMap");
		_BlockId = new BlockId(TileMap);
		_GenerateNoise();
		_RenderBlocks();
	}

	private void _GenerateNoise() {
		Seed = Ran.Next();
		Noise.Seed = Seed;
		Noise.Octaves = 4;
		Noise.Period = 20.0f;
		Noise.Persistence = 0.8f;
	}

	private void _RenderBlocks() {
		for (var columnI = 0; columnI < Chunk.ChunkBlockWidth; columnI++) {
			var groundLevel = Chunk.DefaultGroundLevel + Mathf.RoundToInt(
				Noise.GetNoise2d((columnI / Smoothness), Seed) * Chunk.DefaultGroundLevel
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

		var noise = Noise.GetNoise2d(columnI, blockI);
		if (noise < -0.1) {
			return _BlockId.Stone;
		}

		return _BlockId.Dirt;
	}

	public override void _UnhandledInput(InputEvent @event) {
		base._UnhandledInput(@event);
		if (@event is InputEventKey eventKey) {
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space) {
				TileMap.Clear();
				this._GenerateNoise();
				this._RenderBlocks();
			}
		}
	}
}
