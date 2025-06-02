using UnityEngine;

namespace Game.Core
{
    public class NodeController : MonoBehaviour
    {
        public Node Node;
        private ItemController cacheItem;

        public void Init(Node node)
        {
            Node = node;
        }

        public void CreateItem(int id, Sprite spr = null)
        {
            Node.id = id;
            if (id != -1)
            {
                Node.item = GridController.Instance.GetItem(this);
                Node.item.Init(this, spr);
            }
        }

        public void ClearNode()
        {
            Node.id = -1;

            HideItem();
        }

        public void UpdateChangeItem()
        {
            if (Node.id == -1)
            {
                ClearNode();
            }
            else
            {
                if (Node.item != null)
                {
                    if (Node.item != cacheItem)
                    {
                        cacheItem = Node.item;
                        Node.item.node = this;
                        LeanTween.move(Node.item.gameObject, transform.position, 0.2f).setEase(LeanTweenType.easeOutQuad);
                    }
                }
            }
        }

        public void Selected(bool selected = true)
        {
            Node.item.sprBg.enabled = selected;
        }

        public void OnMouseDown()
        {
            if (Node.id == -1) return;

            GridController.Instance.SelectedNode(this);
        }

        public void Hide()
        {
            Node = null;
            HideItem();
        }

        private void HideItem()
        {
            cacheItem = null;
            if (Node.item != null && Node.item.node == this)
            {
                Node.item.Clear();
                GridController.Instance.HideItem(Node.item);
                Node.item = null;
            }
        }
    }
}
