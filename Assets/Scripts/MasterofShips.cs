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

	List<RawImage> stamps;
	Button approve;
	Button reject;

	Text teamNameDisplay;
	Text puzzleRequestDisplay;

	bool automatic = true;

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
	}

	public void setPassport ( Canvas passportIn ) {
		passport = passportIn;

		RawImage scroll = passport.GetComponentInChildren<RawImage> (true);
		RawImage[] images = scroll.GetComponentsInChildren<RawImage> (true);

		stamps = new List<RawImage> ();


		Debug.Log (images.Length);

		for(int i=0; i<5; i++)
			stamps.Add( images[2*i + 2] );

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
		
	public Puzzle requestPuzzle( List<Puzzle> teamProgress ) {
		Puzzle nextPuzzle = null;

		puzzleRequestDisplay.text = "Requesting travel to " + teamProgress[0].getName ();

		if (!processing) {
			for (int index = 0; index < teamProgress.Count; index++) {
				puzzleRequestDisplay.text = "Requesting travel to " + teamProgress[index].getName ();
				if (teamProgress [index].isFree ()) {
					nextPuzzle = teamProgress [index];

					if( waitingTeams.Count > 0 )
						waitingTeams.RemoveAt (0);
					break;
				}
			}
		}
			
		return nextPuzzle;
	}

	void displayPassport( string teamName, List<Puzzle> teamProgress ) {
		passport.enabled = true;

		for (int i = 0; i < teamProgress.Count; i++)
			stamps [teamProgress [i].getID ()].enabled = false;

		teamNameDisplay.text = teamName;
	}

	void hidePassport() {
		for (int i = 0; i < stamps.Count; i++) {
			stamps [i].enabled = true;
		}

		passport.enabled = false;
	}

	public void setAutomatic ( bool automaticIn ) {
		automatic = automaticIn;
	}
}
