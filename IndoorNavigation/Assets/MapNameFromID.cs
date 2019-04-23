using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
public class MapNameFromID : MonoBehaviour
{
	[SerializeField] TMP_Text mapNameUI;
	[SerializeField] CreateMapSample mapManager;
	LibPlacenote.MapInfo triggerMapInfo;
	private void Start()
	{
		transform.localScale = Vector3.zero;
	}

	public void ChangeUI(string ID)
	{
		//pop Up
		transform.DOScale(1, 0.25f);
		LibPlacenote.Instance.ListMaps((mapList) => {
			// render the map list!
			foreach (LibPlacenote.MapInfo mapInfoItem in mapList)
			{
				if (mapInfoItem.metadata.name == null)
					continue;

				if (mapInfoItem.placeId == ID)
				{
					mapNameUI.text = mapInfoItem.metadata.name;
					triggerMapInfo = mapInfoItem;
					break;
				}

				triggerMapInfo = null;
				mapNameUI.text = "Error";
				
			}
		});
	}

	public void CloseUI()
	{
		transform.DOScale(0, 0.25f);
		triggerMapInfo = null;
	}

	public void CloseAndLoadNewMap()
	{
		if(triggerMapInfo==null)
		{
			return;
		}

		mapManager.OnExitClick();
		transform.DOScale(0, 0.25f);
		StartCoroutine(delayLoadMap());
	}

	IEnumerator delayLoadMap()
	{
		yield return new WaitForSeconds(1.5f);
		mapManager.OnMapSelected(triggerMapInfo);
		mapManager.OnLoadMapClicked();
	}


}
