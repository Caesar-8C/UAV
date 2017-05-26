using UnityEngine;
using System.Collections;

public class MonteScript : MonoBehaviour {

	public GameObject cube;
	public int amount;

	void Start () {
		for(int i = 0; i < amount; i++)
		{
			Instantiate(cube, new Vector3(Random.Range(0f, 90f), Random.Range(84f, 114f), Random.Range(-14f, 14f)), Quaternion.identity);
		}
	}
}
