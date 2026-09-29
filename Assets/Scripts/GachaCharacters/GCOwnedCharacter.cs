using System;

[Serializable]
public class OwnedCharacter
{
    public GCData data;
    public int level = 1;

    public OwnedCharacter(GCData data, int level = 1)
    {
        this.data = data;
        this.level = level;
    }
}