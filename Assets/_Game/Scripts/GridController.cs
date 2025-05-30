using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Core
{
    public enum GridType
    {
        Normal,
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        HaftVertical,
        HaftHorizontal,
        VerticalHaft,
        HorizontalHaft
    }

    public enum GameMode
    {
        Classic,
        Online
    }

    public class GridController : MonoBehaviour
    {
        private static GridController instance;
        public static GridController Instance => instance;

        public GameData _data;
        public GridType GridType;
        public GameMode GameMode;

        public int width = 18;
        public int height = 11;
        public Vector2 offset = new Vector2(0.2f, 0.4f);
        public GameObject nodePrefab;
        public List<NodeController> nodePool = new List<NodeController>();
        public List<NodeController> nodes;
        public Transform content;
        public Transform poolParent;
        public LineRenderer lineRenderer;

        private NodeController nodeSelected;
        public bool isMatching;

        void Awake()
        {
            if (instance != null) Destroy(gameObject);
            instance = this;
        }

        #region Spawn

        [ContextMenu("Test create")]
        public void CreateGid()
        {
            Vector3 center = content.transform.position;
            float xStart = center.x - (offset.x * width / 2) + (offset.x / 2);
            float yStart = center.y + (offset.y * height / 2) - (offset.y / 2);

            ClearData();

            for (int i = 0; i < (width); i++)
            {
                float posX = xStart + (i * offset.x);
                for (int j = 0; j < (height); j++)
                {
                    float posY = yStart - (j * offset.y);

                    var node = GetNode(i, j);
                    nodes.Add(node);
                    node.transform.position = new Vector2(posX, posY);

                    Node nodeData = new Node()
                    {
                        x = i,
                        y = j,
                        position = node.transform.position
                    };

                    node.Init(nodeData);
                }
            }

            FindNeighborNode();
            SetIdNode();
        }

        private NodeController GetNode(int i, int j)
        {
            NodeController node = nodePool.FirstOrDefault(a => !a.gameObject.activeSelf);

            if (node == null)
            {
                var obj = Instantiate(nodePrefab, poolParent);
                obj.SetActive(false);
                node = obj.GetComponent<NodeController>();
                nodePool.Add(node);
            }

            node.transform.SetParent(content);
            node.transform.localScale = Vector3.one;
            node.gameObject.SetActive(true);
            node.name = $"Node {i}-{j}";

            return node;
        }

        private void FindNeighborNode()
        {
            foreach (var node in nodes)
            {
                node.Node.top = GetNodeByXY(node.Node.x, node.Node.y + 1);
                node.Node.bottom = GetNodeByXY(node.Node.x, node.Node.y - 1);
                node.Node.left = GetNodeByXY(node.Node.x - 1, node.Node.y);
                node.Node.right = GetNodeByXY(node.Node.x + 1, node.Node.y);
                node.Node.topLeft = GetNodeByXY(node.Node.x - 1, node.Node.y + 1);
                node.Node.topRight = GetNodeByXY(node.Node.x + 1, node.Node.y + 1);
                node.Node.bottomRight = GetNodeByXY(node.Node.x + 1, node.Node.y - 1);
                node.Node.bottomLeft = GetNodeByXY(node.Node.x - 1, node.Node.y - 1);
            }
        }

        private Node GetNodeByXY(int x, int y)
        {
            foreach (var node in nodes)
            {
                if (node.Node.x == x && node.Node.y == y)
                    return node.Node;
            }

            return null;
        }

        private void SetIdNode()
        {
            int totalID = (width - 2) * (height - 2) / 2;
            List<int> ids = new List<int>();
            for (int i = 0; i < totalID; i++)
            {
                ids.Add(Random.Range(0, _data.AssetData[0].SpriteData.Count));
            }
            ids.AddRange(ids);

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node.Node.x == 0 || node.Node.x == (width - 1) || node.Node.y == 0 || node.Node.y == (height - 1))
                {
                    node.SetIdNode(-1);
                    continue;
                }

                int index = Random.Range(0, ids.Count);
                node.SetIdNode(ids[index], _data.AssetData[0].SpriteData[ids[index]]);

                ids.RemoveAt(index);
            }
        }

        public void ClearData()
        {
            foreach (var item in nodePool)
            {
                item.Hide();
                item.transform.SetParent(poolParent);
            }

            nodes = new List<NodeController>();
        }

        #endregion

        #region Path Node

        public void SelectedNode(NodeController nodeSelected)
        {
            if (isMatching) return;

            if (this.nodeSelected == null)
            {
                this.nodeSelected = nodeSelected;
                nodeSelected.Selected();
                return;
            }

            if (this.nodeSelected == nodeSelected)
            {
                nodeSelected.Selected(false);
                this.nodeSelected = null;
                return;
            }

            var path = CanMatchWithPath(this.nodeSelected.Node, nodeSelected.Node);
            if (path == null)
            {
                this.nodeSelected.Selected(false);
                this.nodeSelected = null;
            }
            else
            {
                nodeSelected.Selected();
                isMatching = true;

                List<Vector3> positions = path.Select(n => n.position).ToList();
                lineRenderer.positionCount = positions.Count;
                lineRenderer.SetPositions(positions.ToArray());

                StartCoroutine(ClearNodeDone(this.nodeSelected, nodeSelected));
            }
        }

        private IEnumerator ClearNodeDone(NodeController nodeA, NodeController nodeB)
        {
            yield return new WaitForSeconds(0.35f);
            nodeA.SetIdNode(-1);
            nodeB.SetIdNode(-1);

            lineRenderer.positionCount = 0;
            nodeSelected = null;

            SlideAllNodes(nodes);
            yield return new WaitForSeconds(0.5f);
            isMatching = false;
        }

        public static List<Node> CanMatchWithPath(Node start, Node end)
        {
            if (start == null || end == null || start == end || start.id != end.id)
                return null;

            Queue<PathNode> queue = new Queue<PathNode>();
            HashSet<Node> visited = new HashSet<Node>();

            Vector2Int[] directions = {
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,
                Vector2Int.left
            };

            foreach (var dir in directions)
            {
                Node neighbor = GetNeighbor(start, dir);
                if (neighbor != null && (neighbor == end || neighbor.id == -1))
                {
                    var path = new List<Node> { start };
                    queue.Enqueue(new PathNode(neighbor, dir, 0, path));
                }
            }

            while (queue.Count > 0)
            {
                PathNode current = queue.Dequeue();
                if (current.turns > 2) continue;

                if (current.node == end)
                {
                    return current.path;
                }

                visited.Add(current.node);

                foreach (var dir in directions)
                {
                    Node neighbor = GetNeighbor(current.node, dir);
                    if (neighbor == null || visited.Contains(neighbor)) continue;

                    if (neighbor != end && neighbor.id != -1) continue;

                    int extraTurn = (dir != current.direction) ? 1 : 0;
                    int newTurns = current.turns + extraTurn;

                    if (newTurns <= 2)
                    {
                        queue.Enqueue(new PathNode(neighbor, dir, newTurns, current.path));
                    }
                }
            }

            return null;
        }

        private static Node GetNeighbor(Node node, Vector2Int dir)
        {
            if (dir == Vector2Int.up) return node.top;
            if (dir == Vector2Int.right) return node.right;
            if (dir == Vector2Int.down) return node.bottom;
            if (dir == Vector2Int.left) return node.left;
            return null;
        }

        #endregion

        #region Level

        public void SlideAllNodes(List<NodeController> nodeControllers)
        {
            bool canMove;

            do
            {
                canMove = false;

                foreach (var controller in nodeControllers)
                {
                    Node node = controller.Node;

                    if (node.x == 0 || node.x == (width - 1) || node.y == 0 || node.y == (height - 1))
                        continue;

                    if (node.id == -1 || GridType == GridType.Normal)
                        continue;

                    Node target = GetNeighborByGridType(node);
                    if (target == null || target.id != -1
                     || target.x == 0 || target.x == (width - 1) || target.y == 0 || target.y == (height - 1))
                        continue;

                    target.id = node.id;
                    node.id = -1;

                    var targetCtrl = FindControllerByNode(nodeControllers, target);
                    if (targetCtrl != null)
                        targetCtrl.SetIdNode(target.id, _data.AssetData[0].SpriteData[target.id]);

                    controller.SetIdNode(-1);

                    canMove = true;
                }
            }
            while (canMove);
        }

        private Node GetNeighborByGridType(Node node)
        {
            switch (GridType)
            {
                case GridType.Top: return node.top;
                case GridType.Bottom: return node.bottom;
                case GridType.Left: return node.left;
                case GridType.Right: return node.right;
                case GridType.TopLeft: return node.topLeft;
                case GridType.TopRight: return node.topRight;
                case GridType.BottomLeft: return node.bottomLeft;
                case GridType.BottomRight: return node.bottomRight;
                case GridType.HaftVertical: return node.y > (height / 2) ? node.bottom : node.top;
                case GridType.HaftHorizontal: return node.x > (width / 2) ? node.right : node.left;
                case GridType.VerticalHaft: return node.y < (height / 2) ? node.bottom : node.top;
                case GridType.HorizontalHaft: return node.x < (width / 2) ? node.right : node.left;
                default: return null;
            }
        }

        private NodeController FindControllerByNode(List<NodeController> list, Node node)
        {
            return list.FirstOrDefault(ctrl => ctrl.Node == node);
        }

        #endregion

        #region Suggest

        public (NodeController, NodeController)? FindHintPair(List<NodeController> nodeControllers)
        {
            for (int i = 0; i < nodeControllers.Count - 1; i++)
            {
                NodeController a = nodeControllers[i];
                if (a.Node.id == -1) continue;

                for (int j = i + 1; j < nodeControllers.Count; j++)
                {
                    NodeController b = nodeControllers[j];
                    if (b.Node.id == -1 || a.Node.id != b.Node.id) continue;

                    List<Node> path = CanMatchWithPath(a.Node, b.Node);
                    if (path != null)
                    {
                        return (a, b);
                    }
                }
            }

            return null;
        }

        public void ShuffleGrid()
        {
            
        }

        #endregion
    }
    
    public class PathNode
    {
        public Node node;
        public Vector2Int direction;
        public int turns;
        public List<Node> path;

        public PathNode(Node node, Vector2Int direction, int turns, List<Node> path)
        {
            this.node = node;
            this.direction = direction;
            this.turns = turns;
            this.path = new List<Node>(path) { node };
        }
    }
}
