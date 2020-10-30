using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRTK.Prefabs.CameraRig.TrackedAlias;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig;
using Zinnia.Tracking.CameraRig;
using VRTK.Prefabs.CameraRig.SimulatedCameraRig;
using Zinnia.Data.Operation.Mutation;
using Zinnia.Tracking.Collision.Active.Operation.Extraction;

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
	public bool playerComponentsOpen = true;

	#endregion

	//PlayerComponentsBools { Walk, Teleport, Rotate, Climb, PhysicalBody, Other }
	public PlayerComponent walk;
	public PlayerComponent rotate;
	public PlayerComponent teleport;
	public PlayerComponent climb;
	public PlayerComponent physicalBody;

	public PlayerComponent GetActivePlayerComponent(PlayerComponent.ComponentTypes type)
	{
		switch (type)
		{
			case PlayerComponent.ComponentTypes.Walk:
				return walk;
			case PlayerComponent.ComponentTypes.Teleport:
				return teleport;
			case PlayerComponent.ComponentTypes.Rotate:
				return rotate;
			case PlayerComponent.ComponentTypes.Climb:
				return climb;
			case PlayerComponent.ComponentTypes.PhysicalBody:
				return physicalBody;
		}

		return null;
	}

	public PlayerComponent[] GetActivePlayerComponents()
	{
		PlayerComponent[] tempArray = new PlayerComponent[5];

		tempArray[0] = walk;
		tempArray[1] = rotate;
		tempArray[2] = teleport;
		tempArray[3] = climb;
		tempArray[4] = physicalBody;

		return tempArray;
	}
	public void SetPlayerComponentsInRig(PlayerComponent component)
	{
		switch (component.type)
		{
			case PlayerComponent.ComponentTypes.Walk:
				walk = component;
				break;
			case PlayerComponent.ComponentTypes.Teleport:
				teleport = component;
				break;
			case PlayerComponent.ComponentTypes.Rotate:
				rotate = component;
				break;
			case PlayerComponent.ComponentTypes.Climb:
				climb = component;
				break;
			case PlayerComponent.ComponentTypes.PhysicalBody:
				physicalBody = component;
				break;
		}
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

	public void CanMove(bool canMove)
	{
		if (walk)
			walk.gameObject.SetActive(canMove);
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
		GUILayout.Space(10f);
		rig.playerComponentsOpen = EditorGUILayout.BeginFoldoutHeaderGroup(rig.playerComponentsOpen, $"PlayerComponents");
		EditorGUILayout.EndFoldoutHeaderGroup();

		EditorGUI.indentLevel++;

		if (rig.playerComponentsOpen)
		{
			foreach (PlayerComponent.ComponentTypes type in System.Enum.GetValues(typeof(PlayerComponent.ComponentTypes)))
			{
				var tempComponent = rig.GetActivePlayerComponent(type);

				if (tempComponent != null)
				{
					CreateEditor(tempComponent).OnInspectorGUI();
				}
				else
				{
					if (GUILayout.Button($"Add {type}"))
					{
						var component = PlayerComponent.CreateComponent(type, rig);

						if (component != null)
							rig.SetPlayerComponentsInRig(component);
						else
							Debug.LogError($"No Component for {type}");
					}
				}
			}
			Undo.FlushUndoRecordObjects();
		}

		EditorGUI.indentLevel--;
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