using UnityEngine;

namespace Game.Core
{
    public class ItemController : MonoBehaviour
    {
        public SpriteRenderer sprBg;
        public SpriteRenderer sprItem;

        public NodeController node;

        public void Init(NodeController node, Sprite spr)
        {
            this.node = node;
            sprItem.sprite = spr;
            sprBg.enabled = false;
            sprItem.enabled = true;

            gameObject.name = $"Item {node.Node.x} - {node.Node.y}";
            gameObject.SetActive(true);
        }

        public void Clear()
        {
            sprBg.enabled = false;
            sprItem.enabled = false;
            node = null;
        }
    }
}
