using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveSlotView : MonoBehaviour
{
    [Header("Core UI")]
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text techniqueNameLabel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image moveIconImage;
    [SerializeField] private Image techniqueIconImage;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private GameObject techniqueBadgeRoot;

    [Header("Move Icons")]
    [SerializeField] private Sprite handMoveIcon;
    [SerializeField] private Sprite legMoveIcon;
    [SerializeField] private Sprite swordMoveIcon;
    [SerializeField] private Sprite palmMoveIcon;

    [Header("Technique Icons")]
    [SerializeField] private Sprite dragonFistIcon;
    [SerializeField] private Sprite craneKickIcon;
    [SerializeField] private Sprite moonSlashIcon;
    [SerializeField] private Sprite voidPalmIcon;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color techniqueColor = new Color(1f, 0.9f, 0.45f, 1f);
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color emptyIconColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private float flashDuration = 0.18f;

    private Coroutine flashRoutine;

    public void SetMove(MoveType? move, AttackKind attackKind = AttackKind.Normal, TechniqueType techniqueType = TechniqueType.None)
    {
        if (label != null)
            label.text = move.HasValue ? ToRu(move.Value) : "-";

        bool hasMove = move.HasValue;
        bool isTechnique = hasMove && attackKind == AttackKind.Technique && techniqueType != TechniqueType.None;

        if (backgroundImage != null)
            backgroundImage.color = hasMove ? (isTechnique ? techniqueColor : normalColor) : emptyColor;

        if (moveIconImage != null)
        {
            moveIconImage.sprite = hasMove ? GetMoveSprite(move.Value) : null;
            moveIconImage.enabled = hasMove && moveIconImage.sprite != null;
            moveIconImage.color = hasMove ? Color.white : emptyIconColor;
        }

        if (techniqueBadgeRoot != null)
            techniqueBadgeRoot.SetActive(isTechnique);

        if (techniqueNameLabel != null)
            techniqueNameLabel.text = isTechnique ? techniqueType.ToString() : string.Empty;

        if (techniqueIconImage != null)
        {
            techniqueIconImage.enabled = isTechnique;
            techniqueIconImage.sprite = isTechnique ? GetTechniqueSprite(techniqueType) : null;
        }
    }

    public void PlayTechniqueFlash()
    {
        if (flashOverlay == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashOverlay.gameObject.SetActive(true);
        Color c = flashColor;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / flashDuration);
            c.a = flashColor.a * k;
            flashOverlay.color = c;
            yield return null;
        }
        flashOverlay.gameObject.SetActive(false);
        flashRoutine = null;
    }

    private Sprite GetMoveSprite(MoveType move)
    {
        switch (move)
        {
            case MoveType.Hand: return handMoveIcon;
            case MoveType.Leg: return legMoveIcon;
            case MoveType.Sword: return swordMoveIcon;
            case MoveType.Palm: return palmMoveIcon;
            default: return null;
        }
    }

    private Sprite GetTechniqueSprite(TechniqueType techniqueType)
    {
        switch (techniqueType)
        {
            case TechniqueType.DragonFist: return dragonFistIcon;
            case TechniqueType.CraneKick: return craneKickIcon;
            case TechniqueType.MoonSlash: return moonSlashIcon;
            case TechniqueType.VoidPalm: return voidPalmIcon;
            default: return null;
        }
    }

    private static string ToRu(MoveType move)
    {
        switch (move)
        {
            case MoveType.Hand: return "Кулак";
            case MoveType.Leg: return "Нога";
            case MoveType.Sword: return "Меч";
            case MoveType.Palm: return "Ладонь";
            default: return "?";
        }
    }
}
