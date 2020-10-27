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
				path = "Prefabs/Drive/RotationalTransformDrive.prefab";
		}

		else if (interactType == DriveObject.InteractType.Joint)
		{
			if (driveType == DriveObject.DriveType.Directional)
				path = "Prefabs/Drive/DirectionalJointDrive.prefab";

			else if (driveType == DriveObject.DriveType.Rotational)
				path = "Prefabs/Drive/RotationallJointDrive.prefab";
		}
		else
			NoPathError(interactType);

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}

	public static GameObject GetPlayerComponent(PlayerComponent.ComponentTypes component)
	{
		string path = "";

		switch (component)
		{
			case PlayerComponent.ComponentTypes.Walk:
				path = "Prefabs/PlayerComponents/Walk.prefab";
				break;
			case PlayerComponent.ComponentTypes.Teleport:
				NoPathError(component);
				//path = "Prefabs/PlayerComponents/.prefab";
				break;
			case PlayerComponent.ComponentTypes.Rotate:
				path = "Prefabs/PlayerComponents/Rotate.prefab";
				break;
			case PlayerComponent.ComponentTypes.Climb:
				path = "Prefabs/PlayerComponents/Climb 2.0.prefab";
				break;
			case PlayerComponent.ComponentTypes.PhysicalBody:
				path = "Prefabs/PlayerComponents/PlayerBody.prefab";
				break;
		}

		if (path == "")
			return null;

		return (GameObject)AssetDatabase.LoadAssetAtPath(genericPath + path, typeof(GameObject));
	}

	static void NoPathError(object issue)
	{
		Debug.LogError($"No Path for {issue}");
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
		window.maxSize = new Vector2(115, 23);
		//window.Init();
		window.Show();
	}

	public void OnGUI()
	{
		GUIStyle activeStyle = new GUIStyle(EditorStyles.label);
		activeStyle.normal.textColor = Color.green;

		MyEditorTools.BeginHorizontal();

		EditorGUILayout.LabelField($"In devmode:", inDev ? activeStyle : EditorStyles.label, GUILayout.Width(73));
		if (GUILayout.Button($"{PrefabsXR.inDev}", GUILayout.Width(115 - 73)))
			inDev = !inDev;

		PrefabsXR.inDev = inDev; MyEditorTools.EndHorizontal();
	}

	#region UpdatePackage
	//static UnityEditor.PackageManager.Requests.AddRequest request;

	[MenuItem("Component/XR-Tools/Update Package")]
	public static void UpdatePackage()
	{
		/*request = */Client.Add("https://github.com/Par3val/xr-tools-package.git");

		//	EditorApplication.update += Progress;
	}

	//static void Progress()
	//{
	//	if (request.IsCompleted)
	//	{
	//		if (request.Status == StatusCode.Success)
	//			Debug.Log("Installed: " + request.Result.packageId);
	//		else if (request.Status >= StatusCode.Failure)
	//			Debug.Log(request.Error.message);

	//		EditorApplication.update -= Progress;
	//	}
	//}
	#endregion
}