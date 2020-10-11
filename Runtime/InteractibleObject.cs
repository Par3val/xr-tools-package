using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactables.Grab.Action;
using VRTK.Prefabs.Interactions.Interactables.Grab.Receiver;
using UnityEditor;

[SelectionBase, ExecuteInEditMode]
public class InteractibleObject : MonoBehaviour
{
  [System.Serializable]
  public enum SecondaryTypes { None, Swap, FollowDirection, Scale }

  public InteractableFacade facade;
  public GameObject meshContainer;
  public SecondaryTypes secondaryAction = SecondaryTypes.Swap;

  //if drive child
  public bool isDriveChild = false;
  public bool isThis = false;
  public DriveObject drive;

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

    if (!facade.GrabConfiguration.SecondaryAction)
      SwitchSecondaryAction();
  }

  private void OnDrawGizmos()
  {
    if (PrefabUtility.IsPartOfAnyPrefab(gameObject))
    {
			PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
		}
  }

  public void SwitchSecondaryAction()
  {
    Transform secondaryActionParent = facade.GrabConfiguration.transform.GetChild(2).GetChild(1);

    if (facade.GrabConfiguration.SecondaryAction)
      DestroyImmediate(facade.GrabConfiguration.SecondaryAction.gameObject);
    print(secondaryAction);
    facade.GrabConfiguration.SecondaryAction = Instantiate(PrefabsXR.GetActionByEnum(secondaryAction), secondaryActionParent).GetComponent<GrabInteractableAction>();
    facade.GrabConfiguration.SecondaryAction.gameObject.name = secondaryAction.ToString();
  }
}

#if UNITY_EDITOR

[CustomEditor(typeof(InteractibleObject))]
public class InteractibleObjectInspector : Editor
{
  InteractibleObject interactibleObject;
  SerializedObject serializedFacade;
  InteractableFacade facade;

  private static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);

  #region Foldout Bools

  bool grabSettingsBool = true;
  bool eventBool = false;

  #endregion

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

    var tempEditor = CreateEditor(interactibleObject);

    tempEditor.OnInspectorGUI();
    SceneView.RepaintAll();
    tempEditor.serializedObject.ApplyModifiedProperties();
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
    eventBool = EditorGUILayout.BeginFoldoutHeaderGroup(eventBool, "Event Actions");
    MyEditorTools.ShowRefrenceButton(facade.gameObject);
    EditorGUILayout.EndFoldoutHeaderGroup();
    MyEditorTools.EndHorizontal();

    if (eventBool)
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

    grabSettingsBool = EditorGUILayout.BeginFoldoutHeaderGroup(grabSettingsBool, "Grab Settings");
    EditorGUILayout.EndFoldoutHeaderGroup();
    if (grabSettingsBool)
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

    var grabType = interactibleObject.facade.GrabConfiguration.GrabReceiver.GrabType;
    var newGrabType = (GrabInteractableReceiver.ActiveType)EditorGUILayout.EnumPopup("Grab Type", grabType);

    //var grabRecciver = new SerializedObject(interactibleObject.facade.GrabConfiguration.GrabReceiver);
    //var type = grabRecciver.FindProperty("GrabType");
    //Debug.Log(type);
    ////EditorGUILayout.PropertyField(type);
    //grabRecciver.ApplyModifiedPropertiesWithoutUndo();
    if (grabType != newGrabType)
    {
      interactibleObject.facade.GrabConfiguration.GrabReceiver.GrabType = newGrabType;
      interactibleObject.facade.GrabConfiguration.GrabReceiver.ConfigureGrabType();
    }

    MyEditorTools.ShowRefrenceButton(interactibleObject.facade.GrabConfiguration.GrabReceiver.gameObject);

    MyEditorTools.EndHorizontal();

  }

  public void ShowFollowType()
  {
    MyEditorTools.BeginHorizontal();
    interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<GrabInteractableFollowAction>().FollowTracking =
      (GrabInteractableFollowAction.TrackingType)EditorGUILayout.EnumPopup("Follow Type", interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<GrabInteractableFollowAction>().FollowTracking);

    MyEditorTools.ShowRefrenceButton(interactibleObject.facade.GrabConfiguration.PrimaryAction.gameObject);
    MyEditorTools.EndHorizontal();
  }

  public void ShowOffsetType()
  {
    MyEditorTools.BeginHorizontal();
    interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<GrabInteractableFollowAction>().GrabOffset =
      (GrabInteractableFollowAction.OffsetType)EditorGUILayout.EnumPopup("Grab Offset", interactibleObject.facade.GrabConfiguration.PrimaryAction.GetComponent<GrabInteractableFollowAction>().GrabOffset);

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
        interactibleObject.SwitchSecondaryAction();
    }
    else
    {
      MyEditorTools.BeginHorizontal();

      var previous = interactibleObject.secondaryAction;

      interactibleObject.secondaryAction = (InteractibleObject.SecondaryTypes)EditorGUILayout.EnumPopup("SecondaryAction", interactibleObject.secondaryAction);

      if (interactibleObject.secondaryAction != previous)
        interactibleObject.SwitchSecondaryAction();


      MyEditorTools.ShowRefrenceButton(facade.GrabConfiguration.SecondaryAction.gameObject);


      MyEditorTools.EndHorizontal();
    }
  }
}

#endif