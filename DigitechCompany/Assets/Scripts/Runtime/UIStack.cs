using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class UIStack : MonoBehaviour
    {
        private static UIStack instance;
        public static UIStack Instance => instance ??= FindObjectOfType<UIStack>();
        
        private UIStackWindow current;
        private Stack<UIStackWindow> stacks = new();

        public void Open()
        {
            
        }
    }
}