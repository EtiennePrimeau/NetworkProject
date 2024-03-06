using Mirror;
//using System.Diagnostics;
//using System.Diagnostics;
using UnityEngine;

public class NetworkGamePlayer : NetworkBehaviour
{
    public static int s_index = 0;
    private int m_index;
    private EPlayerType m_playerType;

    [SyncVar]
    private string m_displayName = "Loading...";

    private NetworkManagerCustom manager;
    private NetworkManagerCustom Manager
    {
        get
        {
            if (manager != null) { return manager; }
            return manager = NetworkManager.singleton as NetworkManagerCustom;
        }
    }
    private void Awake()
    {
        Manager.GamePlayers.Add(this);
    }
    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
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
