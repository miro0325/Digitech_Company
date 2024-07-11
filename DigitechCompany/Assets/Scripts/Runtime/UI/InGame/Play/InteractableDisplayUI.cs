using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableDisplayUI : MonoBehaviour
{
    //service
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private UserInput input => UserInput.input;

    //inspector field
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image requireTime;

    private void Update()
    {
        if (ReferenceEquals(player, null)) return;

        display.SetActive(player.LookInteractable != null);

        requireTime.fillAmount =
            player.LookInteractable != null && player.LookInteractable.GetInteractRequireTime(player) > 0 ?
            player.InteractRequireTimes[(int)player.LookInteractable.GetTargetInteractID(player)] / player.LookInteractable.GetInteractRequireTime(player) :
            0;

        if (player.LookInteractable != null)
        {

            if (player.LookInteractable.IsInteractable(player))
            {
                var targetInteractId = player.LookInteractable.GetTargetInteractID(player);
                var keyString = InputString.GetString(input.Player.Interact, (int)targetInteractId - 1);
                text.text = $"{player.LookInteractable.GetInteractionExplain(player)} ({keyString})";
            }
            else //only display explain
                text.text = player.LookInteractable.GetInteractionExplain(player);
        }
    }
}