using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using static UnityEngine.Rendering.DebugUI;

public class RoninController : Enemy, IEnemy
{
    public float speed;
    public float attackRange;
    public GameObject[] bullet;
    public float timeBetweenAttacks;
    private Transform player;
    private Animator anim;
    public float maxHealth;
    public ParticleSystem particles;
    public GameObject cashParticles;
    public GameObject mine;
    public GameObject shield;
    private bool moneySpawned;
    private bool shieldActivated;
    private float shieldPercentage = 1.0f;
    private double meleeDesirability;
    private double shootDesirability;
    private bool damagedPlayer = false;
    private bool shoot = false;
    private RoninRotation rr;
    public GameObject target;
    public GameObject[] targets;
    private float healthAggresiveness;
    private double aggresiveness;
    private double fireRing;
    private double mineDesirability;
    private bool spawnedMine = false;
    private float takenDmg;
    public GameObject flameTarget;

    FuzzyRoninController fuzzyController;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        health = maxHealth;
        nextAttackTime = 1.0f;
        useRoomLogic = true;
        healthBar = GetComponent<HealthBar>();
        fuzzyController = new FuzzyRoninController();
        //meleeDesirability = fuzzyController.MeleeDesirability(returnDistanceInt(), 10, (int)takenDmg);
        rr = GetComponent<RoninRotation>();

        takenDmg = 0;
        //StartCoroutine(moveCooldown());
    }

    void Update()
    {
        timeBetweenAttacks = (float)(1.8f - (1.8f * aggresiveness)/100);
        speed = (float)(8f - (3f * aggresiveness) / 100);

        shieldPercentage = (float)(fuzzyController.Shield(returnDistanceInt()) / 100.0f);
        shootDesirability = fuzzyController.ShootDesirability(returnDistanceInt(),(int)shieldPercentage, (int)takenDmg);
        healthAggresiveness = CalculateHealthPercentage() * 100;
        aggresiveness = fuzzyController.Aggressiveness((int)healthAggresiveness, (int)takenDmg);
        fireRing = fuzzyController.FireRingDesirability(returnDistanceInt(), (int)aggresiveness);
        mineDesirability = fuzzyController.PlantMineDesirability(returnDistanceInt(), (int)aggresiveness);
        Debug.Log(aggresiveness);
        
        if (takenDmg > 600)
        {
            takenDmg = 0;
        }

        switch (currState)
        {

            case (EnemyState.Idle):
                break;
            case (EnemyState.ActivateShield):
                if (!shieldActivated)
                {
                    StartCoroutine(activateShield());
                }
                break;
            case (EnemyState.Teleport):
                {
                    StartCoroutine(Teleport());
                    break;
                }
            case (EnemyState.KeepDistance):
                {
                    HoldDistance(fuzzyController.KeepDistance(shootDesirability) / 100, 6);
                    //Debug.Log("hold distance");
                    break;
                }
            case (EnemyState.StayClose):
                {
                    MoveTowards(speed);
                    //Debug.Log("move towards");
                    break;
                }
            case (EnemyState.MeleeAttack):
                StartCoroutine(MeleeAttack());
                break;
            case (EnemyState.RangeAttack):
                {              
                    StartCoroutine(RangeAttack());
                }
                break;
            case (EnemyState.FlameAttack):
                {
                    StartCoroutine(FlameAttack());
                }
                break;
            case (EnemyState.ThrowMine):
                StartCoroutine(SpawnMine());
                break;


        }


        if (activeBehaviour)
        {
            if (health > 0)
                PrepareNextMove();
        }
        else
        {
            currState = EnemyState.Idle;
        }

   
    }

    [System.Obsolete]
    private void PrepareNextMove()
    {

        if (Time.time > nextAttackTime)
        {

            if (IsInAttackRange())
            {
                damagedPlayer = false;
                currState = EnemyState.MeleeAttack;
            }
            else
            {
                int r = 0;
                r = Random.Range(1, 301);
                anim.SetBool("IsMoving", false);
                if (r == 1)
                {
                    currState = EnemyState.Idle;
                }
                else if (r >= 2 && r <= (int)fuzzyController.ShieldChance((int)(CalculateHealthPercentage() * 100)) && shield.active == false)
                {
                    shieldActivated = false;
                    currState = EnemyState.ActivateShield;
                }
                else if (r >= 101 && r <= shootDesirability + 140)
                {
                    shoot = false;
                    if (r <= fireRing + shootDesirability + 20)
                        currState = EnemyState.FlameAttack;
                    else
                        currState = EnemyState.RangeAttack;
                }
                else if (r >= shootDesirability + 141 && r <= shootDesirability + 141 + mineDesirability)
                {
                    spawnedMine = false;
                    currState = EnemyState.ThrowMine;
                }
                //else if()
                //{
                //currState = EnemyState.Teleport;
                //currState = EnemyState.Idle;
                //}
                else
                {
                    currState = EnemyState.StayClose;
                }

            }
            nextAttackTime = Time.time + timeBetweenAttacks;
        }
    }

    //private void PrepareMovingState(double meleeDesirability, double shootDesirability)
    //{
    //    if (meleeDesirability > shootDesirability)
    //    {
    //        currState = EnemyState.StayClose;
    //    }
    //    else
    //    {
    //        currState = EnemyState.KeepDistance;
    //    }
    //}

    private void MoveTowards(float s)
    {
        if (health >= 0)
        {
            anim.SetBool("IsMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, s * Time.deltaTime);
        }
    }

    private void MoveAway(float s)
    {
        if (health >= 0)
        {
            anim.SetBool("IsMoving", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, -1 * s * Time.deltaTime);
        }
    }

    private void HoldDistance(double distance, float speed)
    {
        if (returnDistanceIntNormal() > distance)
        {
            MoveTowards(speed);
        }
        else if (returnDistanceIntNormal() < distance)
        {
            MoveAway(speed);
        }
        else
        {
            ;
        }
    }



    private int returnDistanceInt()
    {
        float dist = Vector2.Distance(transform.position, player.position) * 100;

        int distInt = (int)dist;


        return distInt;
    }

    private int returnDistanceIntNormal()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        int distInt = (int)dist;


        return distInt;
    }

    public new void TakeDamage(float damage)
    {
        takenDmg += damage;
        if (!particles.isPlaying)
            particles.Play();
        healthBar.SetHealthBarActive();
        health -= damage;
        healthBar.SetHealthBarValue(CalculateHealthPercentage());
        CheckDeath();
    }

    private float CalculateHealthPercentage()
    {
        return (health / maxHealth);
    }


    private new void CheckDeath()
    {
        if (health <= 0)
        {
            currState = EnemyState.Die;
            healthBar.SetHealthBarInActive();
            anim.SetBool("IsMoving", false);
            anim.SetBool("IsDead", true);

            if (useRoomLogic)
                RoomController.instance.StartCoroutine(RoomController.instance.RoomCorutine());

            if (!moneySpawned)
            {
                Instantiate(cashParticles, new Vector2(transform.position.x, transform.position.y + 1), Quaternion.identity);
                moneySpawned = true;
            }

            Destroy(gameObject, 1f);
        }
    }

    private IEnumerator SpawnMine()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsSpawningMine", true);
        yield return new WaitForSeconds(0.3f);
        int rand;
        int rand2;
        if (!spawnedMine)
        {
            rand = Random.Range(-3, 3);
            rand2 = Random.Range(-3, 3);
            Instantiate(mine, new Vector2(transform.position.x + rand, transform.position.y + rand2), Quaternion.identity);
            rand = Random.Range(-3, 3);
            rand2 = Random.Range(-3, 3);
            Instantiate(mine, new Vector2(transform.position.x + rand, transform.position.y + rand2), Quaternion.identity);
            rand = Random.Range(-3, 3);
            rand2 = Random.Range(-3, 3);
            Instantiate(mine, new Vector2(transform.position.x + rand, transform.position.y + rand2), Quaternion.identity);
            spawnedMine = true;
        }
        anim.SetBool("IsSpawningMine", false);
        currState = EnemyState.StayClose;
        if (health < 0)
            currState = EnemyState.Die;

    }

    private IEnumerator activateShield()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsSpawningBarrier", true);

        yield return new WaitForSeconds(0.6f);

        anim.SetBool("IsSpawningBarrier", false);

        if (!shieldActivated)
        {
            shield.GetComponent<Shield>().setMaxHealth((int)(1000 * shieldPercentage));
            shield.SetActive(true);
            shieldActivated = true;
        }

        //PrepareMovingState(meleeDesirability, shootDesirability);
        currState = EnemyState.StayClose;
        if (health < 0)
            currState = EnemyState.Die;

    }

    //private IEnumerator moveCooldown()
    //{
    //    yield return new WaitForSeconds(5f);
    //    shootDesirability = fuzzyController.ShootDesirability(returnDistanceInt(), 55, (int)takenDmg);
    //    meleeDesirability = fuzzyController.MeleeDesirability(returnDistanceInt(), 10, (int)takenDmg);

    //    Debug.Log("I am here!");

    //    StartCoroutine(moveCooldown());

    //}

    private IEnumerator Teleport()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsSpawningBarrier", true);

        yield return new WaitForSeconds(0.6f);

        anim.SetBool("IsSpawningBarrier", false);
        transform.position = new Vector2(player.position.x + 1, player.position.y);
        //PrepareMovingState(meleeDesirability, shootDesirability);
        currState = EnemyState.StayClose;
    }

    private bool IsInAttackRange()
    {
        return returnDistanceIntNormal() + 0.5f < attackRange;
    }

    IEnumerator MeleeAttack()
    {
        rr.flipped = true;
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(0.6f);
        if (!damagedPlayer && IsInAttackRange())
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>().TakeDamage(100f);
            damagedPlayer = true;
        }

        anim.SetBool("IsAttacking", false);
        rr.flipped = false;
        currState = EnemyState.StayClose;
        if (health < 0)
            currState = EnemyState.Die;
    }
    IEnumerator RangeAttack()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsShooting", true);

        yield return new WaitForSeconds(0.6f);

        if (!shoot)
        {
            GameObject[] orbs = new GameObject[5];
            for (int i = 0; i < targets.Length; i++)
            {
                orbs[i] = Instantiate(bullet[0], transform.position, Quaternion.identity);
                orbs[i].GetComponent<EstrellaSaws_v1_1>().SetGameObject(targets[i]);
            }
            shoot = true;
        }
        anim.SetBool("IsShooting", false);
        currState = EnemyState.StayClose;
        if (health < 0)
            currState = EnemyState.Die;
    }

    IEnumerator FlameAttack()
    {
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsShooting", true);
        GameObject flameprojectile = Instantiate(bullet[1], new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
        flameprojectile.GetComponent<EstrellaSaws_v1_1>().SetGameObject(flameTarget);
        yield return new WaitForSeconds(3f);

        anim.SetBool("IsShooting", false);
        currState = EnemyState.Idle;
        if (health < 0)
            currState = EnemyState.Die;
    }
}
