using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource sfxSource;

    [Header("Clips - BGM")]
    [SerializeField]
    private AudioClip bgmClip;

    [Header("Clips - SFX")]
    [SerializeField]
    private AudioClip cardPlaceSfx;

    [SerializeField]
    private AudioClip turnChangeSfx;

    [SerializeField]
    private AudioClip hierarchyConfirmSfx;

    [SerializeField]
    private AudioClip miyakoOchiSfx;

    [SerializeField]
    private AudioClip revolutionSfx;

    [Header("BGM Settings")]
    [SerializeField, Range(0f, 1f)]
    private float bgmVolume = 1.0f;

    [SerializeField, Range(0f, 1f)]
    private float loweredBgmRatio = 0.3f;

    private bool isBgmLowered = false;

    [Header("SFX Master Settings")]
    [SerializeField, Range(0f, 1f)]
    private float sfxMasterVolume = 1.0f;

    [Header("Individual SFX Volumes")]
    [SerializeField, Range(0f, 1f)]
    private float cardPlaceVolume = 0.1f;

    [SerializeField, Range(0f, 1f)]
    private float turnChangeVolume = 0.02f;

    [SerializeField, Range(0f, 1f)]
    private float hierarchyConfirmVolume = 0.1f;

    [SerializeField, Range(0f, 1f)]
    private float miyakoOchiVolume = 0.1f;

    [SerializeField, Range(0f, 1f)]
    private float revolutionVolume = 0.1f;

    private void Awake()
    {

        if (Instance == null)
        {

            Instance = this;

        }
        else
        {

            Destroy(gameObject);
            return;

        }

    }

    private void Start()
    {

        PlayBGM();

    }

    public void PlayBGM()
    {

        if (bgmSource == null || bgmClip == null) return;

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.mute = false;
        ApplyBGMVolume();
        bgmSource.Play();

    }

    public void SetBGMVolume(float value)
    {

        bgmVolume = Mathf.Clamp01(value);

        ApplyBGMVolume();

    }

    public void SetSFXMasterVolume(float value)
    {

        sfxMasterVolume = Mathf.Clamp01(value);

    }

    public float GetBGMVolume()
    {

        return bgmVolume;

    }

    public float GetSFXMasterVolume()
    {

        return sfxMasterVolume;

    }

    public void LowerBGM()
    {

        isBgmLowered = true;
        ApplyBGMVolume();

    }

    public void RestoreBGM()
    {

        isBgmLowered = false;
        ApplyBGMVolume();

    }

    private void ApplyBGMVolume()
    {

        if (bgmSource == null) return;

        if (isBgmLowered)
        {

            bgmSource.volume = bgmVolume * loweredBgmRatio;

        }
        else
        {

            bgmSource.volume = bgmVolume;

        }

    }

    public void PlayCardPlace()
    {

        PlaySFX(cardPlaceSfx, cardPlaceVolume);

    }

    public void PlayTurnChange()
    {

        PlaySFX(turnChangeSfx, turnChangeVolume);

    }

    public void PlayHierarchyConfirm()
    {

        PlaySFX(hierarchyConfirmSfx, hierarchyConfirmVolume);

    }

    public void PlayMiyakoOchi()
    {

        PlaySFX(miyakoOchiSfx, miyakoOchiVolume);

    }

    public void PlayRevolution()
    {

        PlaySFX(revolutionSfx, revolutionVolume);

    }

    private void PlaySFX(AudioClip clip, float individualVolume)
    {

        if (sfxSource == null || clip == null) return;

        float finalVolume = individualVolume * sfxMasterVolume;

        sfxSource.mute = false;
        sfxSource.PlayOneShot(clip, finalVolume);

    }

}