using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditPointBehaviour : MonoBehaviour {
    GameObject linkedLine1 = null;
    GameObject linkedLine2 = null;

    GameObject roomObject = null;

    public GameObject RoomObject
    {
        get
        {
            return roomObject;
        }

        set
        {
            roomObject = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	    if(linkedLine1 != null && linkedLine2 != null)
        {
            LineRenderer lineRenderer1 = linkedLine1.GetComponent<LineRenderer>();

            lineRenderer1.SetPosition(1, transform.position);

            Vector3 center1 = (lineRenderer1.GetPosition(0) + lineRenderer1.GetPosition(1)) / 2.0f;
            center1 = new Vector3(center1.x, center1.y, center1.z - 1.0f); //라인에 가리지 않게 앞으로 이동

            //linkedLine1.transform.position = center1;

            LineRenderer lineRenderer2 = linkedLine2.GetComponent<LineRenderer>();

            lineRenderer2.SetPosition(0, transform.position);

            Vector3 center2 = (lineRenderer2.GetPosition(0) + lineRenderer2.GetPosition(1)) / 2.0f;
            center2 = new Vector3(center2.x, center2.y, center2.z - 1.0f); //라인에 가리지 않게 앞으로 이동
            //linkedLine2.transform.position = center2;
        }
	}

    public void SetLinkedLine(GameObject linkedLine1,GameObject linkedLine2) {
        this.linkedLine1 = linkedLine1;
        this.linkedLine2 = linkedLine2;
    }    
}
