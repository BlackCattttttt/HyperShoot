using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldKey : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text placeHodler;
    [SerializeField] private string action;

    private void Awake()
    {
        inputField.onSubmit.AddListener(HandleMessageSubmit);
        inputField.onEndEdit.AddListener(HandleMessageSubmit);
    }
    private void OnEnable()
    {
        inputField.text = "";
        placeHodler.text = GameManager.Instance.Data.Buttons[action].ToString();
    }
    public void HandleMessageSubmit(string value)
    {
        value = value.Trim();
        if (!string.IsNullOrEmpty(value) && value != " ")
        {
            Dictionary<string, KeyCode> input = GameManager.Instance.Data.Buttons;
            KeyCode thisKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), value.ToUpper());
            foreach (var key in input.Values)
            {
                if (thisKeyCode == key)
                {
                    // inputField.Select();
                    inputField.text = "";
                    Debug.Log("key has been used");
                    return;
                }
            }
            GameManager.Instance.Data.Buttons[action] = thisKeyCode;
            Database.SaveData();
        }
        else
        {
            inputField.placeholder.gameObject.SetActive(true);
        }
    }
}
