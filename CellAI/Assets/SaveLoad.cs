using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Save Gen numbers + number of cells
using System;
using System.IO;
public class SaveLoad : MonoBehaviour {

	public string fileName = "saveFile.txt";
	CellSpawning CellSpawning;
	//public List<GameObject> cells;

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
	void Awake(){
		CellSpawning = gameObject.GetComponent<CellSpawning>();

	}
	void Start () {



	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SaveCells(List<GameObject> cells) {
		Debug.Log ("Saving generation");
		StreamWriter save = new StreamWriter (fileName);
		save.WriteLine("{0}", cells.Count);
		save.Flush();
		foreach (GameObject cell in cells) {
			CellBehaviourScript behaviour = cell.GetComponent<CellBehaviourScript>();
			save.WriteLine(String.Format ("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}",
			                              behaviour.intakeSpeed,behaviour.processingSpeed,behaviour.sugarCapacity,behaviour.maxMovementSpeed,behaviour.useEfficiency,behaviour.energyCapacity,behaviour.courage,behaviour.hostility,behaviour.cowardice,behaviour.greed));
			
			save.Flush();
		}
		save.Close ();
	}

	public void LoadCells() {
		//Debug.Log (CellSpawning.name);
		bool needRandom = false;
		int numRandomSpawn = 0;
		StreamReader Load = new StreamReader (fileName);
		int numCellsToSpawn;
		string FileString =  Load.ReadLine();
		//print (FileString);
		numCellsToSpawn = Int32.Parse (FileString);
		//print (FileString);
		//print (numCellsToSpawn);
		
		//FileString = Load.ReadLine();
		for (int i = 0; i < numCellsToSpawn;i++){
		FileString = Load.ReadLine();
		string[] stats = FileString.Split (':');	
		foreach (string stat in stats){
		//	print (stat);
		}
			CellSpawning.GenerateCellFromTemplate(float.Parse(stats[0]),float.Parse(stats[1]),float.Parse(stats[2]),float.Parse(stats[3]),float.Parse(stats[4]),float.Parse(stats[5]),float.Parse(stats[6]),float.Parse(stats[7]),float.Parse(stats[8]),float.Parse(stats[9]));
		}
		//print (FileString);
		//Debug.Log (CellSpawning.GetComponent<CellSpawning> ().cellsPerGeneration);
		if(CellSpawning.cellsPerGeneration > numCellsToSpawn){
			needRandom = true;
			numRandomSpawn = CellSpawning.cellsPerGeneration - numCellsToSpawn;
		}
		if (needRandom) {
			Debug.Log ("Spawning " + numRandomSpawn.ToString() + " more cells");
			for (int i = 0 ; i < numRandomSpawn; i++){
			CellSpawning.GenerateCellRandom();		
			}
		}
		//
		//FileString = Load.ReadLine ();


	}
}
