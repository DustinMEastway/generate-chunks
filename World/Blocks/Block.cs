using Godot;
using System;

public class Block : StaticBody2D {
	/// <summary>Color to use for the block</summary>
	[Export]
	public Color Color = new Color(1, 1, 1);

	public override void _Ready() {
		GetNode<ColorRect>("ColorRect").Color = Color;
	}
}
