public class MovingPlatform : LevelElement
{
    public float Range;
    public float Speed;

    public override void Initialize(ElementData data)
    {
        base.Initialize(data);

        Range = data.GetParameter("Range", 5f); // Default Range is 5 if not provided.
        Speed = data.GetParameter("Speed", 1f); // Default Speed is 1 if not provided.
    }
}
