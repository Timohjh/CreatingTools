using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;

//created during Freya Holmér's tool dev course

public class ScatterTool : EditorWindow {

    [MenuItem("Tools/ScatterTool")]
    private static void ShowWindow() {
        var window = GetWindow<ScatterTool>();
        window.titleContent = new GUIContent("ScatterTool");
        window.Show();
    }
    List<GameObject> spawnPrefabs = new List<GameObject>();
    [Tooltip("Hold alt and scroll to change radius in the scene.")]
    public float radius = 2f;
    [Tooltip("Amount of points to spawn.")]
    public int spawnCount = 8;
    [Tooltip("Randomize new points after placing objects.")]
    public bool randomizeAfterPlacement = true;


    SerializedObject so;
    SerializedProperty propRadius;
    SerializedProperty propSpawnCount;
    SerializedProperty propSpawnPrefabs;
    SerializedProperty propRandomizeAfterPlacement;
    public struct RandomizingData
    {
        public Vector2 discPoints;
        public float ranAngle;
        public GameObject spawnPrefab;

        public void Randomize(List<GameObject> prefabs)
        {
            discPoints = Random.insideUnitCircle;
            ranAngle = Random.value * 360;
            spawnPrefab = prefabs.Count == 0 ? null : prefabs[Random.Range(0, prefabs.Count)];

        }
    }

    public class SpawnPoint
    {
        public RandomizingData randomizingData;
        public Vector3 position;
        public Quaternion rotation;
        public bool isValid = false;
        public Vector3 Up => rotation * Vector3.up;

        public SpawnPoint(Vector3 position, Quaternion rotation, RandomizingData randomizingData)
        {
            this.randomizingData = randomizingData;
            this.position = position;
            this.rotation = rotation;

            if(randomizingData.spawnPrefab != null)
            {
                SpawnablePrefab spawnablePrefab = randomizingData.spawnPrefab.GetComponent<SpawnablePrefab>();
                if (spawnablePrefab == null)
                {
                    isValid = true;
                }
                else
                {
                    float h = spawnablePrefab.height;
                    Ray ray = new Ray(position, Up);
                    isValid = Physics.Raycast(ray, h) == false;
                }
            }

        }
    }

    RandomizingData[] radPoints;
    GameObject[] prefabs;
    [SerializeField] bool[] prefabSelection;

    void OnEnable() {
        so = new SerializedObject(this);
        propRadius = so.FindProperty("radius");
        propSpawnCount = so.FindProperty("spawnCount");
        propRandomizeAfterPlacement = so.FindProperty("randomizeAfterPlacement");
        GetRandomPoints();
        SceneView.duringSceneGui += DuringSceneGUI;

        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/Prefabs/Environment" });
        IEnumerable<string> paths = guids.Select(AssetDatabase.GUIDToAssetPath);
        prefabs = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToArray();

        if (prefabSelection == null || prefabSelection.Length != prefabs.Length)
            prefabSelection = new bool[prefabs.Length];
    }
    void OnDisable() => SceneView.duringSceneGui -= DuringSceneGUI;

    //create random points inside the radius
    void GetRandomPoints(){
        radPoints = new RandomizingData[spawnCount];
        for(int i = 0; i < spawnCount; i++){
            radPoints[i].Randomize(spawnPrefabs);
        }
    }

    void OnGUI() {
        so.Update();
        //set min values
        EditorGUILayout.PropertyField(propRadius);
        propRadius.floatValue = Mathf.Max(0.1f, propRadius.floatValue);
        EditorGUILayout.PropertyField(propSpawnCount);
        propSpawnCount.intValue = Mathf.Max(1, propSpawnCount.intValue);
        EditorGUILayout.PropertyField(propRandomizeAfterPlacement);

        if ( so.ApplyModifiedProperties()){
            GetRandomPoints();
            SceneView.RepaintAll();
        }
        //UX
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
            GUI.FocusControl(null);
            Repaint();
    }
    void DrawSphere(Vector3 pos){
        Handles.SphereHandleCap(-1, pos, Quaternion.identity, 0.1f, EventType.Repaint);
    }

    void TrySpawnObjects(List<SpawnPoint> spawnPoints)
    {
        if (spawnPrefabs.Count == 0)
            return;

        foreach(SpawnPoint spawnPoint in spawnPoints)
        {
            GameObject spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(spawnPoint.randomizingData.spawnPrefab);
            Undo.RegisterCreatedObjectUndo(spawnedObject, "Spawn Object");
            spawnedObject.transform.position = spawnPoint.position;
            spawnedObject.transform.rotation = spawnPoint.rotation;
        }
        if(randomizeAfterPlacement)
            GetRandomPoints();
    }

    void DuringSceneGUI( SceneView sceneView ) {

        Handles.BeginGUI();
        Rect rect = new Rect(8, 8, 64, 64);
        for(int i = 0; i< prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            Texture icon = AssetPreview.GetAssetPreview(prefab);

            EditorGUI.BeginChangeCheck();
            prefabSelection[i] = GUI.Toggle(rect, prefabSelection[i], new GUIContent(icon));
            if (EditorGUI.EndChangeCheck())
            {
                spawnPrefabs.Clear();
                for (int j = 0; j < prefabs.Length; j++)
                {
                    if (prefabSelection[j])
                        spawnPrefabs.Add(prefabs[j]);

                }
                GetRandomPoints();
            }

            //spawnPrefabs = prefab;
            rect.y += rect.height + 2;
        }

        Handles.EndGUI();

        Handles.zTest = CompareFunction.LessEqual;
        Transform camTF = sceneView.camera.transform;
        
        if(Event.current.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }

        //change radius by scrolling
        bool pressingAlt = (Event.current.modifiers & EventModifiers.Alt) != 0;
        if (Event.current.type == EventType.ScrollWheel && pressingAlt){
            float scroll = Mathf.Sign(Event.current.delta.y);
            so.Update();
            propRadius.floatValue *= 1f + scroll * 0.05f;
            so.ApplyModifiedProperties();
            Repaint();
            Event.current.Use();
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        //Ray ray = new Ray(camTF.position, camTF.forward);
        //draw ray from mouse
        if (Physics.Raycast(ray, out RaycastHit hit )){
            Vector3 hitNormal = hit.normal;
            Vector3 hitTangent = Vector3.Cross(hitNormal, camTF.forward).normalized;
            Vector3 hitBitangent = Vector3.Cross(hitNormal, hitTangent);

            Ray GetTangentRay(Vector2 tangentPos)
            {
                Vector3 rayOrigin = hit.point + (hitTangent * tangentPos.x + hitBitangent * tangentPos.y) * radius;
                rayOrigin += hitNormal * 2; //offset
                Vector3 rayDir = -hitNormal;
                return new Ray(rayOrigin, rayDir);
            }

            List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

            //get points 
            foreach(RandomizingData rndPoint in radPoints){
                Ray pointRay = GetTangentRay(rndPoint.discPoints);

                //raycast to find point on surface
                if(Physics.Raycast(pointRay, out RaycastHit pHit)){
                    //assign rotation and position to pose
                    Quaternion randRot = Quaternion.Euler(0f, 0f, rndPoint.ranAngle);
                    Quaternion rot = Quaternion.LookRotation(pHit.normal) * (randRot * Quaternion.Euler(90f, 0f, 0f));

                    SpawnPoint spawnPoint = new SpawnPoint(pHit.point, rot, rndPoint);
                    spawnPoints.Add(spawnPoint);
                    
                    DrawSphere(pHit.point);
                    Handles.DrawAAPolyLine(pHit.point, pHit.point + pHit.normal);

                    if (spawnPoint.randomizingData.spawnPrefab != null && spawnPoint.isValid)
                    {
                        Matrix4x4 poseMtx = Matrix4x4.TRS(spawnPoint.position, spawnPoint.rotation, Vector3.one);
                        MeshFilter[] filters = spawnPoint.randomizingData.spawnPrefab.GetComponentsInChildren<MeshFilter>();
                        foreach (MeshFilter filter in filters)
                        {
                            Matrix4x4 localMtx = filter.transform.localToWorldMatrix;
                            Matrix4x4 objTf = poseMtx * localMtx;
                            Mesh mesh = filter.sharedMesh;
                            Material mat = filter.GetComponent<MeshRenderer>().sharedMaterial;
                            //mat.SetPass(0);
                            if(Event.current.type == EventType.Repaint)
                                Graphics.DrawMesh(mesh, objTf, mat, 0, sceneView.camera);
                        }
                    }

                }
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                TrySpawnObjects(spawnPoints);
            }

            Handles.color = Color.blue;
            Handles.DrawAAPolyLine(5f, hit.point, hit.point + hit.normal);
            //Handles.DrawWireDisc(hit.point, hit.normal, radius);
            Handles.color = Color.white;

            const int circleDetail = 64;
            Vector3[] points = new Vector3[circleDetail];
            const float Tau = 6.28318530718f;
            for (int i = 0; i < circleDetail; i++){
                float t = i / (float)circleDetail-1;
                float angle = t * Tau;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Ray cRay = GetTangentRay(dir);
                if(Physics.Raycast(cRay, out RaycastHit cHit)){
                    points[i] = cHit.point + cHit.normal * 0.02f;
                }
                else{
                    points[i] = cRay.origin;
                }
            }
            Handles.DrawAAPolyLine(points);
        }

        //Handles.DrawAAPolyLine(Vector3.zero, Vector3.one);
    }
}
