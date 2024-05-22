using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableDisplay : MonoBehaviour
{
    //service
    private Player player;
    private Player Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<Player>();
            return player;
        }
    }

    //inspector field
    [SerializeField] InputActionAsset playerActionAsset;
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image requireTime;

    private InputAction[] interactActions = new InputAction[(int)InteractID.End];

    private void Start()
    {
        for (int i = 1; i < (int)InteractID.End; i++)
            interactActions[i] = playerActionAsset[$"Interact{i}"];
    }

    private void Update()
    {
        if (ReferenceEquals(Player, null)) return;
        
        display.SetActive(Player.LookInteractable != null);

        requireTime.fillAmount =
            Player.LookInteractable != null && Player.LookInteractable.GetInteractRequireTime(Player) > 0 ?
            Player.InteractRequireTimes[(int)Player.LookInteractable.GetTargetInteractID(Player)] / Player.LookInteractable.GetInteractRequireTime(Player) :
            0;

        if (display.activeSelf)
        {
            var targetInteractId = Player.LookInteractable.GetTargetInteractID(Player);
            var targetAction = interactActions[(int)targetInteractId].bindings[0];
            var keyString = InputControlPath.ToHumanReadableString(targetAction.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames);

            if (Player.LookInteractable.IsInteractable(Player))
                text.text = $"{Player.LookInteractable.GetInteractionExplain(Player)} ({keyString})";
            else //only display explain
                text.text = Player.LookInteractable.GetInteractionExplain(Player);
        }
    }
}