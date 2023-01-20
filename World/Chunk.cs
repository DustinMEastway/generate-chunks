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
		for (int x = 0; x < width; x++) {
			map[x] = new int[height];
			for (int y = 0; y < height; y++) {
				map[x][y] = empty ? 0 : 1;
			}
		}

		return map;
	}

	private int[][] _TerrainGeneration(int[][] map, long width, long height) {
		int noiseHeight;
		for (int x = 0; x < width; x++) {
			noiseHeight = Mathf.RoundToInt(Noise.GetNoise2d((x / Smoothness), Seed) * (height));
			// noiseHeight += Mathf.RoundToInt(height / 2);
			for (int y = 0; y < noiseHeight; y++) {
				map[x][y] = 1;
			}
		}

		return map;
	}

	private void _RenderBlocks() {
		long width = Chunk.ChunkBlockWidth;
		long height = Chunk.DefaultGroundLevel;
		int[][] map;

		// generate underground layer
		int[][] undergroundMap = _GenerateArray(width, height / 2, false);

		// generate ground layer
		int[][] groundMap = _GenerateArray(width, height / 2, true);
		groundMap = _TerrainGeneration(groundMap, width, height);

		// stack layers
		map = new int[undergroundMap.Length][];
		undergroundMap.CopyTo(map, 0);
		for (int columnId = 0; columnId < groundMap.Length; columnId++) {
			int[] newColumn = new int[map[columnId].Length + groundMap[columnId].Length];
			map[columnId].CopyTo(newColumn, 0);
			groundMap[columnId].CopyTo(newColumn, undergroundMap[columnId].Length);
			map[columnId] = newColumn;
		}

		long undergroundHeight = height / 2;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < map[x].Length; y++) {
				Block block = null;
				int lastIndex = Array.FindLastIndex(map[x], i => i == 1);

				// render underground layer
				if (y < undergroundHeight) {
					if (lastIndex == y) {
						block = new Grass();
					} else {
						var noise = Noise.GetNoise2d(x, y);
						if (noise < -0.1) {
							block = new Stone();
						} else {
							block = new Dirt();
						}
					}
					// render ground layer
				} else if (map[x][y] == 1) {
					if (lastIndex == y) {
						block = new Grass();
					} else {
						block = new Dirt();
					}
				}

				if (block != null) {
					var blockInstance = Chunk.BlockScene.Instance<Block>();
					blockInstance.Color = block.Color;
					blockInstance.Position = new Vector2(
						x * Block.Width,
						// Height minus 1 since we position blocks by their top left corner.
						(Chunk.ChunkBlockHeight - 1 - y) * Block.Height
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
