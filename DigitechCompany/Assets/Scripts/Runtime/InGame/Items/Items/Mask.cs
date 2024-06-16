using UnityEngine;
using Photon.Pun;

public class Mask : ItemBase
{
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public override void OnUsePressed(InteractID id)
    {
        base.OnUsePressed(id);
        targetPosition = new Vector3(0, 0.2f, 0.1f);
        targetRotation = Quaternion.Euler(270, 180, 0);
    }

    [PunRPC]
    protected override void OnUsePressedRpc(int id)
    {
        base.OnUsePressedRpc(id);
    }

    public override void OnUseReleased()
    {
        base.OnUseReleased();
        targetPosition = camHoldPos;
        targetRotation = Quaternion.Euler(camHoldRot);
    }

    [PunRPC]
    protected override void OnUseReleasedRpc()
    {
        base.OnUseReleasedRpc();
    }

    protected override void Update()
    {
        base.Update();
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 3);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * 3);
    }
}