using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingCombatText : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text valueText;

    [Header("Damage Motion")]
    [SerializeField] private Vector2 riseOffset = new Vector2(0f, 90f);
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private AnimationCurve moveEase = null;
    [SerializeField] private AnimationCurve fadeEase = null;

    [Header("Damage Style")]
    [SerializeField] private string prefix = "-";
    [SerializeField] private Color textColor = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Info Message Motion")]
    [SerializeField] private Vector2 infoRiseOffset = new Vector2(0f, 45f);
    [SerializeField] private float infoDuration = 1.1f;

    [Header("Info Message Style")]
    [SerializeField] private Color infoTextColor = new Color(1f, 0.93f, 0.72f, 1f);
    [SerializeField] private float infoFontSizeMultiplier = 0.85f;

    private Coroutine playRoutine;
    private float initialFontSize = -1f;

    private void Reset()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        valueText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (valueText == null)
            valueText = GetComponentInChildren<TMP_Text>();

        if (valueText != null && initialFontSize <= 0f)
            initialFontSize = valueText.fontSize;

        if (moveEase == null || moveEase.length == 0)
        {
            moveEase = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2.2f),
                new Keyframe(1f, 1f, 0.15f, 0f));
        }

        if (fadeEase == null || fadeEase.length == 0)
        {
            fadeEase = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.15f, 1f),
                new Keyframe(1f, 0f));
        }
    }

    public void Play(Vector2 anchoredStartPos, int amount)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(
            anchoredStartPos,
            string.Concat(prefix, Mathf.Abs(amount).ToString()),
            textColor,
            riseOffset,
            duration,
            1f));
    }

    public void PlayInfo(Vector2 anchoredStartPos, string message)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(
            anchoredStartPos,
            message,
            infoTextColor,
            infoRiseOffset,
            infoDuration,
            infoFontSizeMultiplier));
    }

    private IEnumerator PlayRoutine(
        Vector2 anchoredStartPos,
        string text,
        Color color,
        Vector2 localRiseOffset,
        float localDuration,
        float fontSizeMultiplier)
    {
        if (rectTransform != null)
            rectTransform.anchoredPosition = anchoredStartPos;

        if (valueText != null)
        {
            valueText.text = text;
            valueText.color = color;

            if (initialFontSize <= 0f)
                initialFontSize = valueText.fontSize;

            valueText.fontSize = initialFontSize * Mathf.Max(0.1f, fontSizeMultiplier);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        float time = 0f;
        float safeDuration = Mathf.Max(0.01f, localDuration);

        while (time < safeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / safeDuration);

            float moveT = Mathf.Clamp01(moveEase.Evaluate(t));
            float fadeT = Mathf.Clamp01(fadeEase.Evaluate(t));

            if (rectTransform != null)
                rectTransform.anchoredPosition = anchoredStartPos + localRiseOffset * moveT;

            if (canvasGroup != null)
                canvasGroup.alpha = fadeT;

            yield return null;
        }

        Destroy(gameObject);
    }
}
