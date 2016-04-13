﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Team : MonoBehaviour {

	enum State{
		Waiting,
		SolvingPuzzle,
		MovingToPuzzle,
		Done
	};

	State currentState = State.Waiting;

	List<Player> players;
	List<Puzzle> remainingPuzzles;

	double puzzleProgress;
	double timeToSolve;
	Puzzle currentPuzzle = null;

	double timeSpentWaiting = 0;
	double timeSpentSolving = 0;
	double waitPerPuzzle = 0;

	double quickestSolve = double.MaxValue;
	double slowestSolve = 0;

	Vector3 waitLocation;

	public GameObject playerPrefab;

	string teamName = "Team Name";
	float teamSkill = 1;
	float puzzleSTDev = 0.2f;

	void Start () {
		waitLocation = transform.position;

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

		EnterState_Waiting ();
	}
		
	
	// Update is called once per frame
	void Update () {
		switch (currentState) {
		case State.Waiting:
			ProcessState_Waiting ();
			break;
		case State.SolvingPuzzle:
			ProcessState_SolvingPuzzle ();
			break;
		}
	}


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

	void goHome() {
		for (int i = 0; i < 4; i++) {
			float wiggle = 0.5f;
			float x = Random.Range (-wiggle, wiggle);
			float y = Random.Range (-wiggle, wiggle);
			Vector3 location = waitLocation + new Vector3 (x, y, 0);
			players [i].moveTo (location);
		}
	}

	/********************************************/
	/*			State Machine Functions			*/
	/********************************************/

	void EnterState_Waiting() {
		currentState = State.Waiting;
		waitPerPuzzle = 0;
		goHome ();
	}

	void ProcessState_Waiting() {
		currentPuzzle = findOpenPuzzle ();

		if (currentPuzzle != null) {
			EnterState_SolvingPuzzle ();
		} else {
			timeSpentWaiting += GameController.getDeltaTime ();
			waitPerPuzzle += GameController.getDeltaTime ();
		}
	}





	void EnterState_SolvingPuzzle() {
		
		currentState = State.SolvingPuzzle;
		GameObject table = currentPuzzle.addTeam (teamName);

		Transform[] positions = table.GetComponentsInChildren<Transform> ();

		int randomVal = Random.Range (0, 2);

		for (int i = 0; i < players.Count; i++) {
			int index = i;

			if( randomVal == 1)
				index = players.Count - i - 1;

			players [index].moveTo (positions [i + 1].position);
		}

		puzzleProgress = 0;
		timeToSolve = Mathf.Max(1, GameController.gaussianFloat(currentPuzzle.getSolveTime () * teamSkill, currentPuzzle.getSolveTime () * puzzleSTDev));

		if (timeToSolve < quickestSolve)
			quickestSolve = timeToSolve;

		if (timeToSolve > slowestSolve)
			slowestSolve = timeToSolve;

		GameController.reportWaitPerPuzzle (waitPerPuzzle);
		waitPerPuzzle = 0;
	}

	void ProcessState_SolvingPuzzle() {
		puzzleProgress += GameController.getDeltaTime();
		timeSpentSolving += GameController.getDeltaTime ();

		if (puzzleProgress >= timeToSolve) {
			currentPuzzle.removeTeam (teamName);
			remainingPuzzles.Remove (currentPuzzle);
			currentPuzzle = null;

			if (remainingPuzzles.Count == 0)
				EnterState_Done ();
			else
				EnterState_Waiting ();
		}
	}





	void EnterState_MovingToPuzzle() {
		currentState = State.MovingToPuzzle;
	}
		
	void ProcessState_MovingToPuzzle() {
	}






	void EnterState_Done() {
		currentState = State.Done;
		goHome ();
		GameController.reportTeamDone ();
		GameController.reportCompletionTime (timeSpentWaiting + timeSpentSolving);
	}

	void ProcessState_Done() {
	}

}
