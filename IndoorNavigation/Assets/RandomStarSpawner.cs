using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RandomStarSpawner : MonoBehaviour
{
	[SerializeField] GameObject[] stars;
	[SerializeField] float spawnPerSecond = 1;

	private void Start()
	{
		for (int i = 0; i <= stars.Length - 1; i++)
		{
			stars[i].transform.localScale = Vector3.zero;
		}
	}

	public void StartSpawning()
	{
		for (int i = 0; i <= stars.Length - 1; i++)
		{
			stars[i].transform.localScale = Vector3.zero;
		}
		StartCoroutine(spawnStar());
	}

	public void StopSpawning()
	{
		StopAllCoroutines();
		for (int i = 0; i <= stars.Length - 1; i++)
		{
			stars[i].transform.localScale = Vector3.zero;
		}
	}

	IEnumerator spawnStar()
	{
		while (true)
		{
			int[] randomIndex = new int[stars.Length];
			for (int i = 0; i <= randomIndex.Length - 1; i++)
			{
				LibPlacenote.PNFeaturePointUnity[] points = LibPlacenote.Instance.GetTrackedFeatures();
				randomIndex[i] = Random.Range(0, points.Length);
				Vector3 pointPos = new Vector3(points[randomIndex[i]].point.x,
												points[randomIndex[i]].point.y,
												points[randomIndex[i]].point.z);
				stars[i].transform.position = pointPos;
				stars[i].transform.localScale = Vector3.zero;
				stars[i].transform.DOScale(1, 0.25f);
				yield return new WaitForSeconds(spawnPerSecond);
			}
		}
	}
}
