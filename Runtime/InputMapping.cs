using UnityEngine;
using UnityEditor;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using Zinnia.Data.Type;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[System.Serializable, ExecuteInEditMode]
public class InputMapping : MonoBehaviour
{
	[SerializeField]
	public string mapName = "New Map";
	public AxisType axisType;

	public InputManager manager;

	public InputTypeButton typeButton;
	public InputTypeAxis1D type1D;
	public InputTypeAxis2D type2D;

	public bool editorOpen;
	public bool interacted;

	bool isLeft(InputManager.hands handedness) => handedness == InputManager.hands.Left;
	[SerializeField]
	bool prevState;

	UnityAxis1DAction axis1D;
	UnityAxis2DAction axis2D;
	UnityButtonAction button;

	public Vector2 activationRange;


	[Tooltip("if not a 1d axis input will never be invokeed")]
	public UnityEventVoid Activated;
	[Tooltip("if not a 1d axis input will never be invokeed")]
	public UnityEventVoid Deactivated;

	public UnityEventButton UpdateBool;
	public UnityEventFloat Update1D;
	[Tooltip("if not a 2d axis input will never be invokeed")]
	public UnityEventAxis2D Moved2D;
	public UnityEventFloat Moved2DX;
	public UnityEventFloat Moved2DY;


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

	[System.Serializable]
	public enum AxisType { Button, Axis1D, Axis2D }
	[System.Serializable]
	public enum InputTypeButton
	{
		ThumbClick, ThumbTouch,
		TopClick, TopTouch,
		BottomClick, BottomTouch,
		Menu
	}
	[System.Serializable]
	public enum InputTypeAxis1D { Trigger, Hand }
	[System.Serializable]
	public enum InputTypeAxis2D { Thumb2D }

	#endregion

	public void Setup(string _name = "")
	{
		if (!string.IsNullOrEmpty(_name))
			mapName = _name;

		editorOpen = false;
		prevState = false;

		button = null;
		axis1D = null;
		axis2D = null;

		axisType = AxisType.Button;

		typeButton = InputTypeButton.ThumbClick;
		type1D = InputTypeAxis1D.Trigger;
		type2D = InputTypeAxis2D.Thumb2D;

		Activated = new UnityEventVoid();
		Deactivated = new UnityEventVoid();

		UpdateBool = new UnityEventButton();
		Update1D = new UnityEventFloat();

		Moved2D = new UnityEventAxis2D();
		Moved2DX = new UnityEventFloat();
		Moved2DY = new UnityEventFloat();
	}

	public void Awake()
	{
		manager = GetComponentInParent<InputManager>();
		if (manager && Application.isPlaying)
			switch (axisType)
			{
				case AxisType.Button:
					button = gameObject.AddComponent<UnityButtonAction>();
					button.KeyCode = ButtonTranslation(manager.handedness);
					button.ValueChanged.AddListener(ButtonUpdate);
					break;
				case AxisType.Axis1D:
					axis1D = gameObject.AddComponent<UnityAxis1DAction>();
					axis1D.AxisName = Axis1DTranslation(manager.handedness);
					axis1D.ValueChanged.AddListener(Axis1DUpdate);
					break;
				case AxisType.Axis2D:
					axis2D = gameObject.AddComponent<UnityAxis2DAction>();
					axis2D = Axis2DTranslationVRTK(axis2D, manager.handedness);
					axis2D.ValueChanged.AddListener(Axis2DUpdate);
					break;
			}
		activationRange = new Vector2(.8f, 1);
	}

	public void ButtonUpdate(bool data)
	{
		UpdateBool.Invoke(data);
		if (prevState != data)
		{
			if (data)
				Activated.Invoke();
			else
				Deactivated.Invoke();
		}

		prevState = data;
	}

	public void Axis1DUpdate(float data)
	{
		Update1D.Invoke(data);
		bool active = data >= activationRange.x && data <= activationRange.y;

		if (active != prevState)
		{
			if (active)
				Activated.Invoke();
			else
				Deactivated.Invoke();
		}

		prevState = active;
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
				return isLeft(handedness) ? KeyCode.JoystickButton16 : KeyCode.JoystickButton17;
			case InputTypeButton.ThumbClick:
				return isLeft(handedness) ? KeyCode.JoystickButton8 : KeyCode.JoystickButton9;
			case InputTypeButton.TopClick:
				return isLeft(handedness) ? KeyCode.JoystickButton3 : KeyCode.JoystickButton1;
			case InputTypeButton.TopTouch:
				return isLeft(handedness) ? KeyCode.JoystickButton13 : KeyCode.JoystickButton11;
			case InputTypeButton.BottomClick:
				return isLeft(handedness) ? KeyCode.JoystickButton2 : KeyCode.JoystickButton0;
			case InputTypeButton.BottomTouch:
				return isLeft(handedness) ? KeyCode.JoystickButton12 : KeyCode.JoystickButton10;
			case InputTypeButton.Menu:
				return isLeft(handedness) ? KeyCode.JoystickButton6 : KeyCode.JoystickButton6;
			default:
				return KeyCode.Space;
		}
	}

	//Oculus
	/*
   * Click 8 T:16 : R:9 T:17
   * 
   * Top Button 3 T:13 ; R:1 T:11
   * Bottom Buttom 2 T:12; R:0 T:10
   * 
   * Menu: 6
   * 
   * */

	string Axis1DTranslation(InputManager.hands handedness)
	{
		switch (type1D)
		{
			case InputTypeAxis1D.Hand:
				return isLeft(handedness) ? "XRI_Left_Grip" : "XRI_Right_Grip";
			case InputTypeAxis1D.Trigger:
				return isLeft(handedness) ? "XRI_Left_Trigger" : "XRI_Right_Trigger";
			default:
				return "";
		}
	}

	UnityAxis2DAction Axis2DTranslationVRTK(UnityAxis2DAction axis, InputManager.hands handedness)
	{
		UnityAxis2DAction tempAxis = axis;

		axis.XAxisName = isLeft(handedness) ? "XRI_Left_Primary2DAxis_Horizontal" : "XRI_Right_Primary2DAxis_Horizontal";
		axis.YAxisName = isLeft(handedness) ? "XRI_Left_Primary2DAxis_Vertical" : "XRI_Right_Primary2DAxis_Vertical";

		return axis;
	}

}

//public SerializableMap Serialize()
//{
//	int _ref = 0;

//	switch (axisType)
//	{
//		case AxisType.Button:
//			_ref = (int)typeButton;
//			break;
//		case AxisType.Axis1D:
//			_ref = (int)type1D;
//			break;
//		case AxisType.Axis2D:
//			_ref = (int)type2D;
//			break;
//	}

//	return new SerializableMap(mapName, (int)axisType, _ref, new Vector2(activationRange.x, activationRange.y));
//}

//public void DeSerialize(SerializableMap serializedMap)
//{
//	mapName = serializedMap.name;
//	axisType = (AxisType)serializedMap.axis;

//	switch (axisType)
//	{
//		case AxisType.Button:
//			typeButton = (InputTypeButton)serializedMap.refrence;
//			break;
//		case AxisType.Axis1D:
//			type1D = (InputTypeAxis1D)serializedMap.refrence;
//			break;
//		case AxisType.Axis2D:
//			type2D = (InputTypeAxis2D)serializedMap.refrence;
//			break;
//	}
//}

//public void OnBeforeSerialize()
//{
//	serialized = Serialize();
//	//Debug.Log(serialized.name + " Serialized");
//}


//public void OnAfterDeserialize()
//{
//	DeSerialize(serialized);
//	//Debug.Log(serialized.name + " DeSerialized");
//}

//[System.Serializable]
//public class SerializableMap
//{
//	public string name;
//	public int axis;
//	public int refrence;
//	public Vector2 activationRange;

//	public SerializableMap(string _name, int _axis, int _ref, Vector2 _range)
//	{
//		name = _name;
//		axis = _axis;
//		refrence = _ref;
//		activationRange = _range;
//	}
//}