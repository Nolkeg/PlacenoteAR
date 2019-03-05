using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {
	

	
    public List<Node> FindPath(Node startNode, Node targetNode, List<Node> allNodes) {
        List<Node> openSet = new List<Node>();
        openSet.Add(startNode);

        List<Node> closedSet = new List<Node>();

        while (openSet.Count > 0) {

            Node currentNode = openSet[0]; //add start node to current node

            for (int i = 1; i < openSet.Count; i++) { //loop to check cost
                if (openSet[i].FCost < currentNode.FCost
                    || (openSet[i].FCost.Equals(currentNode.FCost)
                        && openSet[i].HCost < currentNode.HCost)) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                Debug.Log("RETURNING CORRECT NODE!!");
                return RetracePath(startNode, targetNode);
            }
			//recalculating the cost;
			//check every neighbors node of the current node
            foreach (Node connection in currentNode.neighbors) {
                if (!closedSet.Contains(connection)) {
                    float costToConnection = currentNode.GCost + GetEstimate(currentNode, connection);

                    if (costToConnection < connection.GCost || !openSet.Contains(connection)) {
                        connection.GCost = costToConnection;
                        connection.HCost = GetEstimate(connection, targetNode);
                        connection.Parent = currentNode;

                        if (!openSet.Contains(connection)) {
                            openSet.Add(connection);
                        }
                    }
                }
            }
        }
        Debug.Log("RETURNING NULL");
        return null;
    }

    private static List<Node> RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();

        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        path.Reverse();

        return path;
    }

	//huaristic function to estimast the distance between 2 node
    private float GetEstimate(Node first, Node second) {
        float distance;

        float xDistance = Mathf.Abs(first.pos.x - first.pos.x);
        float yDistance = Mathf.Abs(second.pos.z - second.pos.z);

        if (xDistance > yDistance) {
            distance = 14 * yDistance + 10 * (xDistance - yDistance);
        } else {
            distance = 14 * xDistance + 10 * (yDistance - xDistance);
        }

        return distance;
    }
}