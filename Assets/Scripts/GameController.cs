using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour 
{
    public GameObject m_lowPolyTreePfb;
    public GameObject m_emptyMeshObjectPfb;

    private GameObject m_treesContainer;

    public Material m_dbgWoodMaterial;

    public void Start()
    {
        CreateForest();
        //CombineTrees();

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        CombineDummyObjects();

        sw.Stop();
        Debug.Log("elapsed time:" + sw.ElapsedMilliseconds + " ms");
    }

    private void CreateForest()
    {
        m_treesContainer = new GameObject("TreesContainer");

        int numTrees = 20;
        for (int i = 0; i != numTrees; i++)
        {
            GameObject clonedTree = (GameObject)Instantiate(m_lowPolyTreePfb);
            clonedTree.name = "Tree";
            clonedTree.transform.parent = m_treesContainer.transform;
            
            float posX = (Random.value - 0.5f) * 55.0f;
            float posY = clonedTree.transform.localPosition.y;
            float posZ = (Random.value - 0.5f) * 75.0f;

            //clonedTree.transform.localScale = new Vector3(scale, scale, scale);
            clonedTree.transform.localPosition = new Vector3(posX, posY, posZ);
        }
    }

    //private void CombineTrees()
    //{
    //    MeshFilter[] meshFilters = m_treesContainer.GetComponentsInChildren<MeshFilter>();

    //    if (meshFilters.Length == 0)
    //        return;

    //    Material[] treeMaterials = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterials;

    //    CombineInstance[] combine = new CombineInstance[meshFilters.Length];
    //    int p = 0;

    //    while (p < meshFilters.Length)
    //    {
    //        MeshFilter meshFilter = meshFilters[p];
    //        Debug.Log("submesh COUNT:" + meshFilter.sharedMesh.subMeshCount);
    //        combine[p].mesh = meshFilter.sharedMesh;
    //        combine[p].transform = meshFilters[p].transform.localToWorldMatrix;
    //        p++;
    //    }

    //    GameObject combinedMeshObject = (GameObject)Instantiate(m_emptyMeshObjectPfb);
    //    combinedMeshObject.name = "Combined trees";
    //    combinedMeshObject.GetComponent<MeshFilter>().mesh = new Mesh();
    //    combinedMeshObject.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    //    combinedMeshObject.GetComponent<MeshRenderer>().sharedMaterials = treeMaterials;
    //    Debug.Log("combined submesh COUNT:" + combinedMeshObject.GetComponent<MeshFilter>().sharedMesh.subMeshCount);

    //    m_treesContainer.SetActive(false);
    //}

    private void CombineDummyObjects()
    {
        MeshFilter[] meshFilters = m_treesContainer.GetComponentsInChildren<MeshFilter>();
        List<GameObject> objectsToCombine = new List<GameObject>(meshFilters.Length);
        for (int p = 0; p != meshFilters.Length; p++)
        {
            GameObject objectToCombine = meshFilters[p].gameObject;
            objectsToCombine.Add(objectToCombine);
            //objectToCombine.SetActive(false);
        }
        CombineObjectMeshes(objectsToCombine);
    }

    /**
     * Take the objects list and combine all its meshes into one
     * **/
    public void CombineObjectMeshes(List<GameObject> objects)
    {
        Dictionary<Material, List<SingleMaterialMesh>> meshesByMaterial = new Dictionary<Material, List<SingleMaterialMesh>>();

        for (int i = 0; i != objects.Count; i++)
        {
            MeshFilter meshFilter = objects[i].GetComponent<MeshFilter>();
            List<SingleMaterialMesh> submeshes = ExplodeMeshSubmeshes(meshFilter);

            for (int p = 0; p != submeshes.Count; p++)
            {
                Material submeshMaterial = submeshes[p].m_material;
                List<SingleMaterialMesh> listForMaterial;
                meshesByMaterial.TryGetValue(submeshes[p].m_material, out listForMaterial);

                if (listForMaterial == null)
                {
                    listForMaterial = new List<SingleMaterialMesh>();
                    listForMaterial.Add(submeshes[p]);
                    meshesByMaterial.Add(submeshMaterial, listForMaterial);
                }
                else
                    listForMaterial.Add(submeshes[p]);
            }
        }

        //combine meshes for each material
        int materialCount = meshesByMaterial.Count;
        Material[] combinedMaterials = new Material[materialCount];
        List<Mesh> combinedMeshesByMaterial = new List<Mesh>(materialCount);
        int m = 0;
        foreach (KeyValuePair<Material, List<SingleMaterialMesh>> kvp in meshesByMaterial)
        {
            combinedMaterials[m] = kvp.Key;
            List<SingleMaterialMesh> meshesToCombine = kvp.Value;
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
        GameObject clusterGameObject = (GameObject)Instantiate(m_emptyMeshObjectPfb);
        clusterGameObject.name = "Cluster";

        clusterGameObject.GetComponent<MeshRenderer>().sharedMaterials = combinedMaterials;
        clusterGameObject.GetComponent<MeshFilter>().sharedMesh = finalMesh;
    }

    /**
     * Separate all submeshes from one mesh. If the mesh contains only one submesh just return it with its material
     * **/
    private List<SingleMaterialMesh> ExplodeMeshSubmeshes(MeshFilter originalMeshFilter)
    {
        Mesh originalMesh = originalMeshFilter.sharedMesh;
        List<SingleMaterialMesh> outputMeshes = new List<SingleMaterialMesh>();

        if (originalMesh.subMeshCount == 1)
            return null;

        Material[] originalMaterials = originalMeshFilter.GetComponent<MeshRenderer>().sharedMaterials;

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
            Vector2[] originalMeshUV2 = originalMesh.uv2.Length > 0 ? originalMesh.uv2 : null;
            Vector2[] originalMeshUV3 = originalMesh.uv3.Length > 0 ? originalMesh.uv3 : null;
            Vector2[] originalMeshUV4 = originalMesh.uv4.Length > 0 ? originalMesh.uv4 : null;

            int submeshVertexCount = sortedTriangles.Length;
            Vector3[] submeshVertices = new Vector3[submeshVertexCount];
            int[] submeshTriangles = new int[submeshOriginalTriangles.Length];
            Color[] submeshColors = originalMesh.colors.Length > 0 ? new Color[submeshVertexCount] : null;
            Color32[] submeshColors32 = originalMesh.colors32.Length > 0 ? new Color32[submeshVertexCount] : null;
            Vector2[] submeshUV = originalMesh.uv.Length > 0 ? new Vector2[submeshVertexCount] : null;
            Vector2[] submeshUV2 = originalMesh.uv2.Length > 0 ? new Vector2[submeshVertexCount] : null;
            Vector2[] submeshUV3 = originalMesh.uv3.Length > 0 ? new Vector2[submeshVertexCount] : null;
            Vector2[] submeshUV4 = originalMesh.uv4.Length > 0 ? new Vector2[submeshVertexCount] : null;

            for (int p = 0; p != submeshVertexCount; p++)
            {
                submeshVertices[p] = originalVertices[sortedTriangles[p]];                
                if (submeshColors != null)
                    submeshColors[p] = originalMeshColors[p];
                if (submeshColors32 != null)
                    submeshColors32[p] = originalMeshColors32[p];
                if (submeshUV != null)
                    submeshUV[p] = originalMeshUV[p];
                if (submeshUV2 != null)
                    submeshUV2[p] = originalMeshUV2[p];
                if (submeshUV3 != null)
                    submeshUV3[p] = originalMeshUV3[p];
                if (submeshUV4 != null)
                    submeshUV4[p] = originalMeshUV4[p];
            }

            for (int p = 0; p != submeshOriginalTriangles.Length;  p++)
            {
                submeshTriangles[p] = submeshOriginalTriangles[p] - minIndex; //offset the triangles by the min index of this submesh triangles so it starts at zero
            }

            Mesh mesh = new Mesh();
            mesh.vertices = submeshVertices;
            mesh.triangles = submeshTriangles;
            mesh.colors = submeshColors;
            mesh.colors32 = submeshColors32;
            mesh.uv = submeshUV;
            mesh.uv2 = submeshUV2;
            mesh.uv3 = submeshUV3;
            mesh.uv4 = submeshUV4;

            outputMeshes.Add(new SingleMaterialMesh(mesh, originalMaterials[i], originalMeshFilter.transform.localToWorldMatrix));
        }

        return outputMeshes;
    }
}