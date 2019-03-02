using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node : MonoBehaviour {

    public Vector3 pos;
	public Renderer meshRenderer;

    [Header("A*")]
    public List<Node> neighbors = new List<Node>();
    public float FCost { get { return GCost + HCost; } } //the cost to move
    public float HCost { get; set; } //the cost to move from current node to destination
    public float GCost { get; set; } //the cost to move from current node to next node;
    //public float Cost { get; set; }
    public Node Parent { get; set; }

    //next node in navigation list
    public Node NextInList { get; set; }

    private Vector3 scale;
    private bool isDestination = false;

    private void Awake() {
        transform.GetChild(0).gameObject.SetActive(false);
        //save scale
        scale = transform.localScale;
        //check if destination
        if (GetComponent<DestinationTarget>() != null) isDestination = true;
#if UNITY_EDITOR
        pos = transform.position;
#endif
    }
	//call after next in list is set
    public void Activate(bool active) {
        transform.GetChild(0).gameObject.SetActive(active);
        if (NextInList != null) {
            transform.LookAt(NextInList.transform);
        }
    }

    void Update() {
        //make pulsate
        if (!isDestination)
            transform.localScale = scale * (1 + Mathf.Sin(Mathf.PI * Time.time) * .2f); 
    }

    public void FindNeighbors(float maxDistance) {
        foreach (Node node in FindObjectsOfType<Node>()) {
            if (Vector3.Distance(node.pos, this.pos) < maxDistance) {
                neighbors.Add(node);
            }
        }
    }
}
