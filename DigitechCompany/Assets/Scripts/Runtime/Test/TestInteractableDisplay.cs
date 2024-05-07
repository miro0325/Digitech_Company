using UnityEngine;
using TMPro;

public class TestInteractableDisplay : MonoBehaviour
{
    //service
    private Player player;

    //inspector field
    [SerializeField] private GameObject display;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        player = Services.Get<Player>();
    }

    private void Update()
    {
        display.SetActive(player.LookInteractable != null);
        if(display.activeSelf)
        {
            text.text = player.LookInteractable.GetInteractionExplain(player);
        }
    }
}