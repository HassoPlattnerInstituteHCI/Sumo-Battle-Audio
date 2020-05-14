using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    // public -> visible in inspector and other scripts
    // private -> not visible in inspector and other scripts
    public float rotationSpeed = 50.0f; // floating point numbers require 'f' at the end

    // Update is called once per frame
    void Update()
    {
        // gets the horizontal user input from
        // joystick/keyboard (a/d or left/right) as a float between -1 and 1
        float horizontalInput = Input.GetAxis("Horizontal");
        // rotates the focal point and all of it's attached children (main camera)
        // requires an axis to be rotated around and a rotation angle
        transform.Rotate(Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);
    }
}
