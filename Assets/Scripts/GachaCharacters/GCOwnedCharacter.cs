using System;

[Serializable]
public class OwnedCharacter
{
    public GCData data;
    public int level = 1;
    // Copies banked toward the next level (keeps accumulating once at max level)
    public int copies = 0;

    public OwnedCharacter(GCData data, int level = 1, int copies = 0)
    {
        this.data = data;
        this.level = level;
        this.copies = copies;
    }
}
