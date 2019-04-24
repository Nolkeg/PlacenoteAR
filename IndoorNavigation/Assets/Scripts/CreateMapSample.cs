using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using TMPro;

[RequireComponent(typeof(InputManager))]
public class CreateMapSample : MonoBehaviour, PlacenoteListener
{
	[SerializeField] GameObject mExtendedPanel;
	[SerializeField] GameObject mMapSelectedPanel;
	[SerializeField] GameObject mInitButtonPanel;
	[SerializeField] GameObject mMappingButtonPanel;
	[SerializeField] GameObject mSimulatorAddShapeButton;
	[SerializeField] GameObject mMapListPanel;
	[SerializeField] GameObject mExitButton;
	[SerializeField] GameObject mActivateDesButton;
	[SerializeField] GameObject mListElement;
	[SerializeField] RectTransform mListContentParent;
	[SerializeField] ToggleGroup mToggleGroup;
	[SerializeField] Slider mRadiusSlider;
	[SerializeField] float mMaxRadiusSearch;
	[SerializeField] Text mRadiusLabel;
	[SerializeField] GameObject MapnameInputPopUp;
	[SerializeField] TMP_Dropdown DropdownList;
	[SerializeField] TextMeshProUGUI statusText;
	[SerializeField] GameObject waitPopUp;
	[SerializeField] GameObject scanPopup;
	public GameObject selectDesPopUp;
	
	private bool localizeFirstTime;
	private InputManager inputManager;
	private InfoManager infoManager;
	public NavController navController;
	private UnityARSessionNativeInterface mSession;
	private LibPlacenote.MapInfo mSelectedMapInfo;
	private AddShapeWaypoint shapeManager;
	public Node destination;

	private bool initialized;
	public bool IsInitialized
	{
		get
		{
			return initialized;
		}
	}

	private bool mapping;
	public bool IsMapping
	{
		get
		{
			return mapping;
		}
	}

	[SerializeField]List<DestinationTarget> destinationList = new List<DestinationTarget>();
	private string mSelectedMapId
	{
		get
		{
			return mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
		}
	}

	private string mSelectedMapName
	{
		get
		{
			return mSelectedMapInfo != null ? mSelectedMapInfo.metadata.name : null;
		}
	}
	private string mSaveMapId = null;
	private bool mReportDebug = false;
	private bool mARInit = false;
	public bool haveMapName = false;
	private string mapname;
	
	public enum Status { Mapping,Waiting,Running,Lost };
	public static Status mapStatus;

	private LibPlacenote.MapMetadataSettable mCurrMapDetails;

	public string ReturnMapName()
	{
		return mSelectedMapInfo != null ? mSelectedMapInfo.metadata.name : null;
	}

	void Start()
    {
		localizeFirstTime = false;
		haveMapName = false;
		mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
		StartARKit();
		FeaturesVisualizer.EnablePointcloud(); // Optional - to see the debug features
		LibPlacenote.Instance.RegisterListener(this);
		shapeManager = GetComponent<AddShapeWaypoint>();
		inputManager = GetComponent<InputManager>();
		infoManager = GetComponent<InfoManager>();
	}

	private void StartARKit()
	{
		Application.targetFrameRate = 60;
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
		mSession.RunWithConfig(config);
	}

	void Update()
	{
		if (!mARInit && LibPlacenote.Instance.Initialized())
		{
			mARInit = true;
			statusText.text = "Ready to Start!";
		}
	}

	public void OnListMapClick()
	{
		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}

		foreach (Transform t in mListContentParent.transform)
		{
			Destroy(t.gameObject);
		}


		mMapListPanel.SetActive(true);
		mInitButtonPanel.SetActive(false);
		//mRadiusSlider.gameObject.SetActive(true);
		LibPlacenote.Instance.ListMaps((mapList) => {
			// render the map list!
			foreach (LibPlacenote.MapInfo mapInfoItem in mapList)
			{
				if (mapInfoItem.metadata.userdata != null)
				{
					Debug.Log(mapInfoItem.metadata.userdata.ToString(Formatting.None));
				}
				AddMapToList(mapInfoItem);
			}
		});
	}

	public void OnRadiusSelect()
	{
		if (!Input.location.isEnabledByUser)
		{
			statusText.text = "Location service is unavailable";
			return;
		}
			
		Debug.Log("Map search:" + mRadiusSlider.value.ToString("F2"));
		statusText.text = "Filtering maps by GPS location";

		LocationInfo locationInfo = Input.location.lastData;


		float radiusSearch = mRadiusSlider.value * mMaxRadiusSearch;
		mRadiusLabel.text = "Distance Filter: " + (radiusSearch / 1000.0).ToString("F2") + " km";

		LibPlacenote.Instance.SearchMaps(locationInfo.latitude, locationInfo.longitude, radiusSearch,
			(mapList) => {
				foreach (Transform t in mListContentParent.transform)
				{
					Destroy(t.gameObject);
				}
				// render the map list!
				foreach (LibPlacenote.MapInfo mapId in mapList)
				{
					if (mapId.metadata.userdata != null)
					{
						Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
					}
					AddMapToList(mapId);
				}

				statusText.text = "Map List Complete";
			});
	}

	void AddMapToList(LibPlacenote.MapInfo mapInfo)
	{
		GameObject newElement = Instantiate(mListElement) as GameObject;
		MapInfoElement listElement = newElement.GetComponent<MapInfoElement>();
		listElement.Initialize(mapInfo, mToggleGroup, mListContentParent, (value) => {
			OnMapSelected(mapInfo);
		});
	}

	public void OnMapSelected(LibPlacenote.MapInfo mapInfo)
	{
		mSelectedMapInfo = mapInfo;
		mMapSelectedPanel.SetActive(true);
		mRadiusSlider.gameObject.SetActive(false);
	}

	public void ResetSlider()
	{
		mRadiusSlider.value = 1.0f;
		mRadiusLabel.text = "Distance Filter: Off";
	}

	public void OnCancelClick()
	{
		mMapSelectedPanel.SetActive(false);
		mMapListPanel.SetActive(false);
		mInitButtonPanel.SetActive(true);
		ResetSlider();
	}

	public void OnDeleteMapClicked()
	{
		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}

		statusText.text = "Deleting Map ID: " + mSelectedMapId;
		LibPlacenote.Instance.DeleteMap(mSelectedMapId, (deleted, errMsg) => {
			if (deleted)
			{
				mMapSelectedPanel.SetActive(false);
				statusText.text = "Deleted ID: " + mSelectedMapId;
				OnListMapClick();
			}
			else
			{
				statusText.text = "Failed to delete ID: " + mSelectedMapId;
			}
		});
	}

	public void OnNewMapClicked()
	{
		ConfigureSession();

		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}
		mInitButtonPanel.SetActive(false);
		mMappingButtonPanel.SetActive(true);
		mapping = true;

		LibPlacenote.Instance.StartSession();
		if (mReportDebug)
		{
			LibPlacenote.Instance.StartRecordDataset(
				(completed, faulted, percentage) => {
					if (completed)
					{
						statusText.text = "Dataset Upload Complete";
						
					}
					else if (faulted)
					{
						statusText.text = "Dataset Upload Faulted";
					}
					else
					{
						statusText.text = "Dataset Upload: (" + percentage.ToString("F2") + "/1.0)";
					}
				});
			Debug.Log("Started Debug Report");
		}
	}


	IEnumerator SaveMap()
	{
		bool useLocation = Input.location.status == LocationServiceStatus.Running;
		mapping = false;
		LocationInfo locationInfo = Input.location.lastData;
		MapnameInputPopUp.SetActive(true);
		statusText.text = "Wait for map name input....";

		yield return new WaitUntil(() => haveMapName == true);

		statusText.text = "Saving...";
		LibPlacenote.Instance.SaveMap(
			(mapId) => {

				LibPlacenote.Instance.StopSession();
				FeaturesVisualizer.clearPointcloud();

				mSaveMapId = mapId;
				mInitButtonPanel.SetActive(true);
				mMappingButtonPanel.SetActive(false);
				mExitButton.SetActive(false);

				LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();

				metadata.name = inputManager.MapName; // set name here

				statusText.text = "Saved Map Name: " + metadata.name;

				JObject userdata = new JObject();
				metadata.userdata = userdata;

				JObject shapeList = shapeManager.Shapes2JSON();

				userdata["shapeList"] = shapeList;
				//can add userdata of trigger later
				shapeManager.ClearShapes();

				if (useLocation)
				{
					metadata.location = new LibPlacenote.MapLocation();
					metadata.location.latitude = locationInfo.latitude;
					metadata.location.longitude = locationInfo.longitude;
					metadata.location.altitude = locationInfo.altitude;
				}
				LibPlacenote.Instance.SetMetadata(mapId, metadata, (success) => {
					if (success)
					{
						Debug.Log("Meta data successfully saved");
					}
					else
					{
						Debug.Log("Meta data failed to save");
					}
				});
				mCurrMapDetails = metadata;
			},
			(completed, faulted, percentage) => {
			if (completed)
				{
					statusText.text = "Upload Complete:" + mCurrMapDetails.name;
					OnExitClick();
				}
				else if (faulted)
				{
					statusText.text = "Upload of Map Named: " + mCurrMapDetails.name + "faulted";
				}
				else
				{
					statusText.text = "Uploading Map Named: " + mCurrMapDetails.name + "(" + percentage.ToString("F2") + "/1.0)";
				}
			}
		);

	}


	public void OnSaveExtendedMap()
	{
		var mapId = mSelectedMapId;
		mInitButtonPanel.SetActive(true);
		mMappingButtonPanel.SetActive(false);
		mExitButton.SetActive(false);

		LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();

		metadata.name = mSelectedMapName; // set name here

		statusText.text = "Saved Map Name: " + metadata.name;

		JObject userdata = new JObject();
		metadata.userdata = userdata;

		JObject shapeList = shapeManager.Shapes2JSON();
		userdata.ClearItems();
		userdata["shapeList"] = shapeList;
		LibPlacenote.Instance.SetMetadata(mapId, metadata, (success) => {
			if (success)
			{
				Debug.Log("Meta data succesfully saved");
				OnExitClick();
			}
			else
			{
				Debug.Log("Meta data failed to save");
			}
		});
	}
	

	public void OnSaveMapClicked()
	{
		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}
		StartCoroutine(SaveMap());
	}

	IEnumerator PrepareNode()
	{
		shapeManager.CreateNode();
		yield return new WaitUntil(() => shapeManager.nodeLoaded == true);
		if(destination != null)
		{
			navController.InitializeNavigation();
		}
		shapeManager.nodeLoaded = false;
	}

	public void OnFindPathClicked()
	{
		if(navController != null)
		{
			navController.ReSetParameter();
			StartCoroutine(PrepareNode());
			selectDesPopUp.SetActive(false);
		}
	}

	public void OnDropDownSelected() //a method to set the selected destination as current destination
	{
		if(destinationList.Count ==0)
		{
			Debug.Log("No Destination in list");
			return;
		}
				
		int index = DropdownList.value;
		foreach(DestinationTarget des in destinationList)
		{
			if(des.DestinationName == DropdownList.options[index].text)
			{
				destination = des.gameObject.GetComponent<Node>();
			}
		}
		
	}

	public void OnLoadMapClicked()
	{
		ConfigureSession();

		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}

		statusText.text = "Loading Map: " + mSelectedMapName;

		mMapSelectedPanel.SetActive(false);
		mMapListPanel.SetActive(false);
		mInitButtonPanel.SetActive(false);
		mMappingButtonPanel.SetActive(false);
		waitPopUp.SetActive(true);

		LibPlacenote.Instance.LoadMap(mSelectedMapId,
			(completed, faulted, percentage) => {
				if (completed)
				{
					mExitButton.SetActive(true);
					mActivateDesButton.SetActive(true);
					DropdownList.gameObject.SetActive(true);
					LoadDestinationList();
					initialized = true;
					LibPlacenote.Instance.StartSession();

					if (mReportDebug)
					{
						LibPlacenote.Instance.StartRecordDataset(
							(datasetCompleted, datasetFaulted, datasetPercentage) => {

								if (datasetCompleted)
								{
									statusText.text = "Dataset Upload Complete";
								}
								else if (datasetFaulted)
								{
									statusText.text = "Dataset Upload Faulted";
								}
								else
								{
									statusText.text = "Dataset Upload: " + datasetPercentage.ToString("F2") + "/1.0";
								}
							});
						Debug.Log("Started Debug Report");
					}
					Vector3 posOffSet = Vector3.left * 1f + Vector3.down * 2.5f + navController.transform.forward * 5;
					Vector3 signPos = navController.transform.position + posOffSet;

					statusText.text = "Loaded Map: " + mSelectedMapName;
					waitPopUp.SetActive(false);
					scanPopup.SetActive(true);
				}
				else if (faulted)
				{
					statusText.text = "Failed to load ID: " + mSelectedMapId;
				}
				else
				{
					statusText.text = "Map Download: " + percentage.ToString("F2") + "/1.0";
					
				}
			}
		);
	}

	public void OnExtendedMapClicked()
	{
		ConfigureSession();

		if (!LibPlacenote.Instance.Initialized())
		{
			Debug.Log("SDK not yet initialized");
			return;
		}

		statusText.text = "Loading Map: " + mSelectedMapName;

		mMapSelectedPanel.SetActive(false);
		mMapListPanel.SetActive(false);
		mInitButtonPanel.SetActive(false);
		mMappingButtonPanel.SetActive(false);
		waitPopUp.SetActive(true);

		LibPlacenote.Instance.LoadMap(mSelectedMapId,
			(completed, faulted, percentage) => {
				if (completed)
				{
					mExitButton.SetActive(true);
					mActivateDesButton.SetActive(true);
					mExtendedPanel.SetActive(true);
					DropdownList.gameObject.SetActive(true);
					LoadDestinationList();
					initialized = false;
					mapping = true;
					LibPlacenote.Instance.StartSession(true);

					if (mReportDebug)
					{
						LibPlacenote.Instance.StartRecordDataset(
							(datasetCompleted, datasetFaulted, datasetPercentage) => {

								if (datasetCompleted)
								{
									statusText.text = "Dataset Upload Complete";
								}
								else if (datasetFaulted)
								{
									statusText.text = "Dataset Upload Faulted";
								}
								else
								{
									statusText.text = "Dataset Upload: " + datasetPercentage.ToString("F2") + "/1.0";
								}
							});
						Debug.Log("Started Debug Report");
					}
					Vector3 posOffSet = Vector3.left * 1f + Vector3.down * 2.5f + navController.transform.forward * 5;
					Vector3 signPos = navController.transform.position + posOffSet;

					statusText.text = "Loaded Map: " + mSelectedMapName;
					waitPopUp.SetActive(false);
					scanPopup.SetActive(true);
				}
				else if (faulted)
				{
					statusText.text = "Failed to load ID: " + mSelectedMapId;
				}
				else
				{
					statusText.text = "Map Download: " + percentage.ToString("F2") + "/1.0";

				}
			}
		);
	}

	public void OnExitClick()
	{
		LibPlacenote.Instance.StopSession();
		FeaturesVisualizer.clearPointcloud();
		initialized = false;
		haveMapName = false;
		mapping = false;
		localizeFirstTime = false;
		mInitButtonPanel.SetActive(true);
		mExitButton.SetActive(false);
		mExtendedPanel.SetActive(false);
		mActivateDesButton.SetActive(false);
		mMappingButtonPanel.SetActive(false);
		DropdownList.gameObject.SetActive(false);
		scanPopup.SetActive(false);
		selectDesPopUp.SetActive(false);
		waitPopUp.SetActive(false);
		destination = null;
		destinationList.Clear();
		DropdownList.value = 0;
		DropdownList.options.Clear();
		DropdownList.RefreshShownValue();
		shapeManager.ClearShapes();
		navController.ReSetParameter();
		infoManager.Close();
		StopAllCoroutines();
		StartCoroutine(UIcheck());
	}

	IEnumerator UIcheck()
	{
		yield return new WaitForSeconds(1);
		scanPopup.SetActive(false);
		selectDesPopUp.SetActive(false);
		waitPopUp.SetActive(false);
	}


	// Runs when a new pose is received from Placenote.    
	public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }

	
	// Runs when LibPlacenote sends a status change message like Localized!
	public void OnStatusChange(LibPlacenote.MappingStatus prevStatus, LibPlacenote.MappingStatus currStatus)
	{
		Debug.Log("prevStatus: " + prevStatus.ToString() + " currStatus: " + currStatus.ToString());
		if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.LOST)
		{
			if(!localizeFirstTime)
			{
				localizeFirstTime = true;
				mapStatus = Status.Running;
				shapeManager.LoadShapesJSON(mSelectedMapInfo.metadata.userdata);
				FeaturesVisualizer.DisablePointcloud(); //if player is doing navigation, disable point cloud
				LoadDestinationList();
				scanPopup.SetActive(false);
				selectDesPopUp.SetActive(true);
			}
			statusText.text = "Localized";
		}
		else if (currStatus == LibPlacenote.MappingStatus.RUNNING && prevStatus == LibPlacenote.MappingStatus.WAITING)
		{
			statusText.text = "Mapping: Tap to add Shapes";
			mapStatus = Status.Mapping;
			FeaturesVisualizer.EnablePointcloud(); //if mapping enable point cloud to be see
			mExitButton.SetActive(true);
		}
		else if (currStatus == LibPlacenote.MappingStatus.LOST)
		{
			statusText.text = "Searching for position lock";
			mapStatus = Status.Lost;
		}
		else if (currStatus == LibPlacenote.MappingStatus.WAITING)
		{
            
            mapStatus = Status.Waiting;
			if (shapeManager.shapeObjList.Count != 0)
			{
				shapeManager.ClearShapes();
			}
		}

	}

	IEnumerator DestinationToDropDown() //A method to load all destination in map to dropdown list
	{
		yield return new WaitUntil(() => shapeManager.shapesLoaded == true);
		List<string> desname = new List<string>();
		var tempDesArray = FindObjectsOfType<DestinationTarget>();
		if (tempDesArray == null)
		{
			yield break;
		}

		if (DropdownList.options.Count == tempDesArray.Length)
		{
			Debug.Log("already loaded destinationlist");
			yield break;
		}

		foreach (var des in tempDesArray)
		{
			desname.Add(des.DestinationName);
			destinationList.Add(des);
			Debug.Log(des.DestinationName);
		}
		foreach (var name in desname)
		{
			TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
			data.text = name;
			DropdownList.options.Add(data);
		}
		DropdownList.value = 0;
		DropdownList.RefreshShownValue(); // autoselect first value 0 ;

		if(destination == null)
		{
			destination = destinationList[0].GetComponent<Node>();
		}
	}

	public void LoadDestinationList()
	{
		StartCoroutine(DestinationToDropDown());
	}

	private void ConfigureSession()
	{
#if !UNITY_EDITOR
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration ();
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = true;
        config.planeDetection = UnityARPlaneDetection.Horizontal;
		mSession.RunWithConfig (config);
#endif
	}


}
