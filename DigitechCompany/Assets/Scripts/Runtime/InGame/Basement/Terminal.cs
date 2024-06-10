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
        [Header("Cam Move Setting"),Space(5)]
        
        [SerializeField] private Transform watchCamLocation;
        [SerializeField] private float moveDelay;
        private bool isMoving = false;

        private Vector3 prevCamPos;
        private Vector3 prevCamRot;
        
        [Header("Console Input")]
        [SerializeField] private TMP_InputField consoleInput;
        [SerializeField] private TextMeshProUGUI explainText;
        [SerializeField] private bool isConnectTerminal = false;

        [SerializeField] private int wordCountLimit;

        [SerializeField] private List<Command> commands = new List<Command>();
        private Dictionary<string, Command> commandDic = new Dictionary<string, Command>();
        
        private InGamePlayer curPlayer;
        private UserInputAction userInput;

        private Transform cam;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            cam = Camera.main.transform;
            userInput = new();
            foreach(var command in commands)
            {
                command.Init();
                if(!command.IsMultiple)
                    commandDic.Add(command.Cmd, command);
                foreach(var aliases in command.Aliases)
                {
                    commandDic.Add(aliases,command);
                }
            }
            //consoleInput.onEndEdit.AddListener(delegate { SendConsoleCommand(); });
            consoleInput.onSubmit.AddListener(delegate { SendConsoleCommand(); });
            consoleInput.onValueChanged.AddListener(delegate { LimitWordCount(); });
        }

        private void Update()
        {
            InputKey();
        }

        private void InputKey()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isMoving)
            {
                isConnectTerminal = false;
                DisconnectTerminal();
            }
        }

        private void ConnectTerminal()
        {
            if (!curPlayer || !curPlayer.photonView.IsMine) return;
            userInput.Disable();
            prevCamPos = cam.transform.position;
            prevCamRot = cam.transform.eulerAngles;
            isMoving = true;
            cam.transform.DOMove(watchCamLocation.position, moveDelay);
            cam.transform.DORotate(watchCamLocation.eulerAngles, moveDelay).OnComplete(
                () => { consoleInput.ActivateInputField(); isMoving = false; }
            );
        }
        
        private void DisconnectTerminal()
        {
            if (!curPlayer || !curPlayer.photonView.IsMine) return;
            isMoving = true;
            consoleInput.DeactivateInputField();
            cam.transform.DOMove(prevCamPos, moveDelay);
            cam.transform.DORotate(prevCamRot, moveDelay).OnComplete(
                () => { isMoving = false; curPlayer = null; isConnectTerminal = false; userInput.Player.Enable(); }
            );
        }

        private void SendConsoleCommand()
        {
            string msg = consoleInput.text;
            if(CheckCommand(msg, out Command cmd))
            {
                var cd = msg.Split(' ')[0].Trim().ToLower();
                var args = GetArgs(msg);
                explainText.text = cmd.Activate(cd,args);
            } else
            {
                if(!string.IsNullOrEmpty(msg.Replace(" ",string.Empty)) || msg.Replace(" ", string.Empty).Length != 0)
                    SendNotExistCommand(msg);
                //Debug.Log(msg.Replace(" ", string.Empty).Length);
            }
            consoleInput.text = "";
            consoleInput.ActivateInputField();

        }

        private void SendNotExistCommand(string msg)
        {
            //Debug.Log(msg);
            //Debug.Log(msg.Split(' '));
            var notExistCmd = msg.Split(' ')[0].Trim().ToLower();
            
            explainText.text = $"{notExistCmd} is not exist Command";  
        }

        private bool CheckCommand(string msg)
        {
            msg = msg.ToLower();
            string key = msg.Split(' ')[0];
            if(commandDic.ContainsKey(key))
            {
                return true;    
            }
            return false;
        }

        private bool CheckCommand(string msg, out Command cmd)
        {
            msg = msg.ToLower();
            string key = msg.Split(' ')[0];
            if (commandDic.ContainsKey(key))
            {
                cmd = commandDic[key];
                return true;
            } else
            {
                cmd = null;
                return false;
            }
        }

        private string[] GetArgs(string msg)
        {
            msg = msg.ToLower();
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

