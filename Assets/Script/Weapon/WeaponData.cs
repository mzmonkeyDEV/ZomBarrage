using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Survivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject projectilePrefab;
    public bool isMainWeapon;

    [System.Serializable]
    public struct WeaponLevel
    {
        public float damage;
        public float fireRate;
        public float range;
        public int projectileCount;
        public string description; // "Adds +1 projectile"
        public float speed;
    }

    public List<WeaponLevel> levels;
}