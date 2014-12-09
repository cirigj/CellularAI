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
	public float radius;
	public bool dead;
	// non-genetic (private):
	private float splitProgress;
	private string state;
	private Rigidbody2D rb;
	private Transform tf;
	private Transform target;
	private Transform priorTarget;
	private List<GameObject> nearbyCells;
	private List<GameObject> nearbySugar;

	// Use this for initialization
	void Start () {
		tf = GetComponent<Transform>();
		rb = GetComponent<Rigidbody2D>();
		state = "wander";
		target = null;
		nearbyCells = new List<GameObject>();
		nearbySugar = new List<GameObject>();
		dead = false;
		radius = Mathf.Pow(0.75f * Mathf.PI * (sugarCapacity/EnvironmentScript.sugarCapacityToVolumeRatio), 1f/3f);
		tf.localScale = new Vector3(radius * 2f, radius * 2f, radius * 2f);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!dead) {
			ExamineNearbyCells();
			priorTarget = target;
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
		}
		ManageConcentration();
		rb.angularVelocity = 0f;
	}

	// Manage concentration of sugar based on surroundings
	void ManageConcentration () {
		float concentrationDifference = (sugar/sugarCapacity) / EnvironmentScript.baseConcentration;
		float concentrationChange = -Mathf.Log(concentrationDifference)/Mathf.Log(2f);
		sugar += concentrationChange * intakeSpeed;
	}

	// Manage energy, split progress, and sugar based on internal variables
	void ProduceEnergy () {
		float sugarConcentration = sugar/sugarCapacity;
		// first, expend sugar to make energy
		if (energy < energyCapacity) {
			// make sure there's enough sugar left to convert
			if (sugar >= processingSpeed * sugarConcentration * EnvironmentScript.sugarToEnergyRatio) {
				energy += processingSpeed * sugarConcentration;
				sugar -= processingSpeed * sugarConcentration * EnvironmentScript.sugarToEnergyRatio;
			}
			// if not, expend all of it and add energy accordingly
			else {
				energy += sugar / EnvironmentScript.sugarToEnergyRatio;
				sugar = 0f;
			}
		}
		// if this energy pushes the cell over capacity, add the excess to split progress
		if (energy > energyCapacity) {
			splitProgress += (energyCapacity - energy) / EnvironmentScript.energyToSplitProgressRatio;
			energy = energyCapacity;
			// split if threshhold is reached
			if (splitProgress >= EnvironmentScript.splitProgressThreshhold) {
				splitProgress = 0f;
				Split();
			}
		}
	}

	// Add or remove cells from nearbyCells list based on proximity
	void ExamineNearbyCells () {
		// first, check cells
		GameObject[] allCells = GameObject.FindGameObjectsWithTag(EnvironmentScript.cellTag);
		foreach (GameObject cell in allCells) {
			if (Vector3.Distance(cell.GetComponent<Transform>().position, tf.position) <= EnvironmentScript.viewRange) {
				if (!nearbyCells.Contains(cell)) {
					nearbyCells.Add(cell);
					target = cell.GetComponent<Transform>();
					DetermineState();
				}
			}
			else {
				if (nearbyCells.Contains(cell)) {
					nearbyCells.Remove(cell);
					if (target == cell) {
						target = null;
						DetermineState();
					}
				}
			}
		}
		// next, check sugar cubes
		GameObject[] sugarCubes = GameObject.FindGameObjectsWithTag(EnvironmentScript.sugarTag);
		foreach (GameObject cube in sugarCubes) {
			if (Vector3.Distance(cube.GetComponent<Transform>().position, tf.position) <= EnvironmentScript.viewRange) {
				if (!nearbySugar.Contains(cube)) {
					nearbySugar.Add(cube);
					target = cube.GetComponent<Transform>();
					DetermineState();
				}
			}
			else {
				if (nearbySugar.Contains(cube)) {
					nearbySugar.Remove(cube);
					if (target == cube) {
						target = null;
						DetermineState();
					}
				}
			}
		}
	}

	// Determine state based on nearby cells (called within ExamineNearbyCells)
	void DetermineState () {
		// if there's no target, wander
		if (target == null) {
			state = "wander";
		}
		// if the target is a cell:
		else if (target.tag == EnvironmentScript.cellTag) {
			// check if the target is dead or not first
			if (!target.GetComponent<CellBehaviourScript>().dead) {
				// if the target is bigger:
				if (target.GetComponent<CellBehaviourScript>().sugarCapacity > sugarCapacity) {
					// check courage stat first
					float courageCheck = Random.Range(0f, 1f);
					// if courage check succeeds, check hostility stat to aggress
					if (courageCheck < courage) {
						float hostilityCheck = Random.Range(0f, 1f);
						if (hostilityCheck < hostility) {
							state = "aggress";
						}
						else {
							target = priorTarget;
						}
					}
					// if not, check cowardice stat to flee
					else {
						float cowardiceCheck = Random.Range(0f, 1f);
						if (cowardiceCheck < cowardice) {
							state = "flee";
						}
						else {
							target = priorTarget;
						}
					}
				}
				// if the target is not bigger:
				else {
					// check hostility stat to aggress, and check cowardice if not aggressing
					float hostilityCheck = Random.Range(0f, 1f);
					float cowardiceCheck = Random.Range(0f, 1f);
					if (hostilityCheck < hostility) {
						state = "aggress";
					}
					else if (cowardiceCheck < cowardice) {
						state = "flee";
					}
					else {
						target = priorTarget;
					}
				}
			}
			// if the target is dead, check greed to aggress
			else {
				float greedCheck = Random.Range(0f, 1f);
				if (greedCheck < greed) {
					state = "aggress";
				}
				else {
					target = priorTarget;
				}
			}
		}
		// if the target is a sugar cube:
		else if (target.tag == EnvironmentScript.sugarTag) {
			// check greed stat to scavenge
			float greedCheck = Random.Range(0f, 1f);
			if (greedCheck < greed) {
				state = "scavenge";
			}
			else {
				target = priorTarget;
			}
		}
		else {
			Debug.Log("HOW THE HECK DID THIS EVEN HAPPEN?!");
		}
	}

	// Move towards sugar cube to get sugar from it
	void Scavenge () {
		MoveTowardsTarget(maxMovementSpeed);

	}

	// Attack another cell
	void Aggress () {
		CellBehaviorScript targetScript = target.GetComponent<CellBehaviourScript>();
		MoveTowardsTarget(maxMovementSpeed);
		if (Vector3.Distance(tf.position, target.position) < (radius + targetScript.radius + intakeSpeed * EnvironmentScript.intakeRangeRatio)) {
			sugar += (intakeSpeed - targetScript.intakeSpeed) * (targetScript.sugar/targetScript.sugarCapacity);
			targetScript.sugar += (targetScript.intakeSpeed - intakeSpeed) * (sugar/sugarCapacity);
		}
		// if the other cell is more powerful, check cowardice to flee, and check target hostility and greed to aggress
		if (targetScript.intakeSpeed > intakeSpeed) {
			float cowardiceCheck = sugar/sugarCapacity;
			if (cowardiceCheck < cowardice) {
				state = "flee";
			}
			float hostilityCheck = Random.Range(0f, 1f);
			if (hostilityCheck < targetScript.hostility) {
				float greedCheck = targetScript.sugar/targetScript.sugarCapacity;
				if (greedCheck < targetScript.greed) {
					state = "aggress";
					targetScript.target = tf;
					targetScript.priorTarget = tf;
				}
			}
		}
	}

	// Flee from another cell
	void Flee () {
		MoveTowardsTarget(-maxMovementSpeed);
	}

	// Move randomly
	void Wander () {
		MoveRandomly(maxMovementSpeed * EnvironmentScript.wanderSpeedPercentage);
	}

	// Split cell into two cells (called within ProduceEnergy)
	void Split () {

	}

	// Move randomly, then expend energy
	void MoveRandomly (float speed) {
		Vector2 dir2 = Random.insideUnitCircle.normalized;
		rb.velocity += dir2 * EnvironmentScript.maxSpeedChange;
		rb.velocity = Vector2.ClampMagnitude(rb.velocity, speed);
		ExpendEnergy(speed);
	}

	// Set and clamp velocity, then expend energy
	void MoveTowardsTarget (float speed) {
		Vector3 dir3 = target.position - tf.position;
		Vector2 dir2 = new Vector2(dir3.x, dir3.y, 0f).normalized;
		rb.velocity += dir2 * EnvironmentScript.maxSpeedChange;
		rb.velocity = Vector2.ClampMagnitude(rb.velocity, speed);
		ExpendEnergy(speed);
	}

	// Call this whenever a cell is expending energy on movement
	void ExpendEnergy (float speed) {
		// expend energy first
		energy -= useEfficiency * speed;
		// cell dies if energy has been depleted
		if (energy <= 0f) {
			dead = true;
		}
	}
}
