using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField] private GameObject background;
    [SerializeField] private UIView defaultView;

    private UIView current;
    private Stack<UIView> viewStack = new();
    private Dictionary<Type, UIView> views = new();

    private void Awake()
    {
        ServiceLocator.For(this).TryRegister(this);
        this.ObserveEveryValueChanged(x => x.current).Subscribe(view => background.SetActive(!ReferenceEquals(current, null)));
    }

    private void Start()
    {
        current = defaultView;
        defaultView?.Open();
    }

    public void RegisterView<TView>(TView view) where TView : UIView
    {
        Debug.Log(typeof(TView));
        views.TryAdd(typeof(TView), view);
    }

    public TView Open<TView>() where TView : UIView
    {
        if (TryGetView<TView>(out var view))
        {
            // current is not null
            if (!ReferenceEquals(current, null))
            {
                current.Close();
                viewStack.Push(current);
            }

            current = view;
            current.Open();

            return view;
        }

        Debug.LogWarning($"Type {typeof(TView).FullName} is not exist any container");
        return null;
    }

    private bool TryGetView<TView>(out TView view) where TView : UIView
    {
        UIView instance;
        if (views.TryGetValue(typeof(TView), out instance))
        {
            view = instance as TView;
            return true;
        }
        view = null;
        return false;
    }

    public void CloseRecently()
    {
        //has default view
        if (!ReferenceEquals(defaultView, null))
        {
            if (ReferenceEquals(current, defaultView))
            {
                Debug.LogWarning($"Default ui was set, Request was ignored");
                return;
            }

            current.Close();
            if (viewStack.Count > 0)
            {
                current = viewStack.Pop();
                current.Open();
            }
            else
            {
                if (ReferenceEquals(defaultView, null))
                {
                    current = null;
                }
                else
                {
                    current = defaultView;
                    current.Open();
                }
            }
        }
        else
        {
            if (current = null)
            {
                Debug.LogWarning($"There is not exist current open view");
                return;
            }

            current.Close();
            if (viewStack.Count > 0)
            {
                current = viewStack.Pop();
                current.Open();
            }
            else
            {
                current = null;
            }
        }
    }
}
