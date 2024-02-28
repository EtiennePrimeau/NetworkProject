using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField]
    private float m_timer = 3.0f;

    void Start()
    {
        Destroy(gameObject, m_timer);
    }
}



