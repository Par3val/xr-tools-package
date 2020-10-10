using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
[ExecuteInEditMode]
public class PrefabsXR
{
	public static bool inDev = false;
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

	public static GameObject GetPlayerComponent(PlayerComponent.PlayerComponents component)
	{
		string path = "";

		switch (component)
		{
			case PlayerComponent.PlayerComponents.Walk:
				path = "Prefabs/PlayerComponents/Walk.prefab";
				break;
			case PlayerComponent.PlayerComponents.Teleport:
				//path = "Prefabs/PlayerComponents/.prefab";
				break;
			case PlayerComponent.PlayerComponents.Rotate:
				path = "Prefabs/PlayerComponents/Rotate.prefab";
				break;
		}

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}
}

public class XRToolsEditor : EditorWindow
{
	bool inDev;
	[MenuItem("Component/XR-Tools/Open Window")]
	public static void OpenXRToolsEditor()
	{
		XRToolsEditor window = CreateInstance<XRToolsEditor>();
		window.minSize = new Vector2(115, 23);
		window.maxSize= new Vector2(115, 23);
		//window.Init();
		window.Show();
	}

	public void OnGUI()
	{
		MyEditorTools.BeginHorizontal();
		EditorGUILayout.LabelField($"In devmode:", GUILayout.Width(73));
		if (GUILayout.Button($"{PrefabsXR.inDev}", GUILayout.Width(115 - 73)))
			inDev = !inDev;

		PrefabsXR.inDev = inDev;
	}
}