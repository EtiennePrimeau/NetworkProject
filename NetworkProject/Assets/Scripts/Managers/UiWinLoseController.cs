using UnityEngine;
using Mirror;
using TMPro;

public class UiWinLoseController : NetworkBehaviour
{
    [SerializeField] private GameObject m_victoryScreen;
    [SerializeField] private GameObject m_defeatScreen;
    [SerializeField] private float m_fadeDuration = 1.0f;

    private UnityEngine.UI.Image m_victoryScreenImage;
    private TMP_Text m_victoryScreenText;
    private UnityEngine.UI.Image m_defeatScreenImage;
    private TMP_Text m_defeatScreenText;

    private Color m_initialVictoryColor;
    private Color m_initialDefeatColor;

    private bool m_playedEndMusic = false;

    private float m_timer = 0.0f;
    private int m_targetFontSize = 35;

    private void Start()
    {
        m_victoryScreenImage = m_victoryScreen.GetComponentInChildren<UnityEngine.UI.Image>();
        m_victoryScreenText = m_victoryScreen.GetComponentInChildren<TMP_Text>();
        m_defeatScreenImage = m_defeatScreen.GetComponentInChildren<UnityEngine.UI.Image>();
        m_defeatScreenText = m_defeatScreen.GetComponentInChildren<TMP_Text>();

        m_initialVictoryColor = m_victoryScreenImage.color;
        m_initialDefeatColor = m_defeatScreenImage.color;

        m_victoryScreenImage.color = Color.clear;
    }

    private void Update()
    {
        if (m_victoryScreen.gameObject.activeSelf == true && m_timer < m_fadeDuration)
        {
            m_timer += Time.deltaTime;
            float progress = Mathf.Clamp01(m_timer / m_fadeDuration);

            m_victoryScreenImage.color = Color.Lerp(Color.clear, m_initialVictoryColor, progress);
            m_victoryScreenText.fontSize = Mathf.Lerp(m_victoryScreenText.fontSize, m_targetFontSize, progress);
        }

        if (m_defeatScreen.gameObject.activeSelf == true && m_timer < m_fadeDuration)
        {
            m_timer += Time.deltaTime;
            float progress = Mathf.Clamp01(m_timer / m_fadeDuration);

            m_defeatScreenImage.color = Color.Lerp(Color.clear, m_initialDefeatColor, progress);
            m_defeatScreenText.fontSize = Mathf.Lerp(m_defeatScreenText.fontSize, m_targetFontSize, progress);
        }
    }

    [ClientRpc]
    public void RPC_EnableVictoryScreen()
    {        
        if (isLocalPlayer)
        {
            m_victoryScreen.SetActive(true);
        }       
    }

    [ClientRpc]
    public void RPC_EnableDefeatScreen()
    {   
        if (isLocalPlayer) 
        {
            m_defeatScreen.SetActive(true);
        }        
    }  

}
