using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


namespace Basements
{
    public class Terminal : MonoBehaviour
    {
        [Header("Cam Move Setting"),Space(5)]
        
        [SerializeField] private Camera cam;
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
        
        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
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

        // Update is called once per frame
        void Update()
        {
            //InputKey();
        }

        private void InputKey()
        {
            if(Input.GetKeyDown(KeyCode.LeftShift) && !isMoving)
            {
                isConnectTerminal = !isConnectTerminal;
                if(isConnectTerminal)
                    ConnectTerminal();
                else
                    DisconnectTerminal();
            }
        }

        private void ConnectTerminal()
        {
            prevCamPos = cam.transform.position;
            prevCamRot = cam.transform.eulerAngles;
            isMoving = true;
            cam.transform.DOMove(watchCamLocation.position, moveDelay);
            cam.transform.DORotate(watchCamLocation.eulerAngles, moveDelay).OnComplete(
                () => { consoleInput.ActivateInputField(); isMoving = false; }
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
                Debug.Log(msg.Replace(" ", string.Empty).Length);
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
            } else
            {
                return null;
            }
        }

        private void DisconnectTerminal()
        {
            isMoving = true;
            consoleInput.DeactivateInputField();
            cam.transform.DOMove(prevCamPos, moveDelay);
            cam.transform.DORotate(prevCamRot, moveDelay).OnComplete(
                () => {  isMoving = false; }
            );
        }

        private void LimitWordCount()
        {
            var len = consoleInput.text.Length;
            if (len > wordCountLimit)
            {
                consoleInput.text = consoleInput.text.Substring(0,wordCountLimit); 
            }
            
        }
    }
}

