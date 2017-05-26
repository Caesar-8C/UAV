using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Car : MonoBehaviour {

	public GameObject center;
	public int distance;

	private int degree = 0;

	void Start () {
	}
	
	void Update () {
		transform.position = new Vector3((float)(Math.Sin(degree*Math.PI/180)*distance) + center.transform.position.x, 1.0f, (float)(Math.Cos(degree*Math.PI/180)*distance) + center.transform.position.z);
		transform.LookAt(center.transform.position, Vector3.up);

		degree--;

		if(degree < -360)
		{
			degree = 0;
		}
	}
}
