using UnityEngine;

public class GoldExplosionScript : MonoBehaviour
{
    public static Transform TargetTransform { get; set; }
    public static BoxCollider2D TargetCollider { get; set; }

    private ParticleSystem goldExplosion;
    private Transform goldIconTransform;
    private BoxCollider2D goldIconCollider;

    void Start()
    {
        goldExplosion = GetComponent<ParticleSystem>();

        if (TargetTransform != null)
        {
            goldIconTransform = TargetTransform;
            goldIconCollider = TargetCollider;
        }
        else
        {
            GameObject goldIcon = GameObject.Find("GoldIconTarget");
            if (goldIcon != null)
            {
                goldIconTransform = goldIcon.transform;
                goldIconCollider = goldIcon.GetComponent<BoxCollider2D>();
                TargetTransform = goldIconTransform;
                TargetCollider = goldIconCollider;
            }
        }

        if (goldExplosion != null && goldIconCollider != null)
        {
            ParticleSystem.TriggerModule triggerModule = goldExplosion.trigger;
            triggerModule.AddCollider(goldIconCollider);
        }

        Invoke(nameof(StartEffect), 1.5f);
    }

    void StartEffect()
    {
        if (goldExplosion == null || goldIconTransform == null) return;

        ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityModule = goldExplosion.limitVelocityOverLifetime;
        limitVelocityModule.enabled = false;
        Vector3 explosionOffset = goldIconTransform.position - transform.position;
        ParticleSystem.VelocityOverLifetimeModule velocityModule = goldExplosion.velocityOverLifetime;
        velocityModule.orbitalOffsetX = explosionOffset.x;
        velocityModule.orbitalOffsetZ = explosionOffset.y;
        velocityModule.radial = -10f;
        velocityModule.enabled = true;
    }
}

