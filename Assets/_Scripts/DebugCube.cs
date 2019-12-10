using UnityEngine;

namespace MrPuppet
{
    public class DebugCube : MonoBehaviour
    {
        private void Start()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Color[] colors = new Color[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = new Color(Mathf.Abs(normals[i].x), Mathf.Abs(normals[i].y), Mathf.Abs(normals[i].z));

                if (normals[i].x < 0f) colors[i] -= new Color(0.6f, 0f, 0f);
                if (normals[i].y < 0f) colors[i] -= new Color(0f, 0.6f, 0f);
                if (normals[i].z < 0f) colors[i] -= new Color(0f, 0f, 0.6f);
            }

            mesh.colors = colors;
        }
    }
}
