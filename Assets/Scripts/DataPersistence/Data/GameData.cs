using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdate;
    public float gameTimer;
    public int gameSize;

    public List<bool> positions = new List<bool>();
    public List<int> spriteIDs = new List<int>();
    // the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData() 
    {
        this.gameSize = 0;
        lastUpdate = 0;
        positions = new List<bool>();
        spriteIDs = new List<int>();
    }
    public void SetPosition(int index, bool value)
    {
        positions.Add(value);
    }
    public bool GetPosition(int index)
    {
        return positions[index];
    }
    public void ResetPositions(int size)
    {
        int isOdd = size % 2;
        int temp = (size * size) - isOdd;
        positions.Clear();
        for(int i=0; i<temp; i++)
        {
            positions.Add(false);
        }
    }
    public void SetID(int index, int value)
    {
        spriteIDs.Add(value);
    }
    public int GetID(int index)
    {
        return spriteIDs[index];
    }
}
