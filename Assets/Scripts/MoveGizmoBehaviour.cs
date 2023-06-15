using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGizmoBehaviour : MonoBehaviour {
    [SerializeField]
    GameObject arrowX = null;
    [SerializeField]
    GameObject arrowY = null;
    [SerializeField]
    GameObject arrowZ = null;
    [SerializeField]
    GameObject targetObject = null;
    [SerializeField]
    float speed = 1.0f;

    public enum MoveDirection
    {
        DirX,
        DirY,
        DirZ,
        DirNone
    }


    private MoveDirection currentMoveDirection = MoveDirection.DirNone;

    public MoveDirection CurrentMoveDirection
    {
        get
        {
            return currentMoveDirection;
        }

        set
        {
            currentMoveDirection = value;
        }
    }

    public GameObject TargetObject
    {
        get
        {
            return targetObject;
        }

        set
        {
            targetObject = value;
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //this.gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = 1 << 8;

            if (Physics.Raycast(ray, out hit, layerMask))
            {
                if (arrowX == hit.collider.gameObject)
                {
                    CurrentMoveDirection = MoveDirection.DirX;
                }
                else if (arrowY == hit.collider.gameObject)
                {
                    CurrentMoveDirection = MoveDirection.DirY;
                }
                else if (arrowZ == hit.collider.gameObject)
                {
                    CurrentMoveDirection = MoveDirection.DirZ;
                }
                else
                    CurrentMoveDirection = MoveDirection.DirNone;
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            CurrentMoveDirection = MoveDirection.DirNone;
        }

        if(Input.GetMouseButton(0))
        {
            if(null != TargetObject)
            {
                float mouseX = Input.GetAxis("Mouse X") * speed;
                float mouseY = Input.GetAxis("Mouse Y") * speed;

                Vector3 currentPos = TargetObject.transform.position;

                Vector3 movement = new Vector3();

                switch (CurrentMoveDirection)
                {
                    case MoveDirection.DirX:
                        movement = new Vector3( mouseX, 0.0f,0.0f);
                        break;
                    case MoveDirection.DirY:
                        movement = new Vector3(0.0f, mouseY, 0.0f);
                        break;
                    case MoveDirection.DirZ:
                        movement = new Vector3(0.0f, 0.0f, -mouseX + mouseY);
                        break;
                }

                TargetObject.transform.position += movement;
                //this.gameObject.transform.position += movement;

                NodeBehaviour nodeBehaviour = TargetObject.GetComponent<NodeBehaviour>();

                foreach(GameObject nodeLink in nodeBehaviour.NodeLinkList)
                {
                    NodeLinkBehaviour nodeLinkBehaviour = nodeLink.GetComponent<NodeLinkBehaviour>();

                    nodeLinkBehaviour.UpdateLine();
                }
            }            
        }
	}
}
