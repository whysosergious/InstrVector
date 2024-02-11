using Godot;
using System;

public class StartRoundBtn : RigidBody2D {

    // elements, nodes and scripts
    public Global g;
    public AnimatedSprite sprite;
    public Label label;

    // defining original values
    public Vector2 originPosition;

    public override void _Ready() {

        // setting original values
        originPosition = this.Position;
        
        // global script
        g = (Global)GetNode("/root/Global");

        // local elements and nodes
        sprite = (AnimatedSprite)GetNode("Sprite");
        label = (Label)GetNode("Label");

    }




    // input events
    public void _on_StartRoundBtn_input_event(Viewport viewport, InputEvent ev, int shape_idx) {

        if (ev.IsActionPressed("on_pressed")) {
            g.inputPressed = true;

            Pressed();
        }
    }


    // button state methods
    public void Pressed() {
        // GD.Print("pressed");

        this.Translate(new Vector2(0,2f));
        sprite.Animation = "pressed";
        
    }

    public void Released() {
        // GD.Print("released");

        sprite.Animation = "idle";
        
        this.Position = originPosition;
        if (g.main.roundTimer.IsStopped()) {
            g.main.RoundTimer();
            label.Text = "Stop";
        } else {
            label.Text = "Start";
            g.main.ResetRound();
        }
    }




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
