using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

namespace Characters.Player
{
    public class GunManager : MonoBehaviour
    {
        public static GunManager instance;

        // Object pooling of bullets
        Pool bulletsPool, chargedBulletsPool;

        // Bullet pool infos
        [SerializeField] string nameBulletPool;
        [SerializeField] int totalBullets;
        [SerializeField] GameObject bulletPrefab;

        // Charged Bullet pool infos
        [SerializeField] string nameChargedBulletPool;
        [SerializeField] int totalChargedBullets;
        [SerializeField] GameObject chargedBulletPrefab;

        // Bullet properties
        [SerializeField] float timeNextShoot;
        [SerializeField] bool canShoot;
        [SerializeField] Transform bulletSpawn;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            bulletsPool = ObjectPoolManager.instance.GenerateObjectPool(nameBulletPool, bulletPrefab, totalBullets);
            chargedBulletsPool = ObjectPoolManager.instance.GenerateObjectPool(nameChargedBulletPool, chargedBulletPrefab, totalChargedBullets);
            canShoot = true;
        }

        public void NormalShoot()
        {
            if(canShoot)
                StartCoroutine(NormalShootCR());
        }

        public void ChargedShoot()
        {
            if (canShoot)
                StartCoroutine(ChargedShootCR());
        }

        IEnumerator NormalShootCR()
        {
            canShoot = false;
            Shoot(bulletsPool);
            yield return new WaitForSeconds(timeNextShoot);
            canShoot = true;
        }

        IEnumerator ChargedShootCR()
        {
            canShoot = false;
            Shoot(chargedBulletsPool);
            yield return new WaitForSeconds(timeNextShoot);
            canShoot = true;
        }

        void Shoot(Pool bulletPool)
        {
            var bullet = ObjectPoolManager.instance.GetObjectFromPool(bulletPool);
            bullet.transform.position = bulletSpawn.position;
            bullet.GetComponent<Bullet>().SetDirection(transform.forward.z);
            bullet.SetActive(true);
        }
    }
}

