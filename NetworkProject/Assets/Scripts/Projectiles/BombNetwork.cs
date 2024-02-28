using UnityEngine;
using Mirror;

public class BombNetwork : NetworkBehaviour
{
    [SerializeField] private float m_projectileSpeed = 100;
    [SerializeField] private float m_explosionForce = 10;
    [SerializeField] private float m_explosionRadius = 10;
    [SerializeField] private float m_destroyTime = 10;
    [SerializeField] private float m_explosionTime = 2;
    [SerializeField] private float m_height = 20;
    [SerializeField] private float m_expansionDivider = 3;
    [SerializeField] private Rigidbody m_rb;
    [SerializeField] private GameObject m_explosionVFX;

    private float m_destroyTimer = 0;
    private float m_explosionTimer = 0;
    private bool m_stuck = false;

    void Start()
    {
        m_destroyTimer = m_destroyTime;
        m_explosionTimer = m_explosionTime;

    }

    void Update()
    {
        if (!isServer)
        {
            return;
        }


        HandleTimer();
        transform.localRotation = Quaternion.Euler(0, 0, 0);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer)
        {
            return;
        }
        if (collision.gameObject.GetComponent<BombNetwork>() != null)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }
        if (collision.gameObject.GetComponent<RotatingArmAddForce>() != null)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }
        if (m_stuck)
        {
            return;
        }




        m_rb.velocity = Vector3.zero;

        //Debug.Log("collision name: " + collision.gameObject.name);
        int collidedGoIdx = NetworkManagerCustom.Instance.Identifier.GetIndex(collision.collider.gameObject);
        //Debug.Log("collided object idx: " + collidedGoIdx);
        CMD_SetParent(collision.transform.root, collidedGoIdx);
        m_stuck = true;
        transform.position = collision.contacts[0].point;
    }


    [Server]
    private void HandleTimer()
    {
        if (m_stuck)
        {
            if (m_explosionTimer < 0)
            {
                CMD_Explode();
                return;
            }
            Expand();
            m_explosionTimer -= Time.deltaTime;
        }
        
        if (m_destroyTimer < 0)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }
        m_destroyTimer -= Time.deltaTime;

    }

    [Command(requiresAuthority = false)]
    public void CMD_Shoot(Vector3 direction)
    {
        m_rb.AddForce(direction * m_projectileSpeed, ForceMode.Impulse);
    }

    [Command(requiresAuthority = false)]
    public void CMD_Explode()
    {
        //Debug.Log("explode");
        var surroundingObjects = Physics.OverlapSphere(transform.position, m_explosionRadius);

        foreach (var obj in surroundingObjects)
        {
            if (obj.GetComponent<SpawnLocalPlayer>() == null)
            {
                continue;
            }

            var rb = obj.GetComponent<Rigidbody>();
            //Debug.Log(rb.gameObject.name);
            if (rb == null || rb == m_rb)
            {
                continue;
            }

            rb.AddExplosionForce(m_explosionForce, transform.position, m_explosionRadius, m_height, ForceMode.Impulse);

            int collidedGoIdx = NetworkManagerCustom.Instance.Identifier.GetIndex(rb.gameObject);
            RPC_AddExplosionForce(collidedGoIdx);
        }

        Instantiate(m_explosionVFX, transform.position, Quaternion.identity);
        RPC_InstantiateExplosionVFX();        

        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RPC_InstantiateExplosionVFX()
    {
        Instantiate(m_explosionVFX, transform.position, Quaternion.identity);
    }

    [Command(requiresAuthority = false)]
    public void CMD_SetParent(Transform collidedTransformRoot, int collidedObjIdx)
    {
        transform.SetParent(collidedTransformRoot);

        var go = NetworkManagerCustom.Instance.Identifier.GetObjectAtIndex(collidedObjIdx);
        //Debug.Log("CMD: setting parent: " + go.name);
        transform.SetParent(go.transform);

        RPC_SetParent(collidedTransformRoot, collidedObjIdx);
    }

    [ClientRpc]
    public void RPC_SetParent(Transform collidedTransformRoot, int collidedObjIdx)
    {
        gameObject.SetActive(false);

        transform.SetParent(collidedTransformRoot);

        var go = NetworkManagerCustom.Instance.Identifier.GetObjectAtIndex(collidedObjIdx);
        //Debug.Log("RPC: setting parent: " + go.name);
        transform.SetParent(go.transform);
        gameObject.SetActive(true);

    }

    private void Expand()
    {
        float expansion = Time.deltaTime / m_expansionDivider;
        transform.localScale += new Vector3(expansion * transform.localScale.x, expansion * transform.localScale.y, expansion * transform.localScale.z);
    }

    [ClientRpc]
    private void RPC_AddExplosionForce(int collidedGoIdx)
    {
        //Debug.Log("rpc add explosion force");

        var go = NetworkManagerCustom.Instance.Identifier.GetObjectAtIndex(collidedGoIdx);
        var rb = go.GetComponent<Rigidbody>();

        if (rb == null)
        {
            //Debug.Log("no rb");
            return;
        }
        rb.AddExplosionForce(m_explosionForce, transform.position, m_explosionRadius, m_height, ForceMode.Impulse);
    }
}
