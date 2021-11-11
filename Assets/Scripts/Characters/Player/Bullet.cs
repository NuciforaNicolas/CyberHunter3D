using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.Player
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed;
        float direction;

        void Update()
        {
            transform.MoveForward(direction ,speed);
        }

        public void SetDirection(float dir)
        {
            direction = dir;
        }

        private void OnBecameInvisible()
        {
            gameObject.SetActive(false);
        }
    }
}
