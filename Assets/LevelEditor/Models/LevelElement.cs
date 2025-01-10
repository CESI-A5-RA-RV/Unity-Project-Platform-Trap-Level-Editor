using UnityEngine;

public abstract class LevelElement : MonoBehaviour
{
    public virtual void Initialize(ElementData data)
    {
        transform.position = data.position.ToVector3();
        transform.localScale = data.size.ToVector3();
        transform.rotation = Quaternion.Euler(data.rotation.ToVector3());
    }
}