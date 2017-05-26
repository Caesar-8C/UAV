using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;

public class stickRot : MonoBehaviour {

	public GameObject Lstick;
	public GameObject Rstick;

	private Vector3 prevPos;
	private Vector3 prevRot;
	private float prevTime;

	void Start () {
		prevPos = transform.position;
		prevRot = transform.eulerAngles;
		prevTime = Time.time;
	}

	void Update () {
		if(Time.time >= prevTime + 0.1)
		{
			Lstick.transform.eulerAngles = new Vector3((transform.position.y - prevPos.y)*20, (prevRot.y - transform.eulerAngles.y)*2, 0);
			Rstick.transform.eulerAngles = new Vector3(count()*20, 0, 0);

			prevPos = transform.position;
			prevRot = transform.eulerAngles;
			prevTime = Time.time;
		}	
	}

	float count()
	{
		return (float)Math.Sqrt(Math.Pow((transform.position.x - prevPos.x), 2) + Math.Pow((transform.position.z - prevPos.z), 2));
	}
}
