using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterofShips : MonoBehaviour {
	List<string> waitingTeams;

	Vector3 location;

	double timeToProcess;
	double processingTime = 0;

	bool processing = false;

	string lastTeam;

	public MasterofShips( Vector3 location ) {
		setLocation (location);
	}

	// Use this for initialization
	void Start () {
		waitingTeams = new List<string> ();
		lastTeam = "";
	}
	
	// Update is called once per frame
	void Update () {
		if (waitingTeams.Count > 0 && processing) {
			processingTime += GameController.getDeltaTime ();
			if (processingTime >= timeToProcess) {
				processingTime = 0;
				lastTeam = waitingTeams [0];
				waitingTeams.RemoveAt (0);
				processing = false;
			}
		}
	}

	public int getLineIndex( string team ) {
		if (waitingTeams.Contains (team))
			return waitingTeams.IndexOf( team );
		else if( team.Equals(lastTeam) ) {
			lastTeam = "";
			return -1;	
		} else {
			waitingTeams.Add (team);
			return waitingTeams.Count - 1;
		}	
	}

	public int getLastFreeLineIndex() {
		return waitingTeams.Count;
	}

	public void setLocation( Vector3 locationIn) {
		location = locationIn;
	}

	public Vector3 getLocation() {
		return location;
	}

	public void setProcessingTime( double time ) {
		timeToProcess = time;
	}

	public void startProcessing() {
		processing = true;
	}

	public bool getDoneProcessing() {
		return !processing;
	}

	public void setDoneProcessing() {
		processing = false;
	}
		
	public Puzzle requestPuzzle( List<Puzzle> teamProgress ) {
		Puzzle nextPuzzle = null;

		for (int index = 0; index < teamProgress.Count; index++) {
			if (teamProgress [index].isFree ()) {
				nextPuzzle = teamProgress [index];
				break;
			}
		}

		return nextPuzzle;
	}

}
