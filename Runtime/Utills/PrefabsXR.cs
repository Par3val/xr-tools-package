using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public struct PrefabsXR
{
  public static GameObject GetActionByEnum(InteractibleObject.SecondaryTypes actionType)
  {
    switch (actionType)
    {
      case InteractibleObject.SecondaryTypes.None:
        return (GameObject)Resources.Load("XR-Prefabs/Actions/None");
      case InteractibleObject.SecondaryTypes.Swap:
        return (GameObject)Resources.Load("XR-Prefabs/Actions/SwapAction");
      case InteractibleObject.SecondaryTypes.FollowDirection:
        return (GameObject)Resources.Load("XR-Prefabs/Actions/ControlDirectionAction");
      case InteractibleObject.SecondaryTypes.Scale:
        return (GameObject)Resources.Load("XR-Prefabs/Actions/ScaleAction");
    }

    return null;
  }

  public static GameObject GetDrive(DriveObject.DriveType driveType, DriveObject.InteractType interactType)
  {
    if (interactType == DriveObject.InteractType.Transfrom)
    {
      if (driveType == DriveObject.DriveType.Directional)
      {
        return (GameObject)Resources.Load("XR-Prefabs/Drive/DirectionalTransformDrive");
      }

      else if (driveType == DriveObject.DriveType.Rotational)
      {
        return (GameObject)Resources.Load("XR-Prefabs/Drive/RotationalTransformDrive");
      }
    }

    else if (interactType == DriveObject.InteractType.Joint)
    {
      if (driveType == DriveObject.DriveType.Directional)
      {
        return (GameObject)Resources.Load("XR-Prefabs/Drive/DirectionalJointDrive");
      }

      else if (driveType == DriveObject.DriveType.Rotational)
      {
        return (GameObject)Resources.Load("XR-Prefabs/Drive/RotationallJointDrive");
      }
    }

    return null;
  } 
}
