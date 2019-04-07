using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class closeUI : MonoBehaviour
{
	DestinationInfo objectToClose;
	RectTransform parentTranform;
	float timeToDestroy;
	// Start is called before the first frame update

	

	private void Start()
	{
		objectToClose = GetComponentInParent<DestinationInfo>();
		parentTranform = objectToClose.GetComponent<RectTransform>();
	}

	public void Close()
	{
		parentTranform.DOSizeDelta(Vector2.zero, 0.25f);
		RectTransform thisTransform = GetComponent<RectTransform>();
		thisTransform.DOSizeDelta(Vector2.zero, 0.25f);
		parentTranform.gameObject.SetActive(false);
	}
}
