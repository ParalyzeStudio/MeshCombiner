using UnityEngine;

/**
 * This class holds data from a mesh's submesh with its related material
 * **/
public class SingleMaterialMesh
{
    public Material m_material { get; set; }
    public Mesh m_mesh { get; set; }
    public Matrix4x4 m_transformMatrix { get; set; }

    public SingleMaterialMesh(Mesh mesh, Material material, Matrix4x4 transformMatrix)
    {
        m_mesh = mesh;
        m_material = material;
        m_transformMatrix = transformMatrix;
    }
}