using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
Vars:
float Timer between generation
float Mutate Rate (%)
int numGenChild

Screen Wrapping?

Functions:
void findCells
float fitnessVal(GameObject)
void breed(GO,GO)

 */

public class CellSpawning : MonoBehaviour {

	// genetic (physical):
	public float intakeSpeedMin = 0.25f;
	public float intakeSpeedMax = 4.0f;
	public float processingSpeedMin = 0.25f;
	public float processingSpeedMax = 4.0f;
	public float sugarCapacityMin = 25.0f;
	public float sugarCapacityMax = 400.0f;
	public float maxMovementSpeedMin = 0.125f;
	public float maxMovementSpeedMax = 8.0f;
	public float useEfficiencyMin = 0.25f;
	public float useEfficiencyMax = 4.0f;
	public float energyCapacityMin = 25.0f;
	public float energyCapacityMax = 400.0f;
	// genetic (behavioral):
	public float courageMin = 0.0f;
	public float courageMax = 1.0f;
	public float hostilityMin = 0.0f;
	public float hostilityMax = 1.0f;
	public float cowardiceMin = 0.0f;
	public float cowardiceMax = 1.0f;
	public float greedMin = 0.0f;
	public float greedMax = 1.0f;
	// other things
	public GameObject cellPrefab;
	public GameObject cubePrefab;
	public int cellsPerGeneration = 10;
	public int sugarPerGeneration = 3;
	public float sugarMin = 5f;
	public float sugarMax = 8f;
	public float timeBetweenGenerations = 100f;
	public float masterMutationRate = 1.0f;
	private float speedMutationRate = 0.1f;
	private float capacityMutationRate = 10.0f;
	private float behaviorMutationRate = 0.05f;
	public bool constrainGenes = true;
	public bool waitMinSecs = false;
	public float minSecsTilNextGen = 5f;
	public bool loadFirstGenFromFile = false;
	public bool saveGensToFile = false;
	private CellBehaviourScript nextParent1;
	private CellBehaviourScript nextParent2;
	private float doomsdayTimer;
	public static int generationNumber;

	// Use this for initialization
	void Start () {
		GetComponent<EnvironmentScript>().Initialize();
		nextParent1 = null;
		nextParent2 = null;
		CreateLife();
		doomsdayTimer = 0f;
		generationNumber = 0;
		Debug.Log("Generation " + generationNumber.ToString() + " created after " + doomsdayTimer.ToString() + " seconds.");
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		doomsdayTimer += Time.fixedDeltaTime;
		if (((doomsdayTimer >= timeBetweenGenerations || EnvironmentScript.liveCells <= 2) && !waitMinSecs) 
		    || (doomsdayTimer >= minSecsTilNextGen && waitMinSecs && EnvironmentScript.liveCells <= 2)
		    || (waitMinSecs && doomsdayTimer >= timeBetweenGenerations)) {
			generationNumber++;
			PerpetuateLife();
			Debug.Log("Generation " + generationNumber.ToString() + " created after " + doomsdayTimer.ToString() + " seconds.");
			doomsdayTimer = 0f;
		}
	}

	// Instantiate first generation
	void CreateLife () {
		if (loadFirstGenFromFile) {
			Debug.Log("WARNING: Loading from file has not yet been implemented!");
		}
		else {
			for (int i = 0; i < cellsPerGeneration; i++) {
				GenerateCellRandom();
			}
		}
		for (int i = 0; i < sugarPerGeneration; i++) {
			GenerateSugarCube();
		}
		EnvironmentScript.liveCells = cellsPerGeneration;
	}

	// Create new generation and remove old generation
	void PerpetuateLife () {
		GameObject[] allCells = GameObject.FindGameObjectsWithTag(EnvironmentScript.cellTag);
		List<CellBehaviourScript> livingCells = new List<CellBehaviourScript>();
		foreach (GameObject cell in allCells) {
			CellBehaviourScript cellScript = cell.GetComponent<CellBehaviourScript>();
			if (!cellScript.dead) {
				livingCells.Add(cellScript);
			}
		}
		FindNextGenParents(livingCells);
		if (nextParent1 != null && nextParent2 != null) {
			for (int i = 0; i < cellsPerGeneration; i++) {
				GenerateCellFromParents(nextParent1, nextParent2);
			}
		}
		else {
			for (int i = 0; i < cellsPerGeneration; i++) {
				GenerateCellRandom();
			}
			generationNumber = 0;
		}
		GameObject[] allCubes = GameObject.FindGameObjectsWithTag(EnvironmentScript.sugarTag);
		foreach (GameObject cube in allCubes) {
			Destroy(cube);
		}
		for (int i = 0; i < sugarPerGeneration; i++) {
			GenerateSugarCube();
		}
		EnvironmentScript.liveCells = cellsPerGeneration;
		foreach (GameObject cell in allCells) {
			cell.GetComponent<CellBehaviourScript>().markedForExtinction = true;
		}
	}

	// Find two most fit cells
	void FindNextGenParents (List<CellBehaviourScript> livingCells) {
		nextParent1 = null;
		nextParent2 = null;
		float p1Fitness = 0f;
		float p2Fitness = 0f;
		foreach (CellBehaviourScript cell in livingCells) {
			float cellFitness = FitnessAssessment(cell);
			if (cellFitness > p1Fitness) {
				nextParent2 = nextParent1;
				p2Fitness = p1Fitness;
				nextParent1 = cell;
				p1Fitness = cellFitness;
			}
			else if (cellFitness > p2Fitness) {
				nextParent2 = cell;
				p2Fitness = cellFitness;
			}
		}
	}

	// Assess how fit the cell is
	float FitnessAssessment (CellBehaviourScript cell) {
		return cell.energy/cell.energyCapacity + (float)cell.numberOfSplits + cell.splitProgress/EnvironmentScript.splitProgressThreshhold;
	}

	// Generate sugar cube
	void GenerateSugarCube () {
		GameObject newCube = (GameObject)Instantiate(cubePrefab, 
		                      new Vector3(Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      0f), 
		                      Quaternion.identity);
		newCube.GetComponent<SugarCubeScript>().sugarLevel = Random.Range(sugarMin, sugarMax);
	}

	// Generate a random cell
	void GenerateCellRandom () {
		GameObject newCell = (GameObject)Instantiate(cellPrefab, 
		                      new Vector3(Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      0f), 
		                      Quaternion.identity);
		CellBehaviourScript newCellScript = newCell.GetComponent<CellBehaviourScript>();
		newCellScript.intakeSpeed = Random.Range(intakeSpeedMin, intakeSpeedMax);
		newCellScript.processingSpeed = Random.Range(processingSpeedMin, processingSpeedMax);
		newCellScript.sugarCapacity = Random.Range(sugarCapacityMin, sugarCapacityMax);
		newCellScript.maxMovementSpeed = Random.Range(maxMovementSpeedMin, maxMovementSpeedMax);
		newCellScript.useEfficiency = Random.Range(useEfficiencyMin, useEfficiencyMax);
		newCellScript.energyCapacity = Random.Range(energyCapacityMin, energyCapacityMax);
		newCellScript.courage = Random.Range(courageMin, courageMax);
		newCellScript.hostility = Random.Range(hostilityMin, hostilityMax);
		newCellScript.cowardice = Random.Range(cowardiceMin, cowardiceMax);
		newCellScript.greed = Random.Range(greedMin, greedMax);
		newCellScript.cellPrefab = cellPrefab;
		newCellScript.numberOfSplits = 0;
		newCellScript.radius = Mathf.Pow(0.75f * Mathf.PI * (newCellScript.sugarCapacity/EnvironmentScript.sugarCapacityToVolumeRatio), 1f/3f);
		newCellScript.GetComponent<Transform>().localScale = new Vector3(newCellScript.radius * 2f, newCellScript.radius * 2f, newCellScript.radius * 2f);
	}

	// Generate a cell from template
	void GenerateCellFromTemplate (float iS, float pS, float sC, float mMS, float uE, float eC, float cou, float hos, float cow, float gre) {
		GameObject newCell = (GameObject)Instantiate(cellPrefab, 
		                      new Vector3(Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      0f), 
		                      Quaternion.identity);
		CellBehaviourScript newCellScript = newCell.GetComponent<CellBehaviourScript>();
		newCellScript.intakeSpeed = iS;
		newCellScript.processingSpeed = pS;
		newCellScript.sugarCapacity = sC;
		newCellScript.maxMovementSpeed = mMS;
		newCellScript.useEfficiency = uE;
		newCellScript.energyCapacity = eC;
		newCellScript.courage = cou;
		newCellScript.hostility = hos;
		newCellScript.cowardice = cow;
		newCellScript.greed = gre;
		newCellScript.cellPrefab = cellPrefab;
		newCellScript.numberOfSplits = 0;
		newCellScript.radius = Mathf.Pow(0.75f * Mathf.PI * (newCellScript.sugarCapacity/EnvironmentScript.sugarCapacityToVolumeRatio), 1f/3f);
		newCellScript.GetComponent<Transform>().localScale = new Vector3(newCellScript.radius * 2f, newCellScript.radius * 2f, newCellScript.radius * 2f);
	}

	// Generate a cell from two other cells
	void GenerateCellFromParents (CellBehaviourScript parent1, CellBehaviourScript parent2) {
		GameObject newCell = (GameObject)Instantiate(cellPrefab, 
		                      new Vector3(Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      Random.Range(-EnvironmentScript.fieldRadius, EnvironmentScript.fieldRadius), 
		                      0f), 
		                      Quaternion.identity);
		CellBehaviourScript newCellScript = newCell.GetComponent<CellBehaviourScript>();
		if (Random.value >= 0.5f) {
			newCellScript.intakeSpeed = parent1.intakeSpeed 
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		else {
			newCellScript.intakeSpeed = parent2.intakeSpeed 
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.processingSpeed = parent1.processingSpeed
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		else {
			newCellScript.processingSpeed = parent2.processingSpeed
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.sugarCapacity = parent1.sugarCapacity
				+ Random.Range(-masterMutationRate*0.5f*capacityMutationRate, masterMutationRate*0.5f*capacityMutationRate); }
		else {
			newCellScript.sugarCapacity = parent2.sugarCapacity
				+ Random.Range(-masterMutationRate*0.5f*capacityMutationRate, masterMutationRate*0.5f*capacityMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.maxMovementSpeed = parent1.maxMovementSpeed
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		else {
			newCellScript.maxMovementSpeed = parent2.maxMovementSpeed
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.useEfficiency = parent1.useEfficiency
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		else {
			newCellScript.useEfficiency = parent2.useEfficiency
				+ Random.Range(-masterMutationRate*0.5f*speedMutationRate, masterMutationRate*0.5f*speedMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.energyCapacity = parent1.energyCapacity
				+ Random.Range(-masterMutationRate*0.5f*capacityMutationRate, masterMutationRate*0.5f*capacityMutationRate); }
		else {
			newCellScript.energyCapacity = parent2.energyCapacity
				+ Random.Range(-masterMutationRate*0.5f*capacityMutationRate, masterMutationRate*0.5f*capacityMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.courage = parent1.courage
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		else {
			newCellScript.courage = parent2.courage
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.hostility = parent1.hostility
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		else {
			newCellScript.hostility = parent2.hostility
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.cowardice = parent1.cowardice
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		else {
			newCellScript.cowardice = parent2.cowardice
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		if (Random.value >= 0.5f) {
			newCellScript.greed = parent1.greed
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		else {
			newCellScript.greed = parent2.greed
				+ Random.Range(-masterMutationRate*0.5f*behaviorMutationRate, masterMutationRate*0.5f*behaviorMutationRate); }
		newCellScript.cellPrefab = cellPrefab;
		newCellScript.numberOfSplits = 0;
		newCellScript.radius = Mathf.Pow(0.75f * Mathf.PI * (newCellScript.sugarCapacity/EnvironmentScript.sugarCapacityToVolumeRatio), 1f/3f);
		newCellScript.GetComponent<Transform>().localScale = new Vector3(newCellScript.radius * 2f, newCellScript.radius * 2f, newCellScript.radius * 2f);
		if (constrainGenes) {
			CapCellVariables(newCellScript);
		}
	}

	// Make sure the cell variables don't exceed the caps
	void CapCellVariables (CellBehaviourScript cellScript) {
		if (cellScript.intakeSpeed < intakeSpeedMin) {
			cellScript.intakeSpeed = intakeSpeedMin;
		}
		else if (cellScript.intakeSpeed > intakeSpeedMax) {
			cellScript.intakeSpeed = intakeSpeedMax;
		}
		if (cellScript.processingSpeed < processingSpeedMin) {
			cellScript.processingSpeed = processingSpeedMin;
		}
		else if (cellScript.processingSpeed > processingSpeedMax) {
			cellScript.processingSpeed = processingSpeedMax;
		}
		if (cellScript.sugarCapacity < sugarCapacityMin) {
			cellScript.sugarCapacity = sugarCapacityMin;
		}
		else if (cellScript.sugarCapacity > sugarCapacityMax) {
			cellScript.sugarCapacity = sugarCapacityMax;
		}
		if (cellScript.maxMovementSpeed < maxMovementSpeedMin) {
			cellScript.maxMovementSpeed = maxMovementSpeedMin;
		}
		else if (cellScript.maxMovementSpeed > maxMovementSpeedMax) {
			cellScript.maxMovementSpeed = maxMovementSpeedMax;
		}
		if (cellScript.useEfficiency < useEfficiencyMin) {
			cellScript.useEfficiency = useEfficiencyMin;
		}
		else if (cellScript.useEfficiency > useEfficiencyMax) {
			cellScript.useEfficiency = useEfficiencyMax;
		}
		if (cellScript.energyCapacity < energyCapacityMin) {
			cellScript.energyCapacity = energyCapacityMin;
		}
		else if (cellScript.energyCapacity > energyCapacityMax) {
			cellScript.energyCapacity = energyCapacityMax;
		}
		if (cellScript.courage < courageMin) {
			cellScript.courage = courageMin;
		}
		else if (cellScript.courage > courageMax) {
			cellScript.courage = courageMax;
		}
		if (cellScript.hostility < hostilityMin) {
			cellScript.hostility = hostilityMin;
		}
		else if (cellScript.hostility > hostilityMax) {
			cellScript.hostility = hostilityMax;
		}
		if (cellScript.cowardice < cowardiceMin) {
			cellScript.cowardice = cowardiceMin;
		}
		else if (cellScript.cowardice > cowardiceMax) {
			cellScript.cowardice = cowardiceMax;
		}
		if (cellScript.greed < greedMin) {
			cellScript.greed = greedMin;
		}
		else if (cellScript.greed > greedMax) {
			cellScript.greed = greedMax;
		}
	}
}
