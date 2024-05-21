using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractableDisplay : MonoBehaviour
{
    //service
    private Player player;
    private GameManager gameManager;

    //inspector field
    [SerializeField] InputActionAsset playerActionAsset;
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image requireTime;

    private InputAction[] interactActions = new InputAction[(int)InteractID.End];

    private void Start()
    {
        gameManager = ServiceLocator.For(this).Get<GameManager>();
        gameManager.OnInitializeComplete += () =>
        {
            player = ServiceLocator.For(this).Get<Player>();

            for (int i = 1; i < (int)InteractID.End; i++)
                interactActions[i] = playerActionAsset[$"Interact{i}"];
        };
    }

    private void Update()
    {
        if (ReferenceEquals(player, null)) return;
        
        display.SetActive(player.LookInteractable != null);

        requireTime.fillAmount =
            player.LookInteractable != null && player.LookInteractable.GetInteractRequireTime(player) > 0 ?
            player.InteractRequireTimes[(int)player.LookInteractable.GetTargetInteractID(player)] / player.LookInteractable.GetInteractRequireTime(player) :
            0;

        if (display.activeSelf)
        {
            var targetInteractId = player.LookInteractable.GetTargetInteractID(player);
            var targetAction = interactActions[(int)targetInteractId].bindings[0];
            var keyString = InputControlPath.ToHumanReadableString(targetAction.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice | InputControlPath.HumanReadableStringOptions.UseShortNames);

            if (player.LookInteractable.IsInteractable(player))
                text.text = $"{player.LookInteractable.GetInteractionExplain(player)} ({keyString})";
            else //only display explain
                text.text = player.LookInteractable.GetInteractionExplain(player);
        }
    }
}