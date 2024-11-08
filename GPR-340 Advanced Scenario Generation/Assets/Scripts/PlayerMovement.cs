using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float mouseSens = 1f;
    [SerializeField] float sprintSpeed = 4f;
    public Transform playerCamera;

    float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.forward * vertical + transform.right * horizontal;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * mouseSens);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            speed *= sprintSpeed;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            speed /= sprintSpeed;
        }

        if(Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Q))
        {
            transform.position += new Vector3(0f, transform.up.y * speed * Time.deltaTime, 0f);
        }
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.E))
        {
            transform.position += new Vector3(0f, -transform.up.y * speed * Time.deltaTime, 0f);
        }
    }
}
