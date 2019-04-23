using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class InfoManager : MonoBehaviour
{
	[SerializeField] List<DestinationInfo> infoList = new List<DestinationInfo>();
	[SerializeField] RectTransform closeUI;
	CreateMapSample mapManager;
	DestinationInfo currentInfo;
	RaycastHit hit;

	private void Start()
	{
		mapManager = GetComponent<CreateMapSample>();
		closeUI.DOScale(0, 0);
	}

	private void Update()
	{
		if (currentInfo != null|| !mapManager.IsInitialized)
			return;

#if UNITY_EDITOR
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				if (hit.transform.GetComponent<DestinationTarget>() != null &&
					hit.transform.GetComponent<DestinationTarget>().isActive)
				{
					SpawnInfo(hit.transform.GetComponent<DestinationTarget>().DestinationIndex);
				}
			}
		}	
#endif
		for (var i = 0; i < Input.touchCount; ++i)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
					// Construct a ray from the current touch coordinates
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);

				if (Physics.Raycast(ray, out hit))
				{
					if(hit.transform.GetComponent<DestinationTarget>()!=null &&
					   hit.transform.GetComponent<DestinationTarget>().isActive)
					{
						SpawnInfo(hit.transform.GetComponent<DestinationTarget>().DestinationIndex);
					}
				}

			}
		}
	}

	public void SpawnInfo(int index)
	{
		foreach(DestinationInfo info in infoList)
		{
			if (info.InFoIndex != index)
				continue; // if index not matching skip loop
			
			info.gameObject.transform.DOScale(new Vector3(1,1,1), 0.25f);
			StartCoroutine(delayActivate());
			//set refference to the popupInfo
			currentInfo = info;
		}
	}

	IEnumerator delayActivate()
	{
		yield return new WaitForSeconds(0.25f);
		closeUI.DOScale(1, 0.05f);
	}

	public void Close()
	{
		if(currentInfo == null)
		{
			closeUI.DOScale(0, 0f);
		}
		else if(currentInfo != null)
		{
			currentInfo.gameObject.transform.DOScale(0, 0.25f);
			closeUI.DOScale(0, 0);
			currentInfo = null;
		}
		
	}
	
}
