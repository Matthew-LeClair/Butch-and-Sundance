using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destructible : MonoBehaviour, I_Damage
{
    [Header("Health")]
    [SerializeField] private float Health;
    [SerializeField] private float HealthMax;

    [Header("Break Effect")]
    [SerializeField] private ParticleSystem BreakParticle;
    [SerializeField] private float particleDuration;
    [SerializeField] private bool DestroyAfterBreak;

    [Header("Hit Feedback")]
    [SerializeField] private ParticleSystem HitParticle;
    [SerializeField] private float HitFlashDuration;
    [SerializeField] private Color HitFlashColor = Color.red;

    private bool IsBroken;
    private Material[] ObjectMaterials;
    private Color[] OriginalColors;

    private void Awake()
    {
        Health = HealthMax;

        List<Material> Mats = new List<Material>();
        foreach (Renderer R in GetComponentsInChildren<Renderer>())
        {
            Mats.AddRange(R.materials);
        }
        ObjectMaterials = Mats.ToArray();

        OriginalColors = new Color[ObjectMaterials.Length];

        for (int i = 0; i < ObjectMaterials.Length; i++)
        {
            OriginalColors[i] = GetMaterialColor(ObjectMaterials[i]);
        }
    }

    public void TakeDamage(int Amount, bool AlienTech)
    {
        if (IsBroken) return;

        Health -= Amount;

        if (Health <= 0f)
        {
            Break();
            return;
        }

        if (HitParticle != null) HitParticle.Play();
        StartCoroutine(HitFlash());
    }

    void Break()
    {
        IsBroken = true;

        foreach (Renderer R in GetComponentsInChildren<Renderer>()) R.enabled = false;

        foreach (Collider C in GetComponentsInChildren<Collider>()) C.enabled = false;

        if (BreakParticle != null)
        {
            BreakParticle.transform.SetParent(null);
            BreakParticle.Play();
        }

        StartCoroutine(CleanUp());
    }

    IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(particleDuration);

        if(BreakParticle != null) Destroy(BreakParticle.gameObject);

        if (DestroyAfterBreak) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    IEnumerator HitFlash()
    {
        for (int i = 0; i < ObjectMaterials.Length; i++)
        {
            SetMaterialColor(ObjectMaterials[i], HitFlashColor);
        }

        yield return new WaitForSeconds(HitFlashDuration);

        for (int i = 0; i < ObjectMaterials.Length; i++)
        {
            SetMaterialColor(ObjectMaterials[i], OriginalColors[i]);
        }
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
