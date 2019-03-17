using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotating : MonoBehaviour
{
	[SerializeField] Vector3 rotationAngle;
	[SerializeField] float rotationSpeed;

	void Update()
    {
		transform.Rotate(rotationAngle * Time.deltaTime * rotationSpeed);
    }

}
