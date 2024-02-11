using Godot;
using System;


//**** I dislike that Godot forces you to inherit classes from Nodes
//**** making it almost impossible to create shared partial classes..
//**** it'll have to do for now..
public partial class Global : Node {

    // current directory
    string curDir = System.AppDomain.CurrentDomain.BaseDirectory;

    // easy access standards
    public float stressTick = 9f;

    // input values
    public bool inputPressed = false;

    // variable names for elements and nodes
    public Main main;
    public StartRoundBtn startRoundBtn;
    public Label roundTimerLabel,
        userScoreLabel, oppScoreLabel;
    public AnimPack animPack;
    public Node2D userScoreOrigin, oppScoreOrigin;




    public override void _Ready() {

        // binding elements and nodes
        main = (Main)GetNode("/root/Main");
        startRoundBtn = (StartRoundBtn)GetNode("/root/Main/StartRoundBtn");
        // labels
        roundTimerLabel = (Label)GetNode("/root/Main/LabelGroup/RoundTimerLbl");
        userScoreLabel = (Label)GetNode("/root/Main/LabelGroup/UserScoreLbl");
        oppScoreLabel = (Label)GetNode("/root/Main/LabelGroup/OppScoreLbl");
        userScoreOrigin = (Node2D)GetNode("/root/Main/LabelGroup/UserScoreOrigin");
        oppScoreOrigin = (Node2D)GetNode("/root/Main/LabelGroup/OppScoreOrigin");
        

        // animation playes
        animPack = (AnimPack)GetNode("/root/Main/AnimPack");


        // instancing variables
        stressTickCount = stressTick;

    }




    // related variables
    public int stepCount = 0;
    public float stressTickCount;
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        // if round is active
        if (!main.roundTimer.IsStopped()) {
            // display time left
            roundTimerLabel.Text = main.roundTimer.TimeLeft.ToString("f1") + "s";
            userScoreLabel.Text = main.userScore.ToString();


            // check for and play saved score
            if (main.savedSteps.Count >= 1 && main.scoreSaved) {
                
                // take saved steps at consecutive timeStamps
                while (main.roundTimer.TimeLeft <= main.savedSteps[stepCount].timeStamp) {

                    int oppPoints = main.savedSteps[stepCount].stepPoints;
                    main.oppScore += oppPoints;

                    // display opponents score
                    oppScoreLabel.Text = main.oppScore.ToString();

                    
                    // animation
                    animPack.ScalePop("OppScore");

                        ScoreFeedback scoreInstance = (ScoreFeedback)main.scoreFeedback.Instance();

                        oppScoreOrigin.AddChild(scoreInstance);

                        scoreInstance.label.Text = oppPoints > 0 ? "+" + oppPoints : oppPoints.ToString();

                        animPack.ScorePathTween(scoreInstance);


                    // if there are more steps, loop
                    if (stepCount < main.savedSteps.Count) {
                        stepCount++;
                        continue;
                    } else {
                        return;
                    }
                }

                
            }

            // countdown stressTick animation
            while(main.roundTimer.TimeLeft <= stressTickCount + .04f) {
                
                // animation
                animPack.ScalePopMin("RoundTimer");

                // do this until zero
                if (stressTickCount >= 0) {
                    stressTickCount--;
                    continue;
                } else {
                    return;
                }
            }
        }



        // ON_RELEASE actions if inputPressed is true
        // ***I should split this, so it does not loop through simon buttons unnecessarily***
        if (inputPressed && !Input.IsActionPressed("on_pressed")) {
            
            inputPressed = false;

            // check and change button state for all simon buttons
            foreach (Main.SimonButton simonButtonList in main.simonButtonList) {

                if (simonButtonList.el.sprite.Animation == "red" || simonButtonList.el.sprite.Animation == "pressed") {
                    simonButtonList.el.sprite.Animation = "idle";
                    animPack.SyncScaleTween(simonButtonList.el, 1f, 1f);
                    
                }
            }

            // check and change button state for StartRound button
            if (startRoundBtn.sprite.Animation != "idle") {
                startRoundBtn.Released();
            }
        }
    }
}
