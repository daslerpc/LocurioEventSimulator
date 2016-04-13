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

	public static double timeMultiplier = 1.0;
	static bool paused = false;

	/*********************************/
	/*		RUNTIME PARAMATERS	     */
	/*********************************/
	public static int NumberOfTeams = 20;
	public static float skillSTDev = 0.2f;
	public static float teamConsistency = 0.8f;

	public static int NumberOfPuzzles = 6;
	public static int solveTimes = 15;
	public static int tablesPerPuzzle = 4;

	/*********************************/
	/*		CALCULATED VALUES	     */
	/*********************************/
	static int roomHeight = 0;
	static int roomWidth = 0;

	int maxSupportedTeams = 0;

	static int teamsDone = 0;
	static int timesWaited = 0;

	static double fastestTime = float.MaxValue;
	static double slowestTime = 0;
	static double averageTime = 0;
	static double shortestWait = double.MaxValue;
	static double longestWait = 0;
	static double averageWait = 0;

	/*********************************/

	static List<Puzzle> puzzles;
	List<Team> teams;

	// Use this for initialization
	void Start () {
		createPuzzles ();
		buildFloor ();
		createTeams ();

		Canvas[] canvases = GetComponentsInChildren<Canvas> (true);

		for( int i=0; i<canvases.Length; i++)
			canvases[i].enabled = false;
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

		if (Input.GetKeyUp (KeyCode.Space) && GetComponentsInChildren<Canvas> (true) [1].enabled == false) {
			paused = !paused;
			GetComponentInChildren<Canvas> ().enabled = paused;
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

		NumberOfPuzzles = 6;
		solveTimes = 15;
		tablesPerPuzzle = 4;

		teamsDone = 0;
		timesWaited = 0;

		fastestTime = float.MaxValue;
		slowestTime = 0;
		averageTime = 0;
		shortestWait = float.MaxValue;
		longestWait = 0;
		averageWait = 0;
	}

	void createPuzzles() {
		puzzles = new List<Puzzle>();
		int tableOffset = roomFloorPadding * floorSquaresPerTable + 1;

		for (int i = 0; i < NumberOfPuzzles; i++) {
			puzzles.Add (new Puzzle ());	
			puzzles [i].setName ("Puzzle " + i);

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


		roomHeight = (NumberOfPuzzles + 2*roomFloorPadding) * floorSquaresPerTable;
		roomWidth = (maxSupportedTeams + 2*roomFloorPadding) * floorSquaresPerTable;
	}

	void buildFloor () {
		Camera cam = GetComponentInChildren <Camera> ();
		cam.orthographicSize = Mathf.Max(roomHeight, roomWidth)/2.0f + 0.5f;
		cam.transform.position = new Vector3 (roomWidth/2f - 0.5f, roomHeight/2f - 0.5f, -10);

		for (int i = 0; i < roomWidth; i++)
			for (int j = 0; j < roomHeight; j++) {
				Instantiate (floorTile, new Vector3 (i, j, 0), Quaternion.identity);
			}
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

			teams.Add ( thisTeam );
		}
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
	}



	public static void reportCompletionTime( double time) {
		if (time < fastestTime)
			fastestTime = time ;
		
		if (time > slowestTime)
			slowestTime = time;

		averageTime += time/(double) NumberOfTeams;
	}

	public static void reportWaitPerPuzzle( double wait) {
		double waitThreshold = 0.0;

		timesWaited++;

		if (wait > waitThreshold && wait < shortestWait)
			shortestWait = wait;
		
		if (wait > longestWait)
			longestWait = wait;

		averageWait += wait;
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
		Canvas resultsPane = GetComponentsInChildren<Canvas> (true) [1];

		Text[] textFields = resultsPane.GetComponentsInChildren<Text> ();

		textFields [4].text = returnTimeString(fastestTime);
		textFields [6].text = returnTimeString(slowestTime);
		textFields [8].text = returnTimeString(averageTime);

		// This means all waits were 0, which were ignored during the run
		if (shortestWait > longestWait)
			shortestWait = 0;
		
		textFields [10].text = returnTimeString(shortestWait);
		textFields [12].text = returnTimeString(longestWait);
		textFields [14].text = returnTimeString(averageWait/(float)Mathf.Max(1, timesWaited));

		resultsPane.enabled = true;
	}
}
 