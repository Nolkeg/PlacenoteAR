using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
	[SerializeField] GameObject InfoUI;
	[SerializeField] List<DestinationInfo> infoList = new List<DestinationInfo>();
	DestinationInfo infoPanel;
	RaycastHit hit;

	private void Update()
	{
		if (infoPanel != null)
			return;
//#if !UNITY_EDITOR
		if(Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit))
			{
				if (hit.transform.GetComponent<DestinationTarget>() != null)
				{
					SpawnInfo(hit.transform.GetComponent<DestinationTarget>().DestinationIndex);
				}
			}
		}	
//#endif
		for (var i = 0; i < Input.touchCount; ++i)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{

				// Construct a ray from the current touch coordinates
				Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);

				if (Physics.Raycast(ray, out hit))
				{
					if(hit.transform.GetComponent<DestinationTarget>()!=null)
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
				continue;
			InfoUI.SetActive(true);
			infoPanel = Instantiate(info);
			infoPanel.transform.SetParent(InfoUI.transform);
			var positionn = infoPanel.GetComponent<RectTransform>();
			positionn.anchoredPosition = Vector2.zero;
		}
	}

	public void Close()
	{
		Destroy(infoPanel.gameObject);
		infoPanel = null;
		InfoUI.SetActive(false);
	}
}
