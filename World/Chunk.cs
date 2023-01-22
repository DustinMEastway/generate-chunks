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

	private int[][] _GenerateArray(long width, long height, bool empty) {
		int[][] map = new int[width][];
		for (int columnI = 0; columnI < width; columnI++) {
			map[columnI] = new int[height];
			for (int blockI = 0; blockI < height; blockI++) {
				map[columnI][blockI] = empty ? 0 : 1;
			}
		}

		return map;
	}

	private int[][] _TerrainGeneration(int[][] map, long width, long height) {
		int noiseHeight;
		for (int columnI = 0; columnI < width; columnI++) {
			noiseHeight = Mathf.RoundToInt(Noise.GetNoise2d((columnI / Smoothness), Seed) * (height * 2));
			// noiseHeight += Mathf.RoundToInt(height / 2);
			for (int blockI = 0; (blockI < height && blockI < noiseHeight); blockI++) {
				map[columnI][blockI] = 1;
			}
		}

		return map;
	}

	private void _RenderBlocks() {
		long width = Chunk.ChunkBlockWidth;
		long height = Chunk.DefaultGroundLevel / 2;
		int[][] map;

		// generate underground layer
		int[][] undergroundMap = _GenerateArray(width, height, false);

		// generate ground layer
		int[][] groundMap = _GenerateArray(width, height, true);
		groundMap = _TerrainGeneration(groundMap, width, height);

		// stack layers
		map = new int[undergroundMap.Length][];
		undergroundMap.CopyTo(map, 0);
		for (int columnI = 0; columnI < groundMap.Length; columnI++) {
			int[] newColumn = new int[map[columnI].Length + groundMap[columnI].Length];
			map[columnI].CopyTo(newColumn, 0);
			groundMap[columnI].CopyTo(newColumn, undergroundMap[columnI].Length);
			map[columnI] = newColumn;
		}

		for (int columnI = 0; columnI < width; columnI++) {
			for (int blockI = 0; blockI < map[columnI].Length; blockI++) {
				Block block = null;
				float noise = Noise.GetNoise2d(columnI, blockI);
				int lastIndex = Array.FindLastIndex(map[columnI], i => i == 1);

				if (blockI < height || map[columnI][blockI] == 1) {
					if (lastIndex == blockI) {
						block = new Grass();
					} else if (noise < -0.1) {
						block = new Stone();
					} else {
						block = new Dirt();
					}
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
