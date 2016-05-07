using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class ButtonClick : MonoBehaviour {

	public GameObject loadingImage;

	public InputField teamNumberInput;
	public InputField skillSpreadInput;
	public InputField consistencyInput;

	public InputField puzzNumberInput;
	public InputField solveTimeInput;
	public InputField tablesPerPuzzleInput;

	public Toggle autoMasterOfShipsInput;
	public InputField secondsToProcessPuzzleRequestInput;
	public InputField numberOfHumanTeamsInput;

	static int teamNumber = 20;
	static float skillSpread = 0.2f;
	static float consistency = 0.2f;

	static int puzzNumber = 6;
	static int solveTime = 10;
	static int tablesPerPuzzle = 4;

	static bool autoMasterOfShips = true;
	static double secondsToProcessPuzzleRequest = 0.5;
	static int numberOfHumanTeams = 1;

	void LoadData() {
		teamNumber = int.Parse( teamNumberInput.text );
		skillSpread = float.Parse( skillSpreadInput.text )/100f;
		consistency = float.Parse( consistencyInput.text )/100f;

		puzzNumber = int.Parse( puzzNumberInput.text );
		solveTime = int.Parse( solveTimeInput.text );
		tablesPerPuzzle = int.Parse( tablesPerPuzzleInput.text );

		autoMasterOfShips = autoMasterOfShipsInput.isOn;
		secondsToProcessPuzzleRequest = int.Parse( secondsToProcessPuzzleRequestInput.text );
		numberOfHumanTeams = int.Parse (numberOfHumanTeamsInput.text);
	}

	void WriteDataToGameController() {
		GameController.NumberOfTeams = teamNumber;
		GameController.skillSTDev = skillSpread;
		GameController.teamConsistency = consistency;

		GameController.NumberOfPuzzles = puzzNumber;
		GameController.solveTimes = solveTime;
		GameController.tablesPerPuzzle = tablesPerPuzzle;

		GameController.autoMasterOfShips = autoMasterOfShips;
		GameController.secondsToProcessPuzzleRequest = secondsToProcessPuzzleRequest/60.0;
		GameController.NumberOfHumanTeams = numberOfHumanTeams;
	}

	public void LoadScene( ) {
		loadingImage.SetActive ( true );

		LoadData ();
		WriteDataToGameController ();

		SceneManager.LoadScene ("mainSim");
	}

	public void Rerun() {
		loadingImage.SetActive ( true );
		GameController.Reset ();
		WriteDataToGameController ();

		SceneManager.LoadScene ("mainSim");
	}

	public void Restart() {
		loadingImage.SetActive ( true );
		GameController.Reset ();
		SceneManager.LoadScene ("menu");
	}

	public void updateMasterInputs( bool value ) {
		Text secondsValue = secondsToProcessPuzzleRequestInput.GetComponentsInChildren<Text> () [0];
		Text secondsTitle = secondsToProcessPuzzleRequestInput.GetComponentsInChildren<Text> () [1];

		Text humansValue = numberOfHumanTeamsInput.GetComponentsInChildren<Text> () [0];
		Text humansTitle = numberOfHumanTeamsInput.GetComponentsInChildren<Text> () [1];

	
		secondsToProcessPuzzleRequestInput.interactable = autoMasterOfShipsInput.isOn;
		numberOfHumanTeamsInput.interactable = !autoMasterOfShipsInput.isOn;

		if (autoMasterOfShipsInput.isOn) {
			secondsValue.color = Color.black;
			secondsTitle.color = Color.black;

			humansValue.color = Color.gray;
			humansTitle.color = Color.gray;
		} else {
			secondsValue.color = Color.gray;
			secondsTitle.color = Color.gray;

			humansValue.color = Color.black;
			humansTitle.color = Color.black;
		}
	}
		
}
