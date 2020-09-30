using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRTK.Prefabs.CameraRig.TrackedAlias;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig;
using Zinnia.Tracking.CameraRig;
using VRTK.Prefabs.CameraRig.SimulatedCameraRig;
using System;

[ExecuteInEditMode, Serializable]
public class PlayerRig : MonoBehaviour
{
  public TrackedAliasFacade alias;

  public UnityXRConfigurator xrConfig;
  public SimulatorFacade simFacade;

  public LinkedAliasAssociationCollection linkedAliasXR;
  public LinkedAliasAssociationCollection linkedAliasSim;

  public bool isSim;
  public bool leftHand;
  public bool rightHand;

  private void Start()
  {
    Debug.Log(leftHand);
  }

  public void SwitchRigs()
  {
    if (isSim)
    {
      alias.CameraRigs.NonSubscribableElements[0].gameObject.SetActive(false);
      alias.CameraRigs.NonSubscribableElements[1].gameObject.SetActive(true);
    }
    else
    {
      alias.CameraRigs.NonSubscribableElements[0].gameObject.SetActive(true);
      alias.CameraRigs.NonSubscribableElements[1].gameObject.SetActive(false);
    }
  }
}

#if UNITY_EDITOR

[CustomEditor(typeof(PlayerRig))]
public class PlayerRigInspector : Editor
{
  public PlayerRig rig;

  private void OnEnable()
  {
    rig = (PlayerRig)target;

    rig.alias = rig.transform.GetChild(0).GetComponent<TrackedAliasFacade>();

    rig.xrConfig = rig.transform.GetChild(1).GetComponent<UnityXRConfigurator>();
    rig.linkedAliasXR = rig.transform.GetChild(1).GetComponent<LinkedAliasAssociationCollection>();

    rig.simFacade = rig.transform.GetChild(2).GetComponent<SimulatorFacade>();
    rig.linkedAliasSim = rig.transform.GetChild(2).GetComponent<LinkedAliasAssociationCollection>();
    EditorUtility.SetDirty(target);
  }

  public override void OnInspectorGUI()
  {
    //base.OnInspectorGUI();

    rig = (PlayerRig)target;

    //if (NullRefrenceRisk())
    //  OnEnable();

    ShowHand(ref rig.leftHand, rig.alias.LeftControllerAlias.GetComponentInChildren<InputManager>());
    ShowHand(ref rig.rightHand, rig.alias.RightControllerAlias.GetComponentInChildren<InputManager>());


    if (GUILayout.Button(rig.isSim ? "Simulator" : "Headset"))
    {
      rig.isSim = !rig.isSim;
      rig.SwitchRigs();
    }
  }

  public void ShowHand(ref bool handBool, InputManager hand)
  {
    GUILayout.BeginHorizontal();

    handBool = EditorGUILayout.Foldout(handBool, $"{hand.handedness} Hand");

    MyEditorTools.ShowRefrenceButton(hand.gameObject);

    GUILayout.EndHorizontal();

    if (handBool)
    {
      var tempEditor = CreateEditor(hand);

      tempEditor.OnInspectorGUI();
      SceneView.RepaintAll();
      tempEditor.serializedObject.ApplyModifiedProperties();
    }

    if (hand.rig == null)
      hand.rig = rig;
  }

  public bool NullRefrenceRisk()
  {
    return rig != null || rig.alias != null || rig.xrConfig != null || rig.linkedAliasXR != null || rig.simFacade != null || rig.linkedAliasSim != null;
  }
}

#endif