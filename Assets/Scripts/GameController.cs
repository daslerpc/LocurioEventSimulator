﻿using UnityEngine;
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

	public static double timeMultiplier = 1.0;
	static bool paused = false;

	/*********************************/
	/*		RUNTIME PARAMATERS	     */
	/*********************************/
	public static int NumberOfTeams = 20;
	public static float skillSTDev = 0.2f;
	public static float teamConsistency = 0.8f;

	public static int NumberOfPuzzles = 5;
	public static int solveTimes = 15;
	public static int tablesPerPuzzle = 5;

	public static int NumberOfMasters = 1;
	public static double secondsToProcessPuzzleRequest = 0.5;
	public static bool autoMasterOfShips = true;
	public static int NumberOfHumanTeams = 1;

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

		createPuzzles ();
		buildFloor ();
		createMastersOfShip ();
		createTeams ();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Equals)) {
			timeMultiplier+=2;
		}

		if (Input.GetKeyUp (KeyCode.Minus)) {
			timeMultiplier-=2;

			if (timeMultiplier < 1)
				timeMultiplier = 1;
		}

		if (Input.GetKeyUp (KeyCode.Space) && ResultsPane.enabled == false) {
			paused = !paused;
			PauseOverlay.enabled = paused;
		}

		if (Input.GetKeyUp (KeyCode.Escape)) {
			timeMultiplier = 0;
			endSim ();
		}

		if( teamsDone == NumberOfTeams ) {
			endSim ();
			teamsDone = 0;
		}
	}

	public static void Reset() {
		timeMultiplier = 1;
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
			//puzzles [i].setName ("Puzzle " + i);
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

	public void addHumanToMasterLine() {
		master.addHumanToLine ();
	}

	public static List<Puzzle> getPuzzles() {
		return puzzles;
	}

	public static double getDeltaTime() {
		if (paused)
			return 0;
		else
			return Time.deltaTime * timeMultiplier;
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
		Debug.Log(teamsDone + " of " + NumberOfTeams + " teams done.");
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
		string units = " minute";

		if (time < 2) {
			time *= 60;
			units = " second";
		}

		float floatTime = Mathf.Round ( (float) time );
		timeString = floatTime.ToString ();

		if (time > 1)
			units += "s";

		return timeString + units;
	}

	void endSim() {
		Text[] textFields = ResultsPane.GetComponentsInChildren<Text> ();

		textFields [4].text = returnTimeString(fastestPuzzleTime);
		textFields [6].text = returnTimeString(slowestPuzzleTime);
		textFields [8].text = returnTimeString(averagePuzzleTime);

		// This means all waits were 0, which were ignored during the run
		if (shortestPuzzleWait > longestPuzzleWait)
			shortestPuzzleWait = 0;
		
		textFields [10].text = returnTimeString(shortestPuzzleWait);
		textFields [12].text = returnTimeString(longestPuzzleWait);
		textFields [14].text = returnTimeString(averagePuzzleWait/(float)Mathf.Max(1, timesWaited));

		ResultsPane.enabled = true;
	}
}
 