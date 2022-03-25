using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CubeGame : MonoBehaviour
{
    [Tooltip("Populate this array with context menu")]
    [SerializeField] List<GameObject> cubes;

    [ContextMenu("Find Cubes")]
    public void GetAllCubes(){

        cubes = GameObject.FindGameObjectsWithTag("Cube").ToList();
    }
    [ContextMenu("Clear Cubes")]
    public void ClearAllCubes(){

        cubes.Clear();
    }
}
