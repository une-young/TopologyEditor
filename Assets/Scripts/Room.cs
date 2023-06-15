using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Room
{
    string roomName = "";

    List<Vector3> pointList = new List<Vector3>();

    public string RoomName
    {
        get
        {
            return roomName;
        }

        set
        {
            roomName = value;
        }
    }

    public List<Vector3> PointList
    {
        get
        {
            return pointList;
        }

        set
        {
            pointList = value;
        }
    }
}

