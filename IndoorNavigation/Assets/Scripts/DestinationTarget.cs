using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationTarget : MonoBehaviour
{
	Vector3 rotate = new Vector3(0, 0.5f, 0);
	public GameObject destinationMesh;
	public string DestinationName;
	public int DestinationIndex;

	public bool isActive
	{
		get
		{
			return destinationMesh.activeInHierarchy;
		}
	}

	// Update is called once per frame
	void Update()
	{
		transform.eulerAngles += rotate;
	}

	public void Activate(bool active)
	{
		destinationMesh.SetActive(active);
	}
	
}
