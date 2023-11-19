using System;
using UnityEngine;

//Singleton
public class Sprites : MonoBehaviour
{
    [Serializable] private class PartTypeSpritePair
    {
        public PartTypeSpritePair(PartType type)
        {
            Type = type;
        }

        public PartType Type;
        public Sprite Sprite;
    }

    [Serializable] private class StringSpritePair
    {
        public StringSpritePair(string String)
        {
            this.String = String;
        }

        public string String;
        public Sprite Sprite;
    }

    [SerializeField] private PartTypeSpritePair[] _partTypeSpritePairs = new PartTypeSpritePair[8] {
        new PartTypeSpritePair(PartType.SourceOn),
        new PartTypeSpritePair(PartType.SourceOff),
        new PartTypeSpritePair(PartType.LEDOff),
        new PartTypeSpritePair(PartType.LEDOn),
        new PartTypeSpritePair(PartType.White),
        new PartTypeSpritePair(PartType.LRed),
        new PartTypeSpritePair(PartType.RRed),
        new PartTypeSpritePair(PartType.Blue)
    };
    
    [SerializeField] private StringSpritePair[] _stringSpritePairs = new StringSpritePair[2] {
        new StringSpritePair("Light"),
        new StringSpritePair("Delete")
    };

    [SerializeField] private Sprite _missingSprite; 

    private static PartTypeSpritePair[] _partTypeSpritePairsStatic;
    private static StringSpritePair[] _stringSpritePairsStatic;
    private static Sprite _missingSpriteStatic;

    private void Awake()
    {
        _partTypeSpritePairsStatic = _partTypeSpritePairs;
        _stringSpritePairsStatic = _stringSpritePairs;
        _missingSpriteStatic = _missingSprite;
    }

    public static Sprite GetSprite(PartType type)
    {
        Sprite toReturn = _missingSpriteStatic;
        foreach (PartTypeSpritePair partTypeSpritePair in _partTypeSpritePairsStatic)
        {
            if (partTypeSpritePair.Type == type)
            {
                if(partTypeSpritePair.Sprite != null) toReturn = partTypeSpritePair.Sprite;
            }
        }
        return toReturn;
    }

    public static Sprite GetSprite(string name)
    {
        Sprite toReturn = _missingSpriteStatic;
        foreach (StringSpritePair stringSpritePair in _stringSpritePairsStatic)
        {
            if(stringSpritePair.String == name)
            {
                if (stringSpritePair.Sprite != null) toReturn = stringSpritePair.Sprite;
            }
        }
        return toReturn;
    }
}
