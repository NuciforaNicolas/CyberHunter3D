using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed;
        [SerializeField] ParticleSystem particles;
        ParticleSystem.VelocityOverLifetimeModule particlesVelocity;
        float direction;

        private void Awake()
        {
            particlesVelocity = particles.velocityOverLifetime;
        }

        void Update()
        {
            transform.MoveForward(direction ,speed);
        }

        public void SetDirection(float dir)
        {
            direction = (dir >= 0 ? 1 : -1);
            particlesVelocity.z = -dir * 2;
        }

        private void OnBecameInvisible()
        {
            gameObject.SetActive(false);
        }
    }
}
