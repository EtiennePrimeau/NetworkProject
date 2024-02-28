using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private Button continueButton = null;

    public static string DisplayName { get; private set; }

    private void Start()
    {
        nameInputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string newText)
    {
        continueButton.interactable = !string.IsNullOrEmpty(newText);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;
    }
}
