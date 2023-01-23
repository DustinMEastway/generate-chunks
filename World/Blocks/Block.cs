using Godot;
using System;

public class Block : StaticBody2D {
	public static readonly long Height = 10;
	public static readonly long Width = 10;
	public static readonly Color DefaultColor = new Color(1, 1, 1);

	/// <summary>Color to use for the block</summary>
	public Color Color;

	public Block() : this(Block.DefaultColor) {}

	public Block(Color color) {
		Color = color;
	}

	public override void _Ready() {
		GetNode<ColorRect>("ColorRect").Color = Color;
	}
}
