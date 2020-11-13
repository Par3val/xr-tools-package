using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct MyEditorTools
{
#if UNITY_EDITOR
  public static GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
  public static GUILayoutOption miniFeildWidth = GUILayout.Width(50f);

  public static void ShowRefrenceButton(GameObject _refrence)
  {
    if (GUILayout.Button("ref", EditorStyles.miniButtonLeft, GUILayout.Width(25f)))
    {
      Selection.activeObject = _refrence;
      SceneView.FrameLastActiveSceneView();
    }
  }

  public static void FocusObject(GameObject _refrence, bool updateSceneAngle = false)
  {
    Selection.activeObject = _refrence;
    if (updateSceneAngle)
      SceneView.FrameLastActiveSceneView();
  }

  public static void BeginHorizontal()
  {
    EditorGUILayout.BeginHorizontal();
  }

  public static void EndHorizontal()
  {
    EditorGUILayout.EndHorizontal();
  }
#endif
}
