using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class ClaymoreTrap : NetworkObject
{
    [SerializeField] private Transform ray;
    [SerializeField] private float detectDistance;
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionDamage;
    [SerializeField] private GameObject effect;

    private bool isExplosion;
    private LineRenderer lr;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();

        TrapRoutine().Forget();
    }

    private async UniTask TrapRoutine()
    {
        var repeat = true;
        while (repeat)
        {
            lr.SetPosition(0, ray.position);
            Debug.DrawRay(ray.position, ray.forward, Color.cyan);
            if (Physics.Raycast(ray.position, ray.forward, out var hit, detectDistance))
            {
                Debug.Log(hit.collider.name);
                lr.SetPosition(1, ray.transform.position + ray.forward * hit.distance);
                if (hit.collider.TryGetComponent<InGamePlayer>(out _))
                {
                    photonView.RPC(nameof(SendExplosionToAllRpc), RpcTarget.All);
                    repeat = false;
                }
            }
            else
            {
                lr.SetPosition(1, ray.transform.position + ray.forward * detectDistance);
            }

            await UniTask.WaitForSeconds(0.2f);
        }
    }

    private async UniTask ExplosionRoutine()
    {
        await UniTask.WaitForSeconds(0.5f);

        var explosionHits =
                Physics.SphereCastAll(transform.position, explosionRange, Vector3.up, explosionRange, LayerMask.GetMask("Player"))
                .Where(col => col.collider is CharacterController);

        foreach (var explosion in explosionHits)
        {
            var distanceRatio = explosion.distance / explosionRange;
            var damageFactor = distanceRatio < 0.5f ? 1 : distanceRatio * 2;
            explosion.collider.GetComponent<InGamePlayer>().Damage(explosionDamage * damageFactor);
        }
        GetComponent<MeshRenderer>().enabled = false;
        lr.enabled = false;
        effect.SetActive(true);
        Debug.Log("Explosion");

        await UniTask.WaitForSeconds(3f);
        Destroy(gameObject);
    }

    [PunRPC]
    private void SendExplosionToAllRpc()
    {
        if (isExplosion) return;

        isExplosion = true;
        ExplosionRoutine().Forget();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(ray.position, ray.position + ray.forward * detectDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}