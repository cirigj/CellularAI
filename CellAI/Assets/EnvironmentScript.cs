using UnityEngine;
using System.Collections;

public class EnvironmentScript : MonoBehaviour {

	public static float fieldRadius;
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
	public static float sugarCubeSpeedMin;
	public static float sugarCubeSpeedMax;
	public static float viewCheckRefresh;
	public static float behavioralReinforcement;
	public static float behavioralRegression;
	public static string cellTag;
	public static string sugarTag;
	public static int liveCells;
	public static bool fieldWrapping;

	// Use this for initialization
	void Start () {
		fieldRadius = 80.0f;
		baseConcentration = 1.0f;
		wanderSpeedPercentage = 0.75f;
		maxSpeedChange = 1f;
		viewRange = 40.0f;
		intakeRangeRatio = 2.0f;
		sugarToEnergyRatio = 1.0f;
		energyToSplitProgressRatio = 5.0f;
		splitProgressThreshhold = 500.0f;
		startingSugarPercentage = 1.0f;
		startingEnergyPercentage = 0.25f;
		sugarCapacityToVolumeRatio = 5.0f;
		sugarLevelRangeMultiplier = 2.0f;
		sugarCubeResilience = 0.01f;
		sugarCubeSpeedMin = 2f;
		sugarCubeSpeedMax = 6f;
		viewCheckRefresh = 10f;
		behavioralReinforcement = 0.005f;
		behavioralRegression = -0.0015f;
		cellTag = "cell";
		sugarTag = "sugar";
		fieldWrapping = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
