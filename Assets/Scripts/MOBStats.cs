using UnityEngine;

public enum AttackType
{
    melee,
    ranged,
}

[CreateAssetMenu(fileName = "MOBStats", menuName = "ScriptableObjects/MOBStats", order = 1)]
public class MOBStats : ScriptableObject
{
    public float attackSpeed = 1f;
    public float baseHealth = 300;
    public float baseMoveSpeed = 3;
    public AttackType attackType = AttackType.ranged;
    public float attackRange = 5;
    public float baseAttackDamage = 10;
}