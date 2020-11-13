using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Locomotion.Movement.AxesToVector3;
using Zinnia.Data.Operation.Mutation;
using Zinnia.Tracking.Velocity;
using Zinnia.Action;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

[System.Serializable, ExecuteInEditMode]
public class PlayerComponent : MonoBehaviour
{
	[System.Serializable]
	public enum ComponentTypes { Walk, Teleport, Rotate, Climb, PhysicalBody }

	public ComponentTypes type;
	[HideInInspector]
	public PlayerRig rig;

	[SerializeField]
	public AxesToVector3Facade walk;
	[SerializeField]
	public AxesToVector3Facade rotate;
	[SerializeField]
	public BetterClimb climb;
	[SerializeField]
	public PlayerController playerBody;

	//walk 
	[SerializeField]
	public float speed = 1.75f;

	//rotate
	[SerializeField]
	public bool smoothTurn = false;
	[SerializeField]
	public int turnAmount = 45;

	//climb
	[SerializeField]
	public float throwMul = 1.5f;

	//body
	[SerializeField]
	public float jumpPower = 2.5f;

	//private void Start()
	//{
	//	Setup(rig);
	//	UpdateVariables();
	//}

	public static PlayerComponent CreateComponent(ComponentTypes _type, PlayerRig _rig)
	{
		var tempObject = PrefabsXR.GetPlayerComponent(_type);

		if (!tempObject)
			return null;
		var instantiatedObject = Instantiate(tempObject, _rig.transform).GetComponent<PlayerComponent>();
		//var instantiatedObject = ((GameObject)PrefabUtility.InstantiatePrefab());
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

	public void UpdateVariables()
	{
		switch (type)
		{
			case ComponentTypes.Walk:
				UpdateWalk(speed);
				break;
			case ComponentTypes.Teleport:
				break;
			case ComponentTypes.Rotate:
				UpdateRotate(smoothTurn, turnAmount);
				break;
			case ComponentTypes.Climb:
				UpdateClimb(throwMul);
				break;
			case ComponentTypes.PhysicalBody:
				UpdatePlayerBody(jumpPower);
				break;
		}
	}

	public void UpdateWalk(float _speed)
	{
		walk.LateralSpeedMultiplier = _speed;
		walk.LongitudinalSpeedMultiplier = -_speed;
	}
	public void UpdateRotate(bool _smooth, int _turnAmount)
	{
		smoothTurn = _smooth;
		turnAmount = _turnAmount;

		if (_smooth)
			rotate.AxisUsageType = AxesToVector3Facade.AxisUsage.Incremental;
		else
			rotate.AxisUsageType = AxesToVector3Facade.AxisUsage.Directional;

		rotate.LateralSpeedMultiplier = _turnAmount;
	}
	public void UpdateClimb(float _throwMul)
	{
		throwMul = _throwMul;
		climb.throwMul = _throwMul;
	}
	public void UpdatePlayerBody(float _jumpPower)
	{
		jumpPower = _jumpPower;
		playerBody.jumpPower = _jumpPower;
	}


	#endregion

	#region Setup

	void SetupWalk()
	{
		GetComponent<TransformPositionMutator>().Target = rig.gameObject;
		GetComponent<TransformPositionMutator>().FacingDirection = rig.alias.HeadsetAlias.gameObject;
		walk = GetComponent<AxesToVector3Facade>();

		var walkMap = new InputMapping("Walk");
		walkMap.axisType = InputMapping.AxisType.Axis2D;
		walkMap.type2D = InputMapping.InputTypeAxis2D.Thumb2D;

		UnityEventTools.AddPersistentListener(walkMap.Moved2D, GetComponent<Vector2Action>().Receive);
		rig.leftHand.AddMap(walkMap);
	}
	void SetupTeleport()
	{

	}
	void SetupRotate()
	{
		GetComponent<TransformEulerRotationMutator>().Target = rig.gameObject;
		GetComponent<TransformEulerRotationMutator>().Origin = rig.alias.HeadsetAlias.gameObject;
		rotate = GetComponent<AxesToVector3Facade>();

		var rotateMap = new InputMapping("Walk");
		rotateMap.axisType = InputMapping.AxisType.Axis2D;
		rotateMap.type2D = InputMapping.InputTypeAxis2D.Thumb2D;

		UnityEventTools.AddPersistentListener(rotateMap.Moved2D, GetComponent<Vector2Action>().Receive);
		rig.rightHand.AddMap(rotateMap);
	}
	void SetupClimb()
	{
		climb = GetComponent<BetterClimb>();
		climb.playArea = rig.transform;

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
		playerBody = GetComponent<PlayerController>();

		playerBody.rig = GetComponentInParent<PlayerRig>();
		if (!playerBody.rig.gameObject.GetComponent<Rigidbody>())
		{
			playerBody.rb = playerBody.rig.gameObject.AddComponent<Rigidbody>();
			playerBody.rb.constraints = RigidbodyConstraints.FreezeRotation;
		}
		else
			playerBody.rb = playerBody.rig.gameObject.GetComponent<Rigidbody>();


		if (!playerBody.rig.gameObject.GetComponent<CapsuleCollider>())
		{
			playerBody.col = playerBody.rig.gameObject.AddComponent<CapsuleCollider>();
			playerBody.col.radius = .18f;//1.5
			playerBody.col.center = new Vector3(0, 1.5f / 2, 0);
			playerBody.col.height = 1.5f;
		}
		else
			playerBody.col = playerBody.rig.gameObject.GetComponent<CapsuleCollider>();

		if(!playerBody.rig.gameObject.GetComponent<AverageVelocityEstimator>())
		{
			var vel = playerBody.rig.gameObject.AddComponent<AverageVelocityEstimator>();
			vel.Source = playerBody.rig.gameObject;
		}
		//GetComponent<BodyRepresentationFacade>().Source = rig.alias.HeadsetAlias.gameObject;
		//GetComponent<BodyRepresentationFacade>().Offset = rig.alias.PlayAreaAlias.gameObject;

		//GetComponent<InteractorFacadeObservableList>().Add(rig.leftHand.GetComponent<InteractorFacade>());
		//GetComponent<InteractorFacadeObservableList>().Add(rig.rightHand.GetComponent<InteractorFacade>());
	}


	#endregion

}

#if UNITY_EDITOR


[CustomEditor(typeof(PlayerComponent)), System.Serializable]
class PlayerComponentEditor : Editor
{
	PlayerComponent playerComp;
	public override void OnInspectorGUI()
	{
		playerComp = (PlayerComponent)target;

		if (!playerComp.rig)
			playerComp.type = (PlayerComponent.ComponentTypes)EditorGUILayout.EnumPopup("Type ", playerComp.type);

		ShowData();
		//base.OnInspectorGUI();

	}

	public void ShowData()
	{
		switch (playerComp.type)
		{
			case PlayerComponent.ComponentTypes.Walk:
				ShowWalk();
				break;
			case PlayerComponent.ComponentTypes.Teleport:
				break;
			case PlayerComponent.ComponentTypes.Rotate:
				ShowRotate();
				break;
			case PlayerComponent.ComponentTypes.Climb:
				ShowClimb();
				break;
			case PlayerComponent.ComponentTypes.PhysicalBody:
				ShowPlayerBody();
				break;
		}
	}


	void ShowWalk()
	{
		EditorGUI.BeginChangeCheck();
		playerComp.speed = EditorGUILayout.FloatField("Walk Speed", playerComp.speed);

		if (EditorGUI.EndChangeCheck())
			playerComp.UpdateWalk(playerComp.speed);
	}

	void ShowRotate()
	{
		EditorGUI.BeginChangeCheck();
		playerComp.smoothTurn = EditorGUILayout.Toggle("Smooth Turning", playerComp.smoothTurn);
		playerComp.turnAmount = EditorGUILayout.IntField("Turn Amount " + (playerComp.smoothTurn ? "(smooth)" : "(snap)"), playerComp.turnAmount);

		if (EditorGUI.EndChangeCheck())
			playerComp.UpdateRotate(playerComp.smoothTurn, playerComp.turnAmount);
	}

	void ShowClimb()
	{
		EditorGUI.BeginChangeCheck();
		playerComp.throwMul = EditorGUILayout.FloatField("Dismount Multiplier", playerComp.throwMul);

		if (EditorGUI.EndChangeCheck())
			playerComp.UpdateClimb(playerComp.throwMul);
	}

	void ShowPlayerBody()
	{
		EditorGUI.BeginChangeCheck();
		playerComp.jumpPower = EditorGUILayout.FloatField("Jump Power", playerComp.jumpPower);

		if (EditorGUI.EndChangeCheck())
			playerComp.UpdatePlayerBody(playerComp.jumpPower);
	}
}

#endif
//if (!rig.climb)
//	CreateComponent(ComponentTypes.Climb, rig);
//else
//{
//	UnityEventTools.AddBoolPersistentListener(rig.climb.GetComponent<ClimbFacade>().ClimbStarted, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), false);
//	UnityEventTools.AddBoolPersistentListener(rig.climb.GetComponent<ClimbFacade>().ClimbStopped, new UnityEngine.Events.UnityAction<bool>(rig.CanMove), true);
//}