using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class Fruit
    {
        public EFruitType FruitType;
        public Vector3 Position;

        public Fruit(EFruitType fruitType, Vector3 position)
        {
            FruitType = fruitType;
            Position = position;
        }
    }
}
