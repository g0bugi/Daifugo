using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HierarchyVisualEntry
{

    public PlayerHierarchy hierarchy;
    public Sprite lastBadgeSprite;
    public Sprite currentResolvedSprite;

}

public class PlayerSeatInfoView : MonoBehaviour
{

    [SerializeField]
    private PlayerState playerState;
    public PlayerState PlayerState => playerState;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text lastHierarchyText;

    [SerializeField]
    private TMP_Text thisHierarchyText;

    [Header("Hierarchy Visual")]
    [SerializeField]
    private Image lastBadgeImage;

    [SerializeField]
    private Image currentHierarchyImage;

    [SerializeField]
    private List<HierarchyVisualEntry> hierarchyVisualEntries = new List<HierarchyVisualEntry>();

    [SerializeField]
    private Color normalLastBadgeColor = Color.white;

    [SerializeField]
    private Color dimmedLastBadgeColor = new Color(0.35f, 0.35f, 0.35f, 1.0f);

    [Header("Current Hierarchy Reveal")]
    [SerializeField]
    private float revealScaleDuration = 0.18f;

    [SerializeField]
    private float revealStartScale = 1.2f;

    [SerializeField]
    private float revealFadeDuration = 0.2f;

    [Header("Special Result Visual")]
    [SerializeField]
    private Image miyakoOchiImage;

    [Header("Presentation")]
    [SerializeField]
    private Image highlightImage;

    [SerializeField]
    private TMP_Text statusEffectText;

    [SerializeField]
    private Color normalHighlightColor = Color.white;

    [SerializeField]
    private Color skipHighlightColor = Color.red;

    private Coroutine presentationCoroutine;
    private Coroutine hierarchyRevealCoroutine;

    // "처음 확정된 순간"을 감지하기 위한 상태
    private PlayerHierarchy lastPresentedCurrentHierarchy = PlayerHierarchy.None;

    public void Refresh(bool isCurrentTurn)
    {

        if (playerState == null)
        {

            Debug.Log("[PlayerSeatInfoView] playerState가 비어 있습니다.");
            return;

        }

        RefreshTexts(isCurrentTurn);
        RefreshTurnHighlight(isCurrentTurn);
        RefreshLastBadgeVisual();
        RefreshCurrentHierarchyVisual();
        RefreshSpecialFinishVisual();

    }

    private void RefreshTexts(bool isCurrentTurn)
    {

        if (nameText != null)
        {

            nameText.text = playerState.PlayerName;
            nameText.fontStyle = isCurrentTurn ? FontStyles.Bold : FontStyles.Normal;

        }

        if (lastHierarchyText != null)
        {

            lastHierarchyText.text = $"Last: {HierarchyToKorean(playerState.LastGameHierarchy)}";
            lastHierarchyText.fontStyle = isCurrentTurn ? FontStyles.Bold : FontStyles.Normal;

        }

        if (thisHierarchyText != null)
        {

            thisHierarchyText.text = $"Current: {HierarchyToKorean(playerState.ThisGameHierarchy)}";
            thisHierarchyText.fontStyle = isCurrentTurn ? FontStyles.Bold : FontStyles.Normal;

        }

    }

    private void RefreshLastBadgeVisual()
    {

        if (lastBadgeImage == null)
        {

            return;

        }

        if (playerState.LastGameHierarchy == PlayerHierarchy.None)
        {

            lastBadgeImage.gameObject.SetActive(false);
            return;

        }

        HierarchyVisualEntry visualEntry = FindVisualEntry(playerState.LastGameHierarchy);

        if (visualEntry == null || visualEntry.lastBadgeSprite == null)
        {

            lastBadgeImage.gameObject.SetActive(false);
            return;

        }

        lastBadgeImage.gameObject.SetActive(true);
        lastBadgeImage.sprite = visualEntry.lastBadgeSprite;

        // 현재 게임 계급이 확정되면 이전 계급장은 어둡게
        if (playerState.ThisGameHierarchy != PlayerHierarchy.None)
        {

            lastBadgeImage.color = dimmedLastBadgeColor;

        }
        else
        {

            lastBadgeImage.color = normalLastBadgeColor;

        }

    }

    private void RefreshCurrentHierarchyVisual()
    {

        if (currentHierarchyImage == null)
        {

            return;

        }

        PlayerHierarchy currentHierarchy = playerState.ThisGameHierarchy;

        // 다음 게임 시작 등으로 Current가 None으로 돌아간 경우 초기화
        if (currentHierarchy == PlayerHierarchy.None)
        {

            currentHierarchyImage.gameObject.SetActive(false);
            currentHierarchyImage.color = Color.white;
            currentHierarchyImage.transform.localScale = Vector3.one;

            lastPresentedCurrentHierarchy = PlayerHierarchy.None;
            return;

        }

        HierarchyVisualEntry visualEntry = FindVisualEntry(currentHierarchy);

        if (visualEntry == null || visualEntry.currentResolvedSprite == null)
        {

            currentHierarchyImage.gameObject.SetActive(false);
            return;

        }

        currentHierarchyImage.gameObject.SetActive(true);
        currentHierarchyImage.sprite = visualEntry.currentResolvedSprite;

        bool justResolvedNow =
            lastPresentedCurrentHierarchy == PlayerHierarchy.None &&
            currentHierarchy != PlayerHierarchy.None;

        if (justResolvedNow)
        {

            PlayHierarchyRevealEffect();

        }

        lastPresentedCurrentHierarchy = currentHierarchy;

    }

    private void RefreshSpecialFinishVisual()
    {

        if (miyakoOchiImage == null) return;

        bool showMiyakoOchi = playerState.FinishType == PlayerFinishType.MiyakoOchi;

        miyakoOchiImage.gameObject.SetActive(showMiyakoOchi);

    }

    private void RefreshTurnHighlight(bool isCurrentTurn)
    {

        if (highlightImage == null)
        {

            return;

        }

        highlightImage.color = normalHighlightColor;
        highlightImage.gameObject.SetActive(isCurrentTurn);

    }

    private HierarchyVisualEntry FindVisualEntry(PlayerHierarchy hierarchy)
    {

        for (int i = 0; i < hierarchyVisualEntries.Count; i++)
        {

            if (hierarchyVisualEntries[i].hierarchy == hierarchy)
            {

                return hierarchyVisualEntries[i];

            }

        }

        return null;

    }

    private void PlayHierarchyRevealEffect()
    {

        if (currentHierarchyImage == null)
        {

            return;

        }

        if (hierarchyRevealCoroutine != null)
        {

            StopCoroutine(hierarchyRevealCoroutine);

        }

        hierarchyRevealCoroutine = StartCoroutine(CoPlayHierarchyRevealEffect());

    }

    private IEnumerator CoPlayHierarchyRevealEffect()
    {

        currentHierarchyImage.gameObject.SetActive(true);

        RectTransform rectTransform = currentHierarchyImage.rectTransform;
        rectTransform.localScale = Vector3.one * revealStartScale;

        Color targetColor = currentHierarchyImage.color;
        targetColor.a = 1.0f;
        currentHierarchyImage.color = targetColor;

        float elapsed = 0.0f;

        while (elapsed < revealScaleDuration)
        {

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / revealScaleDuration);
            rectTransform.localScale = Vector3.Lerp(
                Vector3.one * revealStartScale,
                Vector3.one,
                t
            );

            yield return null;

        }

        rectTransform.localScale = Vector3.one;

        elapsed = 0.0f;

        Color startColor = currentHierarchyImage.color;
        startColor.a = 0.0f;
        currentHierarchyImage.color = startColor;

        while (elapsed < revealFadeDuration)
        {

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / revealFadeDuration);

            Color fadeColor = currentHierarchyImage.color;
            fadeColor.a = Mathf.Lerp(0.0f, 1.0f, t);
            currentHierarchyImage.color = fadeColor;

            yield return null;

        }

        Color finalColor = currentHierarchyImage.color;
        finalColor.a = 1.0f;
        currentHierarchyImage.color = finalColor;

    }

    private string HierarchyToKorean(PlayerHierarchy hierarchy)
    {

        switch (hierarchy)
        {

            case PlayerHierarchy.Daifugo:
                return "Daifugo";

            case PlayerHierarchy.Fugo:
                return "Fugo";

            case PlayerHierarchy.Heimin:
                return "Heimin";

            case PlayerHierarchy.Hinmin:
                return "Hinmin";

            case PlayerHierarchy.Daihinmin:
                return "Daihinmin";

            default:
                return "-";

        }

    }

    public void PlayHighlightEffect()
    {

        if (presentationCoroutine != null)
        {

            StopCoroutine(presentationCoroutine);

        }

        presentationCoroutine = StartCoroutine(CoPlayHighlightEffect());

    }

    public void PlaySkipEffect()
    {

        if (presentationCoroutine != null)
        {

            StopCoroutine(presentationCoroutine);

        }

        presentationCoroutine = StartCoroutine(CoPlaySkipEffect());

    }

    private IEnumerator CoPlayHighlightEffect()
    {

        if (highlightImage != null)
        {

            highlightImage.gameObject.SetActive(true);
            highlightImage.color = normalHighlightColor;

        }

        if (statusEffectText != null)
        {

            statusEffectText.gameObject.SetActive(false);

        }

        yield return new WaitForSeconds(0.35f);

        if (highlightImage != null)
        {

            highlightImage.gameObject.SetActive(false);

        }

    }

    private IEnumerator CoPlaySkipEffect()
    {

        if (highlightImage != null)
        {

            highlightImage.gameObject.SetActive(true);
            highlightImage.color = skipHighlightColor;

        }

        if (statusEffectText != null)
        {

            statusEffectText.gameObject.SetActive(true);
            statusEffectText.text = "Skip";

        }

        yield return new WaitForSeconds(0.6f);

        if (highlightImage != null)
        {

            highlightImage.color = normalHighlightColor;
            highlightImage.gameObject.SetActive(false);

        }

        if (statusEffectText != null)
        {

            statusEffectText.gameObject.SetActive(false);

        }

    }

}
