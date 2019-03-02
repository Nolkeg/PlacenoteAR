using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NavController : MonoBehaviour {
	private List<Node> allNodes = new List<Node>();
	public CreateMapSample mapManager;
    public AStar AStar;
    private Transform destination;
    public bool _initialized = false;
    public bool _initializedComplete = false;
    private List<Node> path = new List<Node>();
    private int currNodeIndex = 0;
    private float maxDistance = 1.1f;
	[SerializeField] NodeFinder nodeFinder;
	
	public void ReSetParameter()
	{
		_initialized = false;
		_initializedComplete = false;
		currNodeIndex = 0;
		nodeFinder.ResetParameters();
		allNodes.Clear();
		StopAllCoroutines();
	}

    private void Start() {
#if UNITY_EDITOR //test initialize navigation in unity editor
		//otherwise, only start navigation when load shape from json is done
		//InitializeNavigation();
#endif
    }

    /// <summary>
    /// Returns the closest node to the given position.
    /// </summary>
    /// <returns>The closest node.</returns>
    /// <param name="point">Point.</param>
    Node ReturnClosestNode(List<Node> nodes, Vector3 point) {
        float minDist = Mathf.Infinity;
        Node closestNode = null;
        foreach (Node node in nodes) {
            float dist = Vector3.Distance(node.pos, point);
            if (dist < minDist) {
                closestNode = node;
                minDist = dist;
            }
        }
        return closestNode;
    }

	//a method to call when clicked find path
    public void InitializeNavigation() {
		StopCoroutine(initializeNav());
		StartCoroutine(initializeNav());
	}
	IEnumerator initializeNav()
	{
		yield return new WaitForSeconds(0);
		InitNav();
	}

	//the method to find the path to destination 
	//todo customize it so that I can have many destination in a map and only call this method when clicked the UI button
    void InitNav(){
        if (!_initialized) {
            _initialized = true;
            Debug.Log("INTIALIZING NAVIGATION!!!");
            Node[] tempNodes = FindObjectsOfType<Node>();

			foreach(var node in tempNodes)
			{
				if(node.GetComponent<DestinationTarget>() != null)
				{
					//skip destination
					continue;
				}
				if (allNodes.Contains(node))
					continue;
				allNodes.Add(node);
			}
			

            Debug.Log("NODES: " + allNodes.Count);
            Node closestNode = ReturnClosestNode(allNodes, transform.position);
            Debug.Log("closest: " + closestNode.gameObject.name);

			if (mapManager.destination == null)
			{
				Debug.Log("Destination is not set yet");
				return;
			}
				
			Node target = mapManager.destination;

            Debug.Log("target: " + target.gameObject.name);
            //set neighbor nodes for all nodes
            foreach (Node node in allNodes) {
                node.FindNeighbors(maxDistance);
            }

            //get path from A* algorithm
            path = AStar.FindPath(closestNode, target, allNodes);

            if (path == null) {
                //increase search distance for neighbors
                maxDistance += .1f;
                Debug.Log("Increasing search distance: " + maxDistance);
                _initialized = false;
                InitNav();
                return;
            }

            //set next nodes 
            for (int i = 0; i < path.Count - 1; i++) {
                path[i].NextInList = path[i + 1]; // set the next in list node
            }
            //activate first node and rotate it to the next node
            path[0].Activate(true);
			nodeFinder.currentNodeRen = path[0].meshRenderer;
			_initializedComplete = true; // a flag to check if all node is load before we can move

			foreach(Node node in path)
			{
				if(Vector3.Distance(node.transform.position,this.transform.position) < 3f) //activate all node in this radius
				{
					node.Activate(true);
					nodeFinder.currentNodeRen = node.meshRenderer;
				}
			}

			StartCoroutine(DelayAssign());
		}
    }

    private void OnTriggerEnter(Collider other) {

        if (_initializedComplete && other.CompareTag("waypoint") && path.Contains(other.GetComponent<Node>()))
		{
			print("found node");
            int tempIndex = path.IndexOf(other.GetComponent<Node>()); //get the index of the node we hit

			if(tempIndex > currNodeIndex) //check whether the object we hit is next in path
			{
				currNodeIndex = tempIndex; //if so, set the current index to the next in path
			}
			else if(tempIndex == currNodeIndex)
			{
				currNodeIndex = tempIndex;
				path[currNodeIndex].Activate(true);
			}
			else
			{
				return; //if it is the already activated path just return;
			}

            if (currNodeIndex < path.Count - 1) { //if the index we get is not the destination
                path[currNodeIndex + 1].Activate(true); //activate the next in list node;
			}
        }
    }
	IEnumerator DelayAssign()
	{
		while(true)
		{
			yield return new WaitForSecondsRealtime(0.5f);
			nodeFinder.currentNodeRen = path[currNodeIndex].meshRenderer;
		}
		
	}
	
	


}
