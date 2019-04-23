using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CreateMapSample))]
public class InputManager : MonoBehaviour
{
	public TMP_InputField MapNamePopUp;
	public TMP_InputField DestinationNamePopUp;
	public TMP_InputField InfoIndex;
	public LoadMapList DropdownLink;


	[HideInInspector] public string MapName;
	[HideInInspector] public string DestinationName;
	[HideInInspector] public string LinkID;
	[HideInInspector] public int index;

	CreateMapSample createMapScript;
	DestinationTarget currentDestination;
	AddShapeWaypoint shapemanager;
	private void Start()
	{
		createMapScript = GetComponent<CreateMapSample>();
		shapemanager = GetComponent<AddShapeWaypoint>();
	}

	public void OnApplyMapNameClicked()
	{
		if(MapNamePopUp.text == "")
		{
			createMapScript.haveMapName = false;
			print("don't have map name yet!");
			return;
		}
		else
		{
			MapName = MapNamePopUp.text;
			print("Map Name = " + MapName);
			createMapScript.haveMapName = true;
			MapNamePopUp.gameObject.SetActive(false);
		}
	}

	public void OnApplyDestinationNameClicked()
	{
		
		if(DestinationNamePopUp.text == "" || InfoIndex.text == "")
		{
			shapemanager.canAddDestination = false;
		}
		else
		{
			DestinationName = DestinationNamePopUp.text;
			index = int.Parse(InfoIndex.text);
			LinkID = DropdownLink.linkID;
			shapemanager.canAddDestination = true;
			DestinationNamePopUp.gameObject.SetActive(false);
		}
	}
	
	public void OnCancleDestinationNameClicked()
	{
		DestinationNamePopUp.gameObject.SetActive(false);
		ResetNameAndIndex();
		shapemanager.CanCelAddDestination();
	}

	public void ResetNameAndIndex()
	{
		DestinationName = null;
		index = 0;
		DestinationNamePopUp.text = null;
		MapNamePopUp.text = null;
		InfoIndex.text = null;
		DropdownLink.linkID = null;
		LinkID = null;
	}
}
