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
	public const long ChunkBlockHeight = 32;
	public static readonly long ChunkBlockWidth = 64;
	public static readonly long DefaultGroundLevel = 18;
	[Export]
	public BiomeId BiomeId = BiomeId.Forest;
	public Block[][] Blocks = null;
	[Export(PropertyHint.Range, "0,1024,1")]
	public long GroundLeft = -1;
	[Export(PropertyHint.Range, "0,1024,1")]
	public long GroundRight = -1;

	public override void _Ready() {
		_RenderBlocks();
	}

	private Block[][] _GenerateBlocks() {
		var blocks = new Block[Chunk.ChunkBlockWidth][];
		var groundLevel = (GroundLeft >= 0) ? GroundLeft : Chunk.DefaultGroundLevel;
		for (long columnI = 0; columnI < blocks.Length; ++columnI) {
			var column = new Block[groundLevel];
			for (long blockI = 0; blockI < column.Length; ++blockI) {
				Block block = null;
				if (blockI < groundLevel - 3) {
					block = new Stone();
				} else if (blockI < groundLevel - 1) {
					block = new Dirt();
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
		long baseY = Block.Height * Chunk.ChunkBlockHeight;

		for (long columnI = 0; columnI < Blocks.Length; ++columnI) {
			var column = Blocks[columnI];
			for (long blockI = 0; blockI < column.Length; ++blockI) {
				var blockData = column[blockI];
				var block = Chunk.BlockScene.Instance<Block>();
				block.Color = blockData.Color;
				block.Position = new Vector2(
					columnI * Block.Width,
					baseY - blockI * Block.Height
				);
				AddChild(block);
			}
		}
	}
}
