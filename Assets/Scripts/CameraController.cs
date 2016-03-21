using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    float height = 40f;
    float distanceBack = 40f;

    float camMoveSpeed = 30f;
    float zoomSpeed = 3f;

    void Start()
    {
        transform.position = Vector3.zero;
        //Move up
        transform.position += new Vector3(0f, height, 0f);
        //Move back
        transform.position -= new Vector3(0f, 0f, distanceBack);
        //Look at the center to get an angle
        transform.LookAt(Vector3.zero);
    }

    void LateUpdate()
    {
        //Move camera with keys
        //Move left/right
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position -= new Vector3(camMoveSpeed * Time.deltaTime, 0f, 0f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(camMoveSpeed * Time.deltaTime, 0f, 0f);
        }

        //Move forward/back
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= new Vector3(0f, 0f, camMoveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            transform.position += new Vector3(0f, 0f, camMoveSpeed * Time.deltaTime);
        }

        //Zoom
        float currentHeight = transform.position.y;

        float zoomDistance = 0f;

        if (currentHeight > 20f && currentHeight < 200f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(KeyCode.I))
            {
                zoomDistance += zoomSpeed;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(KeyCode.O))
            {
                zoomDistance -= zoomSpeed;
            }
        }
        //Can only zoom in
        else if (currentHeight > 200f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f || Input.GetKeyDown(KeyCode.I))
            {
                zoomDistance += zoomSpeed;
            }
        }
        //Can only zoom out
        else if (currentHeight < 20f)
        {
            if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetKeyDown(KeyCode.O))
            {
                zoomDistance -= zoomSpeed;
            }
        }

        transform.Translate(Vector3.forward * zoomDistance);
    }
}