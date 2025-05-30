using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    [Serializable]
    public class Node
    {
        public int x;
        public int y;
        public Vector3 position;
        public int id;
        public Node top; 
        public Node bottom;
        public Node left;
        public Node right;
        public Node topLeft;
        public Node bottomLeft;
        public Node topRight;
        public Node bottomRight;
    }
}
