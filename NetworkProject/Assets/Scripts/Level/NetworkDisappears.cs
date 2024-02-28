using UnityEngine;
using Mirror;

public class NetworkDisappears : NetworkBehaviour
{
    [SerializeField] private bool m_triggered = false;
    [Tooltip("Not taken into account when type triggered")]
    [SerializeField] private float m_timeVisible = 5.0f;
    [SerializeField] private float m_timeDisappearing = 5.0f;
    [SerializeField] private float m_timeInvisible = 5.0f;
    [Tooltip("Percentage of alpha before switching to invisble")]
    [SerializeField] private float m_alphaValueBeforeDisappearing = 0.0f;
    [Tooltip("Fading based on time = 1\nSlower pace < 1\nFaster pace > 1")]
    [SerializeField] private float m_fadingPace = 1.0f;    

    private float m_timer = 0.0f;

    private bool m_isVisible = true;
    private bool m_isDisappearing = false;
    private bool m_isInvisible = false;    

    private Renderer m_renderer = null;
    private Collider m_collider = null;

    private void Start()
    {
        m_renderer = GetComponent<Renderer>();
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in childColliders)
        {
            if (!collider.isTrigger)
            {
                m_collider = collider;
                break;
            }
        }
        m_isVisible = true;
        m_timer = 0.0f;
    }   

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_triggered)
        {            
            var runnerSM = other.transform.root.gameObject.GetComponentInChildren<RunnerSM>();

            if (runnerSM != null)
            {                
                CMD_StartDisappearing();
            }
        }       
    }

    [Command (requiresAuthority = false)]
    private void CMD_StartDisappearing()
    {        
        m_isVisible = false;
        m_isDisappearing = true;
    }

    [Server]
    private void ServerUpdate()
    {
        if (m_isVisible && m_triggered)
        {
            return;
        }

        if (m_isVisible)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_timeVisible)
            {
                m_isVisible = false;
                m_isDisappearing = true;
                m_timer = 0.0f;
            }
        }

        if (m_isDisappearing)
        {
            m_timer += Time.deltaTime;

            float alphaValue = Mathf.Clamp01(1 - (m_timer / m_timeDisappearing) * m_fadingPace);
            UpdateClientsAlpha(alphaValue);

            if (m_timer >= m_timeDisappearing || alphaValue <= m_alphaValueBeforeDisappearing)
            {
                m_isDisappearing = false;
                m_isInvisible = true;
                EnableClientsRendererAndCollider(false);
                m_timer = 0.0f;
            }
        }

        if (m_isInvisible)
        {
            m_timer += Time.deltaTime;

            if (m_timer >= m_timeInvisible)
            {
                m_isInvisible = false;
                m_isVisible = true;
                EnableClientsRendererAndCollider(true);
                UpdateClientsAlpha(1);
                m_timer = 0.0f;
            }
        }
    }

    [ClientRpc]
    private void UpdateClientsAlpha(float alphaValue)
    {
        Color color = m_renderer.material.color;
        color.a = alphaValue;
        m_renderer.material.color = color;
    }

    [ClientRpc]
    private void EnableClientsRendererAndCollider(bool value)
    {
        m_renderer.enabled = value;
        m_collider.enabled = value;
    }
}
