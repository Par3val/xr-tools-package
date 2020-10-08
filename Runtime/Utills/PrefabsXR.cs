using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
[ExecuteInEditMode]
public class PrefabsXR
{
	static bool inDev = false;
	//may need to auto switch if detects is dev shouldnt be on
	static string genericPath { get { return inDev ? ("Assets/") : ("Packages/com.shadowsoptional.xr-tools/"); } }
	public static GameObject GetActionByEnum(InteractibleObject.SecondaryTypes actionType)
	{
		string path = "";

		switch (actionType)
		{
			case InteractibleObject.SecondaryTypes.None:
				path = "Prefabs/Actions/None.prefab";
				break;
			case InteractibleObject.SecondaryTypes.Swap:
				path = "Prefabs/Actions/SwapAction.prefab";
				break;
			case InteractibleObject.SecondaryTypes.FollowDirection:
				path = "Prefabs/Actions/ControlDirectionAction.prefab";
				break;
			case InteractibleObject.SecondaryTypes.Scale:
				path = "Prefabs/Actions/ScaleAction.prefab";
				break;
		}

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}

	[MenuItem("Component/XR-Tools/Toggle Devmode")]
	private static void ToggleMode()
	{
		inDev = !inDev;
	}
	[MenuItem("Component/XR-Tools/Check Devmode")]
	private static void TestMode()
	{
		Debug.Log("InDev: " + inDev);
	}

	public static void Test()
	{
		var tootip = (GameObject)AssetDatabase.LoadAssetAtPath("Packages/io.extendreality.vrtk.prefabs/Helpers/Tooltip/Tooltip.prefab", typeof(GameObject));
		Debug.Log(tootip);
	}

	public static GameObject GetDrive(DriveObject.DriveType driveType, DriveObject.InteractType interactType)
	{
		string path = "";

		if (interactType == DriveObject.InteractType.Transfrom)
		{
			if (driveType == DriveObject.DriveType.Directional)
				path = "Prefabs/Drive/DirectionalTransformDrive.prefab";

			else if (driveType == DriveObject.DriveType.Rotational)
				path = "Prefabs / Drive / RotationalTransformDrive.prefab";
		}

		else if (interactType == DriveObject.InteractType.Joint)
		{
			if (driveType == DriveObject.DriveType.Directional)
				path = "Prefabs/Drive/DirectionalJointDrive.prefab";

			else if (driveType == DriveObject.DriveType.Rotational)
				path = "Prefabs/Drive/RotationallJointDrive.prefab";
		}

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}

	public static GameObject GetPlayerComponent(PlayerRig.PlayerComponents component)
	{
		string path = "";

		switch (component)
		{
			case PlayerRig.PlayerComponents.Walk:
				path = "Prefabs/PlayerComponents/Walk.prefab";
				break;
			case PlayerRig.PlayerComponents.Teleport:
				//path = "Prefabs/PlayerComponents/.prefab";
				break;
			case PlayerRig.PlayerComponents.Rotate:
				path = "Prefabs/PlayerComponents/Rotate.prefab";
				break;
		}

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}
}