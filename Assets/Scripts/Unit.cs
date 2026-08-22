using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public float damage;
    public float maxHP;
    public float currentHP;

    public bool TakeDamage(float dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
            return true; //true = dead
        else 
            return false;
    }
}
