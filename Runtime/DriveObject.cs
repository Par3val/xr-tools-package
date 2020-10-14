using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Interactions.Controllables;
using VRTK.Prefabs.Interactions.Interactables;
using Zinnia.Action;
using Zinnia.Data.Type;
using Zinnia.Data.Type.Transformation.Conversion;
using UnityEditor;

[ExecuteInEditMode, System.Serializable]
public class DriveObject : MonoBehaviour
{
	public enum DriveType { Directional, Rotational }
	public enum InteractType { Transfrom, Joint }
	public DirectionalDriveFacade dirDriveFacade;
	public RotationalDriveFacade rotDriveFacade;

	public InteractibleObject interactibleObject;
	public Transform valueEventsParent;

	public DriveType driveType;
	public InteractType interactType;
	public List<ValueEvent> valueEvents;

	public float currentValue = 0.5f;

	public bool hasDrive { get { return !(dirDriveFacade == null && rotDriveFacade == null); } }

	#region Bools

	public bool drivetypeBool = true;
	public bool driveBool = true;
	public bool eventsBool = true;
	public bool interactibleBool = false;
	public bool previewBool = false;
	public List<bool> ValueEventsBools;

	#endregion


	[System.Serializable]
	public class ValueEvent
	{
		public static System.Type[] components = { typeof(MyFloatToBoolean), typeof(BooleanAction) };

		public GameObject refenceObject;
		public FloatRange activationRange;
		public MyFloatToBoolean floatToBoolean;
		public BooleanAction booleanAction;

		public static ValueEvent MakeNewValueEvent(Transform parent)
		{
			var tempEvent = new ValueEvent();

			tempEvent.refenceObject = new GameObject("ValueEvent", ValueEvent.components);
			tempEvent.refenceObject.transform.parent = parent;

			tempEvent.floatToBoolean = tempEvent.refenceObject.GetComponent<MyFloatToBoolean>();
			tempEvent.booleanAction = tempEvent.refenceObject.GetComponent<BooleanAction>();

			tempEvent.floatToBoolean.Transformed.AddListener(tempEvent.booleanAction.Receive);
			tempEvent.floatToBoolean.SetActivationRange(new FloatRange(.9f, 1f));

			return tempEvent;
		}

		public static ValueEvent MakeNewValueEvent(Transform parent, GameObject refrenceGameObject)
		{
			var tempEvent = new ValueEvent();

			tempEvent.refenceObject = refrenceGameObject;
			tempEvent.refenceObject.transform.parent = parent;

			tempEvent.floatToBoolean = tempEvent.refenceObject.GetComponent<MyFloatToBoolean>();
			tempEvent.booleanAction = tempEvent.refenceObject.GetComponent<BooleanAction>();

			tempEvent.floatToBoolean.Transformed.AddListener(tempEvent.booleanAction.Receive);
			tempEvent.floatToBoolean.SetActivationRange(tempEvent.floatToBoolean.GetActivationRange());

			return tempEvent;
		}

		public SerializedObject GetSerializedFloatToBoolean()
		{
			return new UnityEditor.SerializedObject(floatToBoolean);
		}

		public SerializedObject GetSerializedBooleanAction()
		{
			return new UnityEditor.SerializedObject(booleanAction);
		}

	}

	private void OnEnable()
	{
		if (transform.GetChild(0).GetComponent<DirectionalDriveFacade>())
			dirDriveFacade = transform.GetChild(0).gameObject.GetComponent<DirectionalDriveFacade>();
		else if (transform.GetChild(0).GetComponent<RotationalDriveFacade>())
			rotDriveFacade = transform.GetChild(0).gameObject.GetComponent<RotationalDriveFacade>();
		else
		{
			//Debug.LogError("No Drive a 0 child");
			return;
		}

		interactibleObject = GetComponentInChildren<InteractibleObject>();
		valueEventsParent = transform.GetChild(transform.childCount - 1);
		CheckValueEvents();

		if (valueEvents == null)
			valueEvents = new List<ValueEvent>();
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			if (valueEvents != null)
				foreach (var valueEvent in valueEvents)
				{
					valueEvent.floatToBoolean.Transformed.AddListener(valueEvent.booleanAction.Receive);

					if (driveType == DriveType.Directional)
						dirDriveFacade.NormalizedValueChanged.AddListener(valueEvent.floatToBoolean.DoTransform);
					else
						rotDriveFacade.NormalizedValueChanged.AddListener(valueEvent.floatToBoolean.DoTransform);
				}

			if (hasDrive)
			{
				if (driveType == DriveType.Directional)
					SetPreviewDrivePosition(.5f);
				if (driveType == DriveType.Rotational)
					SetPreviewDriveRotation(.5f);
			}
		}
	}

	public void SetupDrivePrefab()
	{
		if (dirDriveFacade)
			DestroyImmediate(dirDriveFacade.gameObject);
		if (rotDriveFacade)
			DestroyImmediate(rotDriveFacade.gameObject);

		GameObject prefab = PrefabsXR.GetDrive(driveType, interactType);
		GameObject newDrive = Instantiate(prefab, transform);
		newDrive.transform.SetAsFirstSibling();
		newDrive.name = prefab.name;

		if (driveType == DriveType.Directional)
			dirDriveFacade = newDrive.GetComponent<DirectionalDriveFacade>();
		else
			rotDriveFacade = newDrive.GetComponent<RotationalDriveFacade>();



		OnEnable();
	}

	public void SetPreviewDrivePosition(float targetValue)
	{
		float lerpValue = Mathf.Lerp(-dirDriveFacade.DriveLimit / 2, dirDriveFacade.DriveLimit / 2, targetValue);

		switch (dirDriveFacade.DriveAxis)
		{
			case DriveAxis.Axis.XAxis:
				interactibleObject.transform.localPosition = new Vector3(lerpValue, 0, 0);
				break;
			case DriveAxis.Axis.YAxis:
				interactibleObject.transform.localPosition = new Vector3(0, lerpValue, 0);
				break;
			case DriveAxis.Axis.ZAxis:
				interactibleObject.transform.localPosition = new Vector3(0, 0, lerpValue);
				break;
		}
	}

	public void SetPreviewDriveRotation(float targetValue)
	{
		float lerpValue = Mathf.Lerp(rotDriveFacade.DriveLimit.minimum, rotDriveFacade.DriveLimit.maximum, targetValue);

		switch (rotDriveFacade.DriveAxis)
		{
			case DriveAxis.Axis.XAxis:
				interactibleObject.transform.localEulerAngles = new Vector3(lerpValue, 0, 0);
				break;
			case DriveAxis.Axis.YAxis:
				interactibleObject.transform.localEulerAngles = new Vector3(0, lerpValue, 0);
				break;
			case DriveAxis.Axis.ZAxis:
				interactibleObject.transform.localEulerAngles = new Vector3(0, 0, lerpValue);
				break;
		}
	}

	internal void RemoveValueEvent(ValueEvent toRemove)
	{
		valueEvents.Remove(toRemove);

		if (driveType == DriveType.Directional)
			dirDriveFacade.NormalizedValueChanged.RemoveListener(toRemove.floatToBoolean.DoTransform);
		else
			rotDriveFacade.NormalizedValueChanged.RemoveListener(toRemove.floatToBoolean.DoTransform);

		DestroyImmediate(toRemove.refenceObject);
	}

	public void CreateValueEvent()
	{
		valueEvents.Add(ValueEvent.MakeNewValueEvent(valueEventsParent));
	}

	public void CreateValueEvent(GameObject refrenceObject)
	{
		valueEvents.Add(ValueEvent.MakeNewValueEvent(valueEventsParent, refrenceObject));
	}

	public void CheckValueEvents()
	{
		if (valueEvents != null)
			if (!valueEventsParent.childCount.Equals(valueEvents.Count))
			{
				valueEvents = new List<ValueEvent>();

				foreach (Transform child in valueEventsParent)
				{
					CreateValueEvent(child.gameObject);
				}
			}
	}

	public void SetTarget(float value)
	{
		currentValue = value;
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(DriveObject))]
public class DriveObjectInspector : Editor
{
	private static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
	private static GUILayoutOption miniFeildWidth = GUILayout.Width(50f);
	private static string driveWarning = " ";// WARNING WILL LOSE ALL CHNAGES IF CHANGED";

	DriveObject drive;

	float previewValue = 0.5f;

	private void OnEnable()
	{
		drive = (DriveObject)target;

		if (PrefabUtility.GetPrefabType(target) == PrefabType.PrefabInstance)
		{
			PrefabUtility.UnpackPrefabInstance(drive.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			return;
		}
	}

	public void OnSceneGUI()
	{
		drive = (DriveObject)target;
		
		if (drive.hasDrive && !Application.isPlaying)
			ShowPreview();
	}

	public override void OnInspectorGUI()
	{
		drive = (DriveObject)target;

		if (ShowDriveTypeSettings())
		{
			if (drive.dirDriveFacade || drive.rotDriveFacade)
				ShowDriveSettings();

			ShowValueEvents();

			if (drive.dirDriveFacade || drive.rotDriveFacade)
				ShowInteractibleGUI();
		}
	}

	private void ShowInteractibleGUI()
	{
		GUILayout.Space(10);

		MyEditorTools.BeginHorizontal();

		drive.interactibleBool = EditorGUILayout.BeginFoldoutHeaderGroup(drive.interactibleBool, "Interactible Object");
		EditorGUILayout.EndFoldoutHeaderGroup();

		MyEditorTools.ShowRefrenceButton(drive.interactibleObject.gameObject);

		MyEditorTools.EndHorizontal();

		if (drive.interactibleBool)
		{
			drive.interactibleObject.isDriveChild = true;
			drive.interactibleObject.drive = drive;

			var tempEditor = CreateEditor(drive.interactibleObject);

			((InteractibleObject)tempEditor.target).isThis = false;
			tempEditor.OnInspectorGUI();
			SceneView.RepaintAll();
			tempEditor.serializedObject.ApplyModifiedProperties();
		}

	}

	bool ShowDriveTypeSettings()
	{
		drive.drivetypeBool = EditorGUILayout.BeginFoldoutHeaderGroup(drive.drivetypeBool, "Drive Type" + (drive.drivetypeBool ? driveWarning : ""));
		EditorGUILayout.EndFoldoutHeaderGroup();

		if (!drive.dirDriveFacade && !drive.rotDriveFacade)
		{
			GUILayout.Label("No Drive");
			if (GUILayout.Button("Create Drive"))
				drive.SetupDrivePrefab();

			return false;
		}

		if (drive.drivetypeBool)
		{
			if (drive.dirDriveFacade)
				EditorGUILayout.LabelField(drive.dirDriveFacade.name, EditorStyles.boldLabel);
			if (drive.rotDriveFacade)
				EditorGUILayout.LabelField(drive.rotDriveFacade.name, EditorStyles.boldLabel);

			var tempDriveType = (DriveObject.DriveType)EditorGUILayout.EnumPopup("Drive Type", drive.driveType);
			var tempInteractType = (DriveObject.InteractType)EditorGUILayout.EnumPopup("Interact Type", drive.interactType);

			if (tempDriveType != drive.driveType || tempInteractType != drive.interactType)
			{
				drive.driveType = tempDriveType;
				drive.interactType = tempInteractType;
				drive.SetupDrivePrefab();
			}
		}

		return true;
	}

	void ShowDriveSettings()
	{
		GUILayout.Space(10);

		MyEditorTools.BeginHorizontal();

		drive.driveBool = EditorGUILayout.BeginFoldoutHeaderGroup(drive.driveBool, "Drive Settings");
		EditorGUILayout.EndFoldoutHeaderGroup();

		MyEditorTools.ShowRefrenceButton(
			drive.driveType == DriveObject.DriveType.Directional ?
			drive.dirDriveFacade.gameObject :
			drive.rotDriveFacade.gameObject);

		MyEditorTools.EndHorizontal();

		if (drive.driveBool)
		{

			if (drive.driveType == DriveObject.DriveType.Directional)
			{
				DirectionalDriveFacade facade = drive.dirDriveFacade;

				facade.DriveAxis = (DriveAxis.Axis)EditorGUILayout.EnumPopup("Drive Axis", facade.DriveAxis);

				facade.MoveToTargetValue = EditorGUILayout.Toggle("Move To Target Value", facade.MoveToTargetValue);
				facade.TargetValue = EditorGUILayout.Slider("Target Value", facade.TargetValue, 0, 1);
				facade.DriveSpeed = EditorGUILayout.FloatField("Drive Speed", facade.DriveSpeed);
			}

			else
			{
				RotationalDriveFacade facade = drive.rotDriveFacade;

				facade.DriveAxis = (DriveAxis.Axis)EditorGUILayout.EnumPopup("Drive Axis", facade.DriveAxis);

				facade.MoveToTargetValue = EditorGUILayout.Toggle("Move To Target Value", facade.MoveToTargetValue);
				facade.TargetValue = EditorGUILayout.Slider("Target Value", facade.TargetValue, 0, 1);
				facade.DriveSpeed = EditorGUILayout.FloatField("Drive Speed", facade.DriveSpeed);
			}

			ShowDriveLimit();
		}

	}

	void ShowDriveLimit()
	{
		MyEditorTools.BeginHorizontal();

		if (drive.driveType == DriveObject.DriveType.Directional)
		{
			drive.dirDriveFacade.DriveLimit = EditorGUILayout.FloatField("Drive Limit",
																																drive.dirDriveFacade.DriveLimit);
		}
		else if (drive.driveType == DriveObject.DriveType.Rotational)
		{
			Vector2 newLimits = EditorGUILayout.Vector2Field("Drive Limits",
																											new Vector2(drive.rotDriveFacade.DriveLimit.minimum,
																											drive.rotDriveFacade.DriveLimit.maximum));
			drive.rotDriveFacade.DriveLimit = new FloatRange(newLimits.x, newLimits.y);
		}

		if (!Application.isPlaying)
			if (GUILayout.Button(drive.previewBool ? "Stop Preview" : "Preview Drive"))
			{
				drive.previewBool = !drive.previewBool;
			}

		MyEditorTools.EndHorizontal();

		if (!Application.isPlaying)
		{
			ShowPreview();
		}
	}

	void ShowPreview()
	{
		if (drive.previewBool)
		{
			MyEditorTools.BeginHorizontal();
			previewValue = EditorGUILayout.Slider(previewValue, 0, 1);

			if (GUILayout.Button("CNTR", MyEditorTools.miniFeildWidth))
				previewValue = 0.5f;

			MyEditorTools.EndHorizontal();
		}

		if (drive.driveType == DriveObject.DriveType.Directional)
			drive.SetPreviewDrivePosition(drive.previewBool ? previewValue : .5f);
		else
			drive.SetPreviewDriveRotation(drive.previewBool ? previewValue : .5f);
	}

	#region Value Events

	void ShowValueEvents()
	{
		GUILayout.Space(10);

		MyEditorTools.BeginHorizontal();

		drive.eventsBool = EditorGUILayout.BeginFoldoutHeaderGroup(drive.eventsBool, "Value Event Actions");
		EditorGUILayout.EndFoldoutHeaderGroup();

		MyEditorTools.ShowRefrenceButton(drive.valueEventsParent.gameObject);

		MyEditorTools.EndHorizontal();

		GUILayout.Space(10);

		if (drive.eventsBool)
		{
			UpdateValueEventBools(ref drive.ValueEventsBools);


			for (int i = 0; i < drive.valueEvents.Count; i++)
			{
				DriveObject.ValueEvent tempValEvent = ShowValueEvent(drive.valueEvents[i], i);

				if (tempValEvent == null)
				{
					drive.RemoveValueEvent(drive.valueEvents[i]);
					return;
				}

				drive.valueEvents[i] = tempValEvent;
			}

			if (GUILayout.Button("New Value Event"))
			{
				drive.CreateValueEvent();
			}
		}

	}

	DriveObject.ValueEvent ShowValueEvent(DriveObject.ValueEvent valueEvent, int i)
	{

		MyEditorTools.BeginHorizontal();

		drive.ValueEventsBools[i] = EditorGUILayout.BeginFoldoutHeaderGroup(drive.ValueEventsBools[i], "Positive Bounds");
		EditorGUILayout.EndFoldoutHeaderGroup();

		ShowRangeArea(ref valueEvent);

		if (ShowRemoveButton(valueEvent))
			return null;

		MyEditorTools.EndHorizontal();

		if (drive.ValueEventsBools[i])
			ShowValueEvents(valueEvent.booleanAction);

		return valueEvent;
	}

	private void ShowValueEvents(BooleanAction booleanAction)
	{
		var serializedAction = new SerializedObject(booleanAction);

		var activated = serializedAction.FindProperty("Activated");
		var valueChanged = serializedAction.FindProperty("ValueChanged");
		var deactivated = serializedAction.FindProperty("Deactivated");

		EditorGUILayout.PropertyField(activated);
		EditorGUILayout.PropertyField(valueChanged);
		EditorGUILayout.PropertyField(deactivated);

		serializedAction.ApplyModifiedProperties();
	}

	bool ShowRemoveButton(DriveObject.ValueEvent valueEvent)
	{
		if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
		{
			return true;
		}

		return false;
	}

	void ShowRangeArea(ref DriveObject.ValueEvent valueEvent)
	{
		MyEditorTools.BeginHorizontal();

		FloatRange tempRange = valueEvent.floatToBoolean.GetActivationRange();

		tempRange.minimum = EditorGUILayout.FloatField(tempRange.minimum, EditorStyles.numberField, miniFeildWidth);

		EditorGUILayout.MinMaxSlider(ref tempRange.minimum, ref tempRange.maximum, 0, 1);

		tempRange.maximum = EditorGUILayout.FloatField(tempRange.maximum, EditorStyles.numberField, miniFeildWidth);

		if (tempRange.maximum > 1)
			tempRange.maximum = 1;

		valueEvent.floatToBoolean.SetActivationRange(tempRange);

		MyEditorTools.EndHorizontal();
	}

	void UpdateValueEventBools(ref List<bool> bools)
	{
		if (bools == null || bools.Count != drive.valueEvents.Count)
		{
			bools = new List<bool>();

			foreach (var item in drive.valueEvents)
			{
				bools.Add(false);
			}
		}
	}

	#endregion

	private void OnDisable()
	{
		//previewBool = false;
		//((DriveObject)target).ResetDrivePosition();
	}

	//public void OnSceneGUI()
	//{
	//  driveFacade = (DriveObject)target;
	//  int controlId = EditorGUIUtility.GetControlID("TransformLimit".GetHashCode(), FocusType.Keyboard);
	//  Vector3 currentAxisPos = new Vector3();
	//  switch (driveFacade.driveDir.DriveAxis)
	//  {
	//    case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.XAxis:
	//      //handle.axes = PrimitiveBoundsHandle.Axes.X;
	//      currentAxisPos = new Vector3(driveFacade.driveDir.DriveLimit, 0, 0);
	//      //handle.size = new Vector3(driveFacade.driveDir.DriveLimit, 0.25f, 0.25f);
	//      break;
	//    case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.YAxis:
	//      //handle.axes = PrimitiveBoundsHandle.Axes.Y;
	//      currentAxisPos = new Vector3(0, driveFacade.driveDir.DriveLimit, 0);
	//      //handle.size = new Vector3(0.25f, driveFacade.driveDir.DriveLimit, 0.25f);
	//      break;
	//    case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.ZAxis:
	//      //handle.axes = PrimitiveBoundsHandle.Axes.Z;
	//      currentAxisPos = new Vector3(0, 0, driveFacade.driveDir.DriveLimit);
	//      //handle.size = new Vector3(0.25f, 0.25f, driveFacade.driveDir.DriveLimit);
	//      break;
	//  }

	//  switch (Event.current.type)
	//  {
	//    case EventType.MouseDown:

	//      Handles.CubeHandleCap(controlId, currentAxisPos, Quaternion.identity, 1f, EventType.MouseEnterWindow);
	//      //check nearest control and set hotControl/keyboardControl
	//      break;
	//    case EventType.MouseUp:
	//      //check if i'm controlled and set hotControl/keyboardControl to 0
	//      break;
	//    case EventType.MouseDrag:
	//      //if i'm controlled, move the point
	//      break;
	//    case EventType.Repaint:
	//      //draw point visual
	//      break;
	//    case EventType.Layout:
	//      //register distance from mouse to my point
	//      break;
	//  }



	//  //  //UnityEditor.IMGUI.Controls.ArcHandle
	//  //  var handle = new BoxBoundsHandle();

	//  //  handle.center = driveFacade.transform.position;

	//  //  

	//  //  EditorGUI.BeginChangeCheck();
	//  //  //handle.midpointHandleDrawFunction(1, driveFacade.transform.position, Quaternion.identity, driveFacade.driveDir.DriveLimit, EventType.MouseDrag);
	//  //  handle.DrawHandle();
	//  //  if (EditorGUI.EndChangeCheck())
	//  //  {
	//  //    switch (driveFacade.driveDir.DriveAxis)
	//  //    {
	//  //      case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.XAxis:
	//  //        driveFacade.driveDir.DriveLimit = handle.size.x;
	//  //        break;
	//  //      case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.YAxis:
	//  //        driveFacade.driveDir.DriveLimit = handle.size.y;
	//  //        break;
	//  //      case VRTK.Prefabs.Interactions.Controllables.DriveAxis.Axis.ZAxis:
	//  //        driveFacade.driveDir.DriveLimit = handle.size.z;
	//  //        break;
	//  //      default:
	//  //        break;
	//  //    }
	//  //  }
	//}
}

#endif