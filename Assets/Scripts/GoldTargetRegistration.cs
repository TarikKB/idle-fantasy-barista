using UnityEngine;

public class GoldTargetRegistration : MonoBehaviour
{
    private void Awake()
    {
        RegisterTarget();
    }

    private void OnEnable()
    {
        RegisterTarget();
    }

    private void RegisterTarget()
    {
        GoldExplosionScript.TargetTransform = transform;
        GoldExplosionScript.TargetCollider = GetComponent<BoxCollider2D>();
    }

    private void OnDisable()
    {
        if (GoldExplosionScript.TargetTransform == transform)
        {
            GoldExplosionScript.TargetTransform = null;
            GoldExplosionScript.TargetCollider = null;
        }
    }
}
