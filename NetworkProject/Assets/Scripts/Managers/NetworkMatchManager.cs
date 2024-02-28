using UnityEngine;
using Mirror;
using System.Collections.Generic;

public enum E_TriggerTypes
{
    OutOfVerticalMapBounds,
    Win,
    Count
}

public class NetworkMatchManager : NetworkBehaviour
{
    public static NetworkMatchManager _Instance { get; private set; }

    private List<NetworkConnectionToClient> ConnectedPlayers = new List<NetworkConnectionToClient>();

    [SerializeField] private float m_radius = 10.0f;
    [SerializeField] private float m_respawnHeight = 10.0f;

    [SyncVar] private float m_gameTimer = 0.0f;
    [SerializeField] private float m_maxGameTimer = 300.0f;

    [SyncVar] private float m_shootBombTimer = 0.0f;
    [SerializeField] private float m_maxShootBombTimer = 5.0f;

    [SyncVar] private bool m_canShootBomb = true;
    [SyncVar] private bool m_hasShotBomb = false;

    private bool m_gameTimerHasStarted = false;

    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this);
        }
        _Instance = this;


    }

    private void Start()
    {
        m_gameTimer = m_maxGameTimer;
    }

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }
    }

    private void ServerUpdate()
    {
        HandleGameTimer();
        HandleShootBombTimer();
    }

    private void HandleGameTimer()
    {
        if (!m_gameTimerHasStarted)
        {
            return;
        }
        
        if (m_gameTimer < 0)
        {
            CMD_ShooterWin();
            return;
        }
        m_gameTimer -= Time.deltaTime;
    }

    private void HandleShootBombTimer()
    {
        if (!m_hasShotBomb)
        {
            //Debug.Log("hasSHot is false");
            return;
        }
        //Debug.Log("hasShot is true");
        if (m_shootBombTimer < 0)
        {
            m_canShootBomb = true;
            m_hasShotBomb = false;
            return;
        }
        m_shootBombTimer -= Time.deltaTime;
    }

    public void LaunchGame()
    {
        //Debug.Log("Launching cinematic");
        foreach (var player in ConnectedPlayers)
        {
            var cinematic = player.identity.gameObject.GetComponentInChildren<LaunchCinematic>();
            //Debug.Log(player.m_name + " launched cinematic " + cinematic);
            cinematic.RPC_Launch();
        }
    }

    [Command(requiresAuthority = false)]
    public void StartGameTimer()
    {
        m_gameTimerHasStarted = true;
    }

    public void SetConnectedPlayersList(List<NetworkConnectionToClient> list)
    {
        ConnectedPlayers = list;
    }

    public int GetGameTimer()
    {
        return (int)m_gameTimer;
    }

    public bool GetBombAvailability()
    {
        return m_canShootBomb;
    }

    public float GetBombRemainingPercentage()
    {
        if (m_shootBombTimer < 0)
        {
            return 0.0f;
        }
        return m_shootBombTimer / m_maxShootBombTimer;
    }

    public bool GetPermissionToShoot()
    {
        if (m_canShootBomb)
        {
            CMD_ShooterHasSelectedBomb();
            return true;
        }

        return false;
    }

    [Command(requiresAuthority = false)]
    private void CMD_ShooterHasSelectedBomb()
    {
        //Debug.Log("resetting shootbombtimer");
        m_canShootBomb = false;
    }

    [Command(requiresAuthority = false)]
    public void CMD_SetShootBombBoolToTrue()
    {
        //Debug.Log("resetting shootbombBool to true");
        m_canShootBomb = true;
    }

    [Command(requiresAuthority = false)]
    public void CMD_ShooterHasShot()
    {
        //Debug.Log("resetting hasShot");
        m_shootBombTimer = m_maxShootBombTimer;
        m_hasShotBomb = true;
    }

    [Command(requiresAuthority = false)]
    public void CMD_SendPlayerAndTrigger(GameObject player, E_TriggerTypes triggerType)
    {
        //Debug.Log("in cmd");

        BoundsTriggeredByPlayer(player, triggerType);
    }

    [Server]
    private void BoundsTriggeredByPlayer(GameObject player, E_TriggerTypes triggerType)
    {
        //Debug.Log("We entered the 111111111111111111111111!!!");

        switch (triggerType)
        {
            case E_TriggerTypes.OutOfVerticalMapBounds:
                RespawnPlayerRandomCircle(player);
                break;
            case E_TriggerTypes.Win:
                CMD_RunnerWin();
                break;
            default:
                break;
        }
    }

    [ClientRpc]
    private void RespawnPlayerRandomCircle(GameObject player)
    {
        Vector2 randomPosOnCircle = RandomPosOnCircle();
        Vector3 randomPosition = new Vector3(randomPosOnCircle.x, m_respawnHeight, randomPosOnCircle.y);

        player.transform.position = randomPosition;
        player.transform.LookAt(new Vector3(0, m_respawnHeight, 0));
    }

    private Vector2 RandomPosOnCircle()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);

        float x = m_radius * Mathf.Cos(randomAngle);
        float y = m_radius * Mathf.Sin(randomAngle);

        return new Vector2(x, y);
    }

    [Command(requiresAuthority = false)]
    private void CMD_RunnerWin()
    {
        foreach (var connPlayer in ConnectedPlayers)
        {
            var uiManager = connPlayer.identity.gameObject.GetComponentInChildren<UiWinLoseController>();
            //if (connPlayer.m_tag == "Runner" && uiManager != null)
            //{
            //    uiManager.RPC_EnableVictoryScreen();
            //}
            //else
            //{
            //    uiManager.RPC_EnableDefeatScreen();
            //}
            var gamePlayer = connPlayer.identity.gameObject.GetComponentInChildren<NetworkGamePlayer>();
            if (uiManager != null && gamePlayer.GetPlayerType() == EPlayerType.Runner)
            {
                uiManager.RPC_EnableVictoryScreen();
            }
            else
            {
                uiManager.RPC_EnableDefeatScreen();
            }

        }
    }

    [Command(requiresAuthority = false)]
    private void CMD_ShooterWin()
    {
        foreach (var connPlayer in ConnectedPlayers)
        {
            var uiManager = connPlayer.identity.gameObject.GetComponentInChildren<UiWinLoseController>();
            //if (connPlayer.m_tag == "Shooter" && uiManager != null)
            //{
            //    uiManager.RPC_EnableVictoryScreen();
            //}
            //else
            //{
            //    uiManager.RPC_EnableDefeatScreen();
            //}
            var gamePlayer = connPlayer.identity.gameObject.GetComponentInChildren<NetworkGamePlayer>();
            if (uiManager != null && gamePlayer.GetPlayerType() == EPlayerType.Shooter)
            {
                uiManager.RPC_EnableVictoryScreen();
            }
            else
            {
                uiManager.RPC_EnableDefeatScreen();
            }

        }
    }

    [Command(requiresAuthority = false)]
    public void CMD_ChangeArrows(bool shootingBomb, NetworkConnectionToClient player = null)
    {


        Debug.Log("In changeArrows : not refactored yet");

        //int index = player.m_uiSlotIndex - 2;
        //
        //foreach (var connPlayer in ConnectedPlayers)
        //{
        //    if (connPlayer.m_tag == "Runner")
        //    {
        //
        //        continue;
        //    }
        //
        //    var manager = connPlayer.identity.gameObject.GetComponentInChildren<NetworkLevelPlayerController>();
        //    if (manager == null)
        //    {
        //        //Debug.Log("manager null");
        //        continue;
        //    }
        //    //Debug.Log("calling rpc");
        //    manager.TargetMovePlayerArrow(this.netIdentity.connectionToClient, index, shootingBomb);
        //}
    }
}

