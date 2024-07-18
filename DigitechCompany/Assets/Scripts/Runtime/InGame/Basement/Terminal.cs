using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace Basements
{
    public class Terminal : MonoBehaviour, IInteractable
    {
        private UserInput input => UserInput.input;
        
        [Header("Cam Move Setting"),Space(5)]
        
        [SerializeField] private Transform watchCamLocation;
        [SerializeField] private float moveDelay;
        private bool isMoving = false;
        
        [Header("Console Input")]
        [SerializeField] private TextMeshProUGUI money;
        [SerializeField] private TMP_InputField consoleInput;
        [SerializeField] private TextMeshProUGUI explainText;
        [SerializeField] private bool isConnectTerminal = false;

        [SerializeField] private int wordCountLimit;

        private Dictionary<string, (string original, Command command)> commandDic = new();
        
        private InGamePlayer curPlayer;

        private Transform cam;


        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            cam = Camera.main.transform;
            foreach(var command in Resources.LoadAll<Command>("Commands"))
            {
                foreach(var data in command.CommandDatas)
                {
                    commandDic.Add(data.cmd, (data.cmd, command));
                    foreach(var aliase in data.aliases)
                        commandDic.Add(aliase, (data.cmd, command));
                }
            }
            //consoleInput.onEndEdit.AddListener(delegate { SendConsoleCommand(); });
            // consoleInput.onSubmit.AddListener(delegate { SendConsoleCommand(); });
            // consoleInput.onValueChanged.AddListener(delegate { LimitWordCount(); });

            consoleInput.onSubmit.AddListener(_ => SendConsoleCommand());
            consoleInput.onValueChanged.AddListener(_ => LimitWordCount());
        }

        private void Update()
        {
            InputKey();
            money.text = $"${ServiceLocator.For(this).Get<GameManager>().CurUsableMoney}";
        }

        private void InputKey()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isMoving)
            {
                isConnectTerminal = false;
                DisconnectTerminal();
            }

            if(isConnectTerminal && Input.GetKeyDown(KeyCode.Return))
                SendConsoleCommand();
        }

        private void ConnectTerminal()
        {
            if (!curPlayer || !curPlayer.photonView.IsMine) return;
            input.Player.Disable();
            isMoving = true;
            cam.transform.SetParent(watchCamLocation);
            cam.transform.DOLocalMove(Vector3.zero, moveDelay);
            cam.transform.DOLocalRotate(Vector3.zero, moveDelay).OnComplete(
                () => { consoleInput.ActivateInputField(); isMoving = false; }
            );
        }
        
        private void DisconnectTerminal()
        {
            if (!curPlayer || !curPlayer.photonView.IsMine) return;
            isMoving = true;
            consoleInput.DeactivateInputField();
            cam.transform.SetParent(curPlayer.CameraHolder);
            cam.transform.DOLocalMove(Vector3.zero, moveDelay);
            cam.transform.DOLocalRotate(Vector3.zero, moveDelay).OnComplete(
                () => { isMoving = false; isConnectTerminal = false; input.Player.Enable(); curPlayer = null; }
            );
        }

        private void SendConsoleCommand()
        {
            string msg = consoleInput.text;
            if(TryGetCommand(msg, out string original, out Command cmd))
            {
                explainText.text = cmd.Activate(original, GetArgs(msg));
            } else
            {
                if(!string.IsNullOrEmpty(msg.Replace(" ",string.Empty)) || msg.Replace(" ", string.Empty).Length != 0)
                    SendNotExistCommand(msg);
            }
            consoleInput.text = "";
            consoleInput.ActivateInputField();
        }

        private void SendNotExistCommand(string msg)
        {
            var notExistCmd = msg.Split(' ')[0].Trim();
            explainText.text = $"{notExistCmd}은(는) 존재하지 않는 커맨드 입니다."; 
        }

        private bool CheckCommand(string msg)
        {
            string key = msg.Split(' ')[0];
            return commandDic.ContainsKey(key);
        }

        private bool TryGetCommand(string msg, out string original, out Command cmd)
        {
            string key = msg.Split(' ')[0];
            if (commandDic.ContainsKey(key))
            {
                original = commandDic[key].original;
                cmd = commandDic[key].command;
                return true;
            } else
            {
                original = "";
                cmd = null;
                return false;
            }
        }

        private string[] GetArgs(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return null;
            if(CheckCommand(msg))
            {
                var split = msg.Split(' ');
                string[] args = new string[split.Length-1];
                for(int i = 0; i < split.Length; i++)
                {
                    if (i == 0 || string.IsNullOrEmpty(split[i])) continue;
                    
                    args[i-1] = split[i].Trim();
                }
                return args;
            } 
            else
            {
                return null;
            }
        }

        private void LimitWordCount()
        {
            var len = consoleInput.text.Length;
            if (len > wordCountLimit)
            {
                consoleInput.text = consoleInput.text.Substring(0,wordCountLimit); 
            }
        }

        public InteractID GetTargetInteractID(UnitBase unit)
        {
            return InteractID.ID1;
        }

        public float GetInteractRequireTime(UnitBase unit)
        {
            return 0;
        }

        public bool IsInteractable(UnitBase unit)
        {
            if (isConnectTerminal) return false;
            else return true;
        }

        public string GetInteractionExplain(UnitBase unit)
        {
            if (isConnectTerminal) return "";
            else return "터미널 입력";
        }

        public void OnInteract(UnitBase unit)
        {
            var player = unit as InGamePlayer;
            if(player)
            {
                isConnectTerminal = true;
                curPlayer = player;
                ConnectTerminal();
            }
        }
    }
}

