using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

	public static GameManager instance;

	public List<GameObject> items;

	public UnityEvent onReset;

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else if(instance != this)
			Destroy(this.gameObject);
	}

	// Use this for initialization
	void Start () {
		foreach(var item in items)
		{
			item.SetActive(false);
			GameObject copy = Instantiate(item);
			copy.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Grip) || ViveInput.GetPressDown(HandRole.LeftHand, ControllerButton.Grip ))
			Reset();
	}

	public void Reset()
	{
		Clean();

		foreach(var item in items)
		{
			item.SetActive(false);
			GameObject copy = Instantiate(item);
			copy.SetActive(true);
		}

		onReset.Invoke();
	}

	public void Clean()
	{
		var pieces = FindObjectsOfType<MeshSplitting.Splitables.Splitable>();
		for(int i = pieces.Length - 1; i >= 0; i--)
			Destroy(pieces[i].gameObject);
	}
}
