public interface IWeapon : IUseable
{
    public void InitWeapon(HandItemData dataToInit, Hands inHand);
    public void RemoveWeapon();
    public int GetLoadedAmmo();
    public void TryReloadWeapon();
    public void Grab();
    public bool CheckIsReloading();
}
