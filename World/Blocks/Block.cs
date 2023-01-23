using Godot;
using System;

public class Block : StaticBody2D {
	public static readonly long Height = 6;
	public static readonly long Width = 6;
	public static readonly Vector2 Size = new Vector2(Block.Width, Block.Height);
	public static readonly Color DefaultColor = new Color(1, 1, 1);

	/// <summary>Color to use for the block</summary>
	public Color Color;

	public Block() : this(Block.DefaultColor) { }

	public Block(Color color) {
		Color = color;
	}

	public override void _Ready() {
		var collisionShape = GetNode<CollisionShape2D>("CollisionShape");
		var colorRect = GetNode<ColorRect>("ColorRect");
		colorRect.Color = Color;
		colorRect.RectSize = Block.Size;
		(collisionShape.Shape as RectangleShape2D).Extents = Block.Size;
		collisionShape.Position = Block.Size / 2;
	}
}
