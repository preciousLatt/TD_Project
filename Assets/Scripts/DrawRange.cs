using UnityEngine;

[RequireComponent(typeof(Tower))]
public class DrawRange : MonoBehaviour
{
    [SerializeField] private Color rangeColor = new Color(1f, 1f, 0f, 0.25f);
    [SerializeField] private Material groundOnlyMaterial;

    private GameObject visualInstance;
    private Tower tower;

    private void Awake()
    {
        tower = GetComponent<Tower>();
        CreateVisual();
        HideRange();
    }

    private void CreateVisual()
    {

            var go = new GameObject("RangeCircle");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f,0.001f,0f);
            go.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

            var mr = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();
            mf.mesh = BuildCircleMesh(1f);

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetInt("_Surface", 1); 
            mat.SetInt("_Blend", 0); 
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0); 
            mat.renderQueue = 3000;
            mat.color = rangeColor; 
            mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            mr.material = mat;
            visualInstance = go;

        SetRangeRadius(tower.attackRange);
    }

    public void SetRangeRadius(float radius)
    {
        if (visualInstance != null)
            visualInstance.transform.localScale = new Vector3(radius * 2f, 1f, radius * 2f);
    }

    public void ShowRange() => visualInstance?.SetActive(true);
    public void HideRange() => visualInstance?.SetActive(false);

    private Mesh BuildCircleMesh(float radius, int segments = 64)
    {
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[segments + 1];
        int[] tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            int a = 0;
            int b = i + 1;
            int c = (i + 2 > segments) ? 1 : i + 2;
            tris[i * 3 + 0] = a;
            tris[i * 3 + 1] = b;
            tris[i * 3 + 2] = c;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }
}
