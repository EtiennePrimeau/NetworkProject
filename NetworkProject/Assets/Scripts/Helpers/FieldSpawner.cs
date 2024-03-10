using UnityEngine;

public class FieldSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_prefab;
    [SerializeField] private float m_distance;
    [SerializeField] private float m_quantity;


    void Start()
    {
        Vector3 position = transform.position;

        for (int i = 0; i < m_quantity; i++)
        {

            Instantiate(m_prefab, position, Quaternion.identity);

            position.z += m_distance;
        }
    }
}
