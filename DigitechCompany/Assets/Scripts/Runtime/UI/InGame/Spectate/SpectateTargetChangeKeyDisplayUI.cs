using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpectateTargetChangeKeyDisplayUI : MonoBehaviour
{
    public enum SpectateChangeTarget { Prev, Next }

    [SerializeField] private SpectateChangeTarget target;
    private TextMeshProUGUI text;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if(target == SpectateChangeTarget.Prev)
            text.text = "< " + InputString.GetString(UserInput.input.Spectator.Change, 0);
        else
            text.text = InputString.GetString(UserInput.input.Spectator.Change, 1) + " >";
    }
}
