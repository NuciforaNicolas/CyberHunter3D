using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters.Player;
using UnityEngine.UI;
public class DebugManager : MonoBehaviour
{
    [SerializeField]
    Player player;
    [SerializeField]
    float hitAmount, healAmount, healthUpgradeAmount;
    PlayerHealthBar playerHealthBar;

    public void Hit()
    {
        player.Hit(hitAmount);
    }

    public void Heal()
    {
        player.Heal(healAmount);
    }

    public void HealthUpgrade()
    {
        player.UpgradeHealth(healthUpgradeAmount);
    }
}
