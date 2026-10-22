using UnityEngine;

public class GoldExplosionScript : MonoBehaviour
{
    private ParticleSystem goldExplosion;
    private Transform goldIconTransform;
    private BoxCollider2D goldIconCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        goldExplosion = GetComponent<ParticleSystem>();
        GameObject goldIcon = GameObject.Find("GoldIconTarget");
        goldIconTransform = goldIcon.transform;
        goldIconCollider = goldIcon.GetComponent<BoxCollider2D>();
        ParticleSystem.TriggerModule triggerModule = goldExplosion.trigger;
        triggerModule.AddCollider(goldIconCollider);
        Invoke("StartEffect", 1.5f);
        

        
    }

    void StartEffect()
    {
        ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityModule = goldExplosion.limitVelocityOverLifetime;
        limitVelocityModule.enabled = false;
        Vector3 explosionOffset = goldIconTransform.position - transform.position;
        ParticleSystem.VelocityOverLifetimeModule velocityModule = goldExplosion.velocityOverLifetime;
        velocityModule.orbitalOffsetX = explosionOffset.x;
        // velocityModule.orbitalOffsetY = explosionOffset.y;
        velocityModule.orbitalOffsetY = explosionOffset.y;
        velocityModule.radial = -10f;
        velocityModule.enabled = true;
        
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
