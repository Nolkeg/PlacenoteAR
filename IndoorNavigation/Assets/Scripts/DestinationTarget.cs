using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationTarget : MonoBehaviour
{
	Vector3 rotate = new Vector3(0, 0.5f, 0);
	public GameObject destinationMesh;
	public string DestinationName;
	public int DestinationIndex;
	public string linkMapID;
	[SerializeField] MapNameFromID loadMapPopUp;
	private void Start()
	{
		loadMapPopUp = FindObjectOfType<MapNameFromID>();
	}

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

	private void OnTriggerEnter(Collider other)
	{
		if(!destinationMesh.activeInHierarchy)
		{
			return;
		}

		if(loadMapPopUp == null)
		{
			loadMapPopUp = FindObjectOfType<MapNameFromID>();
		}

		if (other.CompareTag("MainCamera") && linkMapID != null && loadMapPopUp!= null)
		{
			loadMapPopUp.ChangeUI(linkMapID);
		}
	}
}
