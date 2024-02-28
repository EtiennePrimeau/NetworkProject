using Mirror;
using UnityEngine;

public class RotateDiamond : NetworkBehaviour
{
    [SerializeField] private float m_rotationSpeed;
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float floatRange = 0.5f;
    private Vector3 m_currentRotation = Vector3.zero;
    private Vector3 m_startPosition = Vector3.zero;
    private float m_rotation = 0.0f;

    private void Start()
    {
        m_currentRotation = transform.localRotation.eulerAngles;
        m_startPosition = transform.localPosition;
    }

    private void FixedUpdate()
    {
        transform.localRotation = Quaternion.Euler(m_currentRotation.x, m_rotation, m_currentRotation.z);
        m_rotation += m_rotationSpeed;

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        
        Vector3 newPosition = m_startPosition + new Vector3(0f, yOffset, 0f);
        transform.localPosition = newPosition;
    }
}
