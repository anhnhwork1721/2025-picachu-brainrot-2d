using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameData", order = 1)]
    public class GameData : ScriptableObject
    {
        public List<GameAsset> AssetData;
    }

    [Serializable]
    public class GameAsset
    {
        public List<Sprite> SpriteData;
    }
}
