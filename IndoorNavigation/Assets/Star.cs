using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Star : MonoBehaviour
{
	NavController cam;
	private void Start()
	{
		transform.localScale = Vector3.zero;
		cam = FindObjectOfType<NavController>();
	}

	public void Activate(bool active)
	{
		if(active)
		{
			transform.DOScale(1, 0.25f);
		}
		else
		{
			transform.DOScale(0, 0.25f);
		}
	}

	private void Update()
	{
		transform.LookAt(cam.transform);
	}


	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("StarCollider"))
		{
			Activate(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("StarCollider"))
		{
			Activate(false);
		}
	}
}
