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

	Transform child;
	float timeToScale = .5f;
	Vector3 targetScale;

    private void Awake() {
		transform.GetChild(0).gameObject.SetActive(false);
        //save scale
        
#if UNITY_EDITOR
        pos = transform.position;
#endif
    }

	private void Start()
	{
		child = transform.GetChild(0);
		targetScale = child.localScale;
	}

	//call after next in list is set
	public void Activate(bool active) {
		if (child.gameObject.activeInHierarchy)
			return;
		child.localScale = Vector3.zero;
        child.gameObject.SetActive(active);
		StartCoroutine(PopUp());

        if (NextInList != null) {
			Vector3 difference = NextInList.transform.position - transform.position;
			float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + rotationY, transform.rotation.z);
			
		}
    }

	IEnumerator PopUp()
	{
		float growInSecond = targetScale.x / timeToScale;
		while (child.localScale.x < targetScale.x)
		{
			float growFactor = growInSecond * Time.deltaTime;
			Vector3 newScale = new Vector3(child.localScale.x + growFactor, child.localScale.y + growFactor, child.localScale.z + growFactor);
			child.localScale = newScale;
			yield return null;
		}
	}
   

    public void FindNeighbors(float maxDistance) {
        foreach (Node node in FindObjectsOfType<Node>()) {
            if (Vector3.Distance(node.pos, this.pos) < maxDistance) {
                neighbors.Add(node);
            }
        }
    }


}
