using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WelcomeSignText : MonoBehaviour
{
	TMP_Text ui_Text;
	CreateMapSample mapManager;

    void Start()
    {
		ui_Text = GetComponent<TMP_Text>();
		mapManager = FindObjectOfType<CreateMapSample>();
		ui_Text.text = mapManager.ReturnMapName();
    }






    
}
