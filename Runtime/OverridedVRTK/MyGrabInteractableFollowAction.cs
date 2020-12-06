namespace VRTK.Prefabs.Interactions.Interactables.Grab.Action
{
  using UnityEngine;
  using Malimbe.MemberChangeMethod;
  using Malimbe.XmlDocumentationAttribute;
  using Malimbe.PropertySerializationAttribute;
  using Zinnia.Extension;
  using Zinnia.Data.Attribute;
  using Zinnia.Tracking.Velocity;
  using Zinnia.Tracking.Follow;
  using Zinnia.Tracking.Follow.Modifier;

  [System.Serializable]
  /// <summary>
  /// Describes an action that allows the Interactable to follow an Interactor's position, rotation and scale.
  /// </summary>
  public class MyGrabInteractableFollowAction : GrabInteractableFollowAction
  {
    public int _followTracking;
    public int _grabOffset;

  //  private void Start()
		//{
  //    FollowTracking = followTracking;
  //    GrabOffset = grabOffset;
		//}

    /// <summary>
    /// Configures the appropriate processes to be used for follow tracking based on the <see cref="FollowTracking"/> setting.
    /// </summary>
    protected override void ConfigureFollowTracking()
    {
      if (WillInheritIsKinematicWhenInactiveFromConsumerRigidbody)
      {
        IsKinematicWhenInactive = GrabSetup != null ? GrabSetup.Facade.ConsumerRigidbody.isKinematic : false;
      }
      switch ((TrackingType)_followTracking)
      {
        case TrackingType.FollowTransform:
          FollowTransformModifier.gameObject.SetActive(true);
          FollowRigidbodyModifier.gameObject.SetActive(false);
          FollowRigidbodyForceRotateModifier.gameObject.SetActive(false);
          FollowTransformRotateOnPositionDifferenceModifier.gameObject.SetActive(false);
          ObjectFollower.FollowModifier = FollowTransformModifier;
          IsKinematicWhenActive = true;
          break;
        case TrackingType.FollowRigidbody:
          FollowTransformModifier.gameObject.SetActive(false);
          FollowRigidbodyModifier.gameObject.SetActive(true);
          FollowRigidbodyForceRotateModifier.gameObject.SetActive(false);
          FollowTransformRotateOnPositionDifferenceModifier.gameObject.SetActive(false);
          ObjectFollower.FollowModifier = FollowRigidbodyModifier;
          IsKinematicWhenActive = false;
          break;
        case TrackingType.FollowRigidbodyForceRotate:
          FollowTransformModifier.gameObject.SetActive(false);
          FollowRigidbodyModifier.gameObject.SetActive(false);
          FollowRigidbodyForceRotateModifier.gameObject.SetActive(true);
          FollowTransformRotateOnPositionDifferenceModifier.gameObject.SetActive(false);
          ObjectFollower.FollowModifier = FollowRigidbodyForceRotateModifier;
          IsKinematicWhenActive = false;
          break;
        case TrackingType.FollowTransformPositionDifferenceRotate:
          FollowTransformModifier.gameObject.SetActive(false);
          FollowRigidbodyModifier.gameObject.SetActive(false);
          FollowRigidbodyForceRotateModifier.gameObject.SetActive(false);
          FollowTransformRotateOnPositionDifferenceModifier.gameObject.SetActive(true);
          ObjectFollower.FollowModifier = FollowTransformRotateOnPositionDifferenceModifier;
          IsKinematicWhenActive = true;
          break;
      }
    }

    /// <summary>
    /// Configures the appropriate processes to be used for grab offset based on the <see cref="GrabOffset"/> setting.
    /// </summary>
    protected override void ConfigureGrabOffset()
    {
      switch ((OffsetType)_grabOffset)
      {
        case OffsetType.None:
          PrecisionLogicContainer.TrySetActive(false);
          OrientationLogicContainer.TrySetActive(false);
          break;
        case OffsetType.PrecisionPoint:
          PrecisionLogicContainer.TrySetActive(true);
          PrecisionCreateContainer.TrySetActive(true);
          PrecisionForceCreateContainer.TrySetActive(false);
          OrientationLogicContainer.TrySetActive(false);
          break;
        case OffsetType.ForcePrecisionPoint:
          PrecisionLogicContainer.TrySetActive(true);
          PrecisionForceCreateContainer.TrySetActive(true);
          PrecisionCreateContainer.TrySetActive(false);
          OrientationLogicContainer.TrySetActive(false);
          break;
        case OffsetType.OrientationHandle:
          PrecisionLogicContainer.TrySetActive(false);
          OrientationLogicContainer.TrySetActive(true);
          break;
      }
    }


    protected override void OnEnable()
    {
      ConfigureFollowTracking();
      ConfigureGrabOffset();
    }

    /// <inheritdoc />
    protected override void OnAfterGrabSetupChange()
    {
      ObjectFollower.Targets.RunWhenActiveAndEnabled(() => ObjectFollower.Targets.Clear());
      ObjectFollower.Targets.RunWhenActiveAndEnabled(() => ObjectFollower.Targets.Add(GrabSetup.Facade.ConsumerContainer));
      VelocityApplier.Target = GrabSetup.Facade.ConsumerRigidbody != null ? GrabSetup.Facade.ConsumerRigidbody : null;
      ConfigureFollowTracking();
    }

    /// <summary>
    /// Called after <see cref="FollowTracking"/> has been changed.
    /// </summary>
    [CalledAfterChangeOf(nameof(FollowTracking))]
    protected override void OnAfterFollowTrackingChange()
    {
      ConfigureFollowTracking();
    }

    /// <summary>
    /// Called after <see cref="GrabOffset"/> has been changed.
    /// </summary>
    [CalledAfterChangeOf(nameof(GrabOffset))]
    protected override void OnAfterGrabOffsetChange()
    {
      ConfigureGrabOffset();
    }
  }
}