using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CreateMapSample))]
public class InputManager : MonoBehaviour
{
	public TMP_InputField MapNamePopUp;
	public TMP_InputField DestinationNamePopUp;


	[HideInInspector] public string MapName;
	[HideInInspector] public string DestinationName;

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
		MapName = MapNamePopUp.text;
		createMapScript.haveMapName = true;
		MapNamePopUp.gameObject.SetActive(false);
	}

	public void OnApplyDestinationNameClicked()
	{
		DestinationName = DestinationNamePopUp.text;
		shapemanager.canAddDestination = true;
		DestinationNamePopUp.gameObject.SetActive(false);
	}
	
	public void OnCancleDestinationNameClicked()
	{
		DestinationNamePopUp.gameObject.SetActive(false);
		shapemanager.CanCelAddDestination();
	}
}
