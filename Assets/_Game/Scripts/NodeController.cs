using UnityEngine;

namespace Game.Core
{
    public class NodeController : MonoBehaviour
    {
        public Node Node;
        public SpriteRenderer sprBg;
        public SpriteRenderer sprItem;

        public void Init(Node node)
        {
            Node = node;

            sprBg.enabled = false;
        }

        public void SetIdNode(int id, Sprite spr = null)
        {
            Node.id = id;
            sprItem.sprite = spr;
            sprItem.enabled = id != -1;
            sprBg.enabled = false;
        }

        public void Selected(bool selected = true)
        {
            sprBg.enabled = selected;
        }

        public void OnMouseDown()
        {
            if (Node.id == -1) return;

            GridController.Instance.SelectedNode(this);
        }

        public void Hide()
        {
            Node = null;
            sprBg.enabled = false;
            sprItem.enabled = false;
        }
    }
}
