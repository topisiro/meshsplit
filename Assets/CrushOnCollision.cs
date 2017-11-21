using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MeshSplitting.Splitters;
using MeshSplitting.Splitables;
using UnityEngine.Events;
using System.Linq;
using HTC.UnityPlugin.Vive;



public class CrushOnCollision : MonoBehaviour {

	public GameObject hitParticles;

	float cooldown = 2f;
	bool recentCrush = false;
	float crushCooldown = 0.1f;
	float crushTimer = 0f;
	bool superhotTime = false;
	float superhotTimer = 0f;
	float superhotInitialTimer = 0f;
	float superhotInitialDuration = 0.5f;
	float superhotDuration = 0.3f;
	float maxTimeScaleChangePerFrame = 0.01f;

	Vector3 previousPosition;
	Vector3 impactPoint;

	Collider collider;

	public List<GameObject> lastSplitObjects;

	// Use this for initialization
	void Start () {
		lastSplitObjects = new List<GameObject>();
		collider = GetComponent<Collider>();

		GameManager.instance.onReset.AddListener(Reset);
	}

	// Update is called once per frame
	void Update () {

		if(recentCrush)
		{			
			crushTimer += Time.deltaTime;

			if(crushTimer >= crushCooldown)
			{
				recentCrush = false;
				crushTimer = 0f;
			}
		}

		if(superhotTime)
		{
			superhotTimer += Time.deltaTime;
			superhotInitialTimer += Time.unscaledDeltaTime;

			if(superhotInitialTimer < superhotInitialDuration)
			{
				Time.timeScale = 0.1f;
				Time.fixedDeltaTime = 0.0111111f * Time.timeScale;
			}
			else
			{
				collider.enabled = true;
				Time.timeScale = Mathf.Clamp(0.2f* (transform.position - previousPosition).magnitude / Time.unscaledDeltaTime, Time.timeScale - maxTimeScaleChangePerFrame, Time.timeScale + maxTimeScaleChangePerFrame);
				Time.timeScale = Mathf.Clamp(Time.timeScale, 0.0025f, 3f);
				Time.fixedDeltaTime = Time.timeScale < 1 ? 0.0111111f * Time.timeScale : 0.011f;
			}

			if(superhotTimer > superhotDuration)
			{
				superhotTime = false;
				superhotTimer = 0f;
				superhotInitialTimer = 0f;
				Time.timeScale = 1f;
				Time.fixedDeltaTime = 0.0111111f;
			}
		}

		previousPosition = transform.position;
	}

	void OnTriggerEnter(Collider col)
	{
		Valve.VR.InteractionSystem.Hand hand = null;
		Transform p = col.transform.parent;
		if(p != null)
		{
			
			hand = p.GetComponent<Valve.VR.InteractionSystem.Hand>();								

			if(hand == null)
			{
				while(p.parent != null)
				{
					if(p.parent.GetComponent<Valve.VR.InteractionSystem.Hand>() != null)
					{
						hand = p.parent.GetComponent<Valve.VR.InteractionSystem.Hand>();
						break;
					}

					p = p.parent;
				}
			}
		}

		if(hand != null)
		{
			col.GetComponent<Valve.VR.InteractionSystem.Throwable>().enabled = false;
			col.GetComponent<Rigidbody>().isKinematic = false;
			hand.DetachAllObjects();
		}
	}

	void OnCollisionEnter(Collision col)
	{
		if(recentCrush)
			return;

		if((lastSplitObjects.Contains(col.gameObject)))
			return;		

		ISplitable splitable = null;

		MonoBehaviour[] components = col.gameObject.GetComponents<MonoBehaviour>();
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
			recentCrush = true;
			crushTimer = 0f;		
			impactPoint = col.contacts[0].point;
			Instantiate(hitParticles, impactPoint, Quaternion.LookRotation(transform.position - ((Splitable)splitable).transform.position), ((Splitable)splitable).transform);
//			Destroy(hitParticles, 5f);
			superhotTime = true;
			collider.enabled = false;
			StartCoroutine(Crush(splitable, 4));
			col.gameObject.GetComponent<Valve.VR.InteractionSystem.Throwable>().enabled = true;
		}
	}

	IEnumerator Crush(ISplitable splitable, int numIterations)
	{			
		yield return new WaitForSeconds(0.005f);

		
		//impactPoint = transform.position + 0.5f * transform.forward;

		for(int i = 0; i < numIterations; i++)
		{

			GameObject gocrushPlane = new GameObject("crushPlane", typeof(BoxCollider), typeof(Rigidbody)/*, typeof(SplitterSingleCut)*/);

			gocrushPlane.GetComponent<Collider>().isTrigger = true;
			Rigidbody bodycrushPlane = gocrushPlane.GetComponent<Rigidbody>();
			bodycrushPlane.useGravity = false;
			bodycrushPlane.isKinematic = true;

			Transform transformcrushPlane = gocrushPlane.transform;

			Bounds bounds = ((Splitable)splitable).GetComponent<Renderer>().bounds;
			//transformcrushPlane.position = ((Splitable)splitable).transform.position + Random.Range(-bounds.extents.x, bounds.extents.x) * Vector3.right + Random.Range(-bounds.extents.y, bounds.extents.y) * Vector3.up + Random.Range(-bounds.extents.z, bounds.extents.z) * Vector3.forward;		
			transformcrushPlane.position = bounds.center + 0.2f * Random.onUnitSphere;	
			transformcrushPlane.up = Quaternion.AngleAxis(Random.Range(0f, 360f), transform.right) * transform.up;
			transformcrushPlane.up = Quaternion.AngleAxis(Random.Range(0f, 360f), transformcrushPlane.forward) * transformcrushPlane.up;
			transformcrushPlane.localScale = new Vector3(20f, .01f, 20f);

			if((Splitable)splitable != null)
				((Splitable)splitable).onSplitObject.AddListener(SplitResult);

			((Splitable)splitable).SplitForce = 0;
			splitable.Split(transformcrushPlane);

			foreach(var item in lastSplitObjects.ToList())
			{
				if(item != null)
				{
					if(item.GetComponent<Splitable>() != null)
						item.GetComponent<Splitable>().onSplitObject.AddListener(SplitResult);

					item.GetComponent<Splitable>().Split(transformcrushPlane);
				}
			}

			yield return null;
			DestroyObject(transformcrushPlane.gameObject);
		}	

		foreach(var rb in lastSplitObjects.Select(x => x.GetComponent<Rigidbody>()))
		{
			//Debug.Log("BOOM");
			rb.velocity = Vector3.zero;
			rb.AddForce(1000 * (rb.transform.position - impactPoint), ForceMode.Impulse);
		}
	}

	void SplitResult(GameObject original, GameObject newObj)
	{
//		Debug.Log("Called splitresult");
		if(!lastSplitObjects.Contains(newObj))
		{
			lastSplitObjects.Add(newObj);
			StartCoroutine(RemoveFromList(newObj));
		}
		if(!lastSplitObjects.Contains(original))
		{
			lastSplitObjects.Add(original);
			StartCoroutine(RemoveFromList(original));
		}
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

	public void Reset()
	{		
		recentCrush = false;
		crushTimer = 0f;
	    superhotTime = false;
		superhotTimer = 0f;
		superhotInitialTimer = 0f;
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.011f;
		lastSplitObjects = new List<GameObject>();
	}
}
