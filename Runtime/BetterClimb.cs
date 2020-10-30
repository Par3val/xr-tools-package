using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactors;

public class BetterClimb : MonoBehaviour
{
	Vector3 offset;
	Vector3 offsetSec;

	InteractorFacade grabHand;
	InteractorFacade grabHandSec;
	InteractableFacade climbPoint;

	public Transform playArea;
	public float throwMul;

	PlayerController rigController;

	Vector3 relHandPos;
	Vector3 relHandPosSec;

	bool climbing;

	bool allRefs { get { return grabHand && climbPoint && playArea; } }

	bool useOffset = true;
	bool useOffsetSec = true;

	private void Awake()
	{
		rigController = transform.parent.GetComponentInChildren<PlayerController>();
	}

	private void FixedUpdate()
	{
		if (allRefs && climbing)
			Climb(grabHandSec);//passing in bool of grabHandSec == null
	}

	public void LateUpdate()
	{
		if (allRefs && climbing)
			Climb(grabHandSec);//passing in bool of grabHandSec == null

	}


	void Climb(bool sec = false)
	{
		Vector3 calcedGrabPos;
		Vector3 relDir;

		if (sec)
		{
			calcedGrabPos =
				(grabHandSec.transform.position - (useOffsetSec ? offsetSec : Vector3.zero) +
				grabHand.transform.position - (useOffset ? offset : Vector3.zero)) / 2;

			relDir = climbPoint.transform.position - calcedGrabPos;
		}
		else
		{
			calcedGrabPos =
				(grabHand.transform.parent.position -
				(useOffset ? offset : Vector3.zero));

			relDir = climbPoint.transform.position - calcedGrabPos;
		}

		playArea.position += Quaternion.identity * relDir;


		relHandPos = grabHand.transform.position;

		if (grabHandSec)
			relHandPosSec = grabHandSec.transform.position;
	}


	public void StartClimbing(InteractableFacade climbObj, InteractorFacade hand, bool _useOffset, bool useOffsetSec)
	{
		if (!climbObj || !hand)
			return;

		if (grabHand && climbPoint && grabHandSec == null)
		{
			grabHandSec = hand;
			offsetSec = hand.transform.parent.position - climbPoint.transform.position;
			return;
		}

		grabHand = hand;
		climbPoint = climbObj;
		offset = hand.transform.position - climbPoint.transform.position;
		climbing = true;
		useOffset = _useOffset;
		if (rigController)
			rigController.SetClimb(true);
	}

	public void StopClimbing(InteractableFacade climbObj, InteractorFacade hand = null)
	{

		Vector3 handVel = hand ? hand.VelocityTracker.GetVelocity() : Vector3.zero;

		if (climbObj == climbPoint)
		{
			if (hand == grabHand)
			{
				if (grabHandSec != null)
				{
					grabHand = grabHandSec;
					offset = relHandPosSec - climbPoint.transform.position;
					grabHandSec = null;
					offsetSec = Vector3.zero;

					return;
				}
				else
				{
					grabHand = null;
					offset = Vector3.zero;
				}
			}
			else if (hand == grabHandSec)
			{
				if (grabHand != null)
					offset = relHandPos - climbPoint.transform.position;

				grabHandSec = null;

				return;
			}

			if (grabHand == null && grabHandSec == null)
			{
				climbPoint = null;
				climbing = false;
				if (rigController)
				{
					rigController.SetClimb(false);
					rigController.rb.velocity = handVel * -throwMul;
				}
			}

		}
	}

	public float GetThrowMultiplier() => throwMul;

	public void SetThrowMultiplier(float val)
	{
		//GetComponent<PlayerComponent>().UpdateClimb(val);
		throwMul = val;
	}

	Vector3 RemoveY(Vector3 input) => new Vector3(input.x, 0, input.y);

	public float GetAngleFromSides(float ss, float fs, float sf)
	{
		float result = (Mathf.Pow(fs, 2) + Mathf.Pow(sf, 2) - Mathf.Pow(ss, 2)) / (2 * fs * sf);
		result = Mathf.Acos(result) * Mathf.Rad2Deg;
		return result;
	}
}

//two handed turning 
//Vector3 newHandVector = grabHandSec.transform.position - grabHand.transform.position;

//float dot2D = Mathf.Sign(Vector2.Dot(new Vector2(relHandVector.z, relHandVector.x),
//													new Vector2(newHandVector.x, newHandVector.z)));

//float tempAngle = GetAngleBetweenHandsFromStart();
//tempAngle *= dot2D;

//if (lastAngle == 0)
//	lastAngle = tempAngle;


//text.text = (dot2D > 0 ? "" : "-") + tempAngle.ToString();

//playArea.eulerAngles = new Vector3(0, startAngle + ((dot2D > 0 ? 1 : -1) * tempAngle), 0);
//lastAngle = tempAngle;
//prevHandVector = newHandVector;