using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using BehaviorTree;
using UniRx;

public class Siren : MonsterBase
{
        private SoundManager soundManager => ServiceLocator.GetEveryWhere<SoundManager>();


    private enum SirenState
    {
        Searching, Attack, RunAway, Death
    }

    [SerializeField]
    private SirenState state;
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    Transform head;
    private bool isArrive = true;
    private bool isAlerting = false;
    private bool isRunning = false;
    
    protected override void Spawn()
    {
        testBaseStat.SetStat(Stats.Key.Hp, x => 100);
        maxStats.ChangeFrom(testBaseStat);
        tree = new BehaviorTree.Tree(new Loop(new List<Node> {
            new Sequence(new List<Node>
            {
                new Action(() => CheckDeath()),
                new Action(() => DetectDoor()),
                new Selector(
                    new List<Node>
                    {
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(SirenState.Searching)),
                            new Action(() => SearchPlayer()),
                            new Action(() => CheckPlayerInFOV()),
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(SirenState.Attack)),
                            new Action(() => FollowPlayer()),
                            new Action(() => DoAlert())
                        }),
                        new Sequence(new List<Node>
                        {
                            new Action(() => CheckState(SirenState.RunAway)),
                            new Action(() => RunAway())
                        }),
                    }
                       
                ),
            })
        }));
    }

    protected override void Start()
    {
        base.Start();
        this
            .ObserveEveryValueChanged(t => t.state)
            .Subscribe(state =>
            {
                if (photonView.IsMine)
                {
                    switch(state)
                    {
                        case SirenState.Searching:
                            agent.speed = tempSpeed;
                            animator.SetBool("isRun", false);
                            animator.SetBool("isAlert", false);
                            break;
                        case SirenState.RunAway:
                            agent.speed = tempSpeed * 1.6f;
                            animator.SetBool("isRun", true);
                            animator.SetBool("isAlert", false);
                            break;
                        default:
                            agent.speed = tempSpeed;
                            break;
                    }
                }
            });
    }

    protected override void Update()
    {
        if (photonView.IsMine)
        {
            base.Update();
        }
        else
        {
            FixTransform();
        }
        switch(state) 
        {
            case SirenState.Attack:
                agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                break;
            default:
                agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
                break;
        }
        
    }

    [PunRPC]
    protected override void AttackRPC()
    {
        
    }

    private void ChangeHead(bool isAlert)
    {
        photonView.RPC(nameof(ChangeHeadRPC), RpcTarget.All,isAlert);
    }

    [PunRPC]
    private void ChangeHeadRPC(bool isAlert)
    {
        if (isAlert)
        {
            Color color = Color.white * 5;
            meshRenderer.materials[1].SetColor("_EmissionColor", color);
            head.gameObject.SetActive(true);
            soundManager.PlaySound(Sound.Siren_Attack, transform.position, 1f);
        }
        else
        {
            Color color = Color.black;
            meshRenderer.materials[1].SetColor("_EmissionColor", color);
            head.gameObject.SetActive(false);
        }
    }

    private NodeState CheckState(SirenState _state, bool reverse = false)
    {
        if (state != _state) return reverse ? NodeState.Succes : NodeState.Failure;
        return reverse ? NodeState.Failure : NodeState.Succes;
    }

    private NodeState SetState(SirenState _state)
    {
        state = _state;
        return NodeState.Succes;
    }

    private NodeState SearchPlayer()
    {
        if(isArrive)
        {
            curIndex = Random.Range(0, waypoints.Count);
            isArrive = false;
        }
        Move(waypoints[curIndex]);
        if(Vector3.Distance(transform.position,waypoints[curIndex]) < 0.6f)
        {
            isArrive = true;
            return NodeState.Succes;
        }
        return NodeState.Running;
    }

    private NodeState CheckPlayerInFOV()
    {
        if (targetPlayer != null)
        {
            if (targetPlayer.IsDie)
            {
                state = SirenState.Searching;
                targetPlayer = null;
            }
            else
            {
                state = SirenState.Attack;
                return NodeState.Succes;
            }
        }
        var targetUnits = fov.TargetUnits.Where(x => x is InGamePlayer).Select(x => x as InGamePlayer).Where(x => x != x.IsDie).ToList();
        if (targetUnits.Count > 0)
        {
            state = SirenState.Attack;
            targetPlayer = targetUnits[0];
            return NodeState.Succes;
        }
        return NodeState.Failure;
    }

    private NodeState FollowPlayer()
    {
        if (targetPlayer != null)
        {
            if (targetPlayer.IsDie)
            {
                state = SirenState.Searching;
                targetPlayer = null;
                return NodeState.Failure;
            }
            else
            {
                Move(targetPlayer.transform.position);
                return NodeState.Running;
            }
        }
        return NodeState.Failure;
    }

    private NodeState DoAlert()
    {
        if(Vector3.Distance(transform.position,targetPlayer.transform.position) > attackRange) return NodeState.Running;
        if(!isAlerting)
            StartCoroutine(AlertRoutine(4.5f));
        return NodeState.Succes;
        
    }

    private IEnumerator AlertRoutine(float time)
    {
        isAlerting = true;
        agent.isStopped = true;
        ChangeHead(true);
        float curTime = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange * 8,LayerMask.GetMask("Monster"));
        var monstersInAround = hits.Where(x => x.GetComponent<MonsterBase>()).Select(x => x.GetComponent<MonsterBase>()).Where(x => x != x.IsDie);
        foreach(var monster in monstersInAround)
        {
            if (monster.gameObject.Equals(gameObject)) continue;
            monster.CallMonsterToPos(transform.position);
        }
        animator.SetBool("isRun", false);
        animator.SetBool("isAlert", true);
        while (curTime < time)
        {
            yield return null;
            curTime += Time.deltaTime;
            head.Rotate(Vector3.up * Time.deltaTime * 100);
        }
        isAlerting = false;
        animator.SetBool("isAlert", false);
        agent.isStopped = false;
        ChangeHead(false);
        targetPlayer = null;
        state = SirenState.RunAway;
    }

    private NodeState RunAway()
    {
        if(!isRunning)
        {
            curIndex = GetFareastWaypoint();
            isRunning = true;
        }
        Move(waypoints[curIndex]);
        if(Vector3.Distance(transform.position, waypoints[curIndex]) < 0.6f)
        {
            isRunning = false;
            state = SirenState.Searching;
            return NodeState.Succes;
        }
        return NodeState.Running;;

    }

    public override void CallMonsterToPos(Vector3 pos, InGamePlayer targetPlayer = null)
    {
        return;
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        base.OnPhotonSerializeView(stream, info);
        if(stream.IsWriting)
        {
            stream.SendNext(state);
        }
        else
        {
            state = (SirenState)(int)stream.ReceiveNext();
        }
    }
}
