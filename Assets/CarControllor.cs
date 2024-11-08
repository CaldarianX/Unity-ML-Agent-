using UnityEngine;

public class CarControllor : MonoBehaviour
{
    public float speed = 10f;      // Speed for forward and backward movement
    public float turnSpeed = 100f; // Speed for turning

    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input for movement and turning
        float forwardAmount = 0f;
        float turnAmount = 0f;

        // Forward and backward movement (W and S keys)
        if (Input.GetKey(KeyCode.W)) forwardAmount = 1f;
        else if (Input.GetKey(KeyCode.S)) forwardAmount = -1f;

        // Turning left and right (A and D keys)
        if (Input.GetKey(KeyCode.A)) turnAmount = -1f;
        else if (Input.GetKey(KeyCode.D)) turnAmount = 1f;

        // Move and rotate the car
        MoveCar(forwardAmount, turnAmount);
    }

    private void MoveCar(float forwardAmount, float turnAmount)
    {
        // Forward/backward movement
        Vector3 forwardMovement = transform.forward * forwardAmount * speed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        // Rotation for turning
        float turn = turnAmount * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }
}
