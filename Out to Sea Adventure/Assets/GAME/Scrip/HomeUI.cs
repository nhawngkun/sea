using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class HomeUI : MonoBehaviour
{
    public RectTransform uiTop; 
    public RectTransform uiBottom; 
    public float moveDistance = 500f; 
    public float duration = 0.5f; 
    public static UnityAction StartGame;
    private Vector2 _topStartPos;
    private Vector2 _bottomStartPos;

    void Awake()
    {
        _topStartPos = uiTop.anchoredPosition;
        _bottomStartPos = uiBottom.anchoredPosition;
        StartGame += OnClickButton;
        Debug.Log($"duration {duration}");
    }
    void OnDestroy()
    {
        StartGame -= OnClickButton;
        Debug.Log($"moveDistance{moveDistance}");
    }
    void OnEnable()
    {
        // Đặt UI ở ngoài màn hình
        uiTop.anchoredPosition = _topStartPos + new Vector2(0, moveDistance);
        uiBottom.anchoredPosition = _bottomStartPos - new Vector2(0, moveDistance);

        // Hiệu ứng trượt vào
        uiTop.DOAnchorPos(_topStartPos, duration).SetEase(Ease.OutBack);
        uiBottom.DOAnchorPos(_bottomStartPos, duration).SetEase(Ease.OutBack);
    }

    public void OnClickButton()
    {
        uiTop.DOAnchorPos(_topStartPos + new Vector2(0, moveDistance), duration)
             .SetEase(Ease.InBack);
           

        uiBottom.DOAnchorPos(_bottomStartPos - new Vector2(0, moveDistance), duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => DOVirtual.DelayedCall(0.5f,() => gameObject.SetActive(false)));
    }
}
