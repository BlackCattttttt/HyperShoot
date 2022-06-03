using UnityEngine;

public class BulletContainer : Singleton<BulletContainer>
{
    public void ClearAll()
    {
        foreach (Transform child in transform)
        {
            SimplePool.Despawn(child.gameObject);
        }
    }
}
