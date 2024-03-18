using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class UIStack : MonoBehaviour
    {
        private static UIStack instance;
        public static UIStack Instance => instance ??= FindObjectOfType<UIStack>();
                
        private UIStackWindow current;
        private Stack<string> uiStack = new();
        private Dictionary<string, UIStackWindow> windowTable = new();

        private void Awake()
        {
            foreach(var w in FindObjectsOfType<UIStackWindow>().Distinct())
                windowTable.Add(w.GetType().Name, w);
        }

        public T Open<T>() where T : UIStackWindow
        {
            if(!windowTable.ContainsKey(typeof(T).Name))
            {
                Debug.LogWarning("No windows are available.");
                return null;
            }

            if(current != null) uiStack.Push(current.GetType().Name);
            current = windowTable[typeof(T).Name];
            current.Display();
            return current as T;
        }

        public void CloseRecent()
        {
            current.Hide();
            if(uiStack.TryPop(out var key))
            {
                current = windowTable[key];
                current.Display();
            }
        }

        private void OnDestroy()
        {
            if(ReferenceEquals(instance, this))
                instance = null;
        }
    }
}