using Malimbe.MemberChangeMethod;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Interactions.Interactables.Grab.Receiver;
using Zinnia.Extension;

public class MyGrabInteractableReceiver : GrabInteractableReceiver
{
	public int _grabType;

	/// <summary>
	/// Configures the Grab Type to be used.
	/// </summary>
	public override void ConfigureGrabType()
	{
		switch ((ActiveType)_grabType)
		{
			case ActiveType.HoldTillRelease:
				StartStateGrab.TrySetActive(true);
				StopStateGrab.TrySetActive(true);
				ToggleGrab.TrySetActive(false);
				break;
			case ActiveType.Toggle:
				StartStateGrab.TrySetActive(false);
				StopStateGrab.TrySetActive(false);
				ToggleGrab.TrySetActive(true);
				break;
		}
	}

	protected override void OnEnable()
	{
		ConfigureGrabType();
	}

	/// <summary>
	/// Called after <see cref="GrabType"/> has been changed.
	/// </summary>
	[CalledAfterChangeOf(nameof(GrabType))]
	protected override void OnAfterGrabTypeChange()
	{
		ConfigureGrabType();
	}
}
