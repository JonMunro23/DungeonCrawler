using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Hands
{
    right,
    left,
    both
}

public class UseEquipment : MonoBehaviour
{
    WorldInteraction worldInteraction;
    ItemPickupManager itemPickup;

    Transform projectileSpawnLocation;
    [SerializeField]
    Transform weaponSpawnParent;

    [SerializeField]
    HandItemData fists;

    [SerializeField]
    HandItemData leftHandItem, rightHandItem;
    [SerializeField]
    GameObject playerTorchLight;

    [SerializeField] bool canUseRightHand = true, canUseLeftHand = true, isInventoryOpen = false;
    float leftHandItemCooldown, rightHandItemCooldown;
    [SerializeField]
    int bullets, rockets, shells;

    public GameObject currentWeapon;
    private bool canShootBurst = true;

    public bool IsShootingBurst;

    private Coroutine burstCoroutine;
    private bool canShootShot = true;

    public static event Action<Hands, HandItemData> onHandUsed;

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
        PlayerInventory.onInventoryOpened += OnInventoryOpened;
        PlayerInventory.onInventoryClosed += OnInventoryClosed;
    }

    private void OnDisable()
    {
        InventorySlot.onNewHandItem -= OnNewHandItem;
        InventorySlot.onHandItemRemoved -= OnHandItemRemoved;
        PlayerInventory.onInventoryOpened -= OnInventoryOpened;
        PlayerInventory.onInventoryClosed -= OnInventoryClosed;
    }

    private void Start()
    {
        InitialiseHandItem(Hands.both, fists);
        //if (fists.isTwoHanded)
        //    return;

        //InitialiseHandItem(Hands.left, fists);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            if(!isInventoryOpen && worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseLeftHand(leftHandItem);
        }
        if (Input.GetMouseButton(1))
        {
            if(!isInventoryOpen && worldInteraction.isClickable == false && itemPickup.hasGrabbedItem == false && DialogueManager.isInDialogue == false)
                UseRightHand(rightHandItem);
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    void OnInventoryOpened()
    {
        isInventoryOpen = true;
    }

    void OnInventoryClosed()
    {
        isInventoryOpen = false;
    }

    private void Reload()
    {
        currentWeapon.GetComponent<Animator>().Play("Reload");
        //transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(leftHandItem.reloadSFX);
    }

    public void OnNewHandItem(Hands hand, HandItemData newItem)
    {
        InitialiseHandItem(hand, newItem);
    }
    public void OnHandItemRemoved(Hands hand)
    {
        RemoveHandItem(hand);
    }

    public void InitialiseHandItem(Hands handUsed, HandItemData handItemData)
    {
        if(currentWeapon)
        {
            Destroy(currentWeapon);
        }

        if (handItemData.itemPrefab)
            currentWeapon = Instantiate(handItemData.itemPrefab, weaponSpawnParent);

        if(currentWeapon)
        {
            currentWeapon.GetComponent<Animator>().Play("Draw");
            //transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(handItemData.drawSFX);
        }

        if (handUsed == Hands.left)
        {
            if(handItemData == null)
            {
                leftHandItem = fists;
            }
            leftHandItem = handItemData;
        }
        else if (handUsed == Hands.right)
        {
            if (handItemData == null)
            {
                rightHandItem = fists;
            }
            rightHandItem = handItemData;
        }
        else if (handUsed == Hands.both)
        {
            rightHandItem = handItemData;
            leftHandItem = handItemData;
        }
    }

    public void RemoveHandItem(Hands handUsed)
    {
        if (handUsed == Hands.left)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(leftHandItem.hideSFX);
            //currentWeapon.GetComponent<Animator>().Play("Hide");
            leftHandItem = fists;
            //leftHandItemImage.texture = fists.itemSprite;
            Destroy(currentWeapon);
            //if(rightHandItem.itemType != ItemObject.ItemType.torch)
            //{
            //    playerTorchLight.SetActive(false);
            //}
        }
        else if (handUsed == Hands.right)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(rightHandItem.hideSFX);
            //currentWeapon.GetComponent<Animator>().Play("Hide");
            rightHandItem = fists;
            //rightHandItemImage.texture = fists.itemSprite;
            Destroy(currentWeapon);
            //if (leftHandItem.itemType != ItemObject.ItemType.torch)
            //{
            //    playerTorchLight.SetActive(false);
            //}
        }
    }

    public void UseLeftHand(HandItemData handItemData)
    {
        if (!handItemData || !canUseLeftHand)
            return;


        if (handItemData.isMeleeWeapon)
        {
            if (canUseLeftHand == true)
            {
                UseMeleeWeapon(handItemData, Hands.left);
            }
        }
        else
        {
            if (canUseLeftHand == true)
            {
                if (handItemData.isTwoHanded)
                {
                    UseRangedWeapon(handItemData, Hands.both);
                    onHandUsed?.Invoke(Hands.both, handItemData);
                }
                else
                {
                    onHandUsed?.Invoke(Hands.left, handItemData);
                    UseRangedWeapon(handItemData, Hands.left);
                }
            }
        }

        

    }

    public void UseRightHand(HandItemData handItemData)
    {
        if (!handItemData || !canUseRightHand)
            return;

        if (handItemData.isMeleeWeapon)
        {
            if (canUseRightHand == true)
            {
                UseMeleeWeapon(handItemData, Hands.right);
            }
        }
        else
        {
            if (handItemData.isTwoHanded)
            {
                UseRangedWeapon(handItemData, Hands.both);
                onHandUsed?.Invoke(Hands.both, handItemData);
            }
            else
            {
                onHandUsed?.Invoke(Hands.right, handItemData);
                UseRangedWeapon(handItemData, Hands.right);
            }
        }
    }

    public void UseMeleeWeapon(HandItemData handItem, Hands handUsed)
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
            StartCoroutine(LeftHandItemCooldown());
            //StartCoroutine(LeftHandCooldownImageDeactivation());

        }
        else if (handUsed == Hands.right)
        {
            currentWeapon.GetComponent<Animator>().Play("Swing Right 1");
            canUseRightHand = false;
            StartCoroutine(RightHandItemCooldown());
            //StartCoroutine(RightHandCooldownImageDeactivation());

        }
    }

    public void UseRangedWeapon(HandItemData handItemData, Hands handUsed)
    {
        if (CheckAmmo(handItemData.ammoType) != 0)
        {
            if (handItemData.isProjectile)
            {
                GameObject projectile = Instantiate(handItemData.projectileData.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
                //projectile.GetComponentInChildren<Projectile>().projectile = handItemData.itemProjectile;
                projectile.GetComponentInChildren<Projectile>().damage = CalculateDamage(handItemData);
                if(handUsed == Hands.both)
                {
                    canUseLeftHand = false;
                    canUseRightHand = false;
                    leftHandItemCooldown = handItemData.itemCooldown;
                    rightHandItemCooldown = handItemData.itemCooldown;
                    StartCoroutine(LeftHandItemCooldown());
                    StartCoroutine(RightHandItemCooldown());
                }
                else if (handUsed == Hands.left)
                {
                    canUseLeftHand = false;
                    leftHandItemCooldown = handItemData.itemCooldown;
                    StartCoroutine(LeftHandItemCooldown());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    rightHandItemCooldown = handItemData.itemCooldown;
                    StartCoroutine(RightHandItemCooldown());
                }
            }
            else
            {
                if(handItemData.isBurst)
                {
                    TryShootBurst(handItemData);
                }
                else
                    Shoot(handItemData);

                if (handUsed == Hands.both)
                {
                    canUseLeftHand = false;
                    canUseRightHand = false;
                    leftHandItemCooldown = handItemData.itemCooldown;
                    rightHandItemCooldown = handItemData.itemCooldown;
                    StartCoroutine(LeftHandItemCooldown());
                    StartCoroutine(RightHandItemCooldown());
                }
                else if (handUsed == Hands.left)
                {
                    canUseLeftHand = false;
                    StartCoroutine(LeftHandItemCooldown());
                }
                else if (handUsed == Hands.right)
                {
                    canUseRightHand = false;
                    StartCoroutine(RightHandItemCooldown());
                }
            }
        }
    }

    private void Shoot(HandItemData handItemData)
    {
        currentWeapon.GetComponent<Animator>().Play("Fire");
        transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(handItemData.attackSFX[Random.Range(0, handItemData.attackSFX.Length)]);

        RaycastHit hit;
        for (int i = 0; i < handItemData.projectileCount; i++)
        {
            if (Physics.Raycast(weaponSpawnParent.position, weaponSpawnParent.forward, out hit, handItemData.itemRange * 3))
            {

                IDamageable damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    int damage = CalculateDamage(handItemData);
                    bool isCrit = RollForCrit(handItemData);
                    if(isCrit)
                        damage *= Mathf.CeilToInt(handItemData.critDamageMultiplier);

                    damageable.TakeDamage(damage, isCrit);    
                }
            }
        }
    }

    void TryShootBurst(HandItemData handItemData)
    {
        if (canShootBurst)
        {
            canShootBurst = false;
            burstCoroutine = StartCoroutine(ShootBurst(handItemData));
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

    IEnumerator ShootBurst(HandItemData handItemData)
    {
        IsShootingBurst = true;

        for (int i = 0; i < handItemData.burstLength; i++)
        {
            
            if (canShootShot)
            {
                canShootShot = false;
                Shoot(handItemData);
                yield return new WaitForSeconds(handItemData.perShotInBurstDelay);
                canShootShot = true;
            }
        }

        IsShootingBurst = false;
        canShootBurst = true;
    }


    int CalculateDamage(HandItemData handItemData)
    {
        float damage = Random.Range(handItemData.itemDamageMinMax.x, handItemData.itemDamageMinMax.y);
        return Mathf.CeilToInt(damage);
    }

    bool RollForCrit(HandItemData handItemData)
    {
        bool wasCrit = false;
        if (handItemData.critChance > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= handItemData.critChance)
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
        yield return new WaitForSeconds(leftHandItem.itemCooldown);
        canUseLeftHand = true;
    }

    IEnumerator RightHandItemCooldown()
    {
        yield return new WaitForSeconds(rightHandItem.itemCooldown);
        canUseRightHand = true;
    }    
}
