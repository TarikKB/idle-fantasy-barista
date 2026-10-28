using Mono.Cecil;
using UnityEngine;

public class GCSummonScript : MonoBehaviour
{

    [SerializeField] private ResourceManagerScript resourceManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool TrySummon()
    {
        if (resourceManager != null)
        {
            return true;
        }
        return false;
    }
}
