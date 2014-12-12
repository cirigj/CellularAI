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
	public GameObject cellPrefab;
	public bool dead;
	public bool markedForExtinction;
	// non-genetic (private):
	public float splitProgress;
	public float numberOfSplits;
	private string state;
	private Rigidbody2D rb;
	private Transform tf;
	public Transform target;
	private Transform priorTarget;
	private List<GameObject> nearbyCells;
	private List<GameObject> nearbySugar;
	private bool activated;
	private float viewRefresh;
	private bool stateChanged;

	// Use this for initialization
	void Start () {
		activated = false;
		tf = GetComponent<Transform>();
		rb = GetComponent<Rigidbody2D>();
		energy = EnvironmentScript.startingEnergyPercentage * energyCapacity;
		sugar = EnvironmentScript.startingSugarPercentage * sugarCapacity;
		splitProgress = 0f;
		state = "wander";
		priorTarget = null;
		target = null;
		nearbyCells = new List<GameObject>();
		nearbySugar = new List<GameObject>();
		dead = false;
		markedForExtinction = false;
		viewRefresh = 0f;
		stateChanged = false;
		activated = true;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (activated) {
			if (markedForExtinction) {
				Destroy(gameObject);
			}
			if (!dead) {
				stateChanged = false;
				DrawDebugLinesToNearbyCells();
				ExamineNearbyCells();
				priorTarget = target;
				if (target == null) {
					state = "wander";
				}
				ProduceEnergy();
				switch (state) {
				case "scavenge":
					GetComponent<Renderer>().material.color = Color.green;
					Scavenge();
					break;
				case "aggress":
					GetComponent<Renderer>().material.color = Color.red;
					Aggress();
					break;
				case "flee":
					if (target.tag == EnvironmentScript.sugarTag) {
						state = "wander";
						target = null;
						priorTarget = null;
						break;
					}
					if (target.GetComponent<CellBehaviourScript>().dead) {
						target = null;
						priorTarget = null;
						state = "wander";
						break;
					}
					GetComponent<Renderer>().material.color = Color.blue;
					Flee();
					break;
				case "wander":
					GetComponent<Renderer>().material.color = Color.white;
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
			WrapField();
			Color c = GetComponent<Renderer>().material.color;
			c.a = sugar/sugarCapacity;
			GetComponent<Renderer>().material.color = c;
		}
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
			splitProgress += (energy - energyCapacity) / EnvironmentScript.energyToSplitProgressRatio;
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
		// if view refresh, clear lists
		if (viewRefresh > EnvironmentScript.viewCheckRefresh && !dead) {
			viewRefresh = 0f;
			nearbyCells.Clear();
			nearbySugar.Clear();
		}
		// first, check cells
		GameObject[] allCells = GameObject.FindGameObjectsWithTag(EnvironmentScript.cellTag);
		foreach (GameObject cell in allCells) {
			if (stateChanged) {
				return;
			}
			// make sure not to check cells from the last gen
			if (cell.GetComponent<CellBehaviourScript>().markedForExtinction) {
				continue;
			}
			// don't include this cell
			if (cell == gameObject) {
				continue;
			}
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
					if (target == cell.GetComponent<Transform>()) {
						target = null;
						DetermineState();
					}
				}
			}
		}
		// next, check sugar cubes
		GameObject[] sugarCubes = GameObject.FindGameObjectsWithTag(EnvironmentScript.sugarTag);
		foreach (GameObject cube in sugarCubes) {
			if (stateChanged) {
				return;
			}
			// don't look at depleted sugar cubes
			if (cube.GetComponent<SugarCubeScript>().depleted) {
				continue;
			}
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
					if (target == cube.GetComponent<Transform>()) {
						target = null;
						DetermineState();
					}
				}
			}
		}
	}

	// Determine state based on nearby cells (called within ExamineNearbyCells)
	void DetermineState () {
		if (stateChanged) {
			return;
		}
		viewRefresh = 0f;
		// if there's no target, wander
		if (target == null) {
			state = "wander";
			stateChanged = true;
			return;
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
						if (hostilityCheck < hostility*0.5f) {
							state = "aggress";
							stateChanged = true;
							return;
						}
						else {
							target = priorTarget;
						}
					}
					// if not, check cowardice stat to flee
					else {
						float cowardiceCheck = Random.Range(0f, 1f);
						if (cowardiceCheck < cowardice*0.5f) {
							state = "flee";
							stateChanged = true;
							return;
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
					if (hostilityCheck < hostility*0.5f) {
						state = "aggress";
						stateChanged = true;
						return;
					}
					else if (cowardiceCheck < cowardice*0.5f) {
						state = "flee";
						stateChanged = true;
						return;
					}
					else {
						target = priorTarget;
					}
				}
			}
			// if the target is dead, check greed to aggress
			else {
				float greedCheck = Random.Range(0f, 1f);
				if (greedCheck < greed*0.5f) {
					state = "aggress";
					stateChanged = true;
					return;
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
			if (greedCheck < greed*0.5f) {
				state = "scavenge";
				stateChanged = true;
				return;
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
		if (target.tag == EnvironmentScript.cellTag) {
			state = "wander";
			target = null;
			priorTarget = null;
			return;
		}
		if (target.GetComponent<SugarCubeScript>().depleted) {
			state = "wander";
			target = null;
			priorTarget = null;
			return;
		}
		float greedCheck = 0.5f*sugar/sugarCapacity + Random.Range(0f, 0.5f/Time.fixedDeltaTime);
		if (greedCheck < greed*0.5f) {
			state = "wander";
			target = null;
			priorTarget = null;
			return;
		}
		MoveTowardsTarget(maxMovementSpeed);
		SugarCubeScript targetScript = target.GetComponent<SugarCubeScript>();
		float sugarRadius = targetScript.sugarLevel * EnvironmentScript.sugarLevelRangeMultiplier;
		if (Vector3.Distance(tf.position, target.position) < (radius + sugarRadius + intakeSpeed * EnvironmentScript.intakeRangeRatio)) {
			float sugarDist = Vector3.Distance(tf.position, target.position) - radius - (intakeSpeed * EnvironmentScript.intakeRangeRatio);
			sugar += intakeSpeed * (targetScript.sugarLevel * EnvironmentScript.sugarLevelRangeMultiplier - sugarDist);
			targetScript.sugarLevel -= intakeSpeed * (targetScript.sugarLevel * EnvironmentScript.sugarLevelRangeMultiplier - sugarDist) * EnvironmentScript.sugarCubeResilience;
		}
		greed += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
		courage += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		hostility += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		cowardice += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
	}

	// Attack another cell
	void Aggress () {
		if (target.tag == EnvironmentScript.sugarTag) {
			state = "wander";
			target = null;
			priorTarget = null;
			return;
		}
		CellBehaviourScript targetScript = target.GetComponent<CellBehaviourScript>();
		MoveTowardsTarget(maxMovementSpeed);
		float intakeDist = Vector3.Distance(tf.position, target.position) - radius - target.GetComponent<CellBehaviourScript>().radius - intakeSpeed * EnvironmentScript.intakeRangeRatio;
		if (!targetScript.dead) {
			if (Vector3.Distance(tf.position, target.position) < (radius + targetScript.radius + intakeSpeed * EnvironmentScript.intakeRangeRatio)) {
				sugar += (intakeSpeed - targetScript.intakeSpeed - intakeDist) * (targetScript.sugar/targetScript.sugarCapacity);
				targetScript.sugar += (targetScript.intakeSpeed - intakeSpeed + intakeDist) * (sugar/sugarCapacity);
			}
			hostility += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
			cowardice += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
			// if the other cell is more powerful, check cowardice to flee, and check target hostility and greed to aggress
			if (targetScript.intakeSpeed > intakeSpeed) {
				float cowardiceCheck = sugar/sugarCapacity;
				if (cowardiceCheck < cowardice*0.5f) {
					state = "flee";
				}
				float hostilityCheck = Random.Range(0f, 1f/Time.fixedDeltaTime);
				if (hostilityCheck < targetScript.hostility*0.5f) {
					float greedCheck = targetScript.sugar/targetScript.sugarCapacity;
					if (greedCheck < targetScript.greed*0.5f) {
						targetScript.state = "aggress";
						targetScript.target = tf;
						targetScript.priorTarget = tf;
					}
				}
				courage += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
				cowardice += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
			}
			// if not, check for greed to stop aggressing, and check other cell to flee
			else {
				float greedCheck2 = sugar/sugarCapacity;
				if (greedCheck2 >= 1f + greed*2f) {
					state = "wander";
					target = null;
					priorTarget = null;
				}
				float courageCheck = Random.Range(0f, 1f);
				if (courageCheck >= targetScript.courage) {
					float cowardiceCheck = 0.5f*targetScript.sugar/targetScript.sugarCapacity + Random.Range(0f, 0.5f/Time.fixedDeltaTime);
					if (cowardiceCheck < targetScript.cowardice*0.5f) {
						targetScript.state = "flee";
						targetScript.target = tf;
						targetScript.priorTarget = tf;
					}
				}
			}
		}
		// if the target is dead, don't account for its intake
		else {
			if (Vector3.Distance(tf.position, target.position) < (radius + targetScript.radius + intakeSpeed * EnvironmentScript.intakeRangeRatio)) {
				sugar += (intakeSpeed - intakeDist) * (targetScript.sugar/targetScript.sugarCapacity);
				targetScript.sugar += (-intakeSpeed + intakeDist) * (sugar/sugarCapacity);
			}
			hostility += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
			greed += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
			// check for greed to stop aggressing
			float greedCheck2 = sugar/sugarCapacity;
			if (greedCheck2 >= 1f + greed*2f) {
				state = "wander";
				target = null;
				priorTarget = null;
			}
		}
	}

	// Flee from another cell
	void Flee () {
		MoveTowardsTarget(-maxMovementSpeed);
		cowardice += EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
		hostility += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		greed += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		if (target.GetComponent<CellBehaviourScript>().sugarCapacity > sugarCapacity) {
			courage -= EnvironmentScript.behavioralReinforcement * Time.fixedDeltaTime;
		}
	}

	// Move randomly
	void Wander () {
		viewRefresh += Time.fixedDeltaTime;
		MoveRandomly(maxMovementSpeed * EnvironmentScript.wanderSpeedPercentage);
		hostility += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		cowardice += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
		greed += EnvironmentScript.behavioralRegression * Time.fixedDeltaTime;
	}

	// Split cell into two cells (called within ProduceEnergy)
	void Split () {
		Debug.Log("Cell split in generation " + CellSpawning.generationNumber.ToString() + "!");
		GameObject newCell = (GameObject)Instantiate(cellPrefab, tf.position, Quaternion.identity);
		CellBehaviourScript newCellScript = newCell.GetComponent<CellBehaviourScript>();
		newCellScript.intakeSpeed = intakeSpeed;
		newCellScript.processingSpeed = processingSpeed;
		newCellScript.sugarCapacity = sugarCapacity;
		newCellScript.maxMovementSpeed = maxMovementSpeed;
		newCellScript.useEfficiency = useEfficiency;
		newCellScript.energyCapacity = energyCapacity;
		newCellScript.courage = courage;
		newCellScript.hostility = hostility;
		newCellScript.cowardice = cowardice;
		newCellScript.greed = greed;
		newCellScript.cellPrefab = cellPrefab;
		newCellScript.radius = Mathf.Pow(0.75f * Mathf.PI * (newCellScript.sugarCapacity/EnvironmentScript.sugarCapacityToVolumeRatio), 1f/3f);
		newCellScript.GetComponent<Transform>().localScale = new Vector3(newCellScript.radius * 2f, newCellScript.radius * 2f, newCellScript.radius * 2f);
		EnvironmentScript.liveCells++;
		numberOfSplits++;
		newCellScript.numberOfSplits = numberOfSplits;
	}

	// Move randomly, then expend energy
	void MoveRandomly (float speed) {
		Vector2 dir2 = Random.insideUnitCircle.normalized;
		rb.velocity += dir2 * EnvironmentScript.maxSpeedChange;
		if (rb.velocity.magnitude > Mathf.Abs(speed)) {
			rb.velocity = rb.velocity.normalized * speed;
		}
		ExpendEnergy(speed);
		WrapField();
	}

	// Set and clamp velocity, then expend energy
	void MoveTowardsTarget (float speed) {
		Vector3 dir3 = target.position - tf.position;
		Vector2 dir2 = new Vector2(dir3.x, dir3.y).normalized;
		if (speed > 0) {
			rb.velocity += dir2 * EnvironmentScript.maxSpeedChange;
		}
		else {
			rb.velocity += dir2 * -EnvironmentScript.maxSpeedChange;
		}
		if (Mathf.Abs(rb.velocity.magnitude) > Mathf.Abs(speed)) {
			rb.velocity = rb.velocity.normalized * Mathf.Abs(speed);
		}
		ExpendEnergy(speed);
		WrapField();
	}

	// Call this whenever a cell is expending energy on movement
	void ExpendEnergy (float speed) {
		// expend energy first
		energy -= useEfficiency * Mathf.Abs(speed);
		// cell dies if energy has been depleted
		if (energy <= 0f) {
			dead = true;
			rb.velocity = Vector2.zero;
			EnvironmentScript.liveCells--;
			GetComponent<Renderer>().material.color = Color.black;
		}
	}

	// Make sure cells don't leave the boundaries of the field
	void WrapField () {
		if (EnvironmentScript.fieldWrapping) {
			if (tf.position.x > EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x-2*EnvironmentScript.fieldRadius+radius, tf.position.y, 0f);
			}
			if (tf.position.y > EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x, tf.position.y-2*EnvironmentScript.fieldRadius+radius, 0f);
			}
			if (tf.position.x < -EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x+2*EnvironmentScript.fieldRadius-radius, tf.position.y, 0f);
			}
			if (tf.position.y < -EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x, tf.position.y+2*EnvironmentScript.fieldRadius-radius, 0f);
			}
		}
	}

	// Exactly what it says on the tin
	void DrawDebugLinesToNearbyCells () {
		foreach (GameObject cell in nearbyCells) {
			Debug.DrawLine(tf.position, cell.GetComponent<Transform>().position, Color.blue);
		}
		foreach (GameObject cube in nearbySugar) {
			Debug.DrawLine(tf.position, cube.GetComponent<Transform>().position, Color.green);
		}
	}
}
