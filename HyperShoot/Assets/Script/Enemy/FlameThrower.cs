using HyperShoot.Combat;
using UniRx;
using UnityEngine;

namespace HyperShoot.Enemy
{
    public class FlameThrower : MonoBehaviour
    {
        [SerializeField] private float damage = 1;
        [SerializeField] private float tickRate = 1;
        [SerializeField] private float range = 15f;
        [SerializeField] private float turnSpeed = 10f;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private ParticleSystem fireParticle;
        [SerializeField] protected Transform pathToRotate;
        [SerializeField] protected EnemyDamageHandler enemyDamageHandler;

        private bool isDead;
        private float fireCountDown = 0f;
        private float _tickRate;
        protected GameObject player;
        protected readonly CompositeDisposable _healthDisposables = new CompositeDisposable();

        // Start is called before the first frame update
        void Start()
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player");
            enemyDamageHandler.HealthObservable
                .Where(hp => hp <= 0f)
                .Subscribe(_ => Die())
                .AddTo(_healthDisposables);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isDead)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= range)
                {
                    Vector3 dir = player.transform.position - transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(dir);
                    Vector3 rotation = Quaternion.Lerp(pathToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
                    pathToRotate.rotation = Quaternion.Euler(rotation);

                    if (fireCountDown <= 0f)
                    {
                        Fire();
                        fireCountDown = 1f / fireRate;
                    }
                    fireCountDown -= Time.deltaTime;
                }
                else
                {
                    AudioManager.Instance.Stop("FlameThrower");
                    fireParticle.Stop();
                }
            }
        }
        private void FixedUpdate()
        {
            if (!isDead)
            {
                _tickRate -= Time.fixedDeltaTime;
            }
        }
        void Fire()
        {
            AudioManager.Instance.Play("FlameThrower");
            fireParticle.Play();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }

        private void OnTriggerEnter(Collider other)
        {
            TakeDamage(other);
        }
        private void OnTriggerStay(Collider other)
        {
            TakeDamage(other);
        }
        public void TakeDamage(Collider other)
        {
            if (_tickRate < 0 && other.CompareTag("Player"))
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    _tickRate = tickRate;
                    damageable.TakeDamage(new DamageData
                    {
                        Type = DamageType.Bullet,
                        Damage = damage,
                        ImpactObject = gameObject
                    });
                }
            }
        }
        public void Die()
        {
            if (!isDead)
            {
                isDead = true;
                Destroy(gameObject, 2f);
            }
        }
    }
}
