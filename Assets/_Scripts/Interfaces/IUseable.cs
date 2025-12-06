using System.Collections;

public interface IUseable
{
    public void TryUse();
    public IEnumerator UseCooldown();
}
