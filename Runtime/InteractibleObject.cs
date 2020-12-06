using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactables.Grab.Action;
using VRTK.Prefabs.Interactions.Interactables.Grab.Receiver;
using UnityEditor;
using static VRTK.Prefabs.Interactions.Interactables.Grab.Receiver.GrabInteractableReceiver;
using static VRTK.Prefabs.Interactions.Interactables.Grab.Action.GrabInteractableFollowAction;

[SelectionBase, ExecuteInEditMode, System.Serializable]
public class InteractibleObject : MonoBehaviour
{
	[System.Serializable]
	public enum SecondaryTypes { None, Swap, FollowDirection, Scale }

	public InteractableFacade facade;
	public GameObject meshContainer;
	public SecondaryTypes secondaryAction;


	//if drive child
	public bool isDriveChild = false;
	public bool isThis = false;
	public DriveObject drive;


	#region Foldout Bools

	public bool grabSettingsBool;
	public bool eventBool;

	#endregion

	public GrabInteractableReceiver.ActiveType HoldType
	{
		get
		{
			return facade.GrabConfiguration.GrabReceiver.GrabType;

		}
		set
		{
			facade.GrabConfiguration.GrabReceiver.GrabType = value;
		}
	}

	public void OnDrawGizmosSelected()
	{
		isThis = true;
	}

	public void OnEnable()
	{
		facade = transform.GetComponent<InteractableFacade>();
		meshContainer = transform.GetChild(0).gameObject;

		isThis = true;


	}
}



#if UNITY_EDITOR

[CustomEditor(typeof(InteractibleObject)),]
public class InteractibleObjectInspector : Editor
{
	InteractibleObject interactibleObject;
	SerializedObject serializedFacade;
	InteractableFacade facade;

	#region Serialized Propertys

	SerializedProperty touchedAction;
	SerializedProperty unTouchedAction;
	SerializedProperty firstTouched;
	SerializedProperty lastUnTouchedAction;

	SerializedProperty grabbedAction;
	SerializedProperty unGrabbedAction;
	SerializedProperty firstGrabbed;
	SerializedProperty lastUngrabbedAction;

	#endregion

	private void OnSceneGUI()
	{
		interactibleObject = (InteractibleObject)target;

		if (interactibleObject.secondaryAction == InteractibleObject.SecondaryTypes.FollowDirection)
		{
			Handles.color = Color.red;
			Handles.DrawLine(interactibleObject.transform.position, interactibleObject.transform.position + interactibleObject.transform.forward);
		}

		//var tempEditor = CreateEditor(interactibleObject);

		//tempEditor.OnInspectorGUI();
		//SceneView.RepaintAll();
		//tempEditor.serializedObject.ApplyModifiedProperties();
	}

	public override void OnInspectorGUI()
	{
		interactibleObject = (InteractibleObject)target;

		if (interactibleObject.facade)
		{
			serializedObject.Update();

			facade = interactibleObject.facade;
			serializedFacade = new SerializedObject(facade);

			if (facade != null)
				ShowProperties();
			else
				GUILayout.Label("NO INTERACIBLE OBJECT");

			serializedFacade.ApplyModifiedProperties();
			serializedObject.ApplyModifiedProperties();
		}
	}

	private void OnEnable()
	{
		//isThis = true;
		if (facade)
			if (!facade.GrabConfiguration.SecondaryAction)
				SwitchSecondaryAction();
	}

	void ShowProperties()
	{
		ShowMeshColRefrence();
		ShowSecondaryActionType();

		ShowGrabSettings();
		ShowEvents();
		ShowVariations();
	}

	private void ShowVariations()
	{
		if (interactibleObject.isDriveChild && interactibleObject.isThis)
			if (GUILayout.Button("GOTO: Drive"))
				MyEditorTools.FocusObject(interactibleObject.drive.gameObject, true);


	}

	void ShowEvents()
	{
		FindEvents();

		MyEditorTools.BeginHorizontal();
		bool eventBool = EditorGUILayout.BeginFoldoutHeaderGroup(interactibleObject.eventBool, "Event Actions");
		MyEditorTools.ShowRefrenceButton(facade.gameObject);
		EditorGUILayout.EndFoldoutHeaderGroup();
		MyEditorTools.EndHorizontal();

		if (interactibleObject.eventBool != eventBool)
		{
			Undo.RecordObject(interactibleObject, "Toggled Event Foldout");
			interactibleObject.eventBool = eventBool;
		}

		if (interactibleObject.eventBool)
		{
			EditorGUILayout.PropertyField(firstTouched);
			EditorGUILayout.PropertyField(touchedAction);
			EditorGUILayout.PropertyField(unTouchedAction);
			EditorGUILayout.PropertyField(lastUnTouchedAction);

			EditorGUILayout.PropertyField(firstGrabbed);
			EditorGUILayout.PropertyField(grabbedAction);
			EditorGUILayout.PropertyField(unGrabbedAction);
			EditorGUILayout.PropertyField(lastUngrabbedAction);

			serializedFacade.ApplyModifiedProperties();
		}
	}

	void FindEvents()
	{
		touchedAction = serializedFacade.FindProperty("Touched");
		unTouchedAction = serializedFacade.FindProperty("Untouched");
		firstTouched = serializedFacade.FindProperty("FirstTouched");
		lastUnTouchedAction = serializedFacade.FindProperty("LastUntouched");

		grabbedAction = serializedFacade.FindProperty("Grabbed");
		unGrabbedAction = serializedFacade.FindProperty("Ungrabbed");
		firstGrabbed = serializedFacade.FindProperty("FirstGrabbed");
		lastUngrabbedAction = serializedFacade.FindProperty("LastUngrabbed");
	}

	#region ShowGrabSettings

	public void ShowGrabSettings()
	{

		bool grabSettings = EditorGUILayout.BeginFoldoutHeaderGroup(interactibleObject.grabSettingsBool, "Grab Settings");
		EditorGUILayout.EndFoldoutHeaderGroup();

		if (interactibleObject.grabSettingsBool != grabSettings)
		{
			Undo.RecordObject(interactibleObject, "Toggled Grab Settings");
			interactibleObject.grabSettingsBool = grabSettings;
		}

		if (interactibleObject.grabSettingsBool)
		{
			EditorGUI.indentLevel += 1;
			ShowHoldType();
			ShowFollowType();
			ShowOffsetType();
			EditorGUI.indentLevel -= 1;
		}
	}

	public void ShowHoldType()
	{
		MyEditorTools.BeginHorizontal();

		var grabRecciver = new SerializedObject(interactibleObject.facade.GrabConfiguration.GrabReceiver.GetComponent<MyGrabInteractableReceiver>());
		var type = grabRecciver.FindProperty("_grabType");

		EditorGUI.BeginChangeCheck();

		var intVal =
			(int)((GrabInteractableReceiver.ActiveType)EditorGUILayout.EnumPopup("Follow Type", (GrabInteractableReceiver.ActiveType)type.intValue));

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(grabRecciver.targetObject, "Changed HoldType");
			type.intValue = intVal;
			grabRecciver.ApplyModifiedProperties();
		}

		MyEditorTools.ShowRefrenceButton(interactibleObject.facade.GrabConfiguration.PrimaryAction.gameObject);
		MyEditorTools.EndHorizontal();
	}

	public void ShowFollowType()
	{
		MyEditorTools.BeginHorizontal();

		var followAction = new SerializedObject(interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<MyGrabInteractableFollowAction>());
		var type = followAction.FindProperty("_followTracking");

		EditorGUI.BeginChangeCheck();

		var intVal =
			(int)((GrabInteractableFollowAction.TrackingType)EditorGUILayout.EnumPopup("Follow Type", (GrabInteractableFollowAction.TrackingType)type.intValue));

		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(followAction.targetObject, "Changed FollowType");
			type.intValue = intVal;
			followAction.ApplyModifiedProperties();
		}

		MyEditorTools.ShowRefrenceButton(interactibleObject.facade.GrabConfiguration.PrimaryAction.gameObject);
		MyEditorTools.EndHorizontal();
	}

	public void ShowOffsetType()
	{
		MyEditorTools.BeginHorizontal();

		var followAction = new SerializedObject(interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<MyGrabInteractableFollowAction>());
		var type = followAction.FindProperty("_grabOffset");

		EditorGUI.BeginChangeCheck();

		var intVal =
			(int)((GrabInteractableFollowAction.OffsetType)EditorGUILayout.EnumPopup("Follow Type", (GrabInteractableFollowAction.OffsetType)type.intValue));


		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(followAction.targetObject, "Changed OffsetType");
			type.intValue = intVal;
			followAction.ApplyModifiedProperties();
		}

		MyEditorTools.ShowRefrenceButton(interactibleObject.facade.GrabConfiguration.PrimaryAction.gameObject);

		MyEditorTools.EndHorizontal();
	}

	#endregion

	public static void ShowList(SerializedProperty list)
	{
		if (EditorGUILayout.PropertyField(list, false))
		{
			for (int i = 0; i < list.arraySize; i++)
			{
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent("Name: " + i));
			}
		}
	}

	public void ShowMeshColRefrence()
	{
		if (GUILayout.Button("Mesh & Colliders"))
		{
			Selection.activeObject = interactibleObject.meshContainer;
			SceneView.FrameLastActiveSceneView();
		}
	}

	public void ShowSecondaryActionType()
	{
		if (!facade.GrabConfiguration.SecondaryAction)
		{
			if (GUILayout.Button("Generate Secondary Action"))
				SwitchSecondaryAction();
		}
		else
		{
			if (interactibleObject.GetComponent<ClimbableObject>())
				return;

			MyEditorTools.BeginHorizontal();

			var secondaryType = (InteractibleObject.SecondaryTypes)EditorGUILayout.EnumPopup("SecondaryAction", interactibleObject.secondaryAction);

			if (interactibleObject.secondaryAction != secondaryType)
			{
				Undo.RecordObject(interactibleObject, "Changed Secondary Type");
				interactibleObject.secondaryAction = secondaryType;
				SwitchSecondaryAction();
				Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
			}


			MyEditorTools.ShowRefrenceButton(facade.GrabConfiguration.SecondaryAction.gameObject);


			MyEditorTools.EndHorizontal();
		}
	}

	public void SwitchSecondaryAction()
	{
		if (PrefabsXR.inDev)
			Debug.Log(interactibleObject.secondaryAction);


		if (facade.GrabConfiguration.SecondaryAction)
		{
			Undo.RecordObject(facade.GrabConfiguration.SecondaryAction.gameObject, "Changed Secondary Type");
			facade.GrabConfiguration.SecondaryAction.gameObject.SetActive(false);
		}

		var actions = facade.GrabConfiguration.GetComponentsInChildren<InteractibleAction>(true);

		foreach (var action in actions)
		{
			if (action.type == interactibleObject.secondaryAction)
			{
				Undo.RecordObjects(new Object[] { action.gameObject, facade.GrabConfiguration.SecondaryAction.gameObject }, "Changed Secondary Type");
				action.gameObject.SetActive(true);
				Undo.RecordObject(facade.GrabConfiguration, "Changed Secondary Type");
				var grabConfigObject = new SerializedObject(facade.GrabConfiguration);

				var secondaryActionProperty = grabConfigObject.FindProperty("_secondaryAction");
				secondaryActionProperty.objectReferenceValue = action.gameObject.GetComponent<GrabInteractableAction>();

				grabConfigObject.ApplyModifiedProperties();
				//facade.GrabConfiguration.SecondaryAction = action.gameObject.GetComponent<GrabInteractableAction>();
			}
		}
	}
}

#endif