using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    //Configuration Parameters
    [Header("Player")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float pcmoveSpeed = 4f;
    [SerializeField] float xPadding = 0.75f;
    [SerializeField] float yPadding = 0.5f;
    [SerializeField] int health = 500;
    [SerializeField] [Range(0, 1)] float deathSoundVolume = 0.8f;
    [SerializeField] AudioClip deathSFX;
    [SerializeField] GameObject deathVFX;
    [SerializeField] float durationOfExplosion = 1f;
    [SerializeField] AudioClip enemyContactSound;
    [SerializeField] [Range(0, 1)] float contactSoundVolume = 0.4f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 15f;
    [SerializeField] float projectileFiringPeriod = .5f;
    [SerializeField] AudioClip shootSound;
    [SerializeField] [Range(0, 1)] float shootSoundVolume = 0.15f;
    [SerializeField] int enemyProjectileDamage = 100;
    [SerializeField] float increaseProjectileSpeed = .2f;
    float fastestSpeed = .1f;

    Coroutine firingCoroutine;
    Coroutine firingCoroutine2;
    Coroutine firingCoroutine3;
    Coroutine firingCoroutine4;


    // Coordinate Variables
    float xMin;
    float xMax;
    float yMin;
    float yMax;
    int fireDirection;
    // Start is called before the first frame update
    void Start()
    {
        SetUpMoveBoundaries();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire(); //Fire method that shoots straight forward
        FireRight();//Fire method that shoots straight right
        
        FireDown();//Fire method that shoots straight down
        FireLeft();//Fire method that shoots straight left
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If you get hit, trigger sound effect
        AudioSource.PlayClipAtPoint(enemyContactSound, Camera.main.transform.position, contactSoundVolume);

        // 'other' is the object that has just bumped into the enemy
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();
        // If there is no damage dealer, exit the method
        if (!damageDealer == null)
        {
            return;
        }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        FindObjectOfType<GameSession>().DeductHealth(damageDealer.GetDamage());
        damageDealer.Hit();

        //FindObjectOfType<GameSession>().DeductHealth(scoreValue);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Game Over
        FindObjectOfType<Level>().LoadGameOver();

        Destroy(gameObject);
        // Create particle explosion effect where the player has died
        GameObject explosion = Instantiate(
            deathVFX,
            transform.position,
            transform.rotation);
        Destroy(explosion, durationOfExplosion);
        //Play sound effect
        // AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, deathSoundVolume);
    }

    public void IncreasePlayerShootingSpeed()
    {
        // You're subtracting since its a time period
        projectileFiringPeriod -= increaseProjectileSpeed;
    }

    public float GetFiringPeriod()
    {
        //Debug.Log("GetFiringPeriod() Now At: " + projectileFiringPeriod);
        return projectileFiringPeriod;
    }

    public void SetFiringPeriodMin()
    {
        projectileFiringPeriod = fastestSpeed;
    }

    private void Move()
    {
        var deltaX = Input.GetAxisRaw("Horizontal") * Time.deltaTime * pcmoveSpeed;
        var deltaY = Input.GetAxisRaw("Vertical") * Time.deltaTime * pcmoveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

        transform.position = new Vector2(newXPos, newYPos);

        if (Application.platform == RuntimePlatform.PS4)
        {
            moveSpeed = 10f; //this is the default speed. at moveSpeed's initilization we set it to to account for getaxisraw reacting too quickly for pc. we 
            deltaX = Input.GetAxisRaw("leftstick1horizontal") * Time.deltaTime * moveSpeed;
            deltaY = Input.GetAxisRaw("leftstick1vertical") * Time.deltaTime * moveSpeed;
        }
        newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);

        transform.position = new Vector2(newXPos, newYPos);
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;

        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + xPadding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - xPadding;

        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + yPadding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - yPadding;
    }

    private void Fire()
    {

        if (Input.GetButtonDown("Fire1"))
        {


            fireDirection = 1; //if X or click, fire Up. up and right direction are positive values
            firingCoroutine = StartCoroutine(FireContinuously(fireDirection));
        }
        if (Input.GetButtonUp("Fire1"))
        {
            fireDirection = 2;

            StopCoroutine(firingCoroutine);
        }


    }

    // Method responsible for firing laser continously
    IEnumerator FireContinuously(int fireDirection)
    {
        while (true)
        {
            GameObject laser = Instantiate(
                laserPrefab,
                transform.position,
                Quaternion.identity) as GameObject;
            switch (fireDirection)
            {
                case 1:
                    if (projectileSpeed < 0)
                        projectileSpeed *= -1;
                    laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, projectileSpeed);//up
                    AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
                    yield return new WaitForSeconds(projectileFiringPeriod);
                    break;
                case 2:
                    if (projectileSpeed < 0)
                        projectileSpeed *= -1;
                    laser.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileSpeed-6, 0f);//right
                    AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
                    yield return new WaitForSeconds(3);
                    break;
                    case 3:
                     if(projectileSpeed>0)
                     projectileSpeed *= -1;
                    laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, projectileSpeed+6);//down
                     AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
                    yield return new WaitForSeconds(3);
                     break;
                      case 4:
                    if (projectileSpeed > 0)
                        projectileSpeed *= -1;
                    laser.GetComponent<Rigidbody2D>().velocity = new Vector2(projectileSpeed+6, 0f);//left
                     AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
                     yield return new WaitForSeconds(3);
                      break;
                     default:

                      break;

            }




        }
    }
    private void FireRight()
    {

        if (Input.GetButtonDown("Fire2"))
        {

            fireDirection = 2;

            firingCoroutine2 = StartCoroutine(FireContinuously(fireDirection));
        }
        if (Input.GetButtonUp("Fire2"))
        {


            StopCoroutine(firingCoroutine2);
        }
    }
    private void FireDown()
    {
        if (Input.GetButtonDown("Fire3"))
        {

            fireDirection = 3;

            firingCoroutine3 = StartCoroutine(FireContinuously(fireDirection));
        }
        if (Input.GetButtonUp("Fire3"))
        {


            StopCoroutine(firingCoroutine3);
        }
    }
    private void FireLeft()
    {
        if (Input.GetButtonDown("Fire4"))
        {

            fireDirection = 4;

            firingCoroutine4 = StartCoroutine(FireContinuously(fireDirection));
        }
        if (Input.GetButtonUp("Fire4"))
        {


            StopCoroutine(firingCoroutine4);
        }
    }
}
