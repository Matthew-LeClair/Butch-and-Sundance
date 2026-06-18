using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

[RequireComponent(typeof(Collider))]
public class DisappearingPlatforms : MonoBehaviour
{
    [Header("Trigger Setting")]
    [SerializeField] private string PlayerTag = "Player";

    [Header("Timing")]
    [SerializeField] private float StandTimeRequired = 3f;
    [SerializeField] private float FadeAwayDur = 1.5f;
    [SerializeField] private bool ResetTimerOnExit = true;

    [Header("After Fading")]
    [SerializeField] private bool DestroyAfterFade;

    [Header("Warning Flash")]
    [SerializeField] private Color FlashColor = Color.red;
    [SerializeField] private float FlashDuration;
    private Coroutine FlashRoutine;

    private float StandTimer;
    private bool PlayerOnPlatform;
    private bool IsFading;

    private Collider[] PlatformColliders;
    private Material[] FadeMaterials;

    void Awake()
    {
        PlatformColliders = GetComponentsInChildren<Collider>();

        List<Material> Mats = new List<Material>();
        foreach (Renderer R in GetComponentsInChildren<Renderer>())
        {
            Mats.AddRange(R.materials);
        }
        FadeMaterials = Mats.ToArray();
    }

    void Update()
    {
        if (IsFading) return;

        if (PlayerOnPlatform)
        {
            StandTimer += Time.deltaTime;
            if (StandTimer >= StandTimeRequired)
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(PlayerTag)) PlayerOnPlatform = true;

        if (FlashRoutine == null) FlashRoutine = StartCoroutine(FlashRed());
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        PlayerOnPlatform = false;

        if (ResetTimerOnExit)
        {
            StandTimer = 0f;

            if(FlashRoutine != null)
            {
                StopCoroutine(FlashRoutine);
                FlashRoutine = null;
                RestoreOriginalColors();
            }
        }
    }

    IEnumerator FadeOut()
    {
        IsFading = true;

        if (FlashRoutine != null)
        {
            StopCoroutine (FlashRoutine);
            FlashRoutine = null;
        }

        float Timer = 0f;

        Color[] StartColors = new Color[FadeMaterials.Length];

        for (int i = 0; i <FadeMaterials.Length; i++)
        {
            StartColors[i] = GetMaterialColor(FadeMaterials[i]);
        }

        while (Timer < FadeAwayDur)
        {
            Timer += Time.deltaTime;
            float Percent = Timer / FadeAwayDur;

            for (int i = 0; i < FadeMaterials.Length; i++)
            {
                Color C = StartColors[i];
                C.a = Mathf.Lerp(StartColors[i].a, 0f, Percent);
                SetMaterialColor(FadeMaterials[i], C);
            }
            yield return null;
        }
        foreach (Collider C in PlatformColliders) C.enabled = false;

        if (DestroyAfterFade) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    Color GetMaterialColor(Material Mat)
    {
        if (Mat.HasProperty("_BaseColor")) return Mat.GetColor("_BaseColor");
        if (Mat.HasProperty("_Color")) return Mat.color;
        return Color.white;
    }

    void SetMaterialColor(Material Mat, Color C)
    {
        if (Mat.HasProperty("_BaseColor")) Mat.SetColor("_BaseColor", C);
        else if (Mat.HasProperty("_Color")) Mat.color = C;
    }

    IEnumerator FlashRed()
    {
        Color[] OriginalColors = new Color[FadeMaterials.Length];

        for (int i = 0; i < FadeMaterials.Length; i++)
        {
            OriginalColors[i] = GetMaterialColor(FadeMaterials[i]);
        }

        while (!IsFading)
        {
            for (int i = 0; i < FadeMaterials.Length; i++)
            {
                Color C = FlashColor;
                C.a = OriginalColors[i].a;
                SetMaterialColor(FadeMaterials[i], C);
            }

            yield return new WaitForSeconds(FlashDuration);

            for (int i = 0; i < FadeMaterials.Length; i++)
            {
                SetMaterialColor(FadeMaterials[i], OriginalColors[i]);
            }

            yield return new WaitForSeconds (1f -  FlashDuration);
        }
    }

    void RestoreOriginalColors()
    {
        foreach (Material Mat in FadeMaterials)
        {
            Color C = GetMaterialColor(Mat);
            C.r = 1f;
            C.g = 1f;
            C.b = 1f;
            SetMaterialColor (Mat, C);
        }
    }
}
