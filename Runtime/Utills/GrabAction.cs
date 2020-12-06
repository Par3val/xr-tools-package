using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactors;
using Zinnia.Action;
using static Zinnia.Cast.PointsCast;
using static Zinnia.Tracking.Collision.Active.ActiveCollisionPublisher;

public class GrabAction : BooleanAction
{
	Collider last;
	bool lastValue;

	InteractorFacade hand;

	private void Awake()
	{
		hand = GetComponent<InteractorFacade>();
	}

	public void GetTouch(PayloadData data)
	{
		if (hand.GrabbedObjects.Count > 0)
			return;

		if (data.ActiveCollisions != null && data.ActiveCollisions.Count > 0)
		{
			if (/*data.ActiveCollisions[0].ColliderData == last || */!Value)
				return;

			var tempInteractible = data.ActiveCollisions[0].ColliderData.GetComponentInParent<InteractableFacade>();
			if (tempInteractible == null)
				return;

			hand.Grab(tempInteractible);

			lastValue = Value;
			//last = data.ActiveCollisions[0].ColliderData;
		}

	}
}
