using UnityEngine;

public class InvertNormals : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;
        int[] triangles = mesh.triangles;

        // Keer alle normaalvectoren om
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        // Draai de volgorde van de driehoeken om
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 1];
            triangles[i + 1] = temp;
        }
        mesh.triangles = triangles;

        // Zet de culling van het materiaal uit zodat de binnenkant van de sphere zichtbaar is
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material material = meshRenderer.material;
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);  // Zet culling uit
        }
    }
}
