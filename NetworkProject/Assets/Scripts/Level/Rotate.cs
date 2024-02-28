using Mirror;
using UnityEngine;

public class Rotate : NetworkBehaviour
{
    [field: SerializeField] public float RotationSpeed { get; private set; } = 0;
    private float m_rotation = 0;    
    
    void FixedUpdate()
    {
        transform.localRotation = Quaternion.Euler(0, m_rotation, 0);
        m_rotation += RotationSpeed;
    }
}
