using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
	public Transform mesh;

	public void Activate(bool active)
	{
		mesh.gameObject.SetActive(active);
	}
}
