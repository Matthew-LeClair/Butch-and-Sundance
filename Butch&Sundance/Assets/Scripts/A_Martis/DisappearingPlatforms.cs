using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        PlayerOnPlatform = false;
        if (ResetTimerOnExit) StandTimer = 0f;
    }

    IEnumerator FadeOut()
    {
        IsFading = true;
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
}
