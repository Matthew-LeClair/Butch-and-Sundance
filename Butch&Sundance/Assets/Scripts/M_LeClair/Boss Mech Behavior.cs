using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossMechBehavior : EnemyBehavior
{
    public enum BossPhase { PhaseOne, PhaseTwo }
    BossPhase currentPhase = BossPhase.PhaseOne;

    [Header("Phase")]
    [SerializeField] float phaseTwoThreshold = 0.5f;
    bool phaseTransitioned = false;

    [Header("Projectile")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform[] shootPoints;
    [SerializeField] int burstCount = 3;
    [SerializeField] float burstInterval = 0.15f;
    [SerializeField] float bulletSpeed = 15f;

    [Header("Charge")]
    [SerializeField] float chargeWindup = 1.2f;
    [SerializeField] float chargeSpeed = 18f;
    [SerializeField] float chargeDuration = 0.5f;
    [SerializeField] ExplosionScript chargeExplosion;

    [Header("Pattern Timing")]
    [SerializeField] float shootPhaseDuration = 4f;
    [SerializeField] float chargePhaseDuration = 2f;
    [SerializeField] float restDuration = 1.5f;

    [Header("Phase 2")]
    [SerializeField] float p2SpeedMultiplier = 1.5f;
    [SerializeField] int p2BurstCount = 5;
    [SerializeField] Color p2Color = Color.red;

    [Header("Gun Rotation")]
    [SerializeField] Transform[] gunPivots;
    [SerializeField] float gunRotateSpeed = 5f;

    bool patternStarted = false;

    public override void Tick()
    {
        CheckPhase();

        if (ai.anim != null)
        {
            float speed = ai.agent.velocity.magnitude;
            float currentSpeed = ai.anim.GetFloat("Speed");
            ai.anim.SetFloat("Speed", Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 5f));
        }

        if (!patternStarted)
        {
            patternStarted = true;
            ai.StartCoroutine(PatternLoop());
        }

        if (ai.seePlayer)
        {
            ai.FOV = 180f;
            ai.rotateToTarget(new Vector3(ai.playerDir.x, 0, ai.playerDir.z).normalized);
        }
        else
        {
            ai.FOV = ai.FOVOrig;
        }

        if(ai.CurrHealth <= 0)
        {
            GameManager.Instance.GoalCompleted = true;
        }
    }

    IEnumerator PatternLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            yield return ai.StartCoroutine(ShootPhase());

            yield return new WaitForSeconds(restDuration);

            yield return ai.StartCoroutine(ChargePhase());

            yield return new WaitForSeconds(restDuration);
        }
    }

    IEnumerator ShootPhase()
    {
        Renderer[] renderers = ai.GetComponentsInChildren<Renderer>();
        float chargeUp = 0f;
        bool flashOn = false;

        float tellDuration = shootPhaseDuration * 0.3f;
        while (chargeUp < tellDuration)
        {
            chargeUp += 0.2f;
            flashOn = !flashOn;
            foreach (Renderer r in renderers)
                r.material.color = flashOn ? Color.white :
                    currentPhase == BossPhase.PhaseTwo ? p2Color : ai.OriginalColor;
            yield return new WaitForSeconds(0.2f);
        }

        foreach (Renderer r in renderers)
            r.material.color = currentPhase == BossPhase.PhaseTwo ? p2Color : ai.OriginalColor;

        if (ai.anim != null) ai.anim.enabled = false;

        float rotateElapsed = 0f;
        while (rotateElapsed < 0.5f)
        {
            rotateElapsed += Time.deltaTime;
            foreach (Transform gun in gunPivots)
            {
                if (gun == null) continue;
                Vector3 targetDir = ai.seePlayer && ai.player != null
                    ? (ai.player.position + Vector3.up * 1f - gun.position).normalized
                    : ai.transform.forward;

                Quaternion worldTarget = Quaternion.LookRotation(targetDir);
                Quaternion localTarget = Quaternion.Inverse(gun.parent.rotation) * worldTarget;
                gun.localRotation = Quaternion.Lerp(gun.localRotation, localTarget,
                    Time.deltaTime * gunRotateSpeed);
            }
            yield return null;
        }

        if (ai.anim != null) { ai.anim.enabled = true; ai.anim.SetTrigger("Shoot"); }

        int count = currentPhase == BossPhase.PhaseTwo ? p2BurstCount : burstCount;
        for (int i = 0; i < count; i++)
        {
            if (shootPoints != null && shootPoints.Length > 0 && bulletPrefab != null)
            {
                Transform sp = shootPoints[Random.Range(0, shootPoints.Length)];

                Vector3 dir = ai.seePlayer && ai.player != null
                    ? (ai.player.position + Vector3.up * 1f - sp.position).normalized
                    : sp.forward;

                GameObject bullet = GameObject.Instantiate(bulletPrefab, sp.position,
                    Quaternion.LookRotation(dir));

                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null) rb.linearVelocity = dir * bulletSpeed;

                Damage d = bullet.GetComponent<Damage>();
                if (d != null)
                {
                    d.OwnerTag = "Enemy";
                    d.shooter = ai.transform;
                    d.MaxRange = 30f;
                }
            }
            yield return new WaitForSeconds(burstInterval);
        }

        foreach (Transform gun in gunPivots)
            if (gun != null) gun.localRotation = Quaternion.identity;
    }

    IEnumerator ChargePhase()
    {
        Renderer[] renderers = ai.GetComponentsInChildren<Renderer>();

        float elapsed = 0f;
        bool flashOn = false;
        while (elapsed < chargeWindup)
        {
            elapsed += 0.15f;
            flashOn = !flashOn;
            foreach (Renderer r in renderers)
                r.material.color = flashOn ? Color.red :
                    currentPhase == BossPhase.PhaseTwo ? p2Color : ai.OriginalColor;
            yield return new WaitForSeconds(0.15f);
        }

        foreach (Renderer r in renderers)
            r.material.color = currentPhase == BossPhase.PhaseTwo ? p2Color : ai.OriginalColor;

        if (ai.anim != null) ai.anim.SetTrigger("Charge");

        Vector3 chargeDir = ai.seePlayer && ai.player != null
            ? (ai.player.position - ai.transform.position).normalized
            : ai.transform.forward;

        ai.agent.enabled = false;
        float timer = 0f;

        while (timer < chargeDuration)
        {
            timer += Time.deltaTime;
            ai.transform.position += chargeDir * chargeSpeed * Time.deltaTime;
            yield return null;
        }

        if (chargeExplosion != null) chargeExplosion.Explode();

        ai.agent.enabled = true;

        ai.CheckRoam();
    }

    void CheckPhase()
    {
        if (phaseTransitioned) return;

        float hpPercent = ai.CurrHealth / ai.MaxHealth;
        if (hpPercent <= phaseTwoThreshold)
        {
            phaseTransitioned = true;
            currentPhase = BossPhase.PhaseTwo;
            ai.StartCoroutine(PhaseTransition());
        }
    }

    IEnumerator PhaseTransition()
    {
        Renderer[] renderers = ai.GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        bool flashOn = false;

        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            flashOn = !flashOn;
            foreach (Renderer r in renderers)
                r.material.color = flashOn ? p2Color : ai.OriginalColor;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (Renderer r in renderers)
            r.material.color = p2Color;

        ai.agent.speed *= p2SpeedMultiplier;
        burstCount = p2BurstCount;
        chargeWindup *= 0.7f;
        restDuration *= 0.7f;
    }
}