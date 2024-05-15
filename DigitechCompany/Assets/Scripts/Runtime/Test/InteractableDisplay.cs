using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InteractableDisplay : MonoBehaviour
{
    //service
    private Player player;

    //inspector field
    [SerializeField] InputActionAsset playerActionAsset;
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;

    private InputAction[] interactActions = new InputAction[(int)InteractID.End];

    private void Start()
    {
        player = ServiceLocator.For(this).Get<Player>();

        for(int i = 1; i < (int)InteractID.End; i++)
            interactActions[i] = playerActionAsset[$"Interact{i}"];
    }

    private void Update()
    {
        display.SetActive(player.LookInteractable != null);
        if(display.activeSelf)
        {
            var targetAction = interactActions[(int)player.LookInteractable.GetTargetInteractID(player)].bindings[0];
            var keyString = InputControlPath.ToHumanReadableString(targetAction.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames);

            if(player.LookInteractable.IsInteractable(player)) //display interact key
                text.text = $"{player.LookInteractable.GetInteractionExplain(player)} ({keyString})";
            else //only display explain
                text.text = player.LookInteractable.GetInteractionExplain(player);

        }
    }
}