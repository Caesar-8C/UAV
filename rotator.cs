using UnityEngine;
using System.Collections;

public class rotator : MonoBehaviour {
	public int spinDirection;
	private float prevTime;

	void Start () {
		prevTime = Time.time;
	}
	
	void Update () {
		Vector3 eulerAngles = new Vector3(0, spinDirection*(Time.time - prevTime)*200, 0);
		prevTime = Time.time;
		transform.Rotate(eulerAngles, Space.Self);
	}
}
