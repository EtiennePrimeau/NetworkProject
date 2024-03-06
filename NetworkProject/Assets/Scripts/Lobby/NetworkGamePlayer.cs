using Mirror;
//using System.Diagnostics;
//using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    public static int s_index = 0;
    private int m_index;
    private EPlayerType m_playerType;

    [SyncVar]
    private string m_displayName = "Loading...";

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
        this.m_displayName = displayName;
    }

    [Server]
    public void SetPlayerType(EPlayerType type)
    {
        this.m_playerType = type;

        if (type == EPlayerType.Shooter)
        {
            m_index = s_index;
            s_index++;
        }
    }

    public string GetDisplayName()
    {
        return m_displayName;
    }
    
    public EPlayerType GetPlayerType()
    {
        return m_playerType;
    }

    public int GetIndex()
    {
        return m_index;
    }
}
