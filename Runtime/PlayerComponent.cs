using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
	[System.Serializable]
	public enum PlayerComponents { Walk, Teleport, Rotate,Other }

	public PlayerComponents type;

	public void Test()
	{
		//Debug.Log($"Red Leader {type} checking in");
	}
}
