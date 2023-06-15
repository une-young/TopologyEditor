using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using PolygonCuttingEar;
using System;

public class CameraMovement2D : MonoBehaviour
{

    [SerializeField]
    MainBehaviour mainBehaviour = null;
    [SerializeField]
    Material lineMaterial = null;
    [SerializeField]
    Transform grid2DTransform = null;
    [SerializeField]
    float gridZPos = 0.0f;
    [SerializeField]
    float gridStep = 1.0f;

    public float speed = 2.0f;
    public float zoomSpeed = 5.0f;

    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -45.0f;
    public float maxY = 45.0f;

    public float sensX = 100.0f;
    public float sensY = 100.0f;

    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    public float rotationSmoothTime = 0.3F;
    private Vector3 rotationVelocity = Vector3.zero;

    private Camera camera2D = null;

    private GameObject selectedSphere = null;

    private Color gridLineColor = new Color(0f, 1f, 0f, 1f);

    public GameObject SelectedSphere
    {
        get
        {
            return selectedSphere;
        }

        set
        {
            selectedSphere = value;
        }
    }

    private void Start()
    {
        camera2D = GetComponent<Camera>();
    }

    public void LookAt(Vector3 lookAt, float distance)
    {
        Vector3 reveseForward = -transform.forward;

        Vector3 newPosition = lookAt + reveseForward * distance;


    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        Vector3 toPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        //toPosition += (transform.forward * scroll * zoomSpeed);

        camera2D.orthographicSize += scroll * zoomSpeed;


        if (Input.GetKey(KeyCode.RightArrow))
        {
            toPosition += transform.right * speed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            toPosition += -transform.right * speed;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            toPosition += transform.forward * speed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            toPosition += -transform.forward * speed;
        }

        switch(Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                toPosition = handleMouseControl(toPosition);
                break;
            case RuntimePlatform.Android:
                toPosition = handleTouchControl(toPosition);
                break;
            default:
                break;
        }


        //else if (Input.GetMouseButton(2) && !EventSystem.current.IsPointerOverGameObject())
        //{
        //    //float mouseX = -Input.GetAxis("Mouse X") * speed;
        //    float mouseZ = -Input.GetAxis("Mouse Y") * speed;

        //    float oldX = transform.position.x;
        //    float oldZ = transform.position.z;

        //    toPosition += transform.up * mouseZ;

        //    toPosition = new Vector3(oldX, toPosition.y, oldZ);
        //}

        transform.position = Vector3.SmoothDamp(transform.position, toPosition, ref velocity, smoothTime);
    }

    private Vector3 handleTouchControl(Vector3 toPosition)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch(touch.phase)
            {
                case TouchPhase.Began:
                    {
                        Ray ray = camera2D.ScreenPointToRay(touch.position);
                        RaycastHit hit;

                        int layerMask = 1 << 9;

                        if (Physics.Raycast(ray, out hit, layerMask))
                        {
                            if (hit.collider.gameObject.tag == "EditPoint")
                                SelectedSphere = hit.collider.gameObject;
                            else
                                SelectedSphere = null;
                        }
                        else
                            SelectedSphere = null;
                    }

                    break;
                case TouchPhase.Moved:
                    {
                        Ray ray = camera2D.ScreenPointToRay(touch.position);
                        RaycastHit hit;

                        int layerMask = 1 << 9;

                        if (Physics.Raycast(ray, out hit, layerMask))
                        {
                            if(null != SelectedSphere)
                            {
                                Vector3 realPosition = new Vector3(hit.point.x, hit.point.y, SelectedSphere.transform.position.z);
                                Vector3 snapPosition = getSnapPosition(realPosition);

                                SelectedSphere.transform.position = snapPosition;

                                mainBehaviour.createOutlinePolygon();
                            }
                            else
                            {
                                toPosition = 
                                    new Vector3(hit.point.x, hit.point.y, transform.position.z);
                            }
                        }
                    }

                    break;
                case TouchPhase.Ended:
                    SelectedSphere = null;
                    break;
            }
        }

        return toPosition;
    }

    private Vector3 handleMouseControl(Vector3 toPosition)
    {
        if (Input.GetMouseButton(0))
        {

            Ray ray = camera2D.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = 1 << 9;

            if (Physics.Raycast(ray, out hit, layerMask))
            {
                if (hit.collider.gameObject.tag == "EditPoint")
                    SelectedSphere = hit.collider.gameObject;
                else
                    SelectedSphere = null;
            }
            else
                SelectedSphere = null;


            float oldY = transform.position.y;

            //Vector3 newForward = new Vector3(transform.forward.x, 0.0f, -transform.forward.y);

            //toPosition += transform.right * mouseX + newForward * mouseZ;

            if (null != SelectedSphere)
            {

                Vector3 realPosition = new Vector3(hit.point.x, hit.point.y, SelectedSphere.transform.position.z);

                Vector3 snapPosition = getSnapPosition(realPosition);

                //GameObject cube = GameObject.Find("SnapCube");

                //if (null != cube)
                //    cube.transform.position = snapPosition;

                //Debug.Log(string.Format("real:{0} snap:{1}", selectedSphere.transform.position, snapPosition));

                SelectedSphere.transform.position = snapPosition;

                mainBehaviour.createOutlinePolygon();
            }
            else
            {
                float mouseX = -Input.GetAxis("Mouse X") * speed;
                float mouseY = -Input.GetAxis("Mouse Y") * speed;

                toPosition = new Vector3(transform.position.x + mouseX, transform.position.y + mouseY, transform.position.z);
            }
        }

        return toPosition;
    }

    void OnPostRender()
    {
        Draw2DGrid();
    }

    Vector3 getSnapPosition(Vector3 pos)
    {
        //transform to camera local pos
        Vector3 localPos = grid2DTransform.InverseTransformVector(pos);
        float localX = localPos.x;
        float localY = localPos.y;

        Bounds bounds = grid2DTransform.gameObject.GetComponent<MeshRenderer>().bounds;

        float width = bounds.max.x - bounds.min.x;
        float halfWidth = width * 0.5f;
        float height = bounds.max.y - bounds.min.y;
        float halfHeight = height * 0.5f;
        float halfStep = gridStep * 0.5f;

        float newX = 0.0f;

        for (float x = -halfWidth; x <= halfWidth; x += gridStep)
        {
            if(Mathf.Abs(localX - x) <= halfStep)
            {
                newX = x;
                break;
            }
        }

        float newY = 0.0f;

        for (float y = -halfHeight; y <= halfHeight; y += gridStep)
        {
            if (Mathf.Abs(localY - y) <= halfStep)
            {
                newY = y;
                break;
            }
        }


        //float newX = Mathf.Round((localX - halfWidth) / gridStep)*gridStep + halfWidth;
        //float newY = Mathf.Round((localY - halfHeight) / gridStep)*gridStep + halfHeight;

        //float newX = Mathf.Round((x) / gridStep) * gridStep ;
        //float newY = Mathf.Round((y ) / gridStep) * gridStep ;

        return grid2DTransform.TransformVector( new Vector3(newX, newY, localPos.z));
    }

    private void Draw2DGrid()
    {
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        GL.MultMatrix(grid2DTransform.localToWorldMatrix);

        GL.Begin(GL.LINES);

        Bounds bounds = grid2DTransform.gameObject.GetComponent<MeshRenderer>().bounds;

        float width = bounds.max.x - bounds.min.x;
        float halfWidth = width * 0.5f;
        float height = bounds.max.y - bounds.min.y;
        float halfHeight = height * 0.5f;
        


        GL.Color(gridLineColor);

        gridStep = Mathf.Max(gridStep, 0.001f); //0 무한루프 방지

        //Layers
        for (float x = -halfWidth; x <= halfWidth; x += gridStep)
        {
            //X axis lines
            GL.Vertex3(x, -halfHeight, gridZPos);
            GL.Vertex3(x, +halfHeight, gridZPos);
        }

        for (float y = -halfHeight; y <= halfHeight; y += gridStep)
        {
            //Y axis lines
            GL.Vertex3(-halfWidth, y, gridZPos);
            GL.Vertex3(halfWidth, y, gridZPos);
        }

        GL.End();
        GL.PopMatrix();
    }
}
