using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables.Grab;
using VRTK.Prefabs.Interactions.Interactables.Grab.Action;

public class MyGrabInteractableConfigurator : GrabInteractableConfigurator
{
	public GrabInteractableAction _secondaryAction;

	protected override void OnEnable()
	{
		if (_secondaryAction)
			SecondaryAction = _secondaryAction;
		base.OnEnable();
	}
}
