using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{

    //[SerializeField] public int damage = 100; not normally public. this doesn't seem to change anything but something may be lazily checking this variable?
    //
    [SerializeField] int damage = 100;
    public int GetDamage()
    {
        return damage;
    }

    public void Hit()
    {
        Destroy(gameObject);
    }
   
}
