using Mirror;
using UnityEngine;

public class RotatingArmAddForce : MonoBehaviour
{
    [SerializeField] private float m_impulseForce;
    [SerializeField] private Transform m_tower;

    private float m_towerSpeed = 0.0f;
    private Rotate m_towerRotate = null;

    private void Start()
    {
        m_towerRotate = m_tower.GetComponent<Rotate>();
        m_towerSpeed = m_towerRotate.RotationSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        var runner = other.gameObject.GetComponentInChildren<RunnerSM>();
        //Debug.Log("Arm hit: " + other.gameObject.name);
        if (runner != null)
        {
            //Debug.Log("addforce");

            if (m_towerSpeed > 0 )
            {
                runner.AddImpulseForce(m_tower.right, m_impulseForce);
            }
            if (m_towerSpeed < 0)
            {
                runner.AddImpulseForce(-m_tower.right, m_impulseForce);
            }


        }

    }
}
