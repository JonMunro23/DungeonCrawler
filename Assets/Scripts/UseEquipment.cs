using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public enum Hands
{
    right,
    left
}

public class UseEquipment : MonoBehaviour
{
    WorldInteraction worldInteraction;
    ItemPickup itemPickup;

    Transform projectileSpawnLocation;
    [SerializeField]
    Transform weaponSpawnParent;

    [SerializeField]
    ItemObject fists;

    [SerializeField]
    ItemObject leftHandItem, rightHandItem;
    [SerializeField]
    GameObject leftHandCooldownImage, rightHandCooldownImage, playerTorchLight;
    [SerializeField]
    RawImage leftHandItemImage, rightHandItemImage;

    bool canUseRightHand = true, canUseLeftHand = true;
    float leftHandItemCooldown, rightHandItemCooldown;
    [SerializeField]
    int bullets, rockets, shells;

    public GameObject currentWeapon;
    private bool canShootBurst = true;

    public bool IsShootingBurst;

    private Coroutine burstCoroutine;
    private bool canShootShot = true;

    private void Awake()
    {
        worldInteraction = GetComponent<WorldInteraction>();
        itemPickup = GetComponent<ItemPickup>();
        projectileSpawnLocation = GameObject.FindGameObjectWithTag("ProjectileSpawnLocation").transform;
    }

    private void Start()
    {
        InitialiseHandItem(Hands.left, fists);
        if (fists.isTwoHanded)
            return;

        InitialiseHandItem(Hands.right, fists);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(worldInteraction.isClickable == false && itemPickup.inventorySlot == null && itemPickup.hasMouseItem == false && DialogueManager.isInDialogue == false)
                UseLeftHand(leftHandItem);
        }
        if (Input.GetMouseButton(1))
        {
            if(worldInteraction.isClickable == false && itemPickup.inventorySlot == null && itemPickup.hasMouseItem == false && DialogueManager.isInDialogue == false)
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

    public void InitialiseHandItem(Hands handUsed, ItemObject item)
    {
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
            else if(item.itemType == ItemObject.ItemType.torch)
            {
                playerTorchLight.SetActive(true);
            }
            leftHandItem = item;
            leftHandItemImage.texture = item.itemTexture;
        }
        else if (handUsed == Hands.right)
        {
            if (item == null)
            {
                rightHandItem = fists;
            }
            else if (item.itemType == ItemObject.ItemType.torch)
            {
                playerTorchLight.SetActive(true);
            }
            rightHandItem = item;
            rightHandItemImage.texture = item.itemTexture;
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
            leftHandItemImage.texture = fists.itemTexture;
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
            rightHandItemImage.texture = fists.itemTexture;
            Destroy(currentWeapon, 2);
            //if (leftHandItem.itemType != ItemObject.ItemType.torch)
            //{
            //    playerTorchLight.SetActive(false);
            //}
        }
    }

    public void UseLeftHand(ItemObject item)
    {
        if (item.itemType == ItemObject.ItemType.meleeWeapon || item.itemType == ItemObject.ItemType.torch)
        {
            if (canUseLeftHand == true)
            {
                //UseMeleeWeapon(item, "left");

                leftHandCooldownImage.SetActive(true);
            }
        }
        else if (item.itemType == ItemObject.ItemType.rangedWeapon)
        {
            if (canUseLeftHand == true)
            {
                UseRangedWeapon(item, Hands.left);
                leftHandCooldownImage.SetActive(true);
            }
        }
    }

    public void UseRightHand(ItemObject item)
    {
        if (item.itemType == ItemObject.ItemType.meleeWeapon || item.itemType == ItemObject.ItemType.torch)
        {
            if (canUseRightHand == true)
            {
                UseMeleeWeapon(item, Hands.right);
                rightHandCooldownImage.SetActive(true);
            }
        }
        else if (item.itemType == ItemObject.ItemType.rangedWeapon)
        {
            if (canUseRightHand == true)
            {
                UseRangedWeapon(item, Hands.right);
                rightHandCooldownImage.SetActive(true);
            }
        }
    }

    public void UseMeleeWeapon(ItemObject weapon, Hands handUsed)
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
            canUseLeftHand = false;
            leftHandItemCooldown = weapon.itemCooldown;
            StartCoroutine(LeftHandItemCooldown());
            StartCoroutine(LeftHandCooldownImageDeactivation());

        }
        else if (handUsed == Hands.right)
        {
            canUseRightHand = false;
            rightHandItemCooldown = weapon.itemCooldown;
            StartCoroutine(RightHandItemCooldown());
            StartCoroutine(RightHandCooldownImageDeactivation());

        }
    }

    public void UseRangedWeapon(ItemObject weapon, Hands handUsed)
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
                    StartCoroutine(LeftHandCooldownImageDeactivation());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    rightHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(RightHandItemCooldown());
                    StartCoroutine(RightHandCooldownImageDeactivation());
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
                    StartCoroutine(LeftHandCooldownImageDeactivation());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    rightHandItemCooldown = weapon.itemCooldown;
                    StartCoroutine(RightHandItemCooldown());
                    StartCoroutine(RightHandCooldownImageDeactivation());
                }
            }
        }
    }

    private void Shoot(ItemObject weapon)
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

    void TryShootBurst(ItemObject weapon)
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

    IEnumerator ShootBurst(ItemObject weapon)
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


    int CalculateDamage(ItemObject weapon)
    {
        int damage = Random.Range(weapon.itemDamageMin, weapon.itemDamageMax + 1);
        return damage;
    }

    bool RollForCrit(ItemObject weapon)
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

    int CheckAmmo(ItemObject.AmmoType ammoType)
    {
        if(ammoType == ItemObject.AmmoType.bullets)
        {
            //check inventory for arrows and return amount
            if(bullets > 0)
            {
                bullets--;
            }
            return bullets;
        }
        else if (ammoType == ItemObject.AmmoType.rockets)
        {
            //check inventory for Bolts and return amount
            if (rockets > 0)
            {
                rockets--;
            }
            return rockets;
        }
        else if (ammoType == ItemObject.AmmoType.shells)
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

    IEnumerator LeftHandCooldownImageDeactivation()
    {
        yield return new WaitForSeconds(leftHandItemCooldown);
        leftHandCooldownImage.SetActive(false);
    }

    IEnumerator RightHandCooldownImageDeactivation()
    {
        yield return new WaitForSeconds(rightHandItemCooldown);
        rightHandCooldownImage.SetActive(false);
    }

    
}
