using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationTarget : MonoBehaviour
{
	Vector3 rotate = new Vector3(0, 1, 0);
	public GameObject diamond;
	public string DestinationName;

	private void Start()
	{
		Activate(true);
	}

	// Update is called once per frame
	void Update()
	{
		transform.eulerAngles += rotate;
	}

	public void Activate(bool active)
	{
		diamond.SetActive(active);
	}
	
}
