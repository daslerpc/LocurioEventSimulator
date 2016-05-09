using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Team : MonoBehaviour {

	enum State{
		Start,
		MovingToPuzzle,
		WorkingOnPuzzle,
		MovingToMaster,
		WaitingInMasterLine,
		DealingWithMaster,
		FinishedWithEvent
	};

	State currentState = State.Start;

	List<Player> players;
	List<Puzzle> remainingPuzzles;

	double puzzleProgress;
	double timeToSolve;
	Puzzle currentPuzzle = null;

	double timeSpentWaiting = 0;  // total time spent waiting for puzzles
	double timeSpentSolving = 0;
	double waitPerPuzzle = 0;  // tracks time waited on each puzzle
	double waitOnMaster = 0;

	double quickestSolve = double.MaxValue;
	double slowestSolve = 0;

	Vector3 waitLocation;

	public GameObject playerPrefab;

	string teamName = "Team Name";
	float teamSkill = 1;
	float puzzleSTDev = 0.2f;

	MasterofShips master = null;
	int lineIndex = -1;
	int puzzleRequestIndex = 0;

	bool approved = false;
	bool rejected = false;

	public Canvas progressBarCanvas;
	public Slider progressBar;
	public Image fillImage;
	Color emptyProgress = new Color(3f, 0f, 0f, 1f);
	Color fullProgress = new Color(0f, 2f, 0f, 1f);


	void Start () {
		waitLocation = transform.position;
		transform.position = waitLocation;

		List<int> indeces = new List<int>();

		players = new List<Player> ();
		remainingPuzzles = new List<Puzzle> ();

		List<Puzzle> puzzles = GameController.getPuzzles ();

		for (int i = 0; i < puzzles.Count; i++)
			indeces.Add (i);

		for (int i = 0; i < puzzles.Count; i++) {
			int index = Random.Range (0, indeces.Count);
			remainingPuzzles.Add ( puzzles[indeces[index]]);
			indeces.RemoveAt (index);
		}

		float colorHue = Random.value;

		for (int i = 0; i < 4; i++) {
			GameObject playerInstance = (GameObject) Instantiate (playerPrefab, waitLocation, Quaternion.identity);

			Player thisPlayer = playerInstance.GetComponent (typeof(Player)) as Player;
			thisPlayer.SetColor (colorHue);
			players.Add ( thisPlayer );
		}

		hideProgressBar ();


		currentPuzzle = master.getStartingPuzzle ( remainingPuzzles );

		if (currentPuzzle != null)
			EnterState_MovingToPuzzle ();
		else
			EnterState_MovingToMaster ();
	}
		

	
	// Update is called once per frame

	/* States:
 		MovingToPuzzle,
		WorkingOnPuzzle,
		MovingToMaster,
		WaitingInMasterLine,
		DealingWithMaster,
		FinishedWithEvent
	*/
	void Update () {
		switch (currentState) {
		case State.MovingToPuzzle:
			ProcessState_MovingToPuzzle ();
			break;
		case State.WorkingOnPuzzle :
			ProcessState_WorkingOnPuzzle ();
			break;
		case State.MovingToMaster:
			ProcessState_MovingToMaster ();
			break;
		case State.WaitingInMasterLine :
			ProcessState_WaitingInMasterLine ();
			break;
		case State.DealingWithMaster :
			ProcessState_DealingWithMaster ();
			break;
		case State.FinishedWithEvent:
			ProcessState_FinishedWithEvent ();
			break;
		}
	}

	/*
	Puzzle findOpenPuzzle() {
		Puzzle nextPuzzle = null;

		for (int index = 0; index < remainingPuzzles.Count; index++) {
			if (remainingPuzzles [index].isFree ()) {
				nextPuzzle = remainingPuzzles [index];
				break;
			}
		}

		return nextPuzzle;
	}
*/
	public void setName(string newName) {
		teamName = newName;
	}

	public string getName() {
		return teamName;
	}

	public void setTeamSkill( float newSkill ) {
		teamSkill = newSkill;
	}

	public void setTeamConsistency( float consistency ) {
		puzzleSTDev = 1.0f - consistency;
	}

	public void setMasterReference( MasterofShips masterIn ) {
		master = masterIn;
	}

	void goHome() {
		goToLocation (waitLocation);
	}

	public void goToLocation( Vector3 destination ) {
		Vector3[] positions = getWiggledPositions (destination);
		transform.position = destination;

		for (int i = 0; i < 4; i++) {
			players [i].moveTo (positions[i]);
		}
	}

	public void goToLocation( Vector3 destination, Vector2 facing ) {
		Vector3[] positions = getWiggledPositions (destination);
		transform.position = destination;

		for (int i = 0; i < 4; i++) {
			players [i].moveTo (positions[i], facing);
		}
	}

	Vector3[] getWiggledPositions( Vector3 destination ) {
		Vector3[] positions = new Vector3[4];

		for (int i = 0; i < 4; i++) {
			float wiggle = 0.5f;
			float x = Random.Range (-wiggle, wiggle);
			float y = Random.Range (-wiggle, wiggle);
			positions[i] = destination + new Vector3 (x, y, 0);
		}

		return positions;
	}

	public bool destinationReached() {
		bool reached = true;
		for (int i = 0; i < players.Count; i++)
			reached &= players [i].getDestinationReached ();
		
		return reached;
	}

	void incrementPuzzleWaitTimers() {
		timeSpentWaiting += GameController.getDeltaTime ();
		waitPerPuzzle += GameController.getDeltaTime ();
	}

	void incrementSolveTimers() {
		puzzleProgress += GameController.getDeltaTime();
		timeSpentSolving += GameController.getDeltaTime ();
	}

	void incrementMasterWaitTimers() {
		waitOnMaster += GameController.getDeltaTime ();
	}

	Vector3 masterLineIndexToLocation( int index ) {
		float padding = 0.75f;
		return master.getLocation () + Vector3.right * (index + 1) * (1 + padding);
	}

	public void approveRequest() {
		approved = true;
	}

	public void rejectRequest() {
		rejected = true;
	}

	public void resetRequestResponses() {
		approved = false;
		rejected = false;
	}

	void startProgressBar() {
		progressBar.value = 0;
		progressBarCanvas.enabled = true;
	}

	void hideProgressBar() {
		progressBar.value = 0;
		progressBarCanvas.enabled = false;
	}

	void updateProgressBar() {
		progressBar.value = (float) puzzleProgress / (float) timeToSolve;
		fillImage.color = Color.Lerp (emptyProgress, fullProgress, progressBar.value);
	}

	/********************************************/
	/*			State Machine Functions			*/
	/********************************************/


	/* States:
	MovingToPuzzle,
	WorkingOnPuzzle,
	MovingToMaster,
	WaitingInMasterLine,
	DealingWithMaster,
	FinishedWithEvent
	*/



	/************************************/
	/*		 MovingToPuzzle State		*/
	/************************************/

	void EnterState_MovingToPuzzle() {
		//Debug.Log (teamName + " entering state MOVING TO PUZZLE");
		currentState = State.MovingToPuzzle;
		GameObject table = currentPuzzle.addTeam (teamName);

		transform.position = table.transform.position;

		Transform[] positions = table.GetComponentsInChildren<Transform> ();
		Vector3 position;
		Vector3 facing3;
		Vector2 facing = new Vector2();

		int randomVal = Random.Range (0, 2);

		for (int i = 0; i < players.Count; i++) {
			int index = i;

			if( randomVal == 1)
				index = players.Count - i - 1;

			position = positions [i + 1].position;
			facing3 = (table.transform.position - position);
			facing.x = facing3.x;
			facing.y = facing3.y;

			players [index].moveTo (position, facing);
		}
	}
		
	void ProcessState_MovingToPuzzle(){
		if (destinationReached ()) 
			EnterState_WorkingOnPuzzle ();
	}




	/************************************/
	/*		 WorkingOnPuzzle State		*/
	/************************************/

	void EnterState_WorkingOnPuzzle() {
		//Debug.Log (teamName + " entering state WORKING ON PUZZLE");
		puzzleProgress = 0;
		timeToSolve = Mathf.Max(1, GameController.gaussianFloat(currentPuzzle.getSolveTime () * teamSkill, currentPuzzle.getSolveTime () * puzzleSTDev));

		if (timeToSolve < quickestSolve)
			quickestSolve = timeToSolve;

		if (timeToSolve > slowestSolve)
			slowestSolve = timeToSolve;

		GameController.reportWaitPerPuzzle (waitPerPuzzle);
		GameController.reportMasterWaitTime (waitOnMaster);
		waitPerPuzzle = 0;
		waitOnMaster = 0;

		startProgressBar ();

		currentState = State.WorkingOnPuzzle;
	}

	void ProcessState_WorkingOnPuzzle(){
		incrementSolveTimers ();

		updateProgressBar ();

		if (puzzleProgress >= timeToSolve) {
			currentPuzzle.removeTeam (teamName);
			remainingPuzzles.Remove (currentPuzzle);
			currentPuzzle = null;
			hideProgressBar ();

			if (remainingPuzzles.Count == 0)
				EnterState_FinishedWithEvent ();
			else {
				EnterState_MovingToMaster ();
			}
		}
	}





	/************************************/
	/*	 	 MovingToMaster State		*/
	/************************************/

	void EnterState_MovingToMaster() {
		//Debug.Log (teamName + " entering state MOVING TO MASTER");

		currentState = State.MovingToMaster;
	}

	void ProcessState_MovingToMaster(){

		int index = master.getLastFreeLineIndex ();
		Vector3 location = masterLineIndexToLocation (index);

		if( index != lineIndex) {
			goToLocation (location, Vector2.left);
			lineIndex = index;
		}
			
		if (destinationReached ()) {
			if (lineIndex == 0) {
				master.getLineIndex ( teamName );
				EnterState_DealingWithMaster ();
			}
			else
				EnterState_WaitingInMasterLine ();
		}

	}





	/************************************/
	/*		WaitingInMasterLine State	*/
	/************************************/

	void EnterState_WaitingInMasterLine() {
		//Debug.Log (teamName + " entering state WAITING IN MASTER LINE");

		currentState = State.WaitingInMasterLine;
	}

	void ProcessState_WaitingInMasterLine(){
		int index = master.getLineIndex (teamName);
		Vector3 location = masterLineIndexToLocation (index);

		incrementMasterWaitTimers ();

		if (index != lineIndex) {
			goToLocation (location, Vector2.left);
			if (index == 0) {
				EnterState_DealingWithMaster ();
			} else {
				lineIndex = index;
			}
		}
	}





	/************************/
	/*		DealingWithMaster State		*/
	/************************/

	void EnterState_DealingWithMaster() {
		//Debug.Log (teamName + " entering state DEALING WITH MASTER");

		currentPuzzle = master.requestPuzzle ( remainingPuzzles [puzzleRequestIndex], teamName, remainingPuzzles );

		lineIndex = -1;
		currentState = State.DealingWithMaster;
	}

	void ProcessState_DealingWithMaster(){
		incrementPuzzleWaitTimers ();

		if (master.isAutomatic ()) {
			if (!master.isProcessing ()){
				if (currentPuzzle != null) {
					handleApprovedRequest ();
				} else {
					incrementPuzzleRequest ();
				}
			}
		} else {  // manual team processing

			if (rejected == true) {
				resetRequestResponses ();
				incrementPuzzleRequest ();
			} else if (approved == true) {
				resetRequestResponses ();

				if (currentPuzzle != null)
					handleApprovedRequest ();
				else {
					incrementPuzzleRequest ();
					GameController.masterMadeMistake ();
				}
			}
		}
	}

	void handleApprovedRequest() {
		puzzleRequestIndex = 0;
		master.stopProcessing ();
		master.removeHeadOfLine ();
		EnterState_MovingToPuzzle ();
	}

	void incrementPuzzleRequest() {
		puzzleRequestIndex++;
		if (puzzleRequestIndex == remainingPuzzles.Count)
			puzzleRequestIndex = 0;
		currentPuzzle = master.requestPuzzle ( remainingPuzzles [puzzleRequestIndex], teamName, remainingPuzzles );
	}




	/************************/
	/*		FinishedWithEvent State		*/
	/************************/

	void EnterState_FinishedWithEvent() {
		//Debug.Log (teamName + " entering state FINISHED WITH EVENT");

		goHome ();
		GameController.reportTeamDone ();
		GameController.reportCompletionTime (timeSpentWaiting + timeSpentSolving);

		currentState = State.FinishedWithEvent;
	}

	void ProcessState_FinishedWithEvent(){
	}
		
}
