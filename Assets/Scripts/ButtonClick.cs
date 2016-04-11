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

	static int teamNumber = 20;
	static float skillSpread = 0.2f;
	static float consistency = 0.2f;

	static int puzzNumber = 6;
	static int solveTime = 10;
	static int tablesPerPuzzle = 4;

	void LoadData() {
		teamNumber = int.Parse( teamNumberInput.text );
		skillSpread = float.Parse( skillSpreadInput.text )/100f;
		consistency = float.Parse( consistencyInput.text )/100f;

		puzzNumber = int.Parse( puzzNumberInput.text );
		solveTime = int.Parse( solveTimeInput.text );
		tablesPerPuzzle = int.Parse( tablesPerPuzzleInput.text );
	}

	void WriteDataToGameController() {
		GameController.NumberOfTeams = teamNumber;
		GameController.skillSTDev = skillSpread;
		GameController.teamConsistency = consistency;

		GameController.NumberOfPuzzles = puzzNumber;
		GameController.solveTimes = solveTime;
		GameController.tablesPerPuzzle = tablesPerPuzzle;
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
		
}
