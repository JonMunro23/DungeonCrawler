using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Hands
{
    right,
    left
}

public class UseEquipment : MonoBehaviour
{
    WorldInteraction worldInteraction;
    ItemPickupManager itemPickup;

    Transform projectileSpawnLocation;
    [SerializeField]
    Transform weaponSpawnParent;

    [SerializeField]
    ItemData fists;

    [SerializeField]
    ItemData leftHandItem, rightHandItem;
    [SerializeField]
    GameObject playerTorchLight;

    bool canUseRightHand = true, canUseLeftHand = true;
    float leftHandItemCooldown, rightHandItemCooldown;
    [SerializeField]
    int bullets, rockets, shells;

    public GameObject currentWeapon;
    private bool canShootBurst = true;

    public bool IsShootingBurst;

    private Coroutine burstCoroutine;
    private bool canShootShot = true;

    public static event Action<Hands, ItemData> onHandUsed;

    private void Awake()
    {
        worldInteraction = GetComponent<WorldInteraction>();
        itemPickup = GetComponent<ItemPickupManager>();
        projectileSpawnLocation = GameObject.FindGameObjectWithTag("ProjectileSpawnLocation").transform;
    }

    private void OnEnable()
    {
        InventorySlot.onNewHandItem += OnNewHandItem;
        InventorySlot.onHandItemRemoved += OnHandItemRemoved;
    }

    private void OnDisable()
    {
        InventorySlot.onNewHandItem -= OnNewHandItem;
        InventorySlot.onHandItemRemoved -= OnHandItemRemoved;
    }

    private void Start()
    {
        InitialiseHandItem(Hands.right, fists);
        if (fists.isTwoHanded)
            return;

        InitialiseHandItem(Hands.left, fists);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseLeftHand(leftHandItem);
        }
        if (Input.GetMouseButton(1))
        {
            if(worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseRightHand(rightHandItem);
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    private void Reload()
    {
        currentWeapon.GetComponent<Animator>().Play("Reload");
        transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(leftHandItem.reloadSFX);
    }

    public void OnNewHandItem(Hands hand, ItemData newItem)
    {
        InitialiseHandItem(hand, newItem);
    }
    public void OnHandItemRemoved(Hands hand)
    {
        RemoveHandItem(hand);
    }

    public void InitialiseHandItem(Hands handUsed, ItemData item)
    {
        if(currentWeapon)
        {
            Destroy(currentWeapon);
        }

        if (item.itemPrefab)
            currentWeapon = Instantiate(item.itemPrefab, weaponSpawnParent);

        if(currentWeapon)
        {
            currentWeapon.GetComponent<Animator>().Play("Draw");
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(item.drawSFX);
        }

        if (handUsed == Hands.left)
        {
            if(item == null)
            {
                leftHandItem = fists;
            }
            else if(item.itemType == ItemType.torch)
            {
                playerTorchLight.SetActive(true);
            }
            leftHandItem = item;
            //leftHandItemImage.texture = item.itemSprite;
        }
        else if (handUsed == Hands.right)
        {
            if (item == null)
            {
                rightHandItem = fists;
            }
            else if (item.itemType == ItemType.torch)
            {
                playerTorchLight.SetActive(true);
            }
            rightHandItem = item;
            //rightHandItemImage.texture = item.itemSprite;
        }
        //else if (handUsed == "both")
        //{
        //    rightHandItem = item;
        //    leftHandItem = item;
        //    rightHandItemImage.texture = item.itemTexture;
        //    leftHandItemImage.texture = item.itemTexture;
        //}
    }

    public void RemoveHandItem(Hands handUsed)
    {
        if (handUsed == Hands.left)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(leftHandItem.hideSFX);
            currentWeapon.GetComponent<Animator>().Play("Hide");
            leftHandItem = fists;
            //leftHandItemImage.texture = fists.itemSprite;
            Destroy(currentWeapon, 2);
            //if(rightHandItem.itemType != ItemObject.ItemType.torch)
            //{
            //    playerTorchLight.SetActive(false);
            //}
        }
        else if (handUsed == Hands.right)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(rightHandItem.hideSFX);
            currentWeapon.GetComponent<Animator>().Play("Hide");
            rightHandItem = fists;
            //rightHandItemImage.texture = fists.itemSprite;
            Destroy(currentWeapon, 2);
            //if (leftHandItem.itemType != ItemObject.ItemType.torch)
            //{
            //    playerTorchLight.SetActive(false);
            //}
        }
    }

    public void UseLeftHand(ItemData item)
    {
        if (!item)
            return;

        if (item.itemType == ItemType.meleeWeapon || item.itemType == ItemType.torch)
        {
            if (canUseLeftHand == true)
            {
                //UseMeleeWeapon(item, "left");
            }
        }
        else if (item.itemType == ItemType.rangedWeapon)
        {
            if (canUseLeftHand == true)
            {
                UseRangedWeapon(item, Hands.left);
            }
        }
        onHandUsed?.Invoke(Hands.left, item);
    }

    public void UseRightHand(ItemData item)
    {
        if (!item)
            return;

        if (item.itemType == ItemType.meleeWeapon || item.itemType == ItemType.torch)
        {
            if (canUseRightHand == true)
            {
                UseMeleeWeapon(item, Hands.right);
            }
        }
        else if (item.itemType == ItemType.rangedWeapon)
        {
            if (canUseRightHand == true)
            {
                UseRangedWeapon(item, Hands.right);
            }
        }
        onHandUsed?.Invoke(Hands.right, item);
    }

    public void UseMeleeWeapon(ItemData weapon, Hands handUsed)
    {
        //if (playerMovement.CheckForward("Enemy"))
        //{
        //    Enemy enemy = playerMovement.GetForwardObject().GetComponent<Enemy>();
        //    int damage = CalculateDamage(weapon);
        //    enemy.TakeDamage(damage);
        //}
        //else if(playerMovement.CheckForward("BreakableWall"))
        //{
        //    BreakableWall wall = playerMovement.GetForwardObject().GetComponent<BreakableWall>();
        //    wall.Break();
        //}


        if (handUsed == Hands.left)
        {
            currentWeapon.GetComponent<Animator>().Play("Swing Left 1");
            canUseLeftHand = false;
            leftHandItemCooldown = weapon.itemCooldown;
            StartCoroutine(LeftHandItemCooldown());
            //StartCoroutine(LeftHandCooldownImageDeactivation());

        }
        else if (handUsed == Hands.right)
        {
            currentWeapon.GetComponent<Animator>().Play("Swing Right 1");
            canUseRightHand = false;
            rightHandItemCooldown = weapon.itemCooldown;
            StartCoroutine(RightHandItemCooldown());
            //StartCoroutine(RightHandCooldownImageDeactivation());

        }
    }

    public void UseRangedWeapon(ItemData weapon, Hands handUsed)
    {
        if (CheckAmmo(weapon.ammoType) != 0)
        {
            if (weapon.usesProjectiles)
            {
                GameObject projectile = Instantiate(weapon.itemProjectile.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
                projectile.GetComponentInChildren<Projectile>().projectile = weapon.itemProjectile;
                projectile.GetComponentInChildren<Projectile>().damage = CalculateDamage(weapon);
                if (handUsed == Hands.left)
                {
                    canUseLeftHand = false;
                    leftHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(LeftHandItemCooldown());
                    //StartCoroutine(LeftHandCooldownImageDeactivation());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    rightHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(RightHandItemCooldown());
                    //StartCoroutine(RightHandCooldownImageDeactivation());
                }
            }
            else
            {
                if(weapon.isBurst)
                {
                    TryShootBurst(weapon);
                }
                else
                    Shoot(weapon);

                if (handUsed == Hands.left)
                {
                    canUseLeftHand = false;
                    leftHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(LeftHandItemCooldown());
                    //StartCoroutine(LeftHandCooldownImageDeactivation());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    rightHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(RightHandItemCooldown());
                    //StartCoroutine(RightHandCooldownImageDeactivation());
                }
            }
        }
    }

    private void Shoot(ItemData weapon)
    {
        currentWeapon.GetComponent<Animator>().Play("Fire");
        transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(weapon.fireSFX[Random.Range(0, weapon.fireSFX.Length)]);

        RaycastHit hit;
        for (int i = 0; i < weapon.projectileAmount; i++)
        {
            if (Physics.Raycast(weaponSpawnParent.position, weaponSpawnParent.forward, out hit, weapon.itemRange * 3))
            {

                IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = CalculateDamage(weapon);
                    bool isCrit = RollForCrit(weapon);
                    if(isCrit)
                        damage *= Mathf.CeilToInt(weapon.critDamageMultiplier);

                    damageable.TakeDamage(damage, isCrit);    
                }
            }
        }
    }

    void TryShootBurst(ItemData weapon)
    {
        if (canShootBurst)
        {
            canShootBurst = false;
            burstCoroutine = StartCoroutine(ShootBurst(weapon));
        }
    }

    public void StopBurst()
    {
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            canShootBurst = true;
            IsShootingBurst = false;
        }
    }

    IEnumerator ShootBurst(ItemData weapon)
    {
        IsShootingBurst = true;

        for (int i = 0; i < weapon.burstLength; i++)
        {
            
            if (canShootShot)
            {
                canShootShot = false;
                Shoot(weapon);
                yield return new WaitForSeconds(weapon.perShotInBurstDelay);
                canShootShot = true;
            }
        }

        IsShootingBurst = false;
        canShootBurst = true;
    }


    int CalculateDamage(ItemData weapon)
    {
        int damage = Random.Range(weapon.itemDamageMin, weapon.itemDamageMax + 1);
        return damage;
    }

    bool RollForCrit(ItemData weapon)
    {
        
        bool wasCrit = false;
        if (weapon.critChance > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= weapon.critChance)
            {
                wasCrit = true;
            }
        }
        return wasCrit;
    }

    int CheckAmmo(AmmoType ammoType)
    {
        if(ammoType == AmmoType.bullets)
        {
            //check inventory for arrows and return amount
            if(bullets > 0)
            {
                bullets--;
            }
            return bullets;
        }
        else if (ammoType == AmmoType.rockets)
        {
            //check inventory for Bolts and return amount
            if (rockets > 0)
            {
                rockets--;
            }
            return rockets;
        }
        else if (ammoType == AmmoType.shells)
        {
            //check inventory for MusketBalls and return amount
            if (shells > 0)
            {
                shells--;
            }
            return shells;
        }
        return 0;
    }

    IEnumerator LeftHandItemCooldown()
    {
        for (int i = 0; i < leftHandItemCooldown; i++)
        {
            //Debug.Log("Current left hand cooldown: " + i);
            yield return new WaitForSeconds(1);
        }
        //Debug.Log("Cooldown left hand ended");
        canUseLeftHand = true;
    }

    IEnumerator RightHandItemCooldown()
    {
        for (int i = 0; i < rightHandItemCooldown; i++)
        {
            //Debug.Log("Current right hand cooldown: " + i);
            yield return new WaitForSeconds(1);
        }
        //Debug.Log("Cooldown right hand ended");
        canUseRightHand = true;
    }

    //IEnumerator LeftHandCooldownImageDeactivation()
    //{
    //    yield return new WaitForSeconds(leftHandItemCooldown);
    //    leftHandCooldownImage.SetActive(false);
    //}

    //IEnumerator RightHandCooldownImageDeactivation()
    //{
    //    yield return new WaitForSeconds(rightHandItemCooldown);
    //    rightHandCooldownImage.SetActive(false);
    //}

    
}
