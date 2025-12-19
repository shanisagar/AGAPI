using System;

[Serializable]
public class GamePanel
{
    public int gameSizeX = 2;
    public int gameSizeY = 2;
    public float time;
    public _CardFrame[] cards;
    public int cardLeft;
}

[Serializable]
public class _CardFrame
{
    public int spriteID;
    public int id;
    public bool flipped;
    public bool isInactive;
}