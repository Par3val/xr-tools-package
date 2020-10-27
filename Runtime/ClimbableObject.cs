using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactors;
using VRTK.Prefabs.Locomotion.Movement.Climb;

[RequireComponent(typeof(InteractibleObject))]
public class ClimbableObject : MonoBehaviour
{
	BetterClimb climbRef;
	InteractableFacade thisInteractible;
	public bool twoHandedClimb = true;
	public bool canGetOff = true;
	public bool useOffset = true;
	public bool useOffsetSec = true;
	public bool canSecondGetOff = true;

	bool tryingToGetOff = false;
	public void Awake()
	{
		climbRef = FindObjectOfType<BetterClimb>();

		if (!climbRef)
			Debug.LogError("No Player BetterClimb Component");

		thisInteractible = GetComponent<InteractableFacade>();
		GetComponent<InteractableFacade>().Grabbed.AddListener(Grabed);
		GetComponent<InteractableFacade>().Ungrabbed.AddListener(UnGrabed);
	}

	public void Grabed(InteractorFacade hand) =>
		climbRef.StartClimbing(thisInteractible, hand, useOffset, useOffsetSec);

	public void UnGrabed(InteractorFacade hand)
	{
		if (canGetOff)
			climbRef.StopClimbing(thisInteractible, hand);
		else if (!tryingToGetOff)
			StartCoroutine(GetOffWhenCan(hand));

	}

	public void SecondHandGrabbed(GameObject handObj)
	{
		if (!twoHandedClimb)
			return;

		var tempInteractor = handObj.transform.parent.parent.parent.GetComponent<InteractorFacade>();
		if (tempInteractor)
			climbRef.StartClimbing(thisInteractible, tempInteractor, true, useOffsetSec);
	}

	public void SecondHandUnGrabbed(GameObject handObj)
	{
		if (!twoHandedClimb)
			return;

		var tempInteractor = handObj.transform.parent.parent.parent.GetComponent<InteractorFacade>();
		if (tempInteractor && canSecondGetOff)
			climbRef.StopClimbing(thisInteractible, tempInteractor);
	}

	//	public ClimbFacade climbFacade;
	//	InputManager hand;
	public IEnumerator GetOffWhenCan(InteractorFacade hand)
	{
		while (!canGetOff)
		{
			yield return new WaitForFixedUpdate();
		}

		yield return new WaitForSeconds(1f);

		climbRef.StopClimbing(thisInteractible, hand);
	}
}

//#if UNITY_EDITOR

//class ClimbableObjectEditor : Editor
//{
//	public override void OnInspectorGUI()
//	{


//		base.OnInspectorGUI();
//	}
//}

//#endif
