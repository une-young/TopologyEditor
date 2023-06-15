using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeLinkBehaviour : MonoBehaviour {

    // Use this for initialization
    GameObject startNode = null;
    GameObject endNode = null;

    public GameObject StartNode
    {
        get
        {
            return startNode;
        }

        set
        {
            startNode = value;
        }
    }

    public GameObject EndNode
    {
        get
        {
            return endNode;
        }

        set
        {
            endNode = value;
        }
    }

    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public bool ContainsNode(GameObject startNode,GameObject endNode)
    {
        if((this.StartNode == startNode && this.EndNode == endNode) || (this.StartNode == endNode && this.EndNode == startNode))
        {
            return true;
        }

        return false;
    }

    public void UpdateLine()
    {
        if (null == StartNode || null == EndNode)
            return;

        LineRenderer lineRenderer = this.gameObject.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, StartNode.transform.position);
        lineRenderer.SetPosition(1, EndNode.transform.position);
    }

    public void Clear()
    {
        //각 노드의 링크 오브젝트 정보를 삭제한다.
        RemoveLinkInfo(startNode);
        RemoveLinkInfo(endNode);

        //각 노드의 상대 노드 연결 정보를 삭제한다.
        RemoveNodeLinkInfo(startNode, endNode);
        RemoveNodeLinkInfo(endNode, startNode);

        startNode = null;
        endNode = null;
    }

    private void RemoveNodeLinkInfo(GameObject node,GameObject linkedNode)
    {
        node.GetComponent<NodeBehaviour>().LinkedNodeList.Remove(linkedNode);
    }

    private void RemoveLinkInfo(GameObject node)
    {
        if (null == node)
            return;

        NodeBehaviour nodeBehaviour = node.GetComponent<NodeBehaviour>();

        nodeBehaviour.NodeLinkList.Remove(this.gameObject);
    }
}
