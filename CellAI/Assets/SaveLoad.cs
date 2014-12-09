using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Save Gen numbers + number of cells
using System;
using System.IO;
public class SaveLoad : MonoBehaviour {

	public string fileName = "saveFile.txt";
	public List<GameObject> cells;

	/*
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
	 */
	// Use this for initialization
	void Start () {

		StreamWriter save = new StreamWriter (fileName);
		save.WriteLine("{0}\n", cells.Count);
		save.Flush();
		foreach (GameObject cell in cells) {
			CellBehaviourScript behaviour = cell.GetComponent<CellBehaviourScript>();
			save.WriteLine(String.Format ("intakeSpeed {0}:processingSpeed {1}:sugarCapacity {2}:maxMovementSpeed {3}:useEfficiency {4}:energyCapacity {5}:courage {6}:hostility {7}:cowardice {8}:greed {9} \n",
			              behaviour.intakeSpeed,behaviour.processingSpeed,behaviour.sugarCapacity,behaviour.maxMovementSpeed,behaviour.useEfficiency,behaviour.energyCapacity,behaviour.courage,behaviour.hostility,behaviour.cowardice,behaviour.greed));

			save.Flush();
		}
		
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
