using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public partial class Main : Node2D {

	[Signal]
	public delegate void Hit();

	private Random r = new Random();
	public PackedScene scoreFeedback = (PackedScene)GD.Load("res://elements/ScoreFeedback.tscn");
	public PackedScene pointNode = (PackedScene)GD.Load("res://elements/Point.tscn");


	// test variables
	public string test4 = "added to list in Main";

	
	// timer settings
	//-- round timer duration in seconds
	//-- (to use decimals, 'f' at the end of the given number is obligatory[i.e. .7f || 44.44f])
	public float roundDuration = 10f;
	//-- delay span for simon says timer in ms
	public int[] simonSaysDelaySpan = { 50, 1200 };


	// standard variables
	public int userScore = 0;
	public int oppScore = 0;

	// variable names for elements, nodes and scripts
	public Global g;




	


	public override void _Ready() {

		// binding global script
		g = (Global)GetNode("/root/Global");

		// timers and everything related
		//-- round timer
		roundTimer = new Timer();
		roundTimer.WaitTime = roundDuration;
		roundTimer.OneShot = true;
		AddChild(roundTimer);
		roundTimer.Connect("timeout", this, nameof(RoundTimeout));
		//-- timer for simon buttons with random delay(time span set in method)
		simonSaysTimer = new Timer();
		simonSaysTimer.OneShot = true;
		AddChild(simonSaysTimer);
		//-- creating dynamic
		simonSaysTimer.Connect("timeout", this, nameof(SimonSaysTimeout));

		
		// bind value thats referenced more than once
		int simonButtonCount = simonButtonList.Count;
		// add individual button count to array
		//-- this is used as a reset
		idleButtons = new int[simonButtonCount];
		for (int i = 1; i <= simonButtonCount; i++) {
			idleButtons[i-1] = i;
		}
		idleButtonList.AddRange(idleButtons);

		
		// testing
		try {
			scoreSaved = true;

			FileStream inStream = new FileStream("usrdata.set", FileMode.Open, FileAccess.Read);
			BinaryReader reader = new BinaryReader(inStream);

			int dataStepsCount = reader.ReadInt32();

			GD.Print($"steps count: {dataStepsCount}");

			savedSteps.Clear();

			for (int i = 0; i < dataStepsCount; i++) {
				float dataTimeStamp = reader.ReadSingle();
				int dataStepPoints = reader.ReadInt32();

				GD.Print($"loaded: {dataStepPoints} at {dataTimeStamp}s");

				ActionStep saveStep = new ActionStep(dataTimeStamp, dataStepPoints);

				savedSteps.Add(saveStep);
			}
			
			reader.Dispose();

		} catch (System.IO.FileNotFoundException) {
			GD.Print("No user data found");
		} catch (Exception e) {
			GD.Print($"Failed to load data: {e}");
		}
		
		
	}


	public async void PointAnimate(RigidBody2D el) {

		

		float tempVel = -120f;

		bool move = true;

		Timer t = new Timer();
		AddChild(t);
		t.OneShot = true;
		t.WaitTime = 1f;
		
		Vector2 tgt = g.userScoreOrigin.Position;

		double radians = Math.Atan2(el.Position.y - g.userScoreOrigin.Position.y, el.Position.x - g.userScoreOrigin.Position.x);

		float angle = (float)(radians * (180/Math.PI));

		double origDeltaX = Math.Pow((tgt.x - el.GlobalPosition.x), 2);
		double origDeltaY = Math.Pow((tgt.y - el.GlobalPosition.y), 2);

		double origDistance = Math.Sqrt(origDeltaX + origDeltaY);

		double origX = el.Position.x;
		double origY = el.Position.y;
		// el.AngularVelocity = 7f;

		while (move) {
			
			t.Start();


			

			double deltaX = Math.Pow((tgt.x - el.GlobalPosition.x), 2);
			double deltaY = Math.Pow((tgt.y - el.GlobalPosition.y), 2);

			double distance = Math.Sqrt(deltaX + deltaY);

			

			// x = 0 + el.Position.x * Math.Cos(angle * (Math.PI / 180));
            // y = 0 + el.Position.y * Math.Sin(angle * (Math.PI / 180));

            // origX = (float)x / 3;
            // origY = (float)y / 3;

			radians = Math.Atan2(el.Position.y - tgt.y, el.Position.x - tgt.x) - 90;

			if ((origX - el.Position.x) + (origY - el.Position.y) > 100 || (origX - el.Position.x) + (origY - el.Position.y) < -100) {
				el.Rotation = (float)radians;
			
				el.Rotation += (float)((el.Rotation - radians) / 2 - el.AngularVelocity + .1f);
			} 
			if ((origX - el.Position.x) + (origY - el.Position.y) < 100 || (origX - el.Position.x) + (origY - el.Position.y) > -100) {
				el.Rotation += (float)el.Position.x >= 180 ? -.05f : .05f;
			}
			

			

			

			el.LinearVelocity = new Vector2(0f, tempVel).Rotated(el.Rotation);
			await ToSignal(t, "timeout");

			t.WaitTime = .04f;

			

			tempVel -= 10f;
			
			continue;
		}

		t.QueueFree();
	}

	
	//****
	//____ Timers, timer methods and everything related ____
	//_____________________________________________________________
	//
	// defining timer names
	public Timer roundTimer, simonSaysTimer;

	// idle buttons array and list
	public int[] idleButtons;
	public List<int> idleButtonList = new List<int>();

	// main round timer
	public void RoundTimer() {

		// reset values
		userScore = 0;
		oppScore = 0;
		g.oppScoreLabel.Text = oppScore.ToString();

		currSteps.Clear();
		idleButtonList.Clear();
		idleButtonList.AddRange(idleButtons);

		// just a separator for console
		// GD.Print("-------------------_________________");

		roundTimer.Start();
		SimonSaysTimer();

	}

	// standard value if no highscore is set
	public bool scoreSaved = false;

	public void RoundTimeout() {

		ResetRound();

		// add final step
		ActionStep finalStep = new ActionStep(-1,0);
		currSteps.Add(finalStep);

		// save steps to 'savedSteps' list if a new or no highscore is set
		if (userScore > oppScore || !scoreSaved) {
			scoreSaved = true;

			// create or update user data file
			try {
				FileStream outStream = new FileStream("usrdata.set", FileMode.Create, FileAccess.Write);
				BinaryWriter writer = new BinaryWriter(outStream);

				writer.Write((Int32)currSteps.Count);
				
				GD.Print($"steps saved: {currSteps.Count}");

				for (int i = 0; i < currSteps.Count; i++) {
					writer.Write((Single)currSteps[i].timeStamp);
					writer.Write((Int32)currSteps[i].stepPoints);
				}
				
				writer.Dispose();

			} catch (Exception e) {
				GD.Print($"Failed to save data: {e}");
			}

			savedSteps.Clear();

			for (int i = 0; i < currSteps.Count; i++) {
				ActionStep saveStep = new ActionStep(currSteps[i].timeStamp, currSteps[i].stepPoints);

				savedSteps.Add(saveStep);
			}

			
		}
	}


	// resets timers and buttons
	public void ResetRound() {


		// stop all local timers
		simonSaysTimer.Stop();
		roundTimer.Stop();

		// stop and reset all simon buttons
		for (int i = 1; i <= idleButtons.Length; i++) {
			simonButtonList[i-1].el.ResetState();
		}

		// reset button label and round values
		g.startRoundBtn.label.Text = "Start";
		g.stepCount = 0;
		g.stressTickCount = g.stressTick;
	}


	// if set to 'true', does not restart the timer when no Simon buttons are available
	public bool simonSaysWait = false;

	// timer that activates random available buttons
	public void SimonSaysTimer() {

		int[] idleButtonArray = idleButtonList.ToArray();
		int idleButtonArrayLength = idleButtonArray.Length;
		

		if (idleButtonArrayLength > 0) {

			int[] randomValues = GenRandom(idleButtonArray, idleButtonArrayLength);
			float randomDelay = (float)randomValues[0] / 1000;
			int randomBtnNr = randomValues[1];

			simonSaysTimer.WaitTime = randomDelay;

			simonButtonList[randomBtnNr-1].el.ButtonStateTimer();
			idleButtonList.Remove(randomBtnNr);

			simonSaysTimer.Start();            

			

		} else {
			simonSaysWait = true;
		}

	}

	public void SimonSaysTimeout() {
		if (simonSaysTimer.IsStopped() && !simonSaysWait) {
			SimonSaysTimer();
		}
	}


	// random method
	public int[] GenRandom(int[] a, int l) {

		int[] result = {1,1};

		
		// random delay
		result[0] = r.Next(simonSaysDelaySpan[0],simonSaysDelaySpan[1]);

		// random button
		result[1] = r.Next(0, l);
		result[1] = a[result[1]];
		

		return result;
	}



	// action lists, current and saved
	public List<ActionStep> currSteps = new List<ActionStep>();
	public List<ActionStep> savedSteps = new List<ActionStep>();


	// action step class, saving a timeStamp, timingScore and the totalScore
	public class ActionStep {
		public float timeStamp { get; set; }
		public int stepPoints { get; set; }
		public int totalScore { get; private set; }

		// constructor
		public ActionStep(float t, int p) {
			this.timeStamp = t;
			this.stepPoints = p;
			this.totalScore += p;
		}
	}




	// instance new lists
	public List<SimonButton> simonButtonList = new List<SimonButton>();


	// base class for lists
	public class GodotElement {
		public GodotElement() {
		}
	}


	// list class for simon buttons
	public class SimonButton : GodotElement {
		public SimonBtn el;
		public SimonButton(SimonBtn el) {
			this.el = el;
		}
	}




//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta) {
//      
//  }
}
