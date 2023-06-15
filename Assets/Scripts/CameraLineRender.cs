using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class CameraLineRender : MonoBehaviour {
    [SerializeField]
    MainBehaviour mainBehaviour = null;
    [SerializeField]
    Material lineMaterialWhite = null;
    [SerializeField]
    Material lineMaterialGreen = null;
    [SerializeField]
    Transform lineTransform = null;

    private Color gridLineColor = new Color(0f, 1f, 0f, 1f);

    private Color doorBaseLineColor = new Color(1.0f, 0.0f, 0.0f, 0.0f);

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnPostRender()
    {
        if (!mainBehaviour.ShowLine)
            return;

        GL.PushMatrix();

        GL.MultMatrix(lineTransform.localToWorldMatrix);
        

        if (mainBehaviour.GlobalBuilding != null)
        {
            foreach(Floor floor in mainBehaviour.GlobalBuilding.floorList)
            {
                lineMaterialWhite.SetPass(0);
                GL.Color(Color.red);                
                drawOutline(floor.outline);
            }

            foreach (DoorBase doorBase in mainBehaviour.GlobalBuilding.doorBaseList)
            {
                lineMaterialGreen.SetPass(0);
                GL.Color(Color.green);                
                drawOutline(doorBase.outline);
            }
        }
        
        GL.PopMatrix();
    }

    private void drawOutline(float [] outline)
    {
        GL.Begin(GL.LINE_STRIP);

        for (int i=0;i<outline.Length;i+=3)
        {
            GL.Vertex3(outline[i], outline[i + 1], outline[i + 2]);
        }

        GL.End();
    }
}
