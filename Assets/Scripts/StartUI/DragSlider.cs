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
        OnMouseUp();             // �ƹ���}��Ĳ�o�i�ק�s
        isDrag = false;
    }

    void OnMouseUp()
    {
        // �Y�ϥΪ̩��̧��ݡA���F�קK�L�kĲ�o�۰ʤU�@���A������ʤ���
        if (Player.instance.progressSlider.value == 1)
        {
            // Player.instance.NextMusic();
            return;
        }

        // �ھڶi�ױ���ҭp�⼽���m
        Player.instance.audioSource.time =
            Player.instance.progressSlider.value * Player.instance.audioSource.clip.length;
    }
}
