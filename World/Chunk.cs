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

	public override void _Ready() {
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
				Block block = null;
				var noise = Noise.GetNoise2d(columnI, blockI);

				if (blockI == groundLevel - 1) {
					block = new Grass();
				} else if (noise < -0.1) {
					block = new Stone();
				} else {
					block = new Dirt();
				}

				if (block != null) {
					var blockInstance = Chunk.BlockScene.Instance<Block>();
					blockInstance.Color = block.Color;
					blockInstance.Position = new Vector2(
						columnI * Block.Width,
						// Height minus 1 since we position blocks by their top left corner.
						(Chunk.ChunkBlockHeight - 1 - blockI) * Block.Height
					);
					AddChild(blockInstance);
				}
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event) {
		base._UnhandledInput(@event);
		if (@event is InputEventKey eventKey) {
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Space) {
				this._RemoveAllChildNodes();
				this._GenerateNoise();
				this._RenderBlocks();
			}
		}
	}

	private void _RemoveAllChildNodes() {
		foreach (Block block in this.GetChildren()) {
			block.QueueFree();
		}
	}
}
