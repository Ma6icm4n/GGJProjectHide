using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{

    [Header("UI")]
    [SerializeField] private TMP_InputField nameField = null;
    [SerializeField] private Button nameButton = null;

    public static string DisplayName { get; private set; }

    private const string PlayerNameKey = "PlayerName";



    // Start is called before the first frame update
    void Start() => SetUpInputField();

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerNameKey)) { return; }

        string defaultName = PlayerPrefs.GetString(PlayerNameKey);

        nameField.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        nameButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameField.text;

        PlayerPrefs.SetString(PlayerNameKey, DisplayName);
    }

}
