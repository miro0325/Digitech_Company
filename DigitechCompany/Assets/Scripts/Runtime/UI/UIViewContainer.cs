using System;
using System.Collections.Generic;
using UnityEngine;

public class UIViewContainer : MonoBehaviour
{
    private Dictionary<Type, UIView> views = new();
    
    internal void BindingViewInternal<TView>(TView view) where TView : UIView
    {
        if(views.ContainsKey(typeof(TView)))
        {
            Debug.LogWarning($"Type {typeof(TView).FullName} already exist in container");
            Destroy(view.gameObject);
            return;
        }

        views.Add(typeof(TView), view);
    }

    internal TView GetViewInternal<TView>() where TView : UIView
    {
        if(views.TryGetValue(typeof(TView), out var uiView))
            return uiView as TView;
        
        Debug.LogWarning($"Type {typeof(TView).FullName} is not exist int view container");
        return null;
    }

    internal bool TryGetViewInternal<TView>(out TView view) where TView : UIView
    {
        if(views.TryGetValue(typeof(TView), out var uiView))
        {    
            view = uiView as TView;
            return true;
        }
        
        Debug.LogWarning($"Type {typeof(TView).FullName} is not exist int view container");
        view = null;
        return false;
    }
}