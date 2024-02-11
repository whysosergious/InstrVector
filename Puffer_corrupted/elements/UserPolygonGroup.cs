using Godot;
using System;

public class UserPolygonGroup : RigidBody2D {

    [Signal]
	public delegate void Hit();

    public override void _Ready() {
        
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
