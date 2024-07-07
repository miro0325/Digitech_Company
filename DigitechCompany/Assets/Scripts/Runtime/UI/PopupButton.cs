using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupButton : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Button button;
    private Action onClick;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        button = GetComponent<Button>();

        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void Initialize(string explain, Action onClick)
    {
        text.text = explain;
        this.onClick = onClick;
    }
}