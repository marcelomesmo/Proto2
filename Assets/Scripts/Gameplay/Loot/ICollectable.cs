using UnityEngine;

public interface ICollectable
{
    public void Collect(GameObject collector);
    public void FinishCollect();
}