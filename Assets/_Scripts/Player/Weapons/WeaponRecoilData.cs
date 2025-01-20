using UnityEngine;

[CreateAssetMenu(fileName = "WeaponRecoilData", menuName = "Weapons/New Weapon Recoil Data")]
public class WeaponRecoilData : ScriptableObject
{
    public Vector3 primaryFireMinRecoil;
    public Vector3 primaryFireMaxRecoil;
    [Space]
    public Vector3 secondaryFireMinRecoil;
    public Vector3 secondaryFireMaxRecoil;

    public Vector3 GetRandomPrimaryFireRecoilValue()
    {
        return new Vector3(
            Random.Range(primaryFireMinRecoil.x, primaryFireMaxRecoil.x), 
            Random.Range(primaryFireMinRecoil.y, primaryFireMaxRecoil.y), 
            Random.Range(primaryFireMinRecoil.z, primaryFireMaxRecoil.z)
        );
    }

    public Vector3 GetRandomSecondaryFireRecoilValue()
    {
        return new Vector3(
            Random.Range(secondaryFireMinRecoil.x, secondaryFireMaxRecoil.x),
            Random.Range(secondaryFireMinRecoil.y, secondaryFireMaxRecoil.y),
            Random.Range(secondaryFireMinRecoil.z, secondaryFireMaxRecoil.z)
        );
    }
}
