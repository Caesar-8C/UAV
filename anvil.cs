using UnityEngine;
using System.Collections;

public class anvil : MonoBehaviour {

	public GameObject tree;
	public Vector3 targetCoordinates;
	public float speed;
	public float rightAboutTime;
	private float startTime;

	void Start () {
		startTime = Time.time;
	}
	
	void Update () {
		if(Time.time >= startTime + rightAboutTime)
		{
			float step = speed*Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, targetCoordinates, step);
		}
		if(transform.position.y <= 5.7f && tree != null)
		{
			Destroy(tree);
		}
	}
}
