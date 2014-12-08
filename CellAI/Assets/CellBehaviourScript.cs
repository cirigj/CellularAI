using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellBehaviourScript : MonoBehaviour {

	// genetic (physical):
	public float intakeSpeed;
	public float processingSpeed;
	public float sugarCapacity;
	public float maxMovementSpeed;
	public float useEfficiency;
	public float energyCapacity;
	// genetic (behavioral):
	public float courage;
	public float hostility;
	public float cowardice;
	public float greed;
	// non-genetic (public):
	public float energy;
	public float sugar;
	public bool dead;
	// non-genetic (private):
	private float viewRange;
	private float splitProgress;
	private string state;
	private Transform target;
	private List<GameObject> nearbyCells;

	// Use this for initialization
	void Start () {
		state = "wander";
		target = null;
		nearbyCells = new List<GameObject>();
		dead = false;
	}
	
	// Update is called once per frame
	void Update () {
		ExamineNearbyCells();
		ProduceEnergy();
		switch (state) {
		case "scavenge":
			Scavenge();
			break;
		case "aggress":
			Aggress();
			break;
		case "flee":
			Flee();
			break;
		case "wander":
			Wander();
			break;
		default:
			Wander();
			Debug.Log("WARNING: INVALID STATE!");
			break;
		}
		ManageConcentration();
	}

	// Manage concentration of sugar based on surroundings
	void ManageConcentration () {

	}

	// Manage energy, split progress, and sugar based on internal variables
	void ProduceEnergy () {

	}

	// Add or remove cells from nearbyCells list based on proximity
	void ExamineNearbyCells () {

	}

	// Determine state based on nearby cells (called within ExamineNearbyCells)
	void DetermineState () {

	}

	// Move towards sugar cube to get sugar from it
	void Scavenge () {

	}

	// Attack another cell
	void Aggress () {

	}

	// Flee from another cell
	void Flee () {

	}

	// Move randomly
	void Wander () {

	}

	// Split cell into two cells (called within ProduceEnergy)
	void Split () {

	}
}
