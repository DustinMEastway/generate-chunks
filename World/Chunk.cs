using Godot;
using System;

public enum BiomeId {
	Forest = 1
}

public interface ChunkData {
	BiomeId BiomeId { get; set; }
	Block[][] Blocks { get; set; }
}

public class Chunk : Node2D {
	public static readonly PackedScene BlockScene = ResourceLoader.Load<PackedScene>("res://World/Blocks/Block.tscn");
	public static readonly long ChunkBlockHeight = 1024;
	public static readonly long ChunkBlockWidth = 64;
	public static readonly long DefaultGroundLevel = 768;
	[Export]
	public BiomeId BiomeId = BiomeId.Forest;
	public Block[][] Blocks = null;
	[Export(PropertyHint.Range, "0,1024,1")]
	public long GroundLeft = -1;
	[Export(PropertyHint.Range, "0,1024,1")]
	public long GroundRight = -1;
	public OpenSimplexNoise Noise = new OpenSimplexNoise();
	public int Seed = new Random().Next();

	public override void _Ready() {
		_GenerateNoise();
		_RenderBlocks();
	}

	private void _GenerateNoise() {
		Noise.Seed = Seed;
		Noise.Octaves = 4;
		Noise.Period = 20.0f;
		Noise.Persistence = 0.8f;
	}

	private Block[][] _GenerateBlocks() {
		var blocks = new Block[Chunk.ChunkBlockWidth][];
		var groundLevel = (GroundLeft >= 0) ? GroundLeft : Chunk.DefaultGroundLevel;
		for (long columnI = 0; columnI < blocks.Length; ++columnI) {
			var column = new Block[groundLevel];
			for (long blockI = 0; blockI < column.Length; ++blockI) {
				Block block = null;
				if (blockI < groundLevel - 1) {
					var noise = Noise.GetNoise2d(columnI, blockI);
					if (noise < -0.1) {
						block = new Stone();
					} else {
						block = new Dirt();
					}
				} else {
					block = new Grass();
				}

				column[blockI] = block;
			}
			blocks[columnI] = column;
		}

		return blocks;
	}

	private void _RenderBlocks() {
		Blocks = Blocks ?? _GenerateBlocks();
		for (long columnI = 0; columnI < Blocks.Length; ++columnI) {
			var column = Blocks[columnI];
			for (long blockI = 0; blockI < column.Length; ++blockI) {
				var blockData = column[blockI];
				var block = Chunk.BlockScene.Instance<Block>();
				block.Color = blockData.Color;
				block.Position = new Vector2(
					columnI * Block.Width,
					// Height minus 1 since we position blocks by their top left corner.
					(Chunk.ChunkBlockHeight - 1 - blockI) * Block.Height
				);
				AddChild(block);
			}
		}
	}
}
