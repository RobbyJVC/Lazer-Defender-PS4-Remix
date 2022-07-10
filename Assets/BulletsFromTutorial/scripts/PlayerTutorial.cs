using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTutorial : MonoBehaviour
{
    public int starthp;
    int hp;
    public float bulletCooldown; //whenever a bullet hits the player, we want a limbo period so the player doesn't get hit constantlly
    float bulletTimer;
    // Start is called before the first frame update
    void Start()
    {
        hp = starthp;
    }

    // Update is called once per frame
    void Update()
    {
        bulletTimer -= Time.deltaTime;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet" && bulletTimer <=0)
        {
            hp = 1;
            print(hp);
            bulletTimer = bulletCooldown;

        }
    }
}
