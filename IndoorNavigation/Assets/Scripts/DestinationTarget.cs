using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DestinationTarget : MonoBehaviour
{
	Vector3 rotate = new Vector3(0, 0.5f, 0);
	public GameObject destinationMesh;
	public string DestinationName;
	public int DestinationIndex;
	public string linkMapID;
	[SerializeField] MapNameFromID loadMapPopUp;
	CreateMapSample mapManager;
	
	private void Start()
	{
		loadMapPopUp = FindObjectOfType<MapNameFromID>();
		mapManager = FindObjectOfType<CreateMapSample>();
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
		if(active)
		{
			StopAllCoroutines();
			destinationMesh.SetActive(true);
			destinationMesh.transform.localScale = Vector3.zero;
			destinationMesh.transform.DOScale(1, 0.25f);
		}
		else
		{
			StopAllCoroutines();
			StartCoroutine(Deactivate());
		}
	}
	IEnumerator Deactivate()
	{
		destinationMesh.transform.localScale = new Vector3(1, 1, 1);
		destinationMesh.transform.DOScale(0, 0.25f);
		yield return new WaitForSeconds(0.25f);
		destinationMesh.SetActive(false);
	}
	

	private void OnTriggerEnter(Collider other)
	{

		if(!destinationMesh.activeInHierarchy || !mapManager.IsInitialized)
		{
			return;
		}

		if(loadMapPopUp == null)
		{
			loadMapPopUp = FindObjectOfType<MapNameFromID>();
		}

		if (other.CompareTag("LoadMapCollider") && linkMapID != null && loadMapPopUp != null)
		{
			loadMapPopUp.ChangeUI(linkMapID);
		}
	}
	
}
