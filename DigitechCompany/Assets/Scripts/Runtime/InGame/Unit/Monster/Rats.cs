using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BehaviorTree;
using UnityEngine.AI;
using Photon.Pun;
using UniRx;


public class Rats : MonsterBase
{
    public enum RatsState
    {
        Idle, Searching, Stolen, Attack, Guard, Bring, Protect, Death
    }

    public static Vector3 NestPosition;
    public static List<ItemBase> itemsInNest = new();

    [Header("Rats Setting")]
    [SerializeField]
    private Weapon weapon;
    [SerializeField]
    private RatsState state = RatsState.Idle;
    [SerializeField]
    private LayerMask obstacleLayer;
    private int targetPlayerViewId = 0;

    [SerializeField]
    private int maxSearchCount = 3;
    private int curSearchCount = 0;
    

    [SerializeField]
    private float itemDetectRange;

    [Header("Texture")]
    [SerializeField] private new SkinnedMeshRenderer renderer;
    [SerializeField] private Texture2D baseTexture;
    [SerializeField] private Texture2D madTexture;
    [SerializeField] private Texture2D emissionMap;

    private bool isArrive = true;
    private bool isAlreadySetRandomPoint = false;
    private Vector3 originItemSize = Vector3.zero;
    private ItemBase targetItem = null;

    private InGamePlayer TargetPlayer;
    

    protected override void Death()
    {
        base.Death();
    }

    protected override void Spawn()
    {
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        maxStats.ChangeFrom(testBaseStat);
        weapon.Initialize(this);
        if (NestPosition == Vector3.zero)
        {
            NestPosition = transform.position;
        }
        tree = new BehaviorTree.Tree(new Loop(new List<Node> {
            new Action(() => CheckItemInNest()),
            new Action(() => CheckPlayerFromNest()),
            new Action(() => DetectDoor()),
            new Sequence(new List<Node>
            {
                new Action(() => CheckDeath()),
                
                new Selector(
                    new List<Node>
                    {

                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Idle)),
                            new Action(() => ReturnToNest())
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Searching)),
                            new Action(() => SearchItem()),
                            new CheckEnemyInFOV(fov,true),
                            new Action(() => SetState(RatsState.Guard)),
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Bring)),
                            new Action(() => GoToItem()),
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Guard)),
                            new Action(() => GuardFromPlayer())
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Attack)),
                            new Action(() => CheckTarget()),
                            new Action(() => FollowTarget()),
                            new Action(() => AttackTarget()),
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(RatsState.Protect)),
                            new Action(() => ProtectNest())
                        })
                    }
                )
            }) }
            
        )) ;
    }

    protected override void Start()
    {
        base.Start();
        var itemManager = ServiceLocator.For(this).Get<ItemManager>();
        //itemManager?.SpawnItem(waypoints[0], "Shovel");

        //this
        //    .ObserveEveryValueChanged(t => targetPlayerViewId)
        //    .Subscribe(viewid =>
        //    {
        //        if(viewid == 0)
        //        {
        //            TargetPlayer = null;
        //            return;
        //        }

        //        TargetPlayer = PhotonView.Find(viewid).GetComponent<InGamePlayer>();
        //    });
    }

    protected override void Update()
    {
        if(photonView.IsMine)
        {
            base.Update();
            Debug.Log($"Agent Stop {agent.isStopped} + {gameObject.name}");
            animator.SetBool("IsRun", !agent.isStopped);
        } else
        {
            FixTransform();
        }
        switch (state)
        {
            case RatsState.Attack:
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
                break;
            case RatsState.Protect:
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                break;
            default:
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                break;
        }

    }

    private void ChangeTexture(bool isMad)
    {
        if (isMad)
        {
            renderer.material.SetTexture("_BaseMap", madTexture);
            renderer.material.SetTexture("_EmissionTex", emissionMap);
            renderer.material.SetColor("_EmissionColor", Color.white * 5);
        }
        else
        {
            renderer.material.SetTexture("_BaseMap", baseTexture);
            renderer.material.SetTexture("_EmissionTex", null);
            renderer.material.SetColor("_EmissionColor", Color.black);
        }
    }

    private NodeState CheckState(RatsState _state,bool reverse = false)
    {
        if (state != _state) return reverse ? NodeState.Succes : NodeState.Failure;
        //Debug.Log(_state);
        return reverse ? NodeState.Failure : NodeState.Succes;
    }

    private NodeState SetState(RatsState _state)
    {
        state = _state;
        return NodeState.Succes;
    }

    private NodeState CheckItemInNest()
    {
        if(TargetPlayer != null && !TargetPlayer.IsDie) return NodeState.Succes;
        ItemBase stolenItem = null;
        foreach(var item in itemsInNest)
        {
            if(item.CurUnit is InGamePlayer)
            {
                stolenItem = item;
                TargetPlayer = item.CurUnit as InGamePlayer;
                //targetPlayerViewId = TargetPlayer.photonView.ViewID;
                ChangeTexture(true);
                state = RatsState.Attack;
                return NodeState.Succes;
            }
        }
        if(stolenItem != null)
            itemsInNest.Remove(stolenItem);
        return NodeState.Failure;
    }

    private NodeState CheckPlayerFromNest()
    {
        if (state.Equals(RatsState.Attack) || state.Equals(RatsState.Idle) || itemsInNest.Count == 0 ) return NodeState.Failure;
        if(itemsInNest.Count == 0) return NodeState.Failure;    
        Collider[] hits = Physics.OverlapBox(NestPosition, Vector3.one * itemDetectRange, Quaternion.identity, LayerMask.GetMask("Player"));
        var players = hits.Select(x => x.GetComponent<InGamePlayer>()).ToArray();
        if (players.Length > 0)
        {
            state = RatsState.Protect;
            return NodeState.Succes;
        }
        return NodeState.Failure;
    }

    //µ’¡ˆ∑Œ ±Õ»Ø
    protected NodeState ReturnToNest()
    {
        if (agent.isStopped) agent.isStopped = false;
        Move(NestPosition);
        if(Vector3.Distance(transform.position, GetNavMeshPosition(NestPosition)) < 0.6f)
        {
            
            DropItem();
            Debug.Log(targetItem != null);
            state = RatsState.Searching;
            curSearchCount = 0;
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    private NodeState SearchItem()
    {
        if (curSearchCount == maxSearchCount)
        {
            state = RatsState.Idle;
            return NodeState.Succes;
        }
        Debug.Log($"Current Search Count {curSearchCount}");
        if(isArrive)
        {
            isArrive = false;
            curIndex = Random.Range(0, waypoints.Count);
        }
        if (!SetDestinationToPosition(waypoints[curIndex]))
        {
            Debug.LogError($"Can't go this waypoint {waypoints[curIndex]}");
            return NodeState.Failure;
        }
        if (agent.isStopped) agent.isStopped = false;
        Move(destination);
        if (DetectItem())
        {
            isArrive = true;
            curSearchCount = 0;
            state = RatsState.Bring;
            return NodeState.Succes;
        }
        if (Vector3.Distance(transform.position, destination) < 1f)
        {
            curSearchCount++;

            isArrive = true;
        }
        return NodeState.Running;
    }

    private NodeState GoToItem()
    {
        if(targetItem == null)
        {
            state = RatsState.Idle;
            return NodeState.Failure;
        }
        if(targetItem != null && targetItem.CurUnit != null)
        {
            targetItem = null;
            state = RatsState.Idle;
            return NodeState.Failure;
        }
        if (agent.isStopped) agent.isStopped = false;
        Move(targetPos);
        if(PickItem())
        {
            state = RatsState.Idle;
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    private NodeState GuardFromPlayer()
    {
        var targetPlayers = fov.TargetUnits.Where(x => x is InGamePlayer).Select(x => x as InGamePlayer).Where(x => !x.IsDie).ToArray();
        if (targetPlayers.Any())
        {
            agent.isStopped = true;
            var player = targetPlayers.First();
            RotateToDir(player.transform.position);
            return NodeState.Running;
        } 
        else
        {
            agent.isStopped = false;
            state = RatsState.Searching;
            return NodeState.Failure;
        }
    }

    private NodeState CheckTarget()
    {
        if(TargetPlayer == null || (TargetPlayer != null && TargetPlayer.IsDie))
        {
            ChangeTexture(false);
            TargetPlayer = null;
            if (DetectItem())
            {
                isArrive = true;
                curSearchCount = 0;
                state = RatsState.Bring;
            }
            else
            {
                isArrive = true;
                curSearchCount = 0;
                state = RatsState.Idle;
            }
            return NodeState.Failure;
        } else
        {
            return NodeState.Succes;
        }
    }

    private NodeState FollowTarget()
    {
        DropItem();
        if (isAttacking) return NodeState.Failure;
        if (SetDestinationToPosition(TargetPlayer.transform.position))
        {
            if (agent.isStopped) agent.isStopped = false;
            Move(destination);
            if (Vector3.Distance(transform.position, TargetPlayer.transform.position) < 0.6f)
            {
                return NodeState.Succes;
            }
        }
        return NodeState.Failure;
    }

    private NodeState AttackTarget()
    {
        agent.isStopped = true;
        isAttacking = true;
        Attack();
        return NodeState.Succes;
    }

    private NodeState ProtectNest()
    {
        if (Vector3.Distance(transform.position, NestPosition) < 2.5f)
        {
            Debug.Log("Protecting");
            DropItem();
            agent.isStopped = true;
            //AvoidOtherMonsters();
            Collider[] hits = Physics.OverlapBox(NestPosition, Vector3.one * itemDetectRange, Quaternion.identity, LayerMask.GetMask("Player"));
            var players = hits.Select(x => x.GetComponent<InGamePlayer>()).ToArray();
            if(players.Length == 0)
            {
                agent.isStopped = false;
                state = RatsState.Idle; 
                return NodeState.Succes;
            }
            else if(players.Length > 0)
            {
                Debug.Log("Looking");
                RotateToDir(players[0].transform.position);
            }
        } else
        {
            agent.isStopped = false;
            Debug.Log("Going To Protect");
            Move(NestPosition);
        }
        return NodeState.Running;
    }

    protected override void Attack()
    {
        isAttacking = true;
        animator.SetInteger("AttackState", Random.Range(1, 4));
        animator.SetTrigger(Animator_AttackHash);
        base.Attack();
    }

    [PunRPC]
    protected override void AttackRPC()
    {
        isAttacking = true;
        animator.SetInteger("AttackState", Random.Range(1, 4));
        animator.SetTrigger(Animator_AttackHash);
    }

    public void OnAttack()
    {
        if (!photonView.IsMine) return;
        if (TargetPlayer == null) return;
        if (Vector3.Distance(transform.position, TargetPlayer.transform.position) < 1.25f)
        {
            TargetPlayer?.Damage(attackDamage, this);
        }
    }

    public void OnAttackAnimEnd()
    {
        animator.SetInteger("AttackTarget", 0);
        agent.isStopped = false;
        isAttacking = false;
    }

    private bool PickItem()
    {
        photonView.RPC(nameof(PickItemRPC), RpcTarget.Others, (TargetPlayer == null) ? 0 : targetItem.photonView.ViewID);
        if (Vector3.Distance(transform.position, targetPos) < 0.7f)
        {
            originItemSize = targetItem.transform.localScale;
            targetItem.OnInteract(this);
            targetItem.transform.localPosition = Vector3.zero;
            targetItem.transform.localScale *= itemHolder.localScale.x;
            weapon.gameObject.SetActive(false);
            
            return true;
        }
        return false;
    }

    [PunRPC]
    private void PickItemRPC(int viewId)
    {
        if(PhotonView.Find(viewId).TryGetComponent(out ItemBase item))
        {
            targetItem = item;
            if (Vector3.Distance(transform.position, targetPos) < 0.7f)
            {
                originItemSize = targetItem.transform.localScale;
                targetItem.transform.localPosition = Vector3.zero;
                targetItem.transform.localScale *= itemHolder.localScale.x;
                weapon.gameObject.SetActive(false);
            }
        }
        
    }

    private bool DropItem()
    {
        photonView.RPC(nameof(DropItemRPC), RpcTarget.Others);
        if (targetItem == null || (targetItem != null && targetItem.CurUnit == null)) return false;
        if (targetItem.CurUnit.gameObject.Equals(gameObject))
        {
            itemsInNest.Add(targetItem);
            targetItem.OnDiscard();
            targetItem.transform.localScale = originItemSize;
            targetItem = null;
            weapon.gameObject.SetActive(true);
            return true;
        }
        return false;
    }

    [PunRPC]
    private void DropItemRPC()
    {
        if (targetItem == null || (targetItem != null && targetItem.CurUnit == null)) return;
        if (targetItem.CurUnit.gameObject.Equals(gameObject))
        {
            itemsInNest.Add(targetItem);
            targetItem.transform.localScale = originItemSize;
            targetItem = null;
            weapon.gameObject.SetActive(true);
        }
    }

    private bool DetectItem()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * itemDetectRange, Quaternion.identity, LayerMask.GetMask("Item"));
        var items = hits.Select(x => x.GetComponent<ItemBase>()).Where(x => !itemsInNest.Contains(x)).Where(x => x.CurUnit == null).ToArray();
        if (items.Length > 0)
        {
            if (targetItem == null)
            {
                foreach(var item in items)
                {
                    if (Vector3.Distance(transform.position, item.transform.position) <= itemDetectRange && !Physics.Linecast(transform.position, item.transform.position, obstacleLayer))
                    {
                        targetItem = item;
                        targetPos = GetNavMeshPosition(targetItem.transform.position, default(NavMeshHit));
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }

    public override void Damage(float damage, UnitBase attacker)
    {
        if (state != RatsState.Attack)
        {
            ChangeTexture(true);
            //targetPlayerViewId = TargetPlayer.photonView.ViewID;
            Collider[] hit = Physics.OverlapSphere(transform.position, 2.5f, LayerMask.GetMask("Monster"));
            foreach(Collider c in hit)
            {
                if(c.TryGetComponent(out Rats rat))
                {
                    rat.MakeAngry(attacker as InGamePlayer);
                }
            }
            TargetPlayer = attacker as InGamePlayer;
            state = RatsState.Attack;
        }
        base.Damage(damage, attacker);
    }

    public void MakeAngry(InGamePlayer player)
    {
        if (state != RatsState.Attack)
        {
            ChangeTexture(true);
            TargetPlayer = player;
            state = RatsState.Attack;
        }
        photonView.RPC(nameof(MakeAngryRPC), RpcTarget.Others, player.photonView.ViewID);
    }

    [PunRPC]
    private void MakeAngryRPC(int playerViewId)
    {
        if (state != RatsState.Attack)
        {
            ChangeTexture(true);
            TargetPlayer = PhotonView.Find(playerViewId).GetComponent<InGamePlayer>();
            state = RatsState.Attack;
        }
    }

    public override void CallMonsterToPos(Vector3 pos, InGamePlayer targetPlayer = null)
    {
        return;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if (stream.IsWriting)
        {
            stream.SendNext((int)state);
            stream.SendNext(targetPlayerViewId);
            stream.SendNext(targetPos);
            stream.SendNext(NestPosition);
            stream.SendNext(isArrive);
            stream.SendNext(curIndex);
        }
        else
        {
            state = (RatsState)(int)stream.ReceiveNext();
            targetPlayerViewId = (int)stream.ReceiveNext();
            targetPos = (Vector3)stream.ReceiveNext();
            NestPosition = (Vector3)stream.ReceiveNext();
            isArrive = (bool)stream.ReceiveNext();
            curIndex = (int)stream.ReceiveNext();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(255,0,0,0.4f);
        Gizmos.DrawSphere(destination, 0.5f);
    }
}
