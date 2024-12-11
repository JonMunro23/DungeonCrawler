using System.Collections;

public interface IUseable
{
    public void TryUse();
    public void TryUseSpecial();
    public IEnumerator UseCooldown();
}
