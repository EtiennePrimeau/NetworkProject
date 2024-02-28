using UnityEngine;
using Mirror;

public class NetworkElevator : NetworkBehaviour
{
    [SerializeField] private bool m_isTriggerable = false;
    [SerializeField] private GameObject m_elevator;
    [SerializeField] private Transform m_startPoint;
    [SerializeField] private Transform m_endPoint;
    [SerializeField] private float m_speed = 5.0f;
    [SerializeField] private float m_waitTime = 2.0f;

    private bool m_isMoving = false;
    private bool m_isMovingTowardsEnd = true;
    private bool m_isWaiting = false;
    private float m_timer = 0.0f;
    private float m_distanceThreshold = 0.1f;
    
    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();            
        }        
    }

    [Server]
    private void ServerUpdate()
    {
        if(!m_isTriggerable)
        {
            if (m_isWaiting)
            {
                m_timer += Time.deltaTime;
                if (m_timer >= m_waitTime)
                {
                    m_timer = 0.0f;
                    m_isWaiting = false;
                }
                return;
            }

            if (m_isMovingTowardsEnd)
            {
                MoveTowards(m_endPoint.position);
                if (Vector3.Distance(m_elevator.transform.position, m_endPoint.position) < m_distanceThreshold)
                {
                    m_isMovingTowardsEnd = false;
                    m_isWaiting = true;
                }
            }
            else
            {
                MoveTowards(m_startPoint.position);

                if (Vector3.Distance(m_elevator.transform.position, m_startPoint.position) < m_distanceThreshold)
                {
                    m_isMovingTowardsEnd = true;
                    m_isWaiting = true;
                }
            }

        }
        else 
        {            
            if(m_isMoving)
            {
                if (m_isWaiting)
                {
                    m_timer += Time.deltaTime;
                    if (m_timer >= m_waitTime)
                    {
                        m_timer = 0.0f;
                        m_isWaiting = false;
                    }
                    return;
                }
                
                if (m_isMovingTowardsEnd)
                {
                    MoveTowards(m_endPoint.position);
                    
                    if (Vector3.Distance(m_elevator.transform.position, m_endPoint.position) < m_distanceThreshold)
                    {
                        m_isMovingTowardsEnd = false;
                        m_isWaiting = true;
                    }
                }
                else
                {
                    MoveTowards(m_startPoint.position);

                    if (Vector3.Distance(m_elevator.transform.position, m_startPoint.position) < m_distanceThreshold)
                    {
                        m_isMovingTowardsEnd = true;
                        m_isMoving = false;
                    }
                }
            }
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_isTriggerable)
        {            
            var runnerSM = other.transform.root.gameObject.GetComponentInChildren<RunnerSM>();
            
            if (runnerSM != null)
            {                
                CMD_StartMoving();
            }
        }
    }

    [Command (requiresAuthority = false)]
    private void CMD_StartMoving()
    {
        m_isMoving = true;
    }

    [ClientRpc]
    private void MoveTowards(Vector3 position)
    {
        m_elevator.transform.position = Vector3.MoveTowards(m_elevator.transform.position, position, m_speed * Time.deltaTime);
    }

}
