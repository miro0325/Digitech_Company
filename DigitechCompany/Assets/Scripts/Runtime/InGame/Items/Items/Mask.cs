using UnityEngine;
using Photon.Pun;

public partial class Mask : ItemBase
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public override bool IsUsable(InteractID id)
    {
        return id == InteractID.ID2;
    }

    public override void OnActive()
    {
        base.OnActive();
        targetPosition = camHoldPos;
        targetRotation = Quaternion.Euler(camHoldRot);
    }

    public override void OnUsePressed(InteractID id)
    {
        Debug.Log("Mask Pressed");
        base.OnUsePressed(id);
        targetPosition = new Vector3(0, 0.2f, 0.1f);
        targetRotation = Quaternion.Euler(270, 180, 0);
    }

    public override void OnUseReleased()
    {
        Debug.Log("Mask Released");
        base.OnUseReleased();
        targetPosition = camHoldPos;
        targetRotation = Quaternion.Euler(camHoldRot);
    }

    protected override void Update()
    {
        base.Update();

        if (InHand)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 5);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 5);
        }
    }
}