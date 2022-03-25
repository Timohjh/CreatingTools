using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class Grouping : Editor
{
    [MenuItem("Edit/Group %g", false)]
    public static void Group()
    {
        if (Selection.transforms.Length > 0)
        {
            GameObject group = new GameObject("New Group");

            Vector3 pivotPosition = Vector3.zero;
            foreach (Transform g in Selection.transforms)
            {
                pivotPosition += g.transform.position;
            }
            pivotPosition /= Selection.transforms.Length;
            group.transform.position = pivotPosition;

            Undo.RegisterCreatedObjectUndo(group, "Group");
            foreach (GameObject s in Selection.gameObjects)
            {
                Undo.SetTransformParent(s.transform, group.transform, "Group");
            }

            Selection.activeGameObject = group;
        }
        else
        {
            Debug.LogWarning("Nothing selected for a group.");
        }
    }
}
