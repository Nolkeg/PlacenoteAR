using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class AddShapeWaypoint : MonoBehaviour
{
	public NavController navController;
	public List<GameObject> ShapePrefabs = new List<GameObject>();
	[HideInInspector]
	public List<ShapeInfo> shapeInfoList = new List<ShapeInfo>();
	[HideInInspector]
	public List<GameObject> shapeObjList = new List<GameObject>();
	public List<GameObject> NodeObjList = new List<GameObject>();
	public List<DestinationTarget> destinationList = new List<DestinationTarget>();
	InputManager inputmanager;
	private GameObject lastShape;
	string currentDesName;
	public bool canAddDestination;
	bool shouldSpawnWaypoint;
	bool loadAllShape = false;
	IEnumerator destinationCouroutine = null;
	CreateMapSample mapManager;

	private void Start()
	{
		inputmanager = GetComponent<InputManager>();
		mapManager = GetComponent<CreateMapSample>();
		canAddDestination = false;
		shouldSpawnWaypoint = false;
	}

	int NodeCount = 0;
	public bool shapesLoaded = false;
	public bool nodeLoaded = false;
	//-------------------------------------------------
	// All shape management functions (add shapes, save shapes to metadata etc.
	//-------------------------------------------------

	public void AddShape(Vector3 shapePosition, Quaternion shapeRotation, int shapeType, string Shapename)
	{

		ShapeInfo shapeInfo = new ShapeInfo();
		shapeInfo.px = shapePosition.x;
		shapeInfo.py = shapePosition.y;
		shapeInfo.pz = shapePosition.z;
		shapeInfo.qx = shapeRotation.x;
		shapeInfo.qy = shapeRotation.y;
		shapeInfo.qz = shapeRotation.z;
		shapeInfo.qw = shapeRotation.w;
		shapeInfo.shapeType = shapeType.GetHashCode();
		shapeInfo.name = Shapename;
		shapeInfo.infoIndex = inputmanager.index;
		shapeInfo.linkMapID = inputmanager.LinkID;
		shapeInfoList.Add(shapeInfo);
		Debug.Log(shapeInfo.name);
		GameObject shape = ShapeFromInfo(shapeInfo, false); // instantiate shape from info
		NodeCount++;

		DestinationTarget temp = shape.GetComponent<DestinationTarget>();
		if (temp != null)
		{
			NodeCount--;
		}
		
		shapeObjList.Add(shape);
		inputmanager.ResetNameAndIndex();
	}

	void Update()
	{
		if (!mapManager.IsMapping) // check to see if user is mapping;
			return;

		if (Input.GetKey(KeyCode.P)||shouldSpawnWaypoint)
		{
			Transform player = navController.transform;
			Collider[] hitColliders = Physics.OverlapSphere(player.position, 1.1f);
			int i = 0;
			while (i < hitColliders.Length)
			{
				if (hitColliders[i].CompareTag("waypoint"))
				{
					return;
				}
				i++;
			}
			Vector3 pos = player.position;
			pos.y -= 1f;
			AddShape(pos, Quaternion.Euler(Vector3.zero), 0, null);
		}

		//test method to see star behaviour
		/*if(Input.GetKeyDown(KeyCode.K))
		{
			Transform player = navController.transform;
			Vector3 pos = player.position;
			pos.y -= 1f;
			AddShape(pos, Quaternion.Euler(Vector3.zero), 3, null);
		}*/


		if (Input.touchCount > 0)
		{
			var touch = Input.GetTouch(0);
			if (touch.phase == TouchPhase.Began)
			{
				if (EventSystem.current.currentSelectedGameObject == null)
				{

					Debug.Log("Not touching a UI button. Moving on.");

					// add new shape
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint
					{
						x = screenPosition.x,
						y = screenPosition.y
					};

					// prioritize reults types
					ARHitTestResultType[] resultTypes = {
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent,
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane,
                        ARHitTestResultType.ARHitTestResultTypeFeaturePoint
					};

					foreach (ARHitTestResultType resultType in resultTypes)
					{
						if (HitTestWithResultType(point, resultType))
						{
							Debug.Log("Found a hit test result");
							return;
						}
					}
				}
			}
		}
	}

	
	public void OnAddShapeUp()
	{
		shouldSpawnWaypoint = false;
	}

	public void OnAddShapeClick()
	{
		shouldSpawnWaypoint = true;
	}

	public void OnAddTriggerClicked()
	{
		inputmanager.DestinationNamePopUp.gameObject.SetActive(true);
		destinationCouroutine = WaitForDestinationName();
		StartCoroutine(destinationCouroutine);
	}
	public void CanCelAddDestination()
	{
		StopCoroutine(destinationCouroutine);
		destinationCouroutine = null;
	}

	IEnumerator WaitForDestinationName()
	{
		yield return new WaitUntil(() => canAddDestination == true);
		Vector3 pos = Camera.main.transform.position;
		AddShape(pos, Quaternion.identity, 1, inputmanager.DestinationName);
		canAddDestination = false;
	}

	//create shape from info method
	public GameObject ShapeFromInfo(ShapeInfo info,bool pathFinding)
	{
		GameObject shape;
		Vector3 position = new Vector3(info.px, info.py, info.pz);
		//if load map, change waypoint to arrow
		if (pathFinding && info.shapeType == 0)
		{
			shape = Instantiate(ShapePrefabs[2]);
		}
		else
		{
			shape = Instantiate(ShapePrefabs[info.shapeType]);
		}

		if (shape.GetComponent<Node>() != null)
		{
			shape.GetComponent<Node>().pos = position; // set the node vector3 to the metadata position so it could find neighbour
			Debug.Log(position);
		}
		shape.tag = "waypoint";
		shape.transform.position = position; // transform the created shape to the metadata position
		shape.transform.rotation = new Quaternion(info.qx, info.qy, info.qz, info.qw);
		shape.transform.localScale = new Vector3(.3f, .3f, .3f);
		var _temptShape = shape.GetComponent<DestinationTarget>();
		if(_temptShape != null)
		{
			_temptShape.DestinationName = info.name;
			_temptShape.DestinationIndex = info.infoIndex;
			_temptShape.linkMapID = info.linkMapID;
			_temptShape.name = info.name;
			destinationList.Add(_temptShape);
		}
		
		if (mapManager.IsMapping) // if mapping see every thing
		{
			if(_temptShape!= null)
			{
				_temptShape.Activate(true);
			}
		}
		else if(mapManager.IsInitialized) //if running map everything to false
		{
			if (_temptShape != null)
			{
				_temptShape.Activate(false);
			}

			if(shape.GetComponent<Waypoint>() != null)
			{
				shape.GetComponent<Waypoint>().Activate(false);
			}
		}
		return shape;
	}

	public void ClearShapes()
	{
		Debug.Log("CLEARING SHAPES!!!!!!!");
		foreach (var obj in shapeObjList)
		{
			Destroy(obj);
		}
		foreach (var obj in NodeObjList)
		{
			Destroy(obj);
		}
		destinationList.Clear();
		NodeObjList.Clear();
		shapeObjList.Clear();
		shapeInfoList.Clear();
		shapesLoaded = false;
		nodeLoaded = false;
		loadAllShape = false;
	}


	public JObject Shapes2JSON()
	{
		ShapeList shapeList = new ShapeList();
		shapeList.shapes = new ShapeInfo[shapeInfoList.Count];
		for (int i = 0; i < shapeInfoList.Count; i++)
		{
			shapeList.shapes[i] = shapeInfoList[i];
		}

		return JObject.FromObject(shapeList);
	}

	public void LoadShapesJSON(JToken mapMetadata)
	{
		if (!shapesLoaded)
		{
			Debug.Log("LOADING SHAPES>>>");
			if (mapMetadata is JObject && mapMetadata["shapeList"] is JObject)
			{
				ShapeList shapeList = mapMetadata["shapeList"].ToObject<ShapeList>();
				if (shapeList.shapes == null)
				{
					Debug.Log("no shapes dropped");
					return;
				}
				// to do add this foreach loop to co routine and only add shape when already choosing destination
				foreach (var shapeInfo in shapeList.shapes)
				{
					shapeInfoList.Add(shapeInfo);
					GameObject shape = ShapeFromInfo(shapeInfo,false);
					shapeObjList.Add(shape);
				}
				shapesLoaded = true;
			}
		}
	}
	

	public void CreateNode()
	{
		if (NodeObjList.Count != 0)
		{
			Debug.Log("already create nodes");
			//set active node
			foreach(var node in NodeObjList)
			{
				node.transform.GetChild(0).gameObject.SetActive(false);
			}
			//set active destination
			foreach (var obj in shapeObjList)
			{
				if (obj.GetComponent<DestinationTarget>() != null)
				{
					obj.GetComponent<DestinationTarget>().Activate(false);
				}
			}
			nodeLoaded = true;
			return;
		}

		foreach(var obj in shapeObjList)
		{
			/*if(obj.GetComponent<DestinationTarget>() != null ) //if is destination deactivate it
			{
				obj.GetComponent<DestinationTarget>().Activate(false);
			}*/
			if(obj.GetComponent<Star>()!=null || obj.GetComponent<DestinationTarget>() != null) // if star, skip
			{
				continue;
			}
			else 
			{
				obj.SetActive(false); //set all other shape except destination to invisibility
			}
		}

		foreach(var shapeinfo in shapeInfoList)
		{
			if(shapeinfo.shapeType == 0) //doesn't have name = normal waypoint. skip if have name
			{
				GameObject shape = ShapeFromInfo(shapeinfo, true); //create the node arrow at the position of waypoint
				NodeObjList.Add(shape); // store in list so it is easy to clear later
			}
		}
		nodeLoaded = true;
	}

	public void ActivateAllDestination()
	{
		mapManager.selectDesPopUp.SetActive(false);
		if (!loadAllShape)
		{
			loadAllShape = true;
			foreach (var des in destinationList)
			{
				if (des.transform == mapManager.destination.transform)
					continue;
				des.Activate(true);
			}
		}
		else if (loadAllShape)
		{
			loadAllShape = false;
			foreach (var des in destinationList)
			{
				if (des.transform == mapManager.destination.transform)
					continue;
				des.Activate(false);
			}
			
		}
	}

	bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
	{
		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);

		if (hitResults.Count > 0)
		{
			foreach (var hitResult in hitResults)
			{

				Debug.Log("Got hit!");

				Vector3 position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
				Quaternion rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);

				//Transform to placenote frame of reference (planes are detected in ARKit frame of reference)
				Matrix4x4 worldTransform = Matrix4x4.TRS(position, rotation, Vector3.one);
				Matrix4x4? placenoteTransform = LibPlacenote.Instance.ProcessPose(worldTransform);

				Vector3 hitPosition = PNUtility.MatrixOps.GetPosition(placenoteTransform.Value);
				Quaternion hitRotation = PNUtility.MatrixOps.GetRotation(placenoteTransform.Value);
				

				// add star
				AddShape(hitPosition, hitRotation,3, "Star");


				return true;
			}
		}
		return false;
	}
}
