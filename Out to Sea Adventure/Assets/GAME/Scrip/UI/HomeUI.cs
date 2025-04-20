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
    public GameObject objectToShow;
    public GameObject objectToShow1;// Đối tượng sẽ hiện lên sau khi animation hoàn tất

    private Vector2 _topStartPos;
    private Vector2 _bottomStartPos;

    void Awake()
    {
        _topStartPos = uiTop.anchoredPosition;
        _bottomStartPos = uiBottom.anchoredPosition;
        StartGame += OnClickButton;

        // Đảm bảo đối tượng cần hiển thị bắt đầu ở trạng thái ẩn
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
        if (objectToShow1 != null)
        {
            objectToShow1.SetActive(false);
        }
    }

    void OnDestroy()
    {
        StartGame -= OnClickButton;
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
        // Hiệu ứng trượt ra cho phần top UI
        uiTop.DOAnchorPos(_topStartPos + new Vector2(0, moveDistance), duration)
             .SetEase(Ease.InBack);

        // Hiệu ứng trượt ra cho phần bottom UI và xử lý sau khi hoàn thành
        uiBottom.DOAnchorPos(_bottomStartPos - new Vector2(0, moveDistance), duration)
                .SetEase(Ease.InBack)
                .OnComplete(() => {
                    DOVirtual.DelayedCall(0.5f, () => {
                        
                        gameObject.SetActive(false);

                        //
                        if (objectToShow != null)
                        {
                            objectToShow.SetActive(true);

                            
                           
                            objectToShow.transform.DOScale(Vector3.one, 0.5f)
                                        .SetEase(Ease.OutBack);
                        }
                        if (objectToShow1 != null)
                        {
                            objectToShow1.SetActive(true);



                            objectToShow1.transform.DOScale(Vector3.one, 0.5f)
                                        .SetEase(Ease.OutBack);
                        }
                    });
                });
    }
}