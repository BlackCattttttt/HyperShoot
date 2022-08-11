using HyperShoot.Combat;
using Pathfinding;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

namespace HyperShoot.Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        [SerializeField] protected bool isNav;
        [SerializeField] protected LayerMask layerMask;
        [HideIf("isNav")] [SerializeField] protected AIPath aIPath;
        [ShowIf("isNav")] [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected Animator anim;
        [SerializeField] protected EnemyDamageHandler enemyDamageHandler;
        [SerializeField] protected float walkPointRange;
        [SerializeField] protected float followDistance = 20.0f;
        [SerializeField] [Range(45,90)] protected float fov = 90f;
        [SerializeField] protected float attackDistance = 12f;
        [SerializeField] protected float delayAttack;
        [SerializeField] protected float damage;

        protected GameObject player;
        protected Vector3 walkPoint;
        protected bool walkPointSet;
        protected bool playerInSightRange, playerInAttackRange;
        protected bool canAttack;
        protected float _delayAttack;
        protected bool isDead;
        protected AudioSource m_Audio = null;
        protected readonly CompositeDisposable _healthDisposables = new CompositeDisposable();

        protected virtual void Awake()
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            isDead = false;
            _delayAttack = delayAttack;
            m_Audio = GetComponent<AudioSource>();
        }
        protected virtual void Start()
        {
            enemyDamageHandler.HealthObservable
              .Where(hp => hp <= 0f)
              .Subscribe(_ => Die())
              .AddTo(_healthDisposables);
        }
        protected virtual void Update()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            Vector3 dirToPlayer = player.transform.position - transform.position;

            if (!Physics.Raycast(transform.position, dirToPlayer, distance, layerMask))
            {
                float angleToPlayer = (Vector3.Angle(dirToPlayer, transform.forward));

                if (angleToPlayer >= -fov && angleToPlayer <= fov) // 180° FOV
                {
                    playerInSightRange = (distance < followDistance) ? true : false;
                    playerInAttackRange = (distance < attackDistance) ? true : false;
                }
                else
                {
                    playerInSightRange = false;
                    playerInAttackRange = false;
                }
            }
            else
            {
                playerInSightRange = false;
                playerInAttackRange = false;
            }
            

            if (!isDead)
            {
                if (!playerInSightRange && !playerInAttackRange) Patrolling();
                if (playerInSightRange && !playerInAttackRange) ChasePlayer();
                if (playerInSightRange && playerInAttackRange) AttackPlayer();
                if (distance > 100f)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (isNav)
                {
                    agent.SetDestination(transform.position);
                }
                else
                {
                    aIPath.destination = transform.position;
                }
            }
        }
        protected virtual void FixedUpdate()
        {
            if (canAttack)
            {
                _delayAttack -= Time.fixedDeltaTime;
                if (_delayAttack < 0)
                {
                    Attack();
                    _delayAttack = delayAttack;
                }
            }
        }
        public virtual void Patrolling()
        {
            if (canAttack) canAttack = false;
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
            {
                if (isNav)
                {
                    agent.SetDestination(walkPoint);
                }
                else
                {
                    aIPath.destination = walkPoint;
                }
                anim.SetBool("walk", true);
                anim.SetBool("run", false);
            }

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f)
            {
                walkPointSet = false;
            }
        }

        public void SearchWalkPoint()
        {
            float randomZ = Random.Range(-walkPointRange, walkPointRange);
            float randomX = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (Physics.Raycast(walkPoint, -transform.up, 2f)) walkPointSet = true;
        }
        public virtual void ChasePlayer()
        {
            if (canAttack) canAttack = false;
            if (isNav)
            {
                agent.SetDestination(player.transform.position);
            }
            else
            {
                aIPath.destination = player.transform.position;
            }
            transform.LookAt(player.transform.position);
            anim.SetBool("run", true);
            anim.SetBool("walk", false);
        }

        public virtual void AttackPlayer()
        {
            if (isNav)
            {
                agent.SetDestination(transform.position);
            }
            else
            {
                aIPath.destination = transform.position;
            }
            transform.LookAt(player.transform.position);
            anim.SetBool("run", false);
            anim.SetBool("walk", false);
            if (!canAttack)
            {
                canAttack = true;
                _delayAttack = delayAttack;
                Attack();
            }
        }
        public virtual void Attack()
        {
            anim.SetTrigger("attack");
        }
        public virtual void Die()
        {
            if (!isDead)
            {
                isDead = true;
                anim.SetTrigger("dead");
                Destroy(gameObject, 2f);
            }
        }
        private void OnDestroy()
        {
            _healthDisposables.Dispose();
        }
    }
}