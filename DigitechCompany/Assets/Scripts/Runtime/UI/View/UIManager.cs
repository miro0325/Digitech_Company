using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour, IService
{
    [SerializeField] private GameObject background;

    private UIView current;
    private Stack<UIView> viewStack = new();
    private UIViewContainer globalViews;
    private Dictionary<Scene, UIViewContainer> sceneViews;

    private void Awake()
    {
        if (ServiceLocator.ForGlobal().TryRegister(this))
        {
            SceneManager.sceneUnloaded += scene =>
            {
                if (!ReferenceEquals(current, null))
                {
                    current.Close();
                    current = null;
                }
                viewStack.Clear();
                sceneViews.Remove(scene);
            };

            this.ObserveEveryValueChanged(x => x.current)
                .Subscribe(view => background.SetActive(!ReferenceEquals(current, null)));
        }
    }

    public void RegisterView<TView>(TView view, bool dontDestoryOnLoad) where TView : UIView
    {
        if (dontDestoryOnLoad) //global
        {
            if (!globalViews)
            {
                globalViews = new GameObject("UI View Container [Global]").AddComponent<UIViewContainer>();
                DontDestroyOnLoad(globalViews);
            }
            globalViews.BindingViewInternal(view);
            DontDestroyOnLoad(view);
        }
        else //current scene
        {
            var scene = view.gameObject.scene;
            if (!sceneViews.ContainsKey(scene))
            {
                var newContainer = new GameObject("UI View Container [Scene]").AddComponent<UIViewContainer>();
                sceneViews.Add(scene, newContainer);
            }
            sceneViews[scene].BindingViewInternal(view);
        }
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
        if (globalViews && globalViews.TryGetViewInternal(out view))
            return true;

        foreach (var sceneView in sceneViews)
        {
            if (sceneView.Value.TryGetViewInternal(out view))
                return true;
        }

        view = null;
        return false;
    }

    public void CloseRecent()
    {
        if (current == null)
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
