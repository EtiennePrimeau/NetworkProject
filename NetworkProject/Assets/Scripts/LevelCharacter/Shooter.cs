using Mirror;
using UnityEngine;

enum EProjectileType
{
    Bomb,
    BigBomb
};

public class Shooter : NetworkBehaviour
{
    [SerializeField] private GameObject m_bombPrefab;
    [SerializeField] private GameObject m_bigBombPrefab;
    [SerializeField] private Camera m_camera;
    [SerializeField] private float m_bombCooldownTimer = 0.0f;
    [SerializeField] private float m_bombCooldownTimerMax = 2.0f;

    private EProjectileType m_currentProjectile;

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 direction = Vector3.zero;
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPosition = hit.point;
                direction = (hitPosition - transform.position).normalized;
            }
            else
            {
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = m_camera.nearClipPlane - 15;
                Vector3 screenPosition = m_camera.ScreenToWorldPoint(mousePosition);
                direction = (transform.position - screenPosition).normalized;
            }

            switch (m_currentProjectile)
            {
                case EProjectileType.Bomb:

                    if (m_bombCooldownTimer < 0)
                    {
                        CMD_ShootBomb(direction);
                        m_bombCooldownTimer = m_bombCooldownTimerMax;
                    }
                    break;
                case EProjectileType.BigBomb:

                    CMD_ShootBigBomb(direction);
                    NetworkManagerCustom.Instance.MatchManager.CMD_ShooterHasShot();
                    m_currentProjectile = EProjectileType.Bomb;
                    NetworkManagerCustom.Instance.MatchManager.CMD_ChangeArrows(false);
                    break;
                default:
                    break;
            }
        }
        HandleInput();
        m_bombCooldownTimer -= Time.deltaTime;
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (m_currentProjectile == EProjectileType.Bomb)
            {
                return;
            }
            m_currentProjectile = EProjectileType.Bomb;
            NetworkManagerCustom.Instance.MatchManager.CMD_SetShootBombBoolToTrue();
            NetworkMatchManager._Instance.CMD_ChangeArrows(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (NetworkManagerCustom.Instance.MatchManager.GetPermissionToShoot() == false)
            {
                //Debug.Log("Cant select weapon !");
                return;
            }

            m_currentProjectile = EProjectileType.BigBomb;
            NetworkMatchManager._Instance.CMD_ChangeArrows(true);
            //Debug.Log("called matchmanager");
        }

    }

    [Command(requiresAuthority = false)]
    public void CMD_ShootBomb(Vector3 direction)
    {
        var bomb = Instantiate(m_bombPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(bomb);

        bomb.GetComponent<BombNetwork>().CMD_Shoot(direction);
    }

    [Command(requiresAuthority = false)]
    public void CMD_ShootBigBomb(Vector3 direction)
    {
        var bigBomb = Instantiate(m_bigBombPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(bigBomb);

        bigBomb.GetComponent<BombNetwork>().CMD_Shoot(direction);
    }

    public void SetCamera(Camera camera)
    {
        m_camera = camera;
    }

    public float GetBulletRemainingPercentage()
    {
        if (m_bombCooldownTimer < 0)
        {
            return 0.0f;
        }
        return m_bombCooldownTimer / m_bombCooldownTimerMax;
    }

}
