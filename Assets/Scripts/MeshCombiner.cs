﻿using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    public List<GameObject> m_objectsToCombine;

    /**
     * Combine all meshes that are children of this object
     * **/
    public void CombineChildrenMeshes()
    {
        AddChidrenMeshesToList();
        CombineGameObjects();
    }

    /**
     * Add all children meshes to the list of objects to combine
     * **/
    public void AddChidrenMeshesToList()
    {
        MeshFilter[] childrenMeshFilters = this.gameObject.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i != childrenMeshFilters.Length; i++)
        {
            m_objectsToCombine.Add(childrenMeshFilters[i].gameObject);
        }
    }

    /**
     * Sort listed meshes by material
     * **/
    public Dictionary<Material, List<SubMesh>> SortMeshesByMaterial()
    {
        Dictionary<Material, List<SubMesh>> meshesByMaterial = new Dictionary<Material, List<SubMesh>>();

        //sort meshes by material
        for (int i = 0; i != m_objectsToCombine.Count; i++)
        {
            MeshFilter meshFilter = m_objectsToCombine[i].GetComponent<MeshFilter>();
            List<SubMesh> submeshes = ExplodeMeshSubmeshes(meshFilter);

            for (int p = 0; p != submeshes.Count; p++)
            {
                Material submeshMaterial = submeshes[p].m_material;
                List<SubMesh> listForMaterial;
                meshesByMaterial.TryGetValue(submeshes[p].m_material, out listForMaterial);

                if (listForMaterial == null)
                {
                    listForMaterial = new List<SubMesh>();
                    listForMaterial.Add(submeshes[p]);
                    meshesByMaterial.Add(submeshMaterial, listForMaterial);
                }
                else
                    listForMaterial.Add(submeshes[p]);
            }
        }

        return meshesByMaterial;
    }

    /**
    * Takes a dictionary of meshes sorted by material and sort them again by shader
    * **/
    private Dictionary<Shader, Dictionary<Material, List<SubMesh>>> SortMeshesByShader(Dictionary<Material, List<SubMesh>> meshesByMaterial)
    {
        Dictionary<Shader, Dictionary<Material, List<SubMesh>>>  meshesByShader = new Dictionary<Shader, Dictionary<Material, List<SubMesh>>>();

        foreach (KeyValuePair<Material, List<SubMesh>> kvp in meshesByMaterial)
        {
            Material material = kvp.Key;
            List<SubMesh> submeshes = kvp.Value;
            Shader shader = material.shader;

            Dictionary<Material, List<SubMesh>> meshesForShader;
            if (!meshesByShader.TryGetValue(shader, out meshesForShader))
            {
                Dictionary<Material, List<SubMesh>> meshesForMaterial = new Dictionary<Material, List<SubMesh>>();
                meshesForMaterial.Add(material, submeshes);
                meshesByShader.Add(shader, meshesForMaterial);
            }
            else
                meshesForShader.Add(material, submeshes);
        }

        return meshesByShader;
    }
    
    /**
     * Take the objects list and combine all its meshes into one
     * **/
    public void CombineGameObjects()
    {
        if (m_objectsToCombine == null || m_objectsToCombine.Count < 2)
            return;

        Dictionary<Material, List<SubMesh>> meshesByMaterial = SortMeshesByMaterial();

        //combine meshes for each material
        int materialCount = meshesByMaterial.Count;
        Material[] combinedMaterials = new Material[materialCount];
        List<Mesh> combinedMeshesByMaterial = new List<Mesh>(materialCount);
        int m = 0;
        foreach (KeyValuePair<Material, List<SubMesh>> kvp in meshesByMaterial)
        {
            combinedMaterials[m] = kvp.Key;
            List<SubMesh> meshesToCombine = kvp.Value;
            CombineInstance[] combine = new CombineInstance[meshesToCombine.Count];

            int p = 0;
            while (p < meshesToCombine.Count)
            {
                combine[p].mesh = meshesToCombine[p].m_mesh;
                combine[p].transform = meshesToCombine[p].m_transformMatrix;
                p++;
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combine);
            combinedMeshesByMaterial.Add(combinedMesh);

            m++;
        }


        //finally combine all those meshes in one single big mesh
        CombineInstance[] combineFinal = new CombineInstance[combinedMeshesByMaterial.Count];
        int q = 0;
        while (q < combinedMeshesByMaterial.Count)
        {
            combineFinal[q].mesh = combinedMeshesByMaterial[q];
            combineFinal[q].transform = Matrix4x4.identity;
            q++;
        }
        Mesh finalMesh = new Mesh();
        finalMesh.name = "FinalMesh";
        finalMesh.CombineMeshes(combineFinal, false);
        finalMesh.RecalculateBounds();
        finalMesh.RecalculateNormals();

        //build a game object for this mesh
        BuildCombinedMeshObject(finalMesh, combinedMaterials);
    }

    /**
     * Separate all submeshes from one mesh as we want to sort meshes by material and 1 submesh = 1 material. 
     * If the mesh contains only one submesh just return null so we know we can handle it directly without any more work on it
     * **/
    private List<SubMesh> ExplodeMeshSubmeshes(MeshFilter originalMeshFilter)
    {
        Mesh originalMesh = originalMeshFilter.sharedMesh;
        Material[] originalMaterials = originalMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;
        List<SubMesh> outputMeshes = new List<SubMesh>();

        if (originalMesh.subMeshCount == 1)
        {
            outputMeshes.Add(new SubMesh(originalMesh, originalMaterials[0], originalMeshFilter.transform.localToWorldMatrix));
            return outputMeshes;
        }

        //Build one SingleMaterialMesh for every submesh
        for (int i = 0; i != originalMesh.subMeshCount; i++)
        {
            //First get the triangles, colors and uvs arrays from the submesh
            int[] submeshOriginalTriangles = originalMesh.GetTriangles(i);
            HashSet<int> trianglesHashset = new HashSet<int>();
            for (int p = 0; p != submeshOriginalTriangles.Length; p++)
            {
                trianglesHashset.Add(submeshOriginalTriangles[p]);
            }
            int[] sortedTriangles = new int[trianglesHashset.Count];
            trianglesHashset.CopyTo(sortedTriangles);

            int minIndex = sortedTriangles[0];

            //build the list of vertices, colors and UVs for this submesh from the original mesh
            Vector3[] originalVertices = originalMesh.vertices;
            Color[] originalMeshColors = originalMesh.colors.Length > 0 ? originalMesh.colors : null;
            Color32[] originalMeshColors32 = originalMesh.colors32.Length > 0 ? originalMesh.colors32 : null;
            Vector2[] originalMeshUV = originalMesh.uv.Length > 0 ? originalMesh.uv : null;
            //Vector2[] originalMeshUV2 = originalMesh.uv2.Length > 0 ? originalMesh.uv2 : null;
            //Vector2[] originalMeshUV3 = originalMesh.uv3.Length > 0 ? originalMesh.uv3 : null;
            //Vector2[] originalMeshUV4 = originalMesh.uv4.Length > 0 ? originalMesh.uv4 : null;

            int submeshVertexCount = sortedTriangles.Length;
            Vector3[] submeshVertices = new Vector3[submeshVertexCount];
            int[] submeshTriangles = new int[submeshOriginalTriangles.Length];
            Color[] submeshColors = originalMesh.colors.Length > 0 ? new Color[submeshVertexCount] : null;
            Color32[] submeshColors32 = originalMesh.colors32.Length > 0 ? new Color32[submeshVertexCount] : null;
            Vector2[] submeshUV = originalMesh.uv.Length > 0 ? new Vector2[submeshVertexCount] : null;
            //Vector2[] submeshUV2 = originalMesh.uv2.Length > 0 ? new Vector2[submeshVertexCount] : null;
            //Vector2[] submeshUV3 = originalMesh.uv3.Length > 0 ? new Vector2[submeshVertexCount] : null;
            //Vector2[] submeshUV4 = originalMesh.uv4.Length > 0 ? new Vector2[submeshVertexCount] : null;

            for (int p = 0; p != submeshVertexCount; p++)
            {
                submeshVertices[p] = originalVertices[sortedTriangles[p]];
                if (submeshColors != null)
                    submeshColors[p] = originalMeshColors[p];
                if (submeshColors32 != null)
                    submeshColors32[p] = originalMeshColors32[p];
                if (submeshUV != null)
                    submeshUV[p] = originalMeshUV[p];
                //if (submeshUV2 != null)
                //    submeshUV2[p] = originalMeshUV2[p];
                //if (submeshUV3 != null)
                //    submeshUV3[p] = originalMeshUV3[p];
                //if (submeshUV4 != null)
                //    submeshUV4[p] = originalMeshUV4[p];
            }

            for (int p = 0; p != submeshOriginalTriangles.Length; p++)
            {
                submeshTriangles[p] = submeshOriginalTriangles[p] - minIndex; //offset the triangles by the min index of this submesh triangles so it starts at zero
            }

            Mesh mesh = new Mesh();
            mesh.vertices = submeshVertices;
            mesh.triangles = submeshTriangles;
            mesh.colors = submeshColors;
            mesh.colors32 = submeshColors32;
            mesh.uv = submeshUV;
            //mesh.uv2 = submeshUV2;
            //mesh.uv3 = submeshUV3;
            //mesh.uv4 = submeshUV4;

            outputMeshes.Add(new SubMesh(mesh, originalMaterials[i], originalMeshFilter.transform.localToWorldMatrix));
        }

        return outputMeshes;
    }

    /**
     * Build the mesh object that will hold the final mesh that combines all the individual meshes
     * **/
    private void BuildCombinedMeshObject(Mesh finalMesh, Material[] materials)
    {
        GetComponent<MeshRenderer>().sharedMaterials = materials;
        GetComponent<MeshFilter>().sharedMesh = finalMesh;
    }
}
