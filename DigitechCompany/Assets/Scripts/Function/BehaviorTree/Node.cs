using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public enum NodeState
    {
        Running, Succes, Failure
    }
    
    public class Node
    {
        public Node parentNode;

        protected NodeState state;
        protected List<Node> children = new();

        private Dictionary<string, object> dataContext = new Dictionary<string, object>();

        public Node()
        {
            parentNode = null;
        }

        public Node(List<Node> children)
        {
            foreach(Node node in children)
            {
                AddChild(node);
            }
        }

        private void AddChild(Node node)
        {
            node.parentNode = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate() => NodeState.Failure;
        
        public void SetData(string key, object value)
        {
            dataContext[key] = value;
        }

        public object GetData(string key)
        {
            object obj = null;
            if(dataContext.TryGetValue(key, out obj)) {
                return obj;
            }
            Node node = parentNode;
            while(node != null)
            {
                obj = node.GetData(key);
                if(obj != null)
                {
                    return obj;
                }
                node = node.parentNode;
            }    
            return null;
        }


        public bool RemoveData(string key)
        {
            if(dataContext.ContainsKey(key))
            {
                dataContext.Remove(key);
                return true;
            }
            Node node = parentNode;
            while(node != null)
            {
                bool isRemoved = node.RemoveData(key);
                if(isRemoved)
                {
                    return true;
                }
                node = node.parentNode;
            }
            return false;
        }
    }
}

