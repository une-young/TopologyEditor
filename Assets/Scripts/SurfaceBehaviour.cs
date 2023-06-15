using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceBehaviour : MonoBehaviour {
    private List<Vector3> outline = new List<Vector3>();

    float height = 350.0f;
    public List<Vector3> Outline
    {
        get
        {
            return outline;
        }

        set
        {
            outline = value;
        }
    }

    public float Height { get => height; set => height = value; }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
