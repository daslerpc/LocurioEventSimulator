using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Puzzle {

	public int ID = 0;
	public int solveTime = 15;
	public int teamsSupported = 4;

	int numTeamsSolving = 0;

	string puzzleName = "Puzzle Name";
	Vector3 location;

	List<GameObject> tables = new List<GameObject>();
	string[] tableOccupants;

	public GameObject addTeam(string teamName) {
		GameObject table = null;
		int index = 0;

		if (isFree()) {
			index = indexOfFirstFreeTable ();
			table = tables[index];
			tableOccupants [index] = teamName;
			numTeamsSolving++;
		}

		// return last free table
		return table;
	}

	public int indexOfFirstFreeTable() {
		int index = 0;

		while (!tableOccupants [index].Equals ("")) {			
			index++;
		}
		
		return index;
	}

	public void removeTeam(string teamName) {
		numTeamsSolving--;

		if (numTeamsSolving < 0) {
			numTeamsSolving = 0;
		}

		int index = 0;
		while (!tableOccupants [index].Equals (teamName)) {
			index++;
		}

		tableOccupants [index] = "";
	}

	public bool isFree() {
		return numTeamsSolving < teamsSupported;
	}

	public void setName(string newName) {
		puzzleName = newName;
	}

	public string getName() {
		return puzzleName;
	}

	public void setLocation(Vector3 newLocation) {
		location = newLocation;
	}

	public Vector3 getLocation() {
		return location;
	}

	public int getNumberOfSupportedTeams() {
		return teamsSupported;
	}

	public void setNumberOfSupportedTeams( int numTables ) {
		teamsSupported = numTables;

		tableOccupants = new string[teamsSupported];

		for (int i = 0; i < tableOccupants.Length; i++)
			tableOccupants [i] = "";
	}

	public void addTable(GameObject table) {
		tables.Add (table);
	}

	public int getSolveTime() {
		return solveTime;
	}

	public void setSolveTime( int newSolveTime ) {
		solveTime = newSolveTime;
	}

	public void setID ( int newID ) {
		ID = newID;
	}

	public int getID () {
		return ID;
	}
}
