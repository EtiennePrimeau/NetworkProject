using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNetworkHUD : NetworkManagerHUD
{
    [SerializeField]
    private NetworkManager m_manager;
    [SerializeField]
    private Button m_hostButton;
    [SerializeField]
    private Button m_joinButton;

    private void Start()
    {
        m_manager = GetComponent<NetworkManager>();
    }

    public void HostAndJoinGame()
    {
        m_manager.StartHost();
    }
}
