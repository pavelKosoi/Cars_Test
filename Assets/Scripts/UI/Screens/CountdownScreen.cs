using System;
using TMPro;
using UnityEngine;

public class CountdownScreen : ScreenBase
{
    [Serializable]
    public class PlayerPointer
    {
        public RectTransform pointerRoot;
        public RectTransform textTransform;

        public float offsetDistance = 150f;
    }

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TweenSettings timerTweenSettings;
    [SerializeField] PlayerPointer playerPointer;

    public ITimerVisual TimerVisual { get; private set; }

    private void Awake()
    {
        TimerVisual = new TextTimerVisual(timerText, timerTweenSettings, Vector2.one * 3f, Vector2.one * 0.3f);
    }

    public void PointOnPlayer(Transform playerTransform, Camera camera)
    {
        playerPointer.pointerRoot.gameObject.SetActive(true);

        Vector3 playerScreenPos = camera.WorldToScreenPoint(playerTransform.position);

        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, playerScreenPos.z);

        Vector3 directionToCenter = (screenCenter - playerScreenPos).normalized;

        playerPointer.pointerRoot.position = playerScreenPos + directionToCenter * playerPointer.offsetDistance;

        Vector3 pointDirection = -directionToCenter;

        float angle = Mathf.Atan2(pointDirection.y, pointDirection.x) * Mathf.Rad2Deg;
        angle += 90f;
        playerPointer.pointerRoot.rotation = Quaternion.Euler(0, 0, angle);

        if (playerPointer.textTransform != null)
        {
            playerPointer.textTransform.rotation = Quaternion.identity;
        }
    }
 
}