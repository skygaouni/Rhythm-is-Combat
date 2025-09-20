using UnityEngine;
using UnityEngine.EventSystems;

public class DragSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static bool isDrag;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnMouseUp();             // 滑鼠放開時觸發進度更新
        isDrag = false;
    }

    void OnMouseUp()
    {
        // 若使用者拖到最尾端，為了避免無法觸發自動下一首，直接手動切換
        if (Player.instance.progressSlider.value == 1)
        {
            // Player.instance.NextMusic();
            return;
        }

        // 根據進度條比例計算播放位置
        Player.instance.audioSource.time =
            Player.instance.progressSlider.value * Player.instance.audioSource.clip.length;
    }
}
