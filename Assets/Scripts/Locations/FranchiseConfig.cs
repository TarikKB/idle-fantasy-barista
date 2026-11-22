using UnityEngine;

[CreateAssetMenu(menuName = "Coffee/Franchise Config")]
public class FranchiseConfig : ScriptableObject
{
    [SerializeField, Min(1)] private int _requiredFame = 50;
    [SerializeField, Min(1)] private double _openingCost = 50000;
    [SerializeField, Range(0, 1)] private float _passiveEfficiency = 0.3f;
    [SerializeField, Min(0)] private int _offlineCapSeconds = 28800;
    [SerializeField] private CMData _starterMachine;
    public int RequiredFame => _requiredFame;
    public double OpeningCost => _openingCost;
    public float PassiveEfficiency => _passiveEfficiency;
    public int OfflineCapSeconds => _offlineCapSeconds;
    public CMData StarterMachine => _starterMachine;
    public bool IsValid => _requiredFame > 0 && _openingCost > 0 && !double.IsNaN(_openingCost) && !double.IsInfinity(_openingCost) &&
        _passiveEfficiency >= 0 && _passiveEfficiency <= 1 && _offlineCapSeconds >= 0 && _starterMachine != null;
}
