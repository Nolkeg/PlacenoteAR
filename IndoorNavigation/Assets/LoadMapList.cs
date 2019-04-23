using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadMapList : MonoBehaviour
{
	TMP_Dropdown dropdownList;
	public string linkID;
	void Start()
	{
		dropdownList = GetComponent<TMP_Dropdown>();
		LoadListMap();
	}

	void LoadListMap()
	{
		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}

		LibPlacenote.Instance.ListMaps((mapList) =>
		{
			TMP_Dropdown.OptionData nullData = new TMP_Dropdown.OptionData();
			nullData.text = "none";
			dropdownList.options.Add(nullData); // add null choice to dropdown

			foreach (LibPlacenote.MapInfo mapInfoItem in mapList)
			{
				if (mapInfoItem.metadata.userdata != null)
				{
					TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
					data.text = mapInfoItem.metadata.name;
					dropdownList.options.Add(data); //add other map to choices to dropdown
				}
			}
		});
		dropdownList.RefreshShownValue();
	}

	public void OnLinkMapSelected()
	{
		LibPlacenote.Instance.ListMaps((mapList) =>
		{
			int index = dropdownList.value;

			foreach (LibPlacenote.MapInfo mapInfoItem in mapList)
			{
				if (mapInfoItem.metadata.userdata != null)
				{
					if(mapInfoItem.metadata.name == dropdownList.options[index].text)
					{
						linkID = mapInfoItem.placeId;
						break;
					}
					else
					{
						linkID = null;
					}
					
				}
			}
		});


	}
	
}
