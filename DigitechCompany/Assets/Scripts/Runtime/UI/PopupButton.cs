using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupButton : MonoBehaviour
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();

    [SerializeField] private TextMeshProUGUI text;
    private Button button;
    private Action onClick;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke();
            popupUI.Close();
        });
    }

    public void Initialize(string explain, Action onClick)
    {
        text.text = explain;
        this.onClick = onClick;
    }
}