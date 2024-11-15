using System;
using System.Collections;

public interface IWeapon : IUseable
{
    public void InitWeapon(HandItemData dataToInit, Hands inHand);
    public IEnumerator HolsterWeapon(Action onHolsteredCallback);
    public IEnumerator DrawWeapon();
    public void RemoveWeapon();
    public int GetLoadedAmmo();
    public void TryReloadWeapon();
    public void Grab();
    public bool IsReloading();
    public bool IsTwoHanded();


}
