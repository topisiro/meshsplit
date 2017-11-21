using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshSplitting.Splitters;
using MeshSplitting.Splitables;
using UnityEngine.Events;
using System.Linq;




public class SplitOnCollision : MonoBehaviour {


	float cooldown = 0.15f;
	bool recentCut = false;
	float cutCooldown = 0f;
	float cutTimer = 0f;

	public List<GameObject> lastSplitObjects;

	// Use this for initialization
	void Start () {
		lastSplitObjects = new List<GameObject>();
	}

	// Update is called once per frame
	void Update () {

		if(recentCut)
		{
			cutTimer += Time.deltaTime;

			if(cutTimer >= cutCooldown)
			{
				recentCut = false;
				cutTimer = 0f;
			}
		}
	}


	void OnTriggerEnter(Collider col)
	{
		if(recentCut)
			return;

		if((lastSplitObjects.Contains(col.gameObject)))
			return;		

		ISplitable splitable = null;
	
		MonoBehaviour[] components = col.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour component in components)
		{
			splitable = component as ISplitable;
			if (splitable != null)
			{				
				break;
			}
		}

		if(splitable != null)
		{			
			//GetComponent<Collider>().enabled = false;
			//Invoke("EnableCollider", 0.1f);
			Debug.Log("Collided");
			recentCut = true;
			cutTimer = 0f;
			lastSplitObjects.Add(col.gameObject);
			StartCoroutine(RemoveFromList(col.gameObject));

			GameObject goCutPlane = new GameObject("CutPlane", typeof(BoxCollider), typeof(Rigidbody)/*, typeof(SplitterSingleCut)*/);

			goCutPlane.GetComponent<Collider>().isTrigger = true;
			Rigidbody bodyCutPlane = goCutPlane.GetComponent<Rigidbody>();
			bodyCutPlane.useGravity = false;
			bodyCutPlane.isKinematic = true;

			Transform transformCutPlane = goCutPlane.transform;
			transformCutPlane.position = transform.position;		
			transformCutPlane.up = -transform.forward;// transform.up;
			transformCutPlane.localScale = new Vector3(20f, .01f, 20f);

			if((Splitable)splitable != null)
				((Splitable)splitable).onSplitObject.AddListener(SplitResult);

			splitable.Split(transformCutPlane);
			DestroyObject(transformCutPlane.gameObject);
		}
	}

	void SplitResult(GameObject original, GameObject newObj)
	{
		
		lastSplitObjects.Add(newObj);
		StartCoroutine(RemoveFromList(newObj));
		lastSplitObjects.Add(original);
		StartCoroutine(RemoveFromList(original));
	}

	void EnableCollider()
	{
		GetComponent<Collider>().enabled = true;
	}

	IEnumerator RemoveFromList(GameObject go)
	{
		yield return new WaitForSeconds(cooldown);
		if(lastSplitObjects.Contains(go))
			lastSplitObjects.Remove(go);
	}
}
