using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.iOS;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class AddShapeWaypoint : MonoBehaviour
{
	public NavController navController;
	public List<GameObject> ShapePrefabs = new List<GameObject>();
	[HideInInspector]
	public List<ShapeInfo> shapeInfoList = new List<ShapeInfo>();
	[HideInInspector]
	public List<GameObject> shapeObjList = new List<GameObject>();
	public List<GameObject> NodeObjList = new List<GameObject>();
	InputManager inputmanager;
	private GameObject lastShape;
	string currentDesName;
	public bool canAddDestination;
	bool shouldSpawnWaypoint;
	IEnumerator destinationCouroutine = null;

	private void Start()
	{
		inputmanager = GetComponent<InputManager>();
		canAddDestination = false;
		shouldSpawnWaypoint = false;
	}

	int NodeCount = 0;
	public bool shapesLoaded = false;
	public bool nodeLoaded = false;
	//-------------------------------------------------
	// All shape management functions (add shapes, save shapes to metadata etc.
	//-------------------------------------------------

	public void AddShape(Vector3 shapePosition, Quaternion shapeRotation, bool isDestination, string name)
	{
		int typeIndex = 0;//sphere
		if (isDestination)
			typeIndex = 1;//diamond
		Debug.Log(typeIndex);

		ShapeInfo shapeInfo = new ShapeInfo();
		shapeInfo.px = shapePosition.x;
		shapeInfo.py = shapePosition.y;
		shapeInfo.pz = shapePosition.z;
		shapeInfo.qx = shapeRotation.x;
		shapeInfo.qy = shapeRotation.y;
		shapeInfo.qz = shapeRotation.z;
		shapeInfo.qw = shapeRotation.w;
		shapeInfo.shapeType = typeIndex.GetHashCode();
		shapeInfo.name = name;
		shapeInfo.infoIndex = inputmanager.index;
		shapeInfoList.Add(shapeInfo);
		Debug.Log(shapeInfo.name);
		GameObject shape = ShapeFromInfo(shapeInfo, false); // instantiate shape from info
		NodeCount++;
		if (shape.GetComponent<DestinationTarget>() != null)
		{
			NodeCount--;
			shape.GetComponent<DestinationTarget>().Activate(true);
			shape.GetComponent<DestinationTarget>().DestinationName = name;
		}
		
		shapeObjList.Add(shape);
		inputmanager.ResetNameAndIndex();
	}

	void Update()
	{
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
			AddShape(pos, Quaternion.Euler(Vector3.zero), false, null);
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
		AddShape(pos, Quaternion.identity, true, inputmanager.DestinationName);
		canAddDestination = false;
	}

	//create shape from info method
	public GameObject ShapeFromInfo(ShapeInfo info,bool pathFinding)
	{
		GameObject shape;
		Vector3 position = new Vector3(info.px, info.py, info.pz);
		//if loading map, change waypoint to arrow
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

		if(CreateMapSample.mapStatus == CreateMapSample.Status.Mapping)
		{
			if (shape.GetComponent<DestinationTarget>() != null)
			{
				var temptShape = shape.GetComponent<DestinationTarget>();
				temptShape.DestinationName = info.name;
				temptShape.DestinationIndex = info.infoIndex;
				shape.name = info.name;
				temptShape.Activate(true);
			}
		}
		else if(CreateMapSample.mapStatus == CreateMapSample.Status.Running)
		{
			if (shape.GetComponent<DestinationTarget>() != null)
			{
				var temptShape = shape.GetComponent<DestinationTarget>();
				temptShape.DestinationName = info.name;
				temptShape.DestinationIndex = info.infoIndex;
				shape.name = info.name;
				shape.GetComponent<DestinationTarget>().Activate(false);
			}
			else if (shape.GetComponent<Waypoint>() != null)
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
		NodeObjList.Clear();
		shapeObjList.Clear();
		shapeInfoList.Clear();
		shapesLoaded = false;
		nodeLoaded = false;
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
		if (NodeObjList.Count !=0)
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
			if(obj.GetComponent<DestinationTarget>() != null)
			{
				obj.GetComponent<DestinationTarget>().Activate(false);
			}
			else
			{
				obj.SetActive(false); //set all other shape except destination to invisibility
			}
		}

		foreach(var shapeinfo in shapeInfoList)
		{
			if(shapeinfo.name == null) //doesn't have name = normal waypoint. skip if have name
			{
				GameObject shape = ShapeFromInfo(shapeinfo, true); //create the node arrow at the position of waypoint
				NodeObjList.Add(shape); // store in list so it is easy to clear later
			}
		}
		nodeLoaded = true;
	}
}
