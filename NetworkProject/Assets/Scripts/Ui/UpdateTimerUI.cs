using TMPro;
using UnityEngine;

public class UpdateTimerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_timerText;

    void Update()
    {
        m_timerText.text = NetworkMatchManager._Instance.GetGameTimer().ToString();
    }
}
