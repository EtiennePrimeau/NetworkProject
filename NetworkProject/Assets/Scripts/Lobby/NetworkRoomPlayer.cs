using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRoomPlayer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject m_lobbyUI = null; // on only if belongs to localPlayer
    [SerializeField] private TMP_Text[] m_playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] m_playerTeamSelectionTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] m_playerReadyTexts = new TMP_Text[4];
    [SerializeField] private Button m_startGameButton = null; // on only if Host
    [SerializeField] private Button m_changeTeamButton = null; // interactable only if not ready

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleTeamSelectionChanged))]
    public EPlayerType PlayerType;
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            m_startGameButton.gameObject.SetActive(value);
        }
    }

    private NetworkManagerCustom room;
    private NetworkManagerCustom Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerCustom; // casting our netManager
        }
    }

    public override void OnStartAuthority() // called on object that belongs to localPlayer
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);

        m_lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);

        UpdateDisplay();
    }

    //public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleReadyStatusChanged(bool oldValue, bool newValue)
    {
        UpdateDisplay();
        HandleChangeTeamButton();
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();
    public void HandleTeamSelectionChanged(EPlayerType oldValue, EPlayerType newValue) => UpdateDisplay();

    private void UpdateDisplay() // updates UI
    {
        if (!isOwned)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.isOwned)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < m_playerNameTexts.Length; i++)
        {
            m_playerNameTexts[i].text = "Waiting For Player...";
            m_playerTeamSelectionTexts[i].text = string.Empty;
            m_playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            m_playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            m_playerTeamSelectionTexts[i].text = Room.RoomPlayers[i].PlayerType.ToString();
            m_playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        m_startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        //m_changeTeamButton.interactable = !IsReady; // only works on host (new function not command?)
        //HandleChangeTeamButton();

        Room.NotifyPlayersOfReadyState();
    }

    public void HandleChangeTeamButton()    // desynced
    {
        Debug.Log("ready = " + IsReady);
        m_changeTeamButton.interactable = !IsReady;
    }

    [Command]
    public void CmdSwitchTeam()
    {
        //needs to check if available spot
        
        if (PlayerType == EPlayerType.Runner)
        {
            PlayerType = EPlayerType.Shooter;
        }
        else if (PlayerType == EPlayerType.Shooter) // in case we add more player types
        {
            PlayerType = EPlayerType.Runner;
        }

        //Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        Room.StartGame();
    }
}
