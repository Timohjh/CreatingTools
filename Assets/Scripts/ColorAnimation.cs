using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//if player is inside the radius of the object when animationcurve is at a certain value => do something
public class ColorAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField, Range (0f,1f)] float value = 0f;
    [SerializeField, Range (0f,1f)] float speed = 0f;
    [SerializeField] AnimationCurve animCurve;
    MaterialPropertyBlock mpb;
    public MaterialPropertyBlock Mpb {
        get{
            if (mpb == null)
                mpb = new MaterialPropertyBlock();
            return mpb;}

    }
    static readonly int mpbColor = Shader.PropertyToID("_BaseColor");
    Renderer _rend;
    [Header("Distance")]
    [SerializeField] Transform player;
    [SerializeField, Range (0f,5f)] float multiplier = 0f;
    float _radius {
        get {
            return value * multiplier;
            }
    }
    void Start()
    {
        _rend = gameObject.GetComponent<Renderer>();

        //create new material
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(shader);
        mat.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
    {
        // do animation based on the curve
        float time = Time.time * speed;
        float tEval = time - Mathf.Floor(Time.time);
        value = animCurve.Evaluate(tEval);
        GetNewColor();

        // check distance to player
        Vector3 relativePos = transform.position - player.position;
        float distance = (relativePos).magnitude;

        if(distance < _radius){
            Debug.Log("Player is inside");
        }
    }

    //add this object to list in boxmanager
    void OnEnable() => BoxManager.boxes.Add(gameObject);
    void OnDisable() => BoxManager.boxes.Remove(gameObject);

    // Get color based on position on the curve 
    void GetNewColor (){
        //Color32 color = new Color32((byte)value, 0,0,1);
        Mpb.SetColor(mpbColor, new Color(value, 0f,0f,1f));
        _rend.SetPropertyBlock(Mpb);
    }

    // debug
    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
