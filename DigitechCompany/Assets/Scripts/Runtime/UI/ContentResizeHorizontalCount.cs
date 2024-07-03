using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ContentResizeHorizontalCount : MonoBehaviour
{
    [SerializeField] private int count = 2;
    [SerializeField] private float space = 10;
    [SerializeField] private float height = 150;

    private RectTransform rectTransform;
    
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(count == 0) return;

        var i = 0;
        foreach(RectTransform rt in transform)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);

            var width = rectTransform.rect.width / count - (space / 2);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(width * (i % count) + (space * (i % count - 1)) + space, -(height + space) * (i / count));
            i++;
        }
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ((transform.childCount - 1) / count + 1) * (height + space) - space);
    }
}
