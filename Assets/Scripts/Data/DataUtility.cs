using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class FruitElement
    {
        public EFruitType FruitType;
        public Vector3 Position;

        public FruitElement(EFruitType fruitType, Vector3 position)
        {
            FruitType = fruitType;
            Position = position;
        }
    }

    [Serializable]
    public class FruitPrefab
    {
        public EFruitType FruitType;
        public GameObject Prefab;
    }

    public enum StopAction
    {
        None,
        Disable,
        Destroy,
    }

    public enum EFruitType
    {
        None,
        Strawberry,
        ShineMuscat,
        Mandarine,
        Blueberry,
        BlackSapphire
    }
}