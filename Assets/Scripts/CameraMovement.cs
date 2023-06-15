using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{

    public float speed = 2.0f;
    public float zoomSpeed = 5.0f;

    public float minX = -360.0f;
    public float maxX = 360.0f;

    public float minY = -45.0f;
    public float maxY = 45.0f;

    public float sensX = 100.0f;
    public float sensY = 100.0f;

    float rotationY = 0.0f;
    float rotationX = 0.0f;

    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;

    public float rotationSmoothTime = 0.3F;
    private Vector3 rotationVelocity = Vector3.zero;

    [SerializeField]
    MoveGizmoBehaviour moveGizmoBehaviour = null;

    private void Start()
    {
        rotationX = transform.localEulerAngles.y;
        rotationY = -transform.localEulerAngles.x;
    }

    void Update()
    {
        if(moveGizmoBehaviour != null && moveGizmoBehaviour.CurrentMoveDirection != MoveGizmoBehaviour.MoveDirection.DirNone)
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        Vector3 toPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        toPosition += (transform.forward * scroll * zoomSpeed);



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

        if (Input.GetMouseButton(0) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            rotationX += Input.GetAxis("Mouse X") * sensX;
            rotationY += Input.GetAxis("Mouse Y") * sensY;
            rotationY = Mathf.Clamp(rotationY, minY, maxY);
            rotationX = Mathf.Clamp(rotationX, minX, maxX);

            Vector3 newRotation = new Vector3(-rotationY, rotationX, 0);
            transform.localEulerAngles = newRotation;
            //transform.localEulerAngles = Vector3.Slerp(transform.localEulerAngles, newRotation, Time.deltaTime * 2.0f);            
        }
        else if (Input.GetMouseButton(1) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            float mouseX = -Input.GetAxis("Mouse X") * speed;
            float mouseZ = -Input.GetAxis("Mouse Y") * speed;

            float oldY = transform.position.y;

            toPosition += transform.right * mouseX + transform.forward * mouseZ;

            toPosition = new Vector3(toPosition.x, oldY, toPosition.z);

            //transform.position = new Vector3(transform.position.x, oldY, transform.position.z);            
            //transform.position = Vector3.Slerp(transform.position, toPosition, Time.time);
        }
        else if (Input.GetMouseButton(2) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() )
        {
            //float mouseX = -Input.GetAxis("Mouse X") * speed;
            float mouseZ = -Input.GetAxis("Mouse Y") * speed;

            float oldX = transform.position.x;
            float oldZ = transform.position.z;

            toPosition += transform.up * mouseZ;

            toPosition = new Vector3(oldX, toPosition.y, oldZ);
        }

        transform.position = Vector3.SmoothDamp(transform.position, toPosition, ref velocity, smoothTime);
    }
}
