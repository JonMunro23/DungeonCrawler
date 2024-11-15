using System.Collections;

public interface IUseable
{
    public void Use();
    public void UseSpecial();
    public IEnumerator UseCooldown();
}
