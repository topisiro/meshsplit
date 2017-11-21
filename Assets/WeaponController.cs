using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;


public class WeaponController : MonoBehaviour {

	public GameObject sword;
	public GameObject hammer;

	// Use this for initialization
	void Start () {
		sword.SetActive(false);
		hammer.SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {

		if(ViveInput.GetPressDown(HandRole.RightHand, ControllerButton.Menu))
			SwitchWeapon();
	}

	public void SwitchWeapon()
	{
		sword.SetActive(!sword.activeInHierarchy);
		hammer.SetActive(!hammer.activeInHierarchy);
	}
}
