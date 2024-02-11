using Godot;
using System;

public class UserPolygonGroup : RigidBody2D {

    [Signal]
	public delegate void Hit();

    public RigidBody2D col;

    public override void _Ready() {
        col = (RigidBody2D)GetNode("/root/Main/UserPolygonGroup");
        this.Connect("Hit", this, "_on_Point_Hit");
    }

    public void _on_Point_Hit(RigidBody2D el) {
        // EmitSignal("Hit");
        GD.Print("worked");
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
