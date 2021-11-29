using Characters.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] protected float healthBarWidthPerUnit;
    [SerializeField] protected float healthBarHeight;
    [SerializeField] Player player;
    [SerializeField] Image playerHealthBar;
    public delegate void UpgradeHealth(float amount);
    public event UpgradeHealth upgradeHealth;

    private void Awake()
    {
        player.onUpgradeHealth += UpgradeHealthBar;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpgradeHealthBar();
    }
    
    public void UpgradeHealthBar()
    {
        var healthBarWidth = playerHealthBar.rectTransform;
        healthBarWidth.sizeDelta = new Vector2(healthBarHeight, player.GetMaxHealth() * healthBarWidthPerUnit);
    }
}
