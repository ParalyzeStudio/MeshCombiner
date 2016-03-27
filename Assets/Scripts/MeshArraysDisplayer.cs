using UnityEngine;
using System.Collections;

public class MeshArraysDisplayer : MonoBehaviour {

	
	public void Start () 
    {
        Mesh mesh = this.GetComponent<MeshFilter>().sharedMesh;

        Debug.Log("mesh is ready to be analyzed");
	}
}
