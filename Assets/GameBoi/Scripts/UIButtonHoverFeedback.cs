using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHoverFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform targetRect;
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Graphic glowGraphic;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio (optional)")]
    [SerializeField] private AudioClip hoverSfx;
    [SerializeField] private AudioClip clickSfx;

    [Header("Scale")]
    [SerializeField] private float hoverScale = 1.06f;
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float lerpSpeed = 14f;

    [Header("Glow")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.95f, 0.65f, 1f);
    [SerializeField] private Color pressedColor = new Color(1f, 0.85f, 0.45f, 1f);
    [SerializeField] private float glowHiddenAlpha = 0f;
    [SerializeField] private float glowHoverAlpha = 0.75f;
    [SerializeField] private float glowPressedAlpha = 1f;

    private Vector3 targetScale = Vector3.one;
    private bool isHovered;
    private bool isPressed;

    private void Reset()
    {
        targetRect = GetComponent<RectTransform>();
        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>();
    }

    private void Awake()
    {
        if (targetRect == null)
            targetRect = GetComponent<RectTransform>();

        targetScale = Vector3.one;
        ApplyVisualImmediate();
    }

    private void Update()
    {
        if (targetRect != null)
            targetRect.localScale = Vector3.Lerp(targetRect.localScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);

        if (glowGraphic != null)
        {
            Color targetColor = isPressed ? pressedColor : (isHovered ? hoverColor : normalColor);
            float targetAlpha = isPressed ? glowPressedAlpha : (isHovered ? glowHoverAlpha : glowHiddenAlpha);
            Color c = glowGraphic.color;
            c = Color.Lerp(c, targetColor, Time.unscaledDeltaTime * lerpSpeed);
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.unscaledDeltaTime * lerpSpeed);
            glowGraphic.color = c;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.one * hoverScale;
        Play(hoverSfx);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;
        targetScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = Vector3.one * pressedScale;
        Play(clickSfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = Vector3.one * (isHovered ? hoverScale : 1f);
    }

    private void ApplyVisualImmediate()
    {
        if (glowGraphic != null)
        {
            Color c = normalColor;
            c.a = glowHiddenAlpha;
            glowGraphic.color = c;
        }

        if (targetRect != null)
            targetRect.localScale = Vector3.one;
    }

    private void Play(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
