using Godot;
using System;

public class Player : KinematicBody2D {
	public static readonly long BlockHeight = 2;
	/// <summary>How quickly the player's <see cref="Velocity"> gets up to <see cref="MaxSpeed"></summary>
	[Export]
	public float Acceleration = 500;
	/// <summary>Max distance per second the player can travel at.</summary>
	[Export]
	public float MaxSpeed = 500;
	private Vector2 _InputDirection = Vector2.Zero;

	/// <summary>How quickly the player's <see cref="Velocity"> slows down to 0.</summary>
	public float Friction {
		get => Acceleration;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	/// <summary>Current distance per second the player is travelling at.</summary>
	public Vector2 Velocity { get; private set; }

	/// <inheritdoc />
	public override void _PhysicsProcess(float delta) {
		StateMove(delta);
	}

	private void StateMove(float delta) {
		_InputDirection = new Vector2(
			Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
			Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up")
		).Normalized();

		if (_InputDirection != Vector2.Zero) {
			Velocity = Velocity.MoveToward(_InputDirection * MaxSpeed, Acceleration * delta);
		} else {
			Velocity = Velocity.MoveToward(Vector2.Zero, Friction * delta);
		}

		Velocity = MoveAndSlide(Velocity);
	}
}
