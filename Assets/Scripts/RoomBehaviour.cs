using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour {

    List<GameObject> positionList = new List<GameObject>();
    List<GameObject> editPointList = new List<GameObject>();
    List<GameObject> lines = new List<GameObject>();

    Room room = null;

    public List<GameObject> PositionList
    {
        get
        {
            return positionList;
        }

        set
        {
            positionList = value;
        }
    }

    public List<GameObject> EditPointList
    {
        get
        {
            return editPointList;
        }

        set
        {
            editPointList = value;
        }
    }

    public List<GameObject> Lines
    {
        get
        {
            return lines;
        }

        set
        {
            lines = value;
        }
    }

    public Room Room
    {
        get
        {
            return room;
        }

        set
        {
            room = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RearrangePositionList()
    {
        int counter = 0;

        foreach (GameObject obj in positionList)
        {
            obj.transform.position = editPointList[counter].transform.position;
            counter++;
        }
    }

    public void Clear()
    {
        foreach (GameObject obj in positionList)
        {
            foreach (Transform trans in obj.transform)
            {
                GameObject.Destroy(trans.gameObject);
            }

            GameObject.Destroy(obj);
        }

        positionList.Clear();

        foreach (GameObject line in Lines)
        {
            GameObject.Destroy(line);
        }

        lines.Clear();

        foreach (GameObject obj in editPointList)
        {
            GameObject.Destroy(obj);
        }

        
        editPointList.Clear();
    }
}
