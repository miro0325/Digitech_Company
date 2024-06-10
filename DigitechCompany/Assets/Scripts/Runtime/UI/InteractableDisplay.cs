using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableDisplay : MonoBehaviour
{
    //service
    private InGamePlayer player;
    private InGamePlayer Player
    {
        get
        {
            if (ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<InGamePlayer>();
            return player;
        }
    }

    //inspector field
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image requireTime;

    private UserInputAction userInput;

    private void Start()
    {
        userInput = new();
    }

    private void Update()
    {
        if (ReferenceEquals(Player, null)) return;

        display.SetActive(Player.LookInteractable != null);

        requireTime.fillAmount =
            Player.LookInteractable != null && Player.LookInteractable.GetInteractRequireTime(Player) > 0 ?
            Player.InteractRequireTimes[(int)Player.LookInteractable.GetTargetInteractID(Player)] / Player.LookInteractable.GetInteractRequireTime(Player) :
            0;

        if (Player.LookInteractable != null)
        {

            if (Player.LookInteractable.IsInteractable(Player))
            {
                var targetInteractId = Player.LookInteractable.GetTargetInteractID(Player);
                var keyString = InputControlPath.ToHumanReadableString
                (
                    userInput.Player.Interact.bindings[(int)targetInteractId - 1].effectivePath, 
                    InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames
                );
                text.text = $"{Player.LookInteractable.GetInteractionExplain(Player)} ({keyString})";
            }
            else //only display explain
                text.text = Player.LookInteractable.GetInteractionExplain(Player);
        }
    }
}