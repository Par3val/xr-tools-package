using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRTK.Prefabs.CameraRig.TrackedAlias;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig;
using Zinnia.Tracking.CameraRig;
using VRTK.Prefabs.CameraRig.SimulatedCameraRig;
using Zinnia.Data.Operation.Mutation;

[ExecuteInEditMode, System.Serializable]
public class PlayerRig : MonoBehaviour
{
	public TrackedAliasFacade alias;

	public UnityXRConfigurator xrConfig;
	public SimulatorFacade simFacade;

	public LinkedAliasAssociationCollection linkedAliasXR;
	public LinkedAliasAssociationCollection linkedAliasSim;

	public InputManager leftHand;
	public InputManager rightHand;


	#region EditorBools

	public bool isSim;
	public bool leftHandOpen;
	public bool rightHandOpen;

	//PlayerComponentsBools
	public PlayerComponent walk;
	public PlayerComponent rotate;

	#endregion

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

		rig.leftHand = rig.alias.LeftControllerAlias.GetComponentInChildren<InputManager>();
		rig.rightHand = rig.alias.RightControllerAlias.GetComponentInChildren<InputManager>();

		if (!System.Enum.IsDefined(typeof(InputManager.hands), rig.leftHand.handedness))
			rig.leftHand.Awake();

		if (!System.Enum.IsDefined(typeof(InputManager.hands), rig.rightHand.handedness))
			rig.rightHand.Awake();

		if (PrefabUtility.GetPrefabType(rig.gameObject) == PrefabType.PrefabInstance)
		{
			PrefabUtility.UnpackPrefabInstance(rig.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
			return;
		}
	}
	
	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI();

		rig = (PlayerRig)target;

		if (NullRefrenceRisk())
			OnEnable();

		ShowHand(ref rig.leftHandOpen, rig.leftHand);
		ShowHand(ref rig.rightHandOpen, rig.rightHand);


		if (GUILayout.Button(rig.isSim ? "Simulator" : "Headset"))
		{
			rig.isSim = !rig.isSim;
			rig.SwitchRigs();
		}

		GUILayout.Space(10f);
		GUILayout.Label("PlayerComponents", EditorStyles.boldLabel);

		ShowPlayerComponents();
	}

	public void ShowHand(ref bool handBool, InputManager hand)
	{
		GUILayout.BeginHorizontal();

		handBool = EditorGUILayout.BeginFoldoutHeaderGroup(handBool, $"{hand.handedness} Hand");
		EditorGUILayout.EndFoldoutHeaderGroup();

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

	public void ShowPlayerComponents()
	{
		if (!rig.walk)
			if (GUILayout.Button("Add Walk"))
			{
				rig.walk = ((GameObject)PrefabUtility.InstantiatePrefab(PrefabsXR.GetPlayerComponent(PlayerComponent.PlayerComponents.Walk), rig.transform)).GetComponent<PlayerComponent>();
				rig.walk.transform.GetComponentInChildren<TransformPositionMutator>().Target = rig.gameObject;
				rig.walk.transform.GetComponentInChildren<TransformPositionMutator>().FacingDirection = rig.alias.HeadsetAlias.gameObject;
			}
		if (!rig.rotate)
			if (GUILayout.Button("Add Rotate"))
			{
				rig.rotate = ((GameObject)PrefabUtility.InstantiatePrefab(PrefabsXR.GetPlayerComponent(PlayerComponent.PlayerComponents.Rotate), rig.transform)).GetComponent<PlayerComponent>();
				rig.rotate.transform.GetComponentInChildren<TransformEulerRotationMutator>().Target = rig.gameObject;
				rig.rotate.transform.GetComponentInChildren<TransformEulerRotationMutator>().Origin = rig.alias.HeadsetAlias.gameObject;
			}
	}

	public bool NullRefrenceRisk()
	{
		return rig != null
			|| rig.alias != null
			|| rig.xrConfig != null
			|| rig.linkedAliasXR != null
			|| rig.simFacade != null
			|| rig.linkedAliasSim != null
			|| rig.leftHand != null
			|| rig.rightHand != null
			|| System.Enum.IsDefined(typeof(InputManager.hands), rig.leftHand.handedness)
			|| System.Enum.IsDefined(typeof(InputManager.hands), rig.rightHand.handedness);
	}
}

#endif