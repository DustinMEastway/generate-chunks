using Godot;
using System;

public class Camera : Camera2D {
	public override void _Ready() {
		LimitBottom = (int)(Block.Height * Chunk.ChunkBlockHeight);
		LimitTop = 0;
	}
}
