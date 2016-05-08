using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour {


	int floorSquaresPerTable = 3;
	int roomFloorPadding = 2;

	public GameObject floorTile;
	public GameObject teamPrefab;
	public GameObject tablePrefab;
	public GameObject masterPrefab;

	public static double timeMultiplier = 1;
	public static int timeExponent = 0;
	static bool paused = false;

	/*********************************/
	/*		RUNTIME PARAMATERS	     */
	/*********************************/
	public static int NumberOfTeams = 20;
	public static float skillSTDev = 0.2f;
	public static float teamConsistency = 0.8f;

	public static int NumberOfPuzzles = 5;
	public static int solveTimes = 15*60;
	public static int tablesPerPuzzle = 5;

	public static int NumberOfMasters = 1;
	public static double secondsToProcessPuzzleRequest = 30;
	public static bool autoMasterOfShips = true;
	public static int NumberOfHumanTeams = 1;

	public static int masterMistakes = 0;

	/*********************************/
	/*		CALCULATED VALUES	     */
	/*********************************/
	static int roomHeight = 0;
	static int roomWidth = 0;

	int maxSupportedTeams = 0;

	static int teamsDone = 0;
	static int timesWaited = 0;

	static double fastestPuzzleTime = float.MaxValue;
	static double slowestPuzzleTime = 0;
	static double averagePuzzleTime = 0;
	static double shortestPuzzleWait = double.MaxValue;
	static double longestPuzzleWait = 0;
	static double averagePuzzleWait = 0;
	static double shortestMasterWait = double.MaxValue;
	static double longestMasterWait = 0;
	static double averageMasterWait = 0;

	/*********************************/

	static List<Puzzle> puzzles;
	List<Team> teams;
	MasterofShips master;

	Canvas PauseOverlay;
	Canvas ResultsPane;
	Canvas Passport;
	Canvas HumanPassport;
	Canvas Controls;

	static Text timeScaleDisplay;

	// Use this for initialization
	void Start () {
		Canvas[] canvases = GetComponentsInChildren<Canvas> (true);

		for( int i=0; i<canvases.Length; i++)
			canvases[i].enabled = false;

		PauseOverlay = canvases [0];
		ResultsPane = canvases [1];
		Passport  = canvases [2];
		HumanPassport = canvases [3];
		Controls = canvases [4];

		Controls.enabled = true;

		if (!autoMasterOfShips)
			NumberOfTeams -= NumberOfHumanTeams;

		timeScaleDisplay = Controls.GetComponentsInChildren<Text> ()[0];

		createPuzzles ();
		buildFloor ();
		createMastersOfShip ();
		createTeams ();

	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Equals)) {
			speedupTime ();
		}

		if (Input.GetKeyUp (KeyCode.Minus)) {
			slowdownTime ();
		}

		if (Input.GetKeyUp (KeyCode.Space) && ResultsPane.enabled == false) {
			paused = !paused;
			PauseOverlay.enabled = paused;
		}

		if (Input.GetKeyUp (KeyCode.Escape)) {
			endSim ();
		}

		if( teamsDone == NumberOfTeams ) {
			endSim ();
			teamsDone = 0;
		}
	}

	public static void Reset() {
		timeExponent = 1;
		paused = false;

		NumberOfTeams = 20;
		skillSTDev = 0.2f;
		teamConsistency = 0.8f;

		NumberOfPuzzles = 5;
		solveTimes = 15;
		tablesPerPuzzle = 5;

		secondsToProcessPuzzleRequest = 0.5;
		autoMasterOfShips = true;
		NumberOfHumanTeams = 1;

		masterMistakes = 0;

		teamsDone = 0;
		timesWaited = 0;

		fastestPuzzleTime = double.MaxValue;
		slowestPuzzleTime = 0;
		averagePuzzleTime = 0;
		shortestPuzzleWait = double.MaxValue;
		longestPuzzleWait = 0;
		averagePuzzleWait = 0;
		shortestMasterWait = double.MaxValue;
		longestMasterWait = 0;
		averageMasterWait = 0;
	} 

	void createMastersOfShip() {
		Vector3 location = new Vector3 (2, roomHeight - roomFloorPadding*floorSquaresPerTable, 0);
		master = ((GameObject) Instantiate (masterPrefab, location, Quaternion.identity)).GetComponent<MasterofShips>();
		master.setLocation (location);
		master.setProcessingTime (secondsToProcessPuzzleRequest);
		master.setAutomatic (autoMasterOfShips);
		master.setPassports (Passport, HumanPassport);

		if (autoMasterOfShips) {
			Button manAdd = Controls.GetComponentInChildren<Button> ();
			manAdd.interactable = false;
		}
	}

	void createPuzzles() {
		puzzles = new List<Puzzle>();
		int tableOffset = roomFloorPadding * floorSquaresPerTable + 1;
		string name = "NAME NOT SET";

		for (int i = 0; i < NumberOfPuzzles; i++) {
			puzzles.Add (new Puzzle ());	
			puzzles [i].setID (i);

			switch (i) {
			case 0:
				name = "the Cave.";
				break;
			case 1:
				name = "the Forest.";
				break;
			case 2:
				name = "the Cliffs.";
				break;
			case 3:
				name = "the Lagoon.";
				break;
			case 4:
				name = "the Mountain.";
				break;
			}

			puzzles[i].setName( name );

			/* This code is for when puzzles can have a different number of tables each
			int numTables = puzzles [i].getNumberOfSupportedTeams ();

			if (numTables > maxSupportedTeams)
				maxSupportedTeams = numTables;
			*/

			int numTables = tablesPerPuzzle;
			maxSupportedTeams = tablesPerPuzzle;  // this var supports the above, but is silly in this context
			puzzles[i].setNumberOfSupportedTeams( numTables );
			puzzles [i].setSolveTime (solveTimes);

			for (int j = 0; j < numTables; j++) {
				
				Vector3 location = new Vector3 (j*3 + tableOffset, i*3 + tableOffset, 0);
				GameObject tableInstance = (GameObject) Instantiate (tablePrefab, location, Quaternion.identity);
				puzzles [i].addTable (tableInstance);
			}
		}


		roomHeight = (NumberOfPuzzles + 2*roomFloorPadding + NumberOfMasters) * floorSquaresPerTable;
		roomWidth = (maxSupportedTeams + 2*roomFloorPadding) * floorSquaresPerTable;
	}

	void buildFloor () {
		setCamera ();

		for (int i = 0; i < roomWidth; i++)
			for (int j = 0; j < roomHeight; j++) {
				Instantiate (floorTile, new Vector3 (i, j, 0), Quaternion.identity);
			}
	}

	void setCamera() {
		Camera cam = GetComponentInChildren <Camera> ();
		cam.orthographicSize = roomHeight/2.0f + 0.5f;
		cam.transform.position = new Vector3 (roomWidth/2f - 2.0f/3.0f*cam.orthographicSize + 2f, roomHeight/2f - 0.5f, -10);
	}

	void createTeams() {
		teams = new List<Team>();
		float x = 0;
		float y = 0;

		for (int i = 0; i < NumberOfTeams; i++) {	
			
			x = (3*roomFloorPadding + NumberOfPuzzles*floorSquaresPerTable - 2f)*Random.value + 0.5f;
			y = (3*(NumberOfPuzzles + 2*roomFloorPadding) - 3)*Random.value + 1;

			while(  x > 3*roomFloorPadding-1 && 
					y > 3*roomFloorPadding-1 &&
					x < 3*(roomFloorPadding + maxSupportedTeams)+1 &&
					y < 3*(roomFloorPadding + NumberOfPuzzles)+1
			) {
				x = (3*roomFloorPadding + NumberOfPuzzles*floorSquaresPerTable - 2f)*Random.value + 0.5f;
				y = (3*(NumberOfPuzzles + 2*roomFloorPadding) - 3)*Random.value + 1;
			}

			Vector3 location = new Vector3 (x, y, 0);
			GameObject teamInstance = (GameObject) Instantiate (teamPrefab, location, Quaternion.identity);

			Team thisTeam = teamInstance.GetComponent (typeof(Team)) as Team;
			thisTeam.setName ("Team " + (i+1));
			thisTeam.setTeamSkill ( Mathf.Clamp(generateTeamSkill(), 0.1f, 3f) );
			thisTeam.setTeamConsistency ( Mathf.Clamp(teamConsistency, 0f, 1f));
			thisTeam.setMasterReference (master);

			teams.Add ( thisTeam );
		}
	}

	static void speedupTime() {
		timeExponent+=1;
		timeMultiplier = Mathf.Pow (2f, timeExponent);
		setTimeDisplay ();
	}

	static void slowdownTime(){
		timeExponent-=1;
		timeMultiplier = Mathf.Pow (2f, timeExponent);
		setTimeDisplay ();
	}

	static void resetTime() {
		timeExponent = 0;
		timeMultiplier = Mathf.Pow (2f, timeExponent);
		setTimeDisplay ();
	}

	static void setTimeDisplay() {
		string fractionalTime = timeMultiplier.ToString();

		if (timeExponent == 0)
			timeScaleDisplay.text = "Time scale: real-time";
		else {
			if (timeExponent < 0) {
				fractionalTime = "1/";
				fractionalTime += (1f / timeMultiplier).ToString();
			}
			timeScaleDisplay.text = "Time scale: " + fractionalTime +"x";
		}
	}

	public void addHumanToMasterLine() {
		master.addHumanToLine ();
	}

	public void dismissHumanTeamFromMasterLine() {
		master.dismissHumanTeamFromMasterLine ();
	}

	public static List<Puzzle> getPuzzles() {
		return puzzles;
	}

	public static double getDeltaTime() {
		if (paused)
			return 0;
		else
			return Time.deltaTime * (float) timeMultiplier;
	}

	public static int getRoomWidth() {
		return roomWidth;
	}

	float generateTeamSkill() {
		return gaussianFloat (1, skillSTDev);
	}

	public static float gaussianFloat(float mean, float std)
	{
	    float u, v, S;

	    do
	    {
			u = 2.0f * Random.value - 1.0f;
			v = 2.0f * Random.value - 1.0f;
	        S = u * u + v * v;
	    }
	    while (S >= 1.0f);

	    float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
		return u * fac * std + mean;
	}

	public static void reportTeamDone() {
		teamsDone++;
		//Debug.Log(teamsDone + " of " + NumberOfTeams + " teams done.");
	}



	public static void reportCompletionTime( double time) {
		if (time < fastestPuzzleTime)
			fastestPuzzleTime = time ;
		
		if (time > slowestPuzzleTime)
			slowestPuzzleTime = time;

		averagePuzzleTime += time/(double) NumberOfTeams;
	}

	public static void reportWaitPerPuzzle( double wait) {
		double waitThreshold = 0.0;

		timesWaited++;

		if (wait > waitThreshold && wait < shortestPuzzleWait)
			shortestPuzzleWait = wait;
		
		if (wait > longestPuzzleWait)
			longestPuzzleWait = wait;

		averagePuzzleWait += wait;
	}

	public static void reportMasterWaitTime( double wait ) {
		double waitThreshold = 0.0;

		if (wait > waitThreshold && wait < shortestMasterWait)
			shortestMasterWait = wait;

		if (wait > longestMasterWait)
			longestMasterWait = wait;

		averageMasterWait += wait;
	}

	string returnTimeString( double time ) {
		string timeString = "";
		string units = " second";

		if (time >= 60) {
			time /= 60;
			units = " minute";
		}

		float floatTime = Mathf.Round ( (float) time );
		timeString = floatTime.ToString ();

		if (time > 1)
			units += "s";

		while (timeString.Length < 5)
			timeString = " " + timeString;

		return timeString + units;
	}

	public void approveRequest() {
		string name = master.getNameOfRequestingTeam ();
		Team requestingTeam = getRequestingTeam (name);

		if (requestingTeam == null)
			Debug.Log ("ERROR: attempting to accept request.  Team not found.");
		else 
			requestingTeam.approveRequest ();
	}

	public void rejectRequest() {
		string name = master.getNameOfRequestingTeam ();
		Team requestingTeam = getRequestingTeam (name);

		if (requestingTeam == null)
			Debug.Log ("ERROR: attempting to accept request.  Team not found.");
		else 
			requestingTeam.rejectRequest ();
	}

	Team getRequestingTeam( string name ) {
		Team requestingTeam = null;

		for (int i = 0; i < teams.Count; i++)
			if (teams [i].getName ().Equals (name)) {
				requestingTeam = teams [i];
				break;
			}
		
		return requestingTeam;
	}

	public static void masterMadeMistake() {
		masterMistakes++;
	}

	void endSim() {
		resetTime ();
		ResultsPane.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Text>().text = compiledResults ();
		ResultsPane.enabled = true;
	}

	string compiledResults() {
		string results = "\n";

		string fastestCompletion = "Fastest Completion:\t";
		string slowestCompletion = "Slowest Completion:\t";
		string averageCompletion = "Average Completion:\t";

		string shortestWait = "Shortest Wait:\t";
		string longestWait = "Longest Wait:\t";
		string averageWait =  "Average Wait:\t";

		string mistakes = "MoS Mistakes:\t";

		results += fastestCompletion + returnTimeString (fastestPuzzleTime) + "\n";
		results += slowestCompletion + returnTimeString (slowestPuzzleTime) + "\n";
		results += averageCompletion + returnTimeString (averagePuzzleTime) + "\n";	

		results += "\n";

		// This means all waits were 0, which were ignored during the run
		if (shortestPuzzleWait > longestPuzzleWait)
			shortestPuzzleWait = 0;

		results += shortestWait + returnTimeString (shortestPuzzleWait) + "\n";
		results += longestWait + returnTimeString (longestPuzzleWait) + "\n";	
		results += averageWait + returnTimeString (averagePuzzleWait/(float)Mathf.Max(1, timesWaited)) + "\n";	

		if (!autoMasterOfShips) {
			results += "\n";
			results += mistakes + masterMistakes.ToString () + "\n";
		}

		return results;
	}
}
 