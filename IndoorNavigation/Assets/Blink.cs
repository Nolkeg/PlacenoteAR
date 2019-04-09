using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Blink : MonoBehaviour
{
	Renderer image;
	[SerializeField] float timeDelay = 1;
	
	bool blinkUp;

	private void Start()
	{
		image = GetComponent<Renderer>();
		StartBlinking();
	}

	IEnumerator Blinking()
	{
		while (true)
		{
			switch (blinkUp)
			{
				case false:
					image.material.DOFade(1, timeDelay);
					yield return new WaitForSeconds(timeDelay);
					blinkUp = true;
					break;

				case true:
					image.material.DOFade(0.5f, timeDelay);
					yield return new WaitForSeconds(timeDelay);
					blinkUp = false;
					break;
			}
		}
	}

	void StartBlinking()
	{
		StopAllCoroutines();
		StartCoroutine(Blinking());
	}

	void StopBlinking()
	{
		StopAllCoroutines();
	}
}
