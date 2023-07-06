using Godot;

public partial class Camera : Camera2D {
	public override void _Ready() {
		LimitBottom = Chunk.BlockHeight * Chunk.ChunkBlockHeight;
		LimitTop = 0;
	}
}
