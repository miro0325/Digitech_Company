using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour, IService
{
    private Stack<string> viewStack = new();
    private Dictionary<string, UIView> sceneViews = new();
    private Dictionary<string, UIView> globalViews = new();

    private void Awake()
    {
        if(!ServiceLocator.For(this).TryRegister(this))
        {
            //
        }
    }

    public void RegisterView<TView>(TView view) where TView : UIView
    {
        sceneViews.Add(typeof(TView).FullName, view);
    }

    public void Open<TView>() where TView : UIView
    {

    }
}
