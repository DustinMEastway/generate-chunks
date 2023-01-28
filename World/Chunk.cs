using Godot;
using System;

public class Chunk : Node2D {
	public static Random Ran = new Random();
	public static readonly PackedScene BlockScene = ResourceLoader.Load<PackedScene>("res://World/Blocks/Block.tscn");
	public static readonly long ChunkBlockHeight = 64;
	public static readonly long ChunkBlockWidth = 64;
	public static readonly long DefaultGroundLevel = 32;
	public OpenSimplexNoise Noise = new OpenSimplexNoise();
	public int Seed = Ran.Next();
	public float Smoothness = 4;
	public TileMap TileMap;

	public override void _Ready() {
		TileMap = GetNode<TileMap>("TileMap");
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
		int dirtTileId = TileMap.TileSet.FindTileByName("dirt");
		int grassTileId = TileMap.TileSet.FindTileByName("grass");
		int stoneTileId = TileMap.TileSet.FindTileByName("stone");

		for (var columnI = 0; columnI < Chunk.ChunkBlockWidth; columnI++) {
			var groundLevel = (int)Chunk.DefaultGroundLevel + Mathf.RoundToInt(
				Noise.GetNoise2d((columnI / Smoothness), Seed) * Chunk.DefaultGroundLevel
			);

			for (var blockI = groundLevel; blockI > 0; blockI--) {
				var noise = Noise.GetNoise2d(columnI, blockI);

				if (blockI == -1) {
					TileMap.SetCell(columnI, blockI, grassTileId);
				} else if (noise < -0.1) {
					TileMap.SetCell(columnI, blockI, stoneTileId);
				} else {
					TileMap.SetCell(columnI, blockI, dirtTileId);
				}
			}
		}
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
