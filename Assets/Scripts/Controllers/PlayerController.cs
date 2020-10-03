using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        private CharacterController _characterController;
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private float _speed = 1f;

        public void Awake()
        {
            _characterController = FindObjectOfType<CharacterController>();
        }

        public void Update()
        {
            _camera.transform.position = new Vector3(transform.position.x, _camera.transform.position.y, transform.position.z);
        }

        public void FixedUpdate()
        {
            #region Player Movement
            var vertical = Input.GetAxis("Vertical");
            var horizontal = Input.GetAxis("Horizontal");
            Vector3 velocity = Vector3.zero;
            velocity += (transform.forward * vertical); //Move forward
            velocity += (transform.right * horizontal); //Strafe
            velocity *= _speed * Time.fixedDeltaTime; //Framerate and speed adjustment
            velocity = Quaternion.Inverse(transform.rotation) * velocity;
            _characterController.Move(velocity);
            #endregion

            #region Player look direction
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                var lookDir = (hitInfo.point - transform.position).Flatten();

                var playerRotation = Quaternion.LookRotation(lookDir, Vector3.up);

                transform.rotation = playerRotation;
            }
            #endregion
        }
    }
}
