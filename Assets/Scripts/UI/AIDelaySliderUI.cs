using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIDelaySliderUI : MonoBehaviour
{

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private Slider delaySlider;

    [SerializeField]
    private TMP_Text delayValueText;

    [SerializeField]
    private string labelPrefix = "AI 턴 속도 : ";

    private void Start()
    {

        if (gameManager == null)
        {

            gameManager = FindFirstObjectByType<GameManager>();

        }

        if (delaySlider == null)
        {

            Debug.Log("[AIDelaySliderUI] delaySlider가 비어 있습니다.");
            return;

        }

        delaySlider.minValue = 0.3f;
        delaySlider.maxValue = 1.5f;
        delaySlider.wholeNumbers = false;

        if (gameManager != null)
        {

            delaySlider.value = gameManager.AITurnDelay;

        }
        else
        {

            delaySlider.value = 0.5f;

        }

        RefreshLabel(delaySlider.value);

        delaySlider.onValueChanged.AddListener(OnSliderValueChanged);

    }

    private void OnDestroy()
    {

        if (delaySlider != null)
        {

            delaySlider.onValueChanged.RemoveListener(OnSliderValueChanged);

        }

    }

    private void OnSliderValueChanged(float value)
    {

        if (gameManager != null)
        {

            gameManager.SetAITurnDelay(value);

        }

        RefreshLabel(value);

    }

    private void RefreshLabel(float value)
    {

        if (delayValueText == null) return;

        delayValueText.text = $"{labelPrefix}{value:F1}s";

    }

}

