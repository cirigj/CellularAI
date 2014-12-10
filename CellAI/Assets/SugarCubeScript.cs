using UnityEngine;
using System.Collections;

public class SugarCubeScript : MonoBehaviour {

	public float sugarLevel;
	private Transform tf;
	private Rigidbody2D rb;
	public bool depleted;

	// Use this for initialization
	void Start () {
		tf = GetComponent<Transform>();
		rb = GetComponent<Rigidbody2D>();
		rb.velocity = Random.insideUnitCircle * Random.Range(EnvironmentScript.sugarCubeSpeedMin, EnvironmentScript.sugarCubeSpeedMax);
		depleted = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		tf.localScale = new Vector3(sugarLevel, sugarLevel, sugarLevel);
		if (sugarLevel <= 0.25f) {
			depleted = true;
		}
		WrapField();
	}

	// Make sure cells don't leave the boundaries of the field
	void WrapField () {
		if (EnvironmentScript.fieldWrapping) {
			if (tf.position.x > EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x-2*EnvironmentScript.fieldRadius, tf.position.y, 0f);
			}
			if (tf.position.y > EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x, tf.position.y-2*EnvironmentScript.fieldRadius, 0f);
			}
			if (tf.position.x < -EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x+2*EnvironmentScript.fieldRadius, tf.position.y, 0f);
			}
			if (tf.position.y < -EnvironmentScript.fieldRadius) {
				tf.position = new Vector3(tf.position.x, tf.position.y+2*EnvironmentScript.fieldRadius, 0f);
			}
		}
	}
}
