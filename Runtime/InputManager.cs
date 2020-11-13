using System;
using UnityEngine;
using UnityEditor;
using Zinnia.Action;
using VRTK.Prefabs.Interactions.Interactors;

[RequireComponent(typeof(BooleanAction)), ExecuteInEditMode]
public class InputManager : MonoBehaviour
{

	[Serializable]
	public enum hands { Right, Left };
	public hands handedness;

	//public AxesToVector3Facade LocomotionAxisCalculator;

	public InputMapping[] inputMappings;
	public BooleanAction grabAction;
	public PlayerRig rig;

	public bool eventsOpen;

	public void Awake()
	{
		string parName = transform.parent.name.Substring(0, 1);

		if (parName == "L")
			handedness = hands.Left;
		else if (parName == "R")
			handedness = hands.Right;
		else
			Debug.Log("Parent Hand Unsure");
	}
	private void Start()
	{
		if (!grabAction)
			grabAction = GetComponent<BooleanAction>();
		if (GetComponent<InteractorFacade>().GrabAction == null)
			GetComponent<InteractorFacade>().GrabAction = grabAction;

		if (Application.isPlaying)
		{
			foreach (var InputMap in inputMappings)
			{
				InputMap.Setup(handedness, gameObject);

				if (InputMap.mapName.Contains("Grab"))
				{
					InputMap.Activated.AddListener(GrabActivated);
					InputMap.Deactivated.AddListener(GrabDeactivated);
				}
				//if (InputMap.name.Contains("Move"))
				//{
				//  InputMap.Moved2D.AddListener(UpdateMove);
				//}
				//if (InputMap.name.Contains("Rotate"))
				//{
				//  InputMap.Moved2D.AddListener(UpdateRot);
				//}
			}
		}
	}

	public void GrabActivated()
	{
		grabAction.Receive(true);
	}
	public void GrabDeactivated()
	{
		grabAction.Receive(false);
	}

	public void AddMap(InputMapping newMap)
	{
		InputMapping[] tempMappings = new InputMapping[inputMappings.Length + 1];
		for (int i = 0; i < inputMappings.Length; i++)
		{
			tempMappings[i] = inputMappings[i];
		}

		tempMappings[inputMappings.Length] = newMap;

		inputMappings = tempMappings;

	}


	//public void UpdateMove(Vector2 data)
	//{
	//  LocomotionAxisCalculator.LateralAxis.Receive(data.x);
	//  LocomotionAxisCalculator.LongitudinalAxis.Receive(data.y);
	//}
	//public void UpdateRot(Vector2 data)
	//{
	//  LocomotionAxisCalculator.LateralAxis.Receive(data.x);
	//}
}

#if UNITY_EDITOR

[CustomEditor(typeof(InputManager))]
public class InputManagerInspector : Editor
{
	InputManager manager;

	private static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
	//private static Text miniButtonWidth = GUILayout.Width(25f);

	public override void OnInspectorGUI()
	{
		manager = (InputManager)target;

		//Input Mappings

		GUILayout.Label($"Input Mappings: {manager.inputMappings.Length}");

		if (manager.grabAction == null)
			((InputManager)target).grabAction = ((InputManager)target).GetComponent<BooleanAction>();

		if (manager.inputMappings == null)
			manager.inputMappings = new InputMapping[0];

		bool hasGrab = false;

		if (manager.inputMappings.Length > 0)
		{
			SerializedObject serialManager = new SerializedObject(target);
			SerializedProperty inputMappingsProperty = serialManager.FindProperty("inputMappings");

			serialManager.Update();


			for (int i = 0; i < manager.inputMappings.Length; i++)
			{
				MyEditorTools.BeginHorizontal();

				manager.inputMappings[i].edtiorOpen = EditorGUILayout.BeginFoldoutHeaderGroup(manager.inputMappings[i].edtiorOpen, manager.inputMappings[i].mapName);
				//manager.inputMappings[i].mapName = EditorGUILayout.TextArea(manager.inputMappings[i].mapName, EditorStyles.label);
				EditorGUILayout.EndFoldoutHeaderGroup();

				if (ShowRemoveButton(manager.inputMappings[i]))
					return;

				MyEditorTools.EndHorizontal();

				if (manager.inputMappings.Length > 0)
				{
					if (manager.inputMappings[i].edtiorOpen)
					{
						ShowMap(manager.inputMappings[i], inputMappingsProperty.GetArrayElementAtIndex(i));
					}
				}

				if (manager.inputMappings[i].mapName.Contains("Grab"))
					hasGrab = true;
			}

			serialManager.ApplyModifiedProperties();
		}

		if (!hasGrab)
		{
			if (GUILayout.Button("Add Grab"))
				AddGrab();
		}

		if (GUILayout.Button("Add InputMap"))
		{
			manager.AddMap(new InputMapping("New Map"));
		}

		//Grab Events
		GUILayout.Space(10);

		manager.eventsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(manager.eventsOpen, "Successfull Grab Events");
		EditorGUILayout.EndFoldoutHeaderGroup();
		if (manager.eventsOpen)
		{
			Transform grabEvent = manager.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0);
			ShowValueEvent(grabEvent.GetComponent<BooleanAction>());

		}
		if (Selection.activeGameObject == manager.gameObject && manager.rig)
			if (GUILayout.Button($"GOTO: {manager.rig.name}"))
				MyEditorTools.FocusObject(manager.rig.gameObject);

		GUILayout.Space(10);
	}

	public void ShowMap(InputMapping map, SerializedProperty property)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Name: ", GUILayout.Width(37));
		map.mapName = EditorGUILayout.TextField(map.mapName, EditorStyles.label);
		EditorGUILayout.EndHorizontal();


		map.axisType = (InputMapping.AxisType)EditorGUILayout.EnumPopup("Input Type: ", map.axisType);

		switch (map.axisType)
		{
			case InputMapping.AxisType.Button:
				map.typeButton = (InputMapping.InputTypeButton)EditorGUILayout.EnumPopup("Which Button: ", map.typeButton);
				break;
			case InputMapping.AxisType.Axis1D:
				map.type1D = (InputMapping.InputTypeAxis1D)EditorGUILayout.EnumPopup("Which Axis1D: ", map.type1D);
				MyEditorTools.BeginHorizontal();

				GUILayout.Label("Activation Range");
				map.activationRange.minimum = EditorGUILayout.FloatField(map.activationRange.minimum, MyEditorTools.miniFeildWidth);

				EditorGUILayout.MinMaxSlider(ref map.activationRange.minimum, ref map.activationRange.maximum, 0, 1);

				map.activationRange.maximum = EditorGUILayout.FloatField(map.activationRange.maximum, MyEditorTools.miniFeildWidth);

				MyEditorTools.EndHorizontal();
				break;
			case InputMapping.AxisType.Axis2D:
				map.type2D = (InputMapping.InputTypeAxis2D)EditorGUILayout.EnumPopup("Which Axis2D: ", map.type2D);
				break;
		}

		switch (map.axisType)
		{
			case InputMapping.AxisType.Button:
				EditorGUILayout.PropertyField(property.FindPropertyRelative("UpdateBool"));

				EditorGUILayout.PropertyField(property.FindPropertyRelative("Activated"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Deactivated"));
				break;
			case InputMapping.AxisType.Axis1D:
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Update1D"));

				EditorGUILayout.PropertyField(property.FindPropertyRelative("Activated"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Deactivated"));
				break;
			case InputMapping.AxisType.Axis2D:
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Moved2D"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Moved2DX"));
				EditorGUILayout.PropertyField(property.FindPropertyRelative("Moved2DY"));
				break;
		}

	}

	bool ShowRemoveButton(InputMapping mapping)
	{
		if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
		{
			RemoveMap(mapping);
			return true;
		}

		return false;
	}


	public void RemoveMap(InputMapping relMap)
	{
		InputMapping[] tempMappings = new InputMapping[manager.inputMappings.Length - 1];

		int removeIndex = 0;

		for (int i = 0, o = 0; i < manager.inputMappings.Length; i++)
		{
			if (manager.inputMappings[i] == relMap)
			{
				o = 1;
				removeIndex = i;
			}
			else
			{
				tempMappings[i - o] = manager.inputMappings[i];
			}
		}

		manager.inputMappings = tempMappings;
	}

	private void ShowValueEvent(BooleanAction booleanAction)
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

	public void AddGrab()
	{
		InputMapping grabMapping = new InputMapping("Grab");
		grabMapping.axisType = InputMapping.AxisType.Axis1D;
		grabMapping.type1D = InputMapping.InputTypeAxis1D.Hand;
		manager.AddMap(grabMapping);
	}

}

#endif