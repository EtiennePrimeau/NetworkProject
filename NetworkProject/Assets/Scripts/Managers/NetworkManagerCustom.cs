using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] public Identifier Identifier { get; private set; }
    [SerializeField] public NetworkMatchManager MatchManager { get; private set; }

    [Header("Lobby")]
    [SerializeField] private NetworkRoomPlayer roomPlayerPrefab = null;
    [Scene][SerializeField] private string lobbyScene = string.Empty; // must use ActiveScene().path
    [SerializeField] private int minPlayers = 2;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayer gamePlayerPrefab = null;
    //[SerializeField] private GameObject playerSpawnSystem = null;
    //[SerializeField] private GameObject roundSystem = null;

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

        if (SceneManager.GetActiveScene().path != lobbyScene) //stops players joining while in-game
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().path == lobbyScene)
        {
            bool isLeader = false;
            if (RoomPlayers.Count == 0)
            {
                isLeader = true;
            }

            NetworkRoomPlayer roomPlayerInstance = Instantiate(roomPlayerPrefab);

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
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void StartGame()
    {
        Debug.Log("Starting game // change scene");
        foreach (var player in RoomPlayers)
        {
            Debug.Log(player.DisplayName + " has joined in team " + player.PlayerType);

        }
        
        if (SceneManager.GetActiveScene().path == lobbyScene)
        {
            if (!IsReadyToStart()) { return; }
        
            //mapHandler = new MapHandler(mapSet, numberOfRounds);
        
            ServerChangeScene("Level_01");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (SceneManager.GetActiveScene().path == lobbyScene && newSceneName.StartsWith("Level"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                gameplayerInstance.SetPlayerType(RoomPlayers[i].PlayerType);

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);

        Debug.Log(SceneManager.GetActiveScene().name + " // with game players : ");
        Debug.Log("GamePlayers count : " + GamePlayers.Count);
        foreach (var player in GamePlayers)
        {
            Debug.Log(player.GetDisplayName() + " has transferred to level in team " + player.GetPlayerType());

        }

    }

    public override void OnServerSceneChanged(string sceneName)
    {
        //if (sceneName.StartsWith("Scene_Map"))
        //{
        //    GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
        //    NetworkServer.Spawn(playerSpawnSystemInstance);
        //
        //    GameObject roundSystemInstance = Instantiate(roundSystem);
        //    NetworkServer.Spawn(roundSystemInstance);
        //}
    }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }
}
