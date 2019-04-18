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
    public List<Node> path = new List<Node>();
    private int currNodeIndex = 0;
    private float maxDistance = 1.5f;
	[SerializeField] NodeFinder nodeFinder;
	public void ReSetParameter()
	{
		foreach(Node node in allNodes)
		{
			node.neighbors.Clear();
		}
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
			currNodeIndex = 0;
			nodeFinder.currentNodeRen = path[currNodeIndex].meshRenderer;
			_initializedComplete = true; // a flag to check if all node is load before we can move

			StartCoroutine(DelayPopUp(path));
		}
    }

	IEnumerator DelayPopUp(List<Node> path)
	{
		foreach (Node node in path)
		{
			if (Vector3.Distance(node.transform.position, this.transform.position) < 6.5f) //activate all node in this radius
			{
				node.Activate(true);
				if(currNodeIndex < path.IndexOf(node))
					currNodeIndex = path.IndexOf(node);
				nodeFinder.currentNodeRen = node.meshRenderer;
				yield return new WaitForSecondsRealtime(0.5f);
			}
		}
	}

    private void OnTriggerEnter(Collider other) {

        if (_initializedComplete && other.CompareTag("waypoint") && path.Contains(other.GetComponent<Node>()))
		{
            int tempIndex = path.IndexOf(other.GetComponent<Node>()); //get the index of the node we hit

			if(tempIndex > currNodeIndex) //check whether the object we hit is next in path
			{
				currNodeIndex = tempIndex; //if so, set the current index to the next in path

				if(currNodeIndex == path.Count - 1) //if hit destination activate it
				{
					path[currNodeIndex].Activate(true);
					nodeFinder.currentNodeRen = path[currNodeIndex].meshRenderer;
				}
			}
			else if(tempIndex == currNodeIndex) // hit destination
			{
				currNodeIndex = tempIndex;
				path[currNodeIndex].Activate(true);
				nodeFinder.currentNodeRen = path[currNodeIndex].meshRenderer;
			}
			else
			{
				return; //if it is the already activated path just return;
			}

            if (currNodeIndex < path.Count - 1) { //if the index we get is not the destination
                path[currNodeIndex + 1].Activate(true); //activate the next in list node;
				nodeFinder.currentNodeRen = path[currNodeIndex + 1].meshRenderer;
			}
			
        }
    }
	


}
