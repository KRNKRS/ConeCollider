using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class ConeCollider : MonoBehaviour {
    [SerializeField, Range(0.01f,88.5f)]
    private float angle = 45;
    [SerializeField]
    private float distance = 1;
    [SerializeField]
    private bool isTrigger;
    private MeshCollider meshCollider;
    private GameObject cone;
    private Mesh mesh;
    private Vector3[] initVertices;
    private int[] initTriangles;

    void Awake()
    {
        //var initPos = this.transform.position;
        var initRot = this.transform.rotation;
        //this.transform.position = Vector3.zero;
        this.transform.rotation = new Quaternion(0, 0, 0, 0);

        //リソースロード
        cone = Resources.Load("Prefab/ConeCollider") as GameObject;
        cone.transform.position = this.transform.position;
        cone.transform.rotation = this.transform.rotation;
        cone.transform.localScale = this.transform.localScale;

        //メッシュ情報作成
        var coneMesh = cone.GetComponent<MeshFilter>().sharedMesh;
        var vertices = coneMesh.vertices;
        var triangles = coneMesh.triangles;
        distance = Mathf.Max(0.1f, Mathf.Abs(distance));

        var count = 0;
        var centerForwardPos = this.transform.position + this.transform.TransformDirection(Vector3.forward) * distance;
        var harf = distance * Mathf.Tan(angle * Mathf.PI / 180f);
        for (int i = 0; i < vertices.Length; i++)
        {
            var verticeWorldPos = vertices[i] + this.transform.position;
            if (count != 2 || i >= 36)
            {
                verticeWorldPos += this.transform.TransformDirection(Vector3.forward) * (distance - 1);
                
                var outPos = centerForwardPos + (verticeWorldPos - centerForwardPos).normalized * harf;
                
                vertices[i] = outPos - this.transform.position;
                count++;
            }
            else
            {
                count = 0;
            }
        }

        //新規メッシュ作成
        mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        meshCollider = this.gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
        meshCollider.isTrigger = isTrigger;
        meshCollider.sharedMesh.RecalculateBounds();
        meshCollider.sharedMesh.RecalculateNormals();
        meshCollider.hideFlags = HideFlags.HideInInspector;
        //this.transform.position = initPos;
        this.transform.rotation = initRot;
    }

    GameObject DebugObject(Vector3 pos, float scale)
    {
        //デバッグ
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.transform.position = pos;
        obj.transform.localScale = new Vector3(scale, scale, scale);
        return obj;
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(ConeCollider))]
[CanEditMultipleObjects]
public class ConeColliderEditor : Editor
{
    private SerializedProperty angle;
    private SerializedProperty distance;
    private SerializedProperty isTrigger;
    private ConeCollider t;

    void OnEnable()
    {
        SetProperty(ref angle, "angle");
        SetProperty(ref distance, "distance");
        SetProperty(ref isTrigger, "isTrigger");
        t = target as ConeCollider;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        {
            DrawPropertyField(angle, "Angle");
            DrawPropertyField(distance, "Distance");
            DrawPropertyField(isTrigger, "isTrigger");
        }
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (!EditorApplication.isPlaying)
        {
            var centerForward = t.transform.position + t.transform.TransformDirection(Vector3.forward) * distance.floatValue;
            var harf = distance.floatValue * Mathf.Tan(angle.floatValue * Mathf.PI / 180f);
            var up = centerForward + t.transform.TransformDirection(Vector3.up) * harf;
            var down = centerForward + t.transform.TransformDirection(Vector3.down) * harf;
            var right = centerForward + t.transform.TransformDirection(Vector3.right) * harf;
            var left = centerForward + t.transform.TransformDirection(Vector3.left) * harf;
            Handles.color = new Color(0.53f, 0.82f, 0.5f);
            Handles.DrawLine(t.transform.position, up);
            Handles.DrawLine(t.transform.position, down);
            Handles.DrawLine(t.transform.position, right);
            Handles.DrawLine(t.transform.position, left);
            Handles.CircleCap(0, centerForward, t.transform.rotation, harf);
            Handles.color = Color.white;
        }
    }

    void SetProperty(ref SerializedProperty property, string name)
    {
        property = serializedObject.FindProperty(name);
    }

    void DrawPropertyField(SerializedProperty property, string name)
    {
        EditorGUILayout.PropertyField(property, new GUIContent(name));
    }
}

#endif