using UnityEngine;
using System.Collections.Generic;

public class TexturePacker : MonoBehaviour
{
    public Texture2D[] m_atlasTextures;
    public Material m_atlasMaterial;

    public void PackTextures()
    {


        if (m_atlasTextures.Length == 1) //no need to pack
            return;

        Texture2D atlas = new Texture2D(2048, 2048);
        Rect[] rects = atlas.PackTextures(m_atlasTextures, 2, 2048);


        if (rects != null)
        {
            m_atlasMaterial.mainTexture = atlas;
        }
        else
            Debug.Log("Packing failed");
    }

    //-recuperer tous les meshfilters de la liste d'objets à combiner
    //-classer les mesh par type de shader
    //-pour chaque shader:
    //    * si le shader contient une texture, pack les textures et sauvegarder le Rect dans le mesh
    //    * sinon ne rien faire
    //

    //public Dictionary<Shader,List<Material>> Pack()
    //{
    //    Component[] filters  = GetComponentsInChildren(typeof(MeshFilter));
    //    Dictionary<Shader,List<Material>> shaderToMaterial = new Dictionary<Shader,List<Material>>();
       
    //    // Find all unique shaders in hierarchy.
    //    for (int i=0;i < filters.Length;i++) 
    //    {         
    //        Renderer curRenderer  = filters[i].renderer;
    //        if (curRenderer != null  && curRenderer.enabled && curRenderer.material != null)  {
    //            Material[] materials = curRenderer.sharedMaterials;
                   
    //            if (materials != null) {
    //                foreach (Material mat in materials) {
    //                    if (((mat.HasProperty("_LightMap")  !(((MeshFilter)filters[i]).mesh.uv2.Length == 0)  mat.GetTexture("_LightMap") != null) || !(mat.HasProperty("_LightMap")))
    //                        (mat.mainTextureScale==new Vector2(1.0f,1.0f)
    //                        (mat.mainTextureOffset==Vector2.zero))
    //                    ) {
    //                        if (mat.shader != null  mat.mainTexture != null) {
    //                            if (shaderToMaterial.ContainsKey(mat.shader)) {
    //                                shaderToMaterial[mat.shader].Add(mat);
    //                            }
    //                            else {
    //                                shaderToMaterial[mat.shader]=new List<Material>();
    //                                shaderToMaterial[mat.shader].Add(mat);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
       
    //    // Pack textures per shader basis and generate UV rect and material dictinaries.
    //    foreach (Shader key in shaderToMaterial.Keys) {
    //        Texture2D packedTexture=new Texture2D(1024,1024);
    //        Texture2D[] texs = new Texture2D[shaderToMaterial[key].Count];
    //        generatedMaterials[key] = new Material(key);
    //        for (int i=0;i < texs.Length; i++) {
    //            texs[i] = shaderToMaterial[key][i].mainTexture as Texture2D;
    //        }
    //        Rect[] uvs = packedTexture.PackTextures(texs,0,2048);
           
    //        generatedMaterials[key].CopyPropertiesFromMaterial(shaderToMaterial[key][0]);      
    //        generatedMaterials[key].mainTexture=packedTexture;
           
    //        for (int i=0;i < texs.Length; i++) {
    //            if (shaderToMaterial[key][i].HasProperty("_LightMap")) {
    //                texs[i] = shaderToMaterial[key][i].GetTexture("_LightMap") as Texture2D;
    //            }  
    //        }
    //        packedTexture=new Texture2D(1024,1024);
    //        Rect[] uvs2 = packedTexture.PackTextures(texs,0,2048);
    //        if (generatedMaterials[key].HasProperty("_LightMap")) {
    //            generatedMaterials[key].SetTexture("_LightMap", packedTexture);
    //        }
           
    //        for (int i=0;i < texs.Length; i++) {
    //            generatedUVs[shaderToMaterial[key][i]] = uvs[i];
    //            generatedUV2s[shaderToMaterial[key][i]] = uvs2[i];
    //        }
    //    }
       
    //    Vector2[] uv,uv2;
       
    //    // Calculate new UVs for all submeshes and assign generated materials.
    //    for (int i=0;i < filters.Length;i++) {
           
    //        int subMeshCount = ((MeshFilter)filters[i]).mesh.subMeshCount;
               
    //        Material[] mats = filters[i].gameObject.renderer.sharedMaterials;  
    //        uv = (Vector2[])(((MeshFilter)filters[i]).mesh.uv);
    //        uv2 = (Vector2[])(((MeshFilter)filters[i]).mesh.uv2);
    //        for (int j=0; j < subMeshCount; j++) {
    //            if ( generatedUVs.ContainsKey(mats[j])) {
    //                Rect uvs = generatedUVs[mats[j]];
    //                Rect uvs2 = generatedUV2s[mats[j]];
    //                int[] subMeshVertices = DeleteDuplicates(((MeshFilter)filters[i]).mesh.GetTriangles(j)) as int[];
    //                mats[j]=generatedMaterials[filters[i].gameObject.renderer.sharedMaterials[j].shader];
    //                foreach (int vert in subMeshVertices) {
    //                    uv[vert]=new Vector2((uv[vert].x*uvs.width)+uvs.x, (uv[vert].y*uvs.height)+uvs.y);
    //                    if (uv2!=null  !(uv2.Length==0)) {
    //                        uv2[vert]=new Vector2((uv2[vert].x*uvs2.width)+uvs2.x, (uv2[vert].y*uvs2.height)+uvs2.y);
    //                    }
    //                }
    //            }
    //        }
    //        filters[i].gameObject.renderer.sharedMaterials=mats;
    //        ((MeshFilter)filters[i]).mesh.uv=uv;
    //        if (uv2!=null  !(uv2.Length==0)) {
    //            ((MeshFilter)filters[i]).mesh.uv2=uv2;
    //        }
    //    }
       
    //    // Combine Meshes
    //    CombineMeshes();   
    //}
}
