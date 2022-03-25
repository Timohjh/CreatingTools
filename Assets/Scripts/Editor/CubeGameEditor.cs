using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeGame))]
public class CubeGameEditor : Editor
{
    [Tooltip("Populate this array with context menu")]
    SerializedProperty cubes;

    void OnEnable()
    {
        cubes = serializedObject.FindProperty("cubes");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CubeGame cg = (CubeGame)target;

        if (GUILayout.Button("Find cubes"))
        {
            cg.GetAllCubes();
        }
        if (GUILayout.Button("Clear cubes"))
        {
            cg.ClearAllCubes();
        }

    }

}
