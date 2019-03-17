using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationInfo : MonoBehaviour
{
	[SerializeField] int infoIndex;
	public int InFoIndex
	{
		get
		{
			if(infoIndex == 0)
			{
				return 0;
			}

			return infoIndex;
		}
		set
		{
			infoIndex = value;
		}
	}
}
