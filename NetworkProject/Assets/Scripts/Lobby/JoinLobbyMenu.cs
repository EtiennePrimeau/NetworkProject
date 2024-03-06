using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerCustom m_networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject m_landingPagePanel = null;
    [SerializeField] private TMP_InputField m_ipAddressInputField = null;
    [SerializeField] private Button m_joinButton = null;

    private void OnEnable()
    {
        NetworkManagerCustom.OnClientConnected += HandleClientConnected;
        NetworkManagerCustom.OnClientDisconnected += HandleClientDisconnected;
        m_ipAddressInputField.text = m_networkManager.networkAddress;
    }

    private void OnDisable()
    {
        NetworkManagerCustom.OnClientConnected -= HandleClientConnected;
        NetworkManagerCustom.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        string ipAddress = m_ipAddressInputField.text;

        m_networkManager.networkAddress = ipAddress;
        m_networkManager.StartClient();

        m_joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        m_joinButton.interactable = true;

        gameObject.SetActive(false);
        m_landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        m_joinButton.interactable = true;
    }
}
