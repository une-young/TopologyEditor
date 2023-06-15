using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBaseBehaviour : MonoBehaviour {
    private List<GameObject> neighborFloorList = new List<GameObject>();
    private List<Vector3> outline = new List<Vector3>();

    public List<GameObject> NeighborFloorList
    {
        get
        {
            return neighborFloorList;
        }        
    }

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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if(!NeighborFloorList.Contains(other.gameObject))
        {
            if(other.gameObject.tag == "Floor")
                NeighborFloorList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NeighborFloorList.Contains(other.gameObject))
        {
            NeighborFloorList.Remove(other.gameObject);
        }
    }
}
