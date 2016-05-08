using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MasterofShips : MonoBehaviour {
	List<string> waitingTeams;

	Vector3 location;

	double timeToProcess;
	double processingTime = 0;

	bool processing = false;
	Canvas passport;
	Canvas humanPassport;

	List<RawImage> stamps;
	List<RawImage> stampSquares;

	Button approve;
	Button reject;
	//Button ok;

	Text teamNameDisplay;
	Text puzzleRequestDisplay;

	bool automatic = true;

	// This is a hack to keep team names unique when adding a human team to the line
	int humanCounter = 0;

	public MasterofShips( Vector3 location ) {
		setLocation (location);
	}

	// Use this for initialization
	void Start () {
		waitingTeams = new List<string> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (waitingTeams.Count > 0 && processing) {
			processingTime += GameController.getDeltaTime ();
			if (processingTime >= timeToProcess) {
				processingTime = 0;
				processing = false;
			}
		}

		if (!automatic && waitingTeams.Count > 0 && waitingTeams [0].StartsWith ("human")) {
			displayHumanPassport ();
		}
	}

	public void setPassports ( Canvas passportIn, Canvas humanPassportIn ) {
		passport = passportIn;
		humanPassport = humanPassportIn;

		//ok = humanPassport.GetComponentInChildren<RawImage> (true).GetComponentInChildren<Button>(true);

		RawImage scroll = passport.GetComponentInChildren<RawImage> (true);
		RawImage[] images = scroll.GetComponentsInChildren<RawImage> (true);

		stamps = new List<RawImage> ();
		stampSquares = new List<RawImage> ();

		for (int i = 0; i < 5; i++) {
			stampSquares.Add (images [2 * i + 1]);
			stamps.Add (images [2 * i + 2]);
		}

		Button[] buttons = scroll.GetComponentsInChildren<Button> (true);
		approve = buttons [0];
		reject = buttons [1];

		if (automatic) {
			approve.enabled = false;
			approve.GetComponent<Image> ().enabled = false;

			reject.enabled = false;
			reject.GetComponent<Image> ().enabled = false;
		}
			
		hidePassport ();

		Text[] textDisplays = scroll.GetComponentsInChildren<Text> (true);
		teamNameDisplay = textDisplays[1];
		puzzleRequestDisplay = textDisplays[2];
	}

	public int getLineIndex( string team ) {
		if (waitingTeams.Contains (team))
			return waitingTeams.IndexOf( team );
		else {
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

	public void startProcessing( string teamName, List<Puzzle> teamProgress ) {
		processing = true;
		displayPassport (teamName, teamProgress);
	}

	public bool getProcessingStatus() {
		return !processing;
	}

	public void stopProcessing() {
		processing = false;
		hidePassport ();
	}
		
	public Puzzle requestPuzzle( Puzzle requestedPuzzle ) {
		Puzzle nextPuzzle = null;

		puzzleRequestDisplay.text = "Requesting travel to " + requestedPuzzle.getName ();

		if (!processing) {
			if (requestedPuzzle.isFree ()) {
				nextPuzzle = requestedPuzzle;
			}
		}

		return nextPuzzle;
	}

	public Puzzle getStartingPuzzle( List<Puzzle> teamProgress ) {
		Puzzle nextPuzzle = null;

		for (int index = 0; index < teamProgress.Count; index++) {
			if (teamProgress [index].isFree ()) {
				nextPuzzle = teamProgress [index];	
				break;
			}
		}

		return nextPuzzle;
	}

	void displayPassport( string teamName, List<Puzzle> teamProgress ) {
		int index;

		passport.enabled = true;

		for (int i = 0; i < teamProgress.Count; i++) {
			index = teamProgress [i].getID ();
			stamps [index].enabled = false;
		}

		teamNameDisplay.text = teamName;
	}

	void hidePassport() {
		float offset = 10f;
		float rotation = 10f;

		Vector3 offsetVector = new Vector3();
		Vector3 rotVector = new Vector3 ();


		for (int i = 0; i < stamps.Count; i++) {
			stamps[i].enabled = true;

			offsetVector.x = Random.value * 2 * offset - offset;
			offsetVector.y = Random.value * 2 * offset - offset;
			stamps[i].transform.position = stampSquares [i].transform.position + offsetVector;

			rotVector.z = Random.value * 2 * rotation - rotation;
			stamps[i].transform.Rotate ( stampSquares [i].transform.rotation.eulerAngles + rotVector );
		}

		passport.enabled = false;
	}

	void displayHumanPassport() {
		humanPassport.enabled = true;
	}

	void hideHumanPassport() {
		humanPassport.enabled = false;
	}

	public void addHumanToLine() {
		getLineIndex ("human" + humanCounter.ToString());
		humanCounter++;
	}

	public void removeHeadOfLine() {
		waitingTeams.RemoveAt (0);
	}

	public void dismissHumanTeamFromMasterLine() {
		removeHeadOfLine ();
		hideHumanPassport ();
	}

	public void setAutomatic ( bool automaticIn ) {
		automatic = automaticIn;
	}

	public bool isAutomatic() {
		return automatic;
	}

	public string getNameOfRequestingTeam() {
		string name = "";

		if (waitingTeams.Count > 0)
			name = waitingTeams [0];

		return name;
	}
}
