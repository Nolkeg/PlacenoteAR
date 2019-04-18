using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NodeFinder : MonoBehaviour
{
	[SerializeField] Camera cam;
	[SerializeField] Transform leftSide;
	[SerializeField] Transform rightSide;
	[SerializeField] GameObject turnLeftUI;
	[SerializeField] GameObject turnRightUI;
	public Renderer currentNodeRen;
	float distanceLeft, distanceRight;
    void Start()
    {
		StartCoroutine(LoopNodeFindingDelay());

    }

    

	void calculateNodeSide()
	{
		distanceLeft = Vector3.Distance(leftSide.position, currentNodeRen.transform.position);
		distanceRight = Vector3.Distance(rightSide.position, currentNodeRen.transform.position);

		if(distanceLeft<distanceRight)
		{
			turnLeftUI.transform.DOScale(1, 0.25f);
			turnRightUI.transform.DOScale(0, 0.25f);
		}
		else if(distanceRight < distanceLeft)
		{
			turnRightUI.transform.DOScale(1, 0.25f);
			turnLeftUI.transform.DOScale(0, 0.25f);
		}
	}

	public void ResetParameters()
	{
		StopCoroutine(LoopNodeFindingDelay());
		currentNodeRen = null;
		turnLeftUI.transform.DOScale(0, 0.25f);
		turnRightUI.transform.DOScale(0, 0.25f);
		distanceLeft = 0;
		distanceRight = 0;
	}

	IEnumerator LoopNodeFindingDelay()
	{
		while(true)
		{
			if (currentNodeRen == null)
			{
				yield return new WaitForSecondsRealtime(1f);
				continue;
			}

			if (!RendererExtensions.IsVisibleFrom(currentNodeRen, cam))
			{
				Debug.Log("current node is not on cam");
				calculateNodeSide();
			}
			else //if node is on screen turn off the UI
			{
				turnLeftUI.transform.DOScale(0, 0.25f);
				turnRightUI.transform.DOScale(0, 0.25f);
				distanceLeft = 0;
				distanceRight = 0;
			}
			yield return new WaitForSecondsRealtime(1f);
		}
		
	}
}
