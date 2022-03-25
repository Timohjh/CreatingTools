using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Moves gameobject in the middle of all the boxes to and sraws a line to them
public class BoxManager : MonoBehaviour
{
    public static List<GameObject> boxes = new List<GameObject>();

#if UNITY_EDITOR
    void Update(){
        gameObject.transform.position = GetCenterPoint();
    }
    //get center point of all boxes
    Vector3 GetCenterPoint(){
        if (boxes.Count == 1)
            return boxes[0].transform.position;
        
        var bounds = new Bounds(boxes[0].transform.position, Vector3.zero);
        foreach(GameObject go in boxes)
            bounds.Encapsulate(go.transform.position);
        
        return bounds.center;
    }
    //draw line to each box
    void OnDrawGizmosSelected(){
        foreach(GameObject go in boxes){
            Gizmos.DrawLine(transform.position, go.transform.position);
        }
    }
#endif
}
