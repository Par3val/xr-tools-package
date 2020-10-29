using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactors;
using VRTK.Prefabs.Interactions.Interactors.Collection;
using VRTK.Prefabs.Locomotion.BodyRepresentation;
using VRTK.Prefabs.Locomotion.Movement.AxesToVector3;
using VRTK.Prefabs.Locomotion.Movement.Climb;
using Zinnia.Data.Operation.Mutation;
using UnityEditor.Events;

[ExecuteInEditMode, System.Serializable]
public class PlayerComponent : MonoBehaviour
{
	[System.Serializable]
	public enum ComponentTypes { Walk, Teleport, Rotate, Climb, PhysicalBody }

	public ComponentTypes type;
	[HideInInspector]
	public PlayerRig rig;

	//walk 
	float speed = 1.75f;
	
	//rotate
	bool smoothTurn = false;
	int turnAmount = 45;

	//climb
	float ThrowMul = 1.5f;

	public static PlayerComponent CreateComponent(ComponentTypes _type, PlayerRig _rig)
	{
		var tempObject = PrefabsXR.GetPlayerComponent(_type);

		if (!tempObject)
			return null;

		var instantiatedObject = ((GameObject)PrefabUtility.InstantiatePrefab(tempObject, _rig.transform)).GetComponent<PlayerComponent>();
		instantiatedObject.Setup(_rig);
		return instantiatedObject;
	}

	public void Setup(PlayerRig _rig)
	{
		rig = _rig;

		switch (type)
		{
			case ComponentTypes.Walk:
				SetupWalk();
				break;
			case ComponentTypes.Teleport:
				SetupTeleport();
				break;
			case ComponentTypes.Rotate:
				SetupRotate();
				break;
			case ComponentTypes.Climb:
				SetupClimb();
				break;
			case ComponentTypes.PhysicalBody:
				SetupPhysicalBody();
				break;
		}
	}

	#region LiveChanges

	public void UpdateWalk(float _speed)
	{
		speed = _speed;

		var axisToVector = GetComponent<AxesToVector3Facade>();
		axisToVector.LateralSpeedMultiplier = _speed;
		axisToVector.LongitudinalSpeedMultiplier = -_speed;
	}

	public void UpdateClimb(float _throwMul)
	{
		ThrowMul = _throwMul;
		var betterClimb = GetComponent<BetterClimb>();

		betterClimb.ThrowMul = _throwMul;
	}


	public void UpdateRotate(bool _smooth, int _turnAmount)
	{
		smoothTurn = _smooth;
		turnAmount = _turnAmount;


		var axisToVector = GetComponent<AxesToVector3Facade>();
		if (smoothTurn)
			axisToVector.AxisUsageType = AxesToVector3Facade.AxisUsage.Incremental;
		else
			axisToVector.AxisUsageType = AxesToVector3Facade.AxisUsage.Directional;

		axisToVector.LateralSpeedMultiplier = turnAmount;
	}

	#endregion

	#region Setup

	void SetupWalk()
	{
		GetComponent<TransformPositionMutator>().Target = rig.gameObject;
		GetComponent<TransformPositionMutator>().FacingDirection = rig.alias.HeadsetAlias.gameObject;
	}
	void SetupTeleport()
	{

	}
	void SetupRotate()
	{
		GetComponent<TransformEulerRotationMutator>().Target = rig.gameObject;
		GetComponent<TransformEulerRotationMutator>().Origin = rig.alias.HeadsetAlias.gameObject;
	}
	void SetupClimb()
	{
		GetComponent<BetterClimb>().playArea = rig.transform;


		//if (!rig.physicalBody)
		//{
		//	PlayerComponent tempBody = CreateComponent(ComponentTypes.PhysicalBody, rig);
		//	rig.SetPlayerComponentsInRig(tempBody);
		//}

		//GetComponent<ClimbFacade>().BodyRepresentationFacade = rig.physicalBody.GetComponent<BodyRepresentationFacade>();

		//UnityEventTools.AddBoolPersistentListener(GetComponent<ClimbFacade>().ClimbStarted, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), false);
		//UnityEventTools.AddBoolPersistentListener(GetComponent<ClimbFacade>().ClimbStopped, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), true);
	}
	void SetupPhysicalBody()
	{
		var playerController = GetComponent<PlayerController>();

		playerController.rig = GetComponentInParent<PlayerRig>();
		if (!playerController.rig.gameObject.GetComponent<Rigidbody>())
		{
			playerController.rb = playerController.rig.gameObject.AddComponent<Rigidbody>();
			playerController.rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
		else
			playerController.rb = playerController.rig.gameObject.GetComponent<Rigidbody>();


		if (!playerController.rig.gameObject.GetComponent<CapsuleCollider>())
		{
			playerController.col = playerController.rig.gameObject.AddComponent<CapsuleCollider>();
			playerController.col.radius = .18f;//1.5
			playerController.col.center = new Vector3(0, 1.5f / 2, 0);
			playerController.col.height = 1.5f;
		}
		else
			playerController.col = playerController.rig.gameObject.GetComponent<CapsuleCollider>();
		//GetComponent<BodyRepresentationFacade>().Source = rig.alias.HeadsetAlias.gameObject;
		//GetComponent<BodyRepresentationFacade>().Offset = rig.alias.PlayAreaAlias.gameObject;

		//GetComponent<InteractorFacadeObservableList>().Add(rig.leftHand.GetComponent<InteractorFacade>());
		//GetComponent<InteractorFacadeObservableList>().Add(rig.rightHand.GetComponent<InteractorFacade>());
	}


	#endregion

	//#if UnityEditor

	public void ShowData()
	{
		switch (type)
		{
			case ComponentTypes.Walk:
				ShowWalk();
				break;
			case ComponentTypes.Teleport:
				break;
			case ComponentTypes.Rotate:
				ShowRotate();
				break;
			case ComponentTypes.Climb:
				ShowClimb();
				break;
			case ComponentTypes.PhysicalBody:
				break;
		}
	}
	void ShowWalk()
	{
		EditorGUI.BeginChangeCheck();
		speed = EditorGUILayout.FloatField("Walk Speed", speed);

		if (EditorGUI.EndChangeCheck())
			UpdateWalk(speed);
	}
	

	void ShowRotate()
	{
		EditorGUI.BeginChangeCheck();
		smoothTurn = EditorGUILayout.Toggle("Smooth Turning", smoothTurn);
		turnAmount = EditorGUILayout.IntField("Turn Amount " + (smoothTurn ? "(smooth)" : "(snap)"), turnAmount);

		if (EditorGUI.EndChangeCheck())
			UpdateRotate(smoothTurn, turnAmount);
	}


	void ShowClimb()
	{
		EditorGUI.BeginChangeCheck();
		ThrowMul = EditorGUILayout.FloatField("Dismount Multiplier", ThrowMul);

		if (EditorGUI.EndChangeCheck())
			UpdateClimb(ThrowMul);
	}
	//#endif
}

//if (!rig.climb)
//	CreateComponent(ComponentTypes.Climb, rig);
//else
//{
//	UnityEventTools.AddBoolPersistentListener(rig.climb.GetComponent<ClimbFacade>().ClimbStarted, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), false);
//	UnityEventTools.AddBoolPersistentListener(rig.climb.GetComponent<ClimbFacade>().ClimbStopped, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), true);
//}