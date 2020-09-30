using System;
using UnityEngine;
using UnityEditor;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using Zinnia.Data.Type;
using UnityEngine.Events;

[Serializable]
public class InputMapping
{
  public string mapName = "New Map";
  public AxisType axisType;

  public InputTypeButton typeButton;
  public InputTypeAxis1D type1D;
  public InputTypeAxis2D type2D;

  public bool edtiorOpen = false;

  UnityAxis1DAction axis1D;
  UnityAxis2DAction axis2D;
  UnityButtonAction button;

  public FloatRange activationRange;


  [Tooltip("if not a 1d axis input will never be invokeed")]
  public UnityEventVoid Activated = new UnityEventVoid();
  [Tooltip("if not a 1d axis input will never be invokeed")]
  public UnityEventVoid Deactivated = new UnityEventVoid();

  public UnityEventButton UpdateBool = new UnityEventButton();
  public UnityEventFloat Update1D = new UnityEventFloat();
  [Tooltip("if not a 2d axis input will never be invokeed")]
  public UnityEventAxis2D Moved2D = new UnityEventAxis2D();
  public UnityEventFloat Moved2DX = new UnityEventFloat();
  public UnityEventFloat Moved2DY = new UnityEventFloat();


  #region UnityEvents

  [Serializable]
  public class UnityEventFloat : UnityEvent<float> { }
  [Serializable]
  public class UnityEventAxis2D : UnityEvent<Vector2> { }
  [Serializable]
  public class UnityEventButton : UnityEvent<bool> { }
  [Serializable]
  public class UnityEventVoid : UnityEvent { }

  #endregion

  #region AxisOptions

  [Serializable]
  public enum AxisType { Button, Axis1D, Axis2D }
  [Serializable]
  public enum InputTypeButton {ThumbTouch, ThumbClick }
  [Serializable]
  public enum InputTypeAxis1D { Trigger, Hand}
  [Serializable]
  public enum InputTypeAxis2D {Thumb2D}

  #endregion

  public InputMapping(string _name)
  {
    mapName = _name;
    activationRange = new FloatRange(.8f, 1);
  }

  public void Setup(InputManager.hands handedness, GameObject actionContainer)
  {
    switch (axisType)
    {
      case AxisType.Button:
        button = actionContainer.AddComponent<UnityButtonAction>();
        button.KeyCode = ButtonTranslation(handedness);
        button.ValueChanged.AddListener(ButtonUpdate);
        break;
      case AxisType.Axis1D:
        axis1D = actionContainer.AddComponent<UnityAxis1DAction>();
        axis1D.AxisName = Axis1DTranslation(handedness);
        axis1D.ValueChanged.AddListener(Axis1DUpdate);
        break;
      case AxisType.Axis2D:
        axis2D = actionContainer.AddComponent<UnityAxis2DAction>();
        axis2D = Axis2DTranslationVRTK(axis2D, handedness);
        axis2D.ValueChanged.AddListener(Axis2DUpdate);
        break;
    }
  }

  public void ButtonUpdate(bool data)
  {
    UpdateBool.Invoke(data);
    if (data)
      Activated.Invoke();
    else
      Deactivated.Invoke();
  }

  public void Axis1DUpdate(float data)
  {
    Update1D.Invoke(data);

    if (data >= activationRange.minimum && data <= activationRange.maximum)
      Activated.Invoke();
    else
      Deactivated.Invoke();

  }

  public void Axis2DUpdate(Vector2 data)
  {
    Moved2D.Invoke(data);
    Moved2DX.Invoke(data.x);
    Moved2DY.Invoke(data.y);
  }

  KeyCode ButtonTranslation(InputManager.hands handedness)
  {
    switch (typeButton)
    {
      case InputTypeButton.ThumbTouch:
        if (handedness == InputManager.hands.Right)
          return KeyCode.Joystick2Button17;
        return KeyCode.Joystick1Button16;
      case InputTypeButton.ThumbClick:
        if (handedness == InputManager.hands.Right)
          return KeyCode.Joystick2Button9;
        return KeyCode.Joystick1Button8;
      default:
        return KeyCode.Space;
    }
  }

  string Axis1DTranslation(InputManager.hands handedness)
  {
    switch (type1D)
    {
      case InputTypeAxis1D.Hand:
        if (handedness == InputManager.hands.Right)
          return "XRI_Right_Grip";
        return "XRI_Left_Grip";

      case InputTypeAxis1D.Trigger:
        if (handedness == InputManager.hands.Right)
          return "XRI_Right_Trigger";
        return "XRI_Left_Trigger";
      default:
        return "";
    }
  }

  UnityAxis2DAction Axis2DTranslationVRTK(UnityAxis2DAction axis, InputManager.hands handedness)
  {
    UnityAxis2DAction tempAxis = axis;

    if (handedness == InputManager.hands.Right)
    {
      axis.XAxisName = "XRI_Right_Primary2DAxis_Horizontal";
      axis.YAxisName = "XRI_Right_Primary2DAxis_Vertical";
    }
    else
    {
      axis.XAxisName = "XRI_Left_Primary2DAxis_Horizontal";
      axis.YAxisName = "XRI_Left_Primary2DAxis_Vertical";
    }
    return axis;
  }
}
