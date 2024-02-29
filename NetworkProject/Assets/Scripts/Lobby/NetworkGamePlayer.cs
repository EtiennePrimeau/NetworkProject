using Mirror;
//using System.Diagnostics;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    private EPlayerType m_playerType;

    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerCustom room;
    private NetworkManagerCustom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerCustom;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetPlayerType(EPlayerType type)
    {
        this.m_playerType = type;
    }

    public string GetDisplayName()
    {
        return displayName;
    }
    
    public EPlayerType GetPlayerType()
    {
        return m_playerType;
    }
}
