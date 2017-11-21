using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour {

	public GameObject target;
	public float offset;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.position = target.transform.position + target.transform.forward * (offset);
		transform.rotation = target.transform.rotation;
	}
}
