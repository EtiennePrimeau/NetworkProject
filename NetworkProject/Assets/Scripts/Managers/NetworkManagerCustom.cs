using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Networking.PlayerConnection;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EPlayerType
{
    Runner,
    Shooter
}

public class NetworkManagerCustom : NetworkManager
{

    private static NetworkManagerCustom instance;
    public static NetworkManagerCustom Instance
    {
        get
        {
            if (instance != null) { return instance; }
            return instance = NetworkManager.singleton as NetworkManagerCustom;
        }
    }
    [field:SerializeField] public Identifier Identifier { get; private set; }
    [field:SerializeField] public NetworkMatchManager MatchManager { get; private set; }
    [field:SerializeField] public NetworkSpawner Spawner { get; private set; }

    [Header("Lobby")]
    [SerializeField] private NetworkRoomPlayer m_roomPlayerPrefab = null;
    [Scene][SerializeField] private string m_lobbyScene = string.Empty; // must use ActiveScene().path
    [SerializeField] private int m_minPlayers = 2;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer m_gamePlayerPrefab = null;
    [SerializeField] private GameObject m_runnerPrefab;
    [SerializeField] private GameObject m_shooterPrefab;
    //[SerializeField] private GameObject playerSpawnSystem = null;
    //[SerializeField] private GameObject roundSystem = null;
    [SerializeField] private GameObject m_mapPrefab;

    //private MapHandler mapHandler;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnectionToClient> OnServerReadied;
    public static event Action OnServerStopped;

    public List<NetworkRoomPlayer> RoomPlayers { get; } = new List<NetworkRoomPlayer>();
    public List<NetworkGamePlayer> GamePlayers { get; } = new List<NetworkGamePlayer>();

    //public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        //var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        //
        //foreach (var prefab in spawnablePrefabs)
        //{
        //    ClientScene.RegisterPrefab(prefab);
        //}
    }

    public override void OnClientConnect() //on client when connected to server
    {
        base.OnClientConnect();

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn) // on server when a client connects
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if (SceneManager.GetActiveScene().path != m_lobbyScene) //stops players joining while in-game
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().path == m_lobbyScene)
        {
            bool isLeader = false;
            if (RoomPlayers.Count == 0)
            {
                isLeader = true;
            }

            NetworkRoomPlayer roomPlayerInstance = Instantiate(m_roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayer>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();

        RoomPlayers.Clear();
        GamePlayers.Clear();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < m_minPlayers) { return false; }

        int runnerTeam = 0;
        int shooterTeam = 0;
        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }

            if (player.PlayerType == EPlayerType.Runner) { runnerTeam++; }
            if (player.PlayerType == EPlayerType.Shooter) { shooterTeam++; }
        }

        if (runnerTeam == 0 || shooterTeam == 0) { return false; }

        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == m_lobbyScene)
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene("Level_01");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        //if (SceneManager.GetActiveScene().path == m_lobbyScene && newSceneName.StartsWith("Level"))
        //{
        //}

        base.ServerChangeScene(newSceneName);
    }

    private void SwitchRoomPlayersToGamePlayers()
    {
        for (int i = RoomPlayers.Count - 1; i >= 0; i--)
        {
            GameObject playerInstance;
            if (RoomPlayers[i].PlayerType == EPlayerType.Runner)
            {
                playerInstance = Instantiate(m_runnerPrefab);
            }
            else
            {
                playerInstance = Instantiate(m_shooterPrefab);
            }

            NetworkGamePlayer playerInfos = playerInstance.GetComponent<NetworkGamePlayer>();
            playerInfos.SetDisplayName(RoomPlayers[i].DisplayName);
            playerInfos.SetPlayerType(RoomPlayers[i].PlayerType);

            var conn = RoomPlayers[i].connectionToClient;
            NetworkServer.Destroy(conn.identity.gameObject);
            NetworkServer.ReplacePlayerForConnection(conn, playerInstance);
        }
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //Debug.Log("OnServerSceneChanged");

        //watch out for clients not being ready
    }

    public override void OnClientSceneChanged()
    {
        //Debug.Log("OnClientSceneChanged");
        
        base.OnClientSceneChanged();    // Readies client here

        if (SceneManager.GetActiveScene().name.StartsWith("Level"))
        {
            Spawner.Spawn();

            //SwitchRoomPlayersToGamePlayers();
        }

    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        //Debug.Log("OnServerReady");
        
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);

        if (SceneManager.GetActiveScene().name == "Level_01")
        {
            if (!AllPlayersAreReady()) { return; }

            SwitchRoomPlayersToGamePlayers();

            MatchManager.SetConnectedPlayersList(GamePlayers);
            MatchManager.LaunchGame();
        }

    }

    private bool AllPlayersAreReady()
    {
        foreach (var player in RoomPlayers)
        {
            if (!player.connectionToClient.isReady)
            {
                //Debug.Log(player.DisplayName + " is not ready");
                return false;
            }
        }

        return true;
    }

    public override void OnValidate()
    {
        base.OnValidate();

        if (m_shooterPrefab != null && !m_shooterPrefab.TryGetComponent(out NetworkIdentity _))
        {
            Debug.LogError("NetworkManager - Player Prefab must have a NetworkIdentity.");
            m_shooterPrefab = null;
        }
        if (m_runnerPrefab != null && !m_runnerPrefab.TryGetComponent(out NetworkIdentity _))
        {
            Debug.LogError("NetworkManager - Player Prefab must have a NetworkIdentity.");
            m_runnerPrefab = null;
        }
        if (m_mapPrefab != null && !m_mapPrefab.TryGetComponent(out NetworkIdentity _))
        {
            Debug.LogError("NetworkManager - Player Prefab must have a NetworkIdentity.");
            m_mapPrefab = null;
        }

        if (m_shooterPrefab != null && spawnPrefabs.Contains(m_shooterPrefab))
        {
            Debug.LogWarning("NetworkManager - Player Prefab doesn't need to be in Spawnable Prefabs list too. Removing it.");
            spawnPrefabs.Remove(m_shooterPrefab);
        }
        if (m_runnerPrefab != null && spawnPrefabs.Contains(m_runnerPrefab))
        {
            Debug.LogWarning("NetworkManager - Player Prefab doesn't need to be in Spawnable Prefabs list too. Removing it.");
            spawnPrefabs.Remove(m_runnerPrefab);
        }
        if (m_mapPrefab != null && spawnPrefabs.Contains(m_mapPrefab))
        {
            Debug.LogWarning("NetworkManager - Player Prefab doesn't need to be in Spawnable Prefabs list too. Removing it.");
            spawnPrefabs.Remove(m_mapPrefab);
        }
    }

    GameObject SpawnLevel(SpawnMessage msg)
    {
        Debug.Log("Spawned at " + DateTime.Now.TimeOfDay);
        
        var level = Instantiate(m_mapPrefab, Spawner.transform);
        Identifier.AssignAllIds(Spawner.transform);

        return level;
    }

    public void UnSpawnLevel(GameObject spawned)
    {
        Destroy(spawned);
    }
    
    public override void RegisterClientMessages()
    {
        base.RegisterClientMessages();

        if (m_shooterPrefab != null)
            NetworkClient.RegisterPrefab(m_shooterPrefab);
        if (m_runnerPrefab != null)
            NetworkClient.RegisterPrefab(m_runnerPrefab);
        if (m_mapPrefab != null)
        {
            NetworkClient.RegisterPrefab(m_mapPrefab, SpawnLevel, UnSpawnLevel);
        }
    }
}
