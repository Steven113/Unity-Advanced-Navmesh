using UnityEngine;
using System.Collections;
using Assets.Scripts.Controllers;
using Assets.Scripts.Audio;

namespace Assets.Scripts.Weapons
{
    public class ProjectileScript : MonoBehaviour
    {

        // Use this for initialization
        [SerializeField]
        private Vector3 _direction; //unit vector of direction that projectile is going
        [SerializeField]
        private float _speed;
        [SerializeField]
        private Ray _intersectionRay;
        [SerializeField]
        private RaycastHit _raycastHit;
        [SerializeField]
        private Vector3 _position; // position in world coordinates
        [SerializeField]
        private float _distanceCovered = 0;
        [SerializeField]
        private float _maxDistanceToCover = 1000000;
        [SerializeField]
        private float _damage = 1;
        [SerializeField]
        private GameObject _gameObjectToSpawnOnImpact;
        [SerializeField]
        private AudioClip _audioClipToPlayOnSpawn;
        [SerializeField]
        private AudioClip[] _audioClipToPlayOnDeath;
        [SerializeField]
        private float _airStabilisation = 0.25f; //a number between zero and 1, indicating how much the projectile is affected by gravity. Smaller z means the projectile falls more slowly
        [SerializeField]
        private float _timeSinceLastUpdate = 1000f;
        [SerializeField]
        private float _projectileUpdateInterval = 0.05f;
        [SerializeField]
        private LayerMask _rayCastMask;
        [SerializeField]
        [Range(0f, 1f)]
        private float impactVolume = 1f;
        [SerializeField]
        [Range(0f, 1f)]
        private float gunshotVolume = 1f;

        public void Init(Vector3 direction)
        {
            _direction = direction;
            transform.forward = direction;
        }

        void Start()
        {
            //position = gameObject.transform.TransformPoint(gameObject.transform.position);
            _position = gameObject.transform.position;
            _intersectionRay = new Ray(_position, _direction);
            _raycastHit = new RaycastHit();
            if (_audioClipToPlayOnSpawn != null)
            {
                AudioSource.PlayClipAtPoint(_audioClipToPlayOnSpawn, gameObject.transform.position, gunshotVolume);
            }
        }

        // Update is called once per frame
        void Update()
        {
            _timeSinceLastUpdate += Time.deltaTime;
            _direction = ((gameObject.transform.position + _speed * _direction * Time.deltaTime + Physics.gravity * _airStabilisation * Time.deltaTime * Time.deltaTime) - gameObject.transform.position).normalized;
            _intersectionRay.origin = gameObject.transform.position;
            _intersectionRay.direction = _direction;
            if (_timeSinceLastUpdate > _projectileUpdateInterval)
            {
                if (Physics.Raycast(_intersectionRay, out _raycastHit, _speed * Time.deltaTime, _rayCastMask))
                {
                    if (_raycastHit.collider.gameObject.TryGetComponent<HealthController>(out HealthController healthController))
                    {
                        healthController.Damage(_damage);
                    }

                    //timeSinceLastUpdate = 0f;
                    //print ("The projectile hit!" + raycastHit.collider.gameObject.name);
                    if (_gameObjectToSpawnOnImpact != null)
                    {
                        GameObject.Instantiate(_gameObjectToSpawnOnImpact, gameObject.transform.position, Quaternion.identity);
                    }

                    if (_raycastHit.collider.gameObject.TryGetComponent<ImpactAudioProvider>(out ImpactAudioProvider impactAudioProvider))
                    {
                        impactAudioProvider.PlayImpactAudioEffect();
                    }
                    else
                    {
                        if (_audioClipToPlayOnDeath != null && _audioClipToPlayOnDeath.Length > 0)
                        {
                            AudioSource.PlayClipAtPoint(_audioClipToPlayOnDeath[(int)UnityEngine.Random.Range(0, _audioClipToPlayOnDeath.Length - 1)], gameObject.transform.position, impactVolume);
                        }
                    }

                    GameObject.Destroy(gameObject);
                }
                else
                {
                    _position = _position + _direction * _speed * Time.deltaTime;
                    gameObject.transform.position = gameObject.transform.position + _direction * _speed * Time.deltaTime;
                    _distanceCovered += _speed * Time.deltaTime;
                    _raycastHit.point = _position;
                }
            }
            else
            {
                _position = _position + _direction * _speed * Time.deltaTime;
                gameObject.transform.position = gameObject.transform.position + _direction * _speed * Time.deltaTime;
                _distanceCovered += _speed * Time.deltaTime;
                _raycastHit.point = _position;
            }

            if (_distanceCovered > _maxDistanceToCover)
            {
                GameObject.Destroy(gameObject);
            }

        }
    }
}
