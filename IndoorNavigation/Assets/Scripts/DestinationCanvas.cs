using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DestinationCanvas : MonoBehaviour
{
	DestinationTarget destination;
	TMP_Text nameText;
    void Start()
    {
		nameText = GetComponent<TextMeshProUGUI>();
		destination = GetComponentInParent<DestinationTarget>();
		nameText.text = destination.DestinationName;
    }

}
