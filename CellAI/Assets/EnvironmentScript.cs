using UnityEngine;
using System.Collections;

public class EnvironmentScript : MonoBehaviour {

	public static float baseConcentration;
	public static float wanderSpeedPercentage;
	public static float maxSpeedChange;
	public static float viewRange;
	public static float intakeRangeRatio;
	public static float sugarToEnergyRatio;
	public static float energyToSplitProgressRatio;
	public static float splitProgressThreshhold;
	public static float startingSugarPercentage;
	public static float startingEnergyPercentage;
	public static float sugarCapacityToVolumeRatio;
	public static float sugarLevelRangeMultiplier;
	public static float sugarCubeResilience;
	public static string cellTag;
	public static string sugarTag;

	// Use this for initialization
	void Start () {
		baseConcentration = 1.0f;
		wanderSpeedPercentage = 0.5f;
		maxSpeedChange = 0.25f;
		viewRange = 150.0f;
		intakeRangeRatio = 2.0f;
		sugarToEnergyRatio = 1.0f;
		energyToSplitProgressRatio = 2.0f;
		splitProgressThreshhold = 100.0f;
		startingSugarPercentage = 1.0f;
		startingEnergyPercentage = 0.5f;
		sugarCapacityToVolumeRatio = 5.0f;
		sugarLevelRangeMultiplier = 5.0f;
		sugarCubeResilience = 0.05f;
		cellTag = "cell";
		sugarTag = "sugar";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
