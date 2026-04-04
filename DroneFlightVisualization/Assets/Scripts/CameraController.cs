using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    public float Sensitivity;
    public float ScrollSensitivity;
    public float MoveSensitivity;

    private Camera cam;
    private Quaternion temp;
    private float zoom = 10;
    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdatePos();
    }

    void Update()
    {
        var scroll = -Input.mouseScrollDelta.y;

        if (scroll != 0)
        {
            zoom = Mathf.Clamp(zoom + scroll * Sensitivity, 2, 100);
        }

        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * Sensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * Sensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            yRotation += mouseX;

            Target.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }

        if (Input.GetMouseButtonDown(2))
        {
            temp = Target.rotation;
        }

        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X") * Sensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * Sensitivity;

            Vector3 dir = new(mouseX, 0, -mouseY);
            dir *= MoveSensitivity;

            Target.position += temp * dir;
        }
        UpdatePos();
    }

    void UpdatePos()
    {
        cam.transform.position = Target.position + Target.up * zoom;
        cam.transform.rotation = Quaternion.LookRotation(Target.position + Target.forward - cam.transform.position, Target.position + Target.up - cam.transform.position);
    }
}
