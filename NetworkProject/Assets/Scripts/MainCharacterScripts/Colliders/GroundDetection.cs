using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    public bool IsGrounded { get; private set; } = false;
    public bool TouchingGround { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        TouchingGround = true;
    }

    private void OnTriggerStay(Collider other)
    {
        IsGrounded = true;
        TouchingGround = false;
    }

    private void OnTriggerExit(Collider other)
    {
        IsGrounded = false;
        TouchingGround = false;

    }

}
