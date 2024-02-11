using Godot;
using System;

public class Point : RigidBody2D {

    [Signal]
	public delegate void Hit();

    public RigidBody2D col;

    public override void _Ready() {
        col = (RigidBody2D)GetNode("/root/Main/UserPolygonGroup");
        // col.Connect("Hit", col, "_on_Point_Hit");
        // GD.Print(col.GetSignalConnectionList("Hit"));
        this.Connect("Hit", this, "_on_Point_Hit");
        // GD.Print(this.GetSignalConnectionList("Hit"));
        
        
    }

    public void _on_Point_Hit(RigidBody2D el) {
        EmitSignal("Hit");
        GD.Print("worked");
        GD.Print(this.GetCollidingBodies());
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta) {
    // GD.Print(this.GetCollidingBodies());
 }
}
