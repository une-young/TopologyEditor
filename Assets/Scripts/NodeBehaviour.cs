using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeBehaviour : MonoBehaviour {
    GameObject parentObject = null;

    List<GameObject> linkedNodeList = new List<GameObject>();

    public List<GameObject> LinkedNodeList
    {
        get
        {
            return linkedNodeList;
        }        
    }

    List<GameObject> nodeLinkList = new List<GameObject>();

    public GameObject ParentObject
    {
        get
        {
            return parentObject;
        }

        set
        {
            parentObject = value;
        }
    }

    public List<GameObject> NodeLinkList
    {
        get
        {
            return nodeLinkList;
        }
    }

    public GameObject FindLink(GameObject targetNode)
    {
        foreach(GameObject linkObject in nodeLinkList)
        {
            if (linkObject.GetComponent<NodeLinkBehaviour>().ContainsNode(this.gameObject, targetNode))
                return linkObject;
        }

        return null;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //public void RemoveLinkedNode(GameObject targetNode)
    //{
    //    foreach (GameObject node in LinkedNodeList)
    //    {

    //    }
    //}

    public void ClearAllData()
    {
        foreach (GameObject linkObject in NodeLinkList)
        {
            NodeLinkBehaviour nodeLinkBehaviour = linkObject.GetComponent<NodeLinkBehaviour>();

            //링크의 상대편 노드에 있는 링크 오브젝트도 상대편 링크 오브젝트 리스트에서 삭제한다.
            if(nodeLinkBehaviour.StartNode == this.gameObject)
            {
                nodeLinkBehaviour.EndNode.GetComponent<NodeBehaviour>().NodeLinkList.Remove(linkObject);
            }
            else if (nodeLinkBehaviour.EndNode == this.gameObject)
            {
                nodeLinkBehaviour.StartNode.GetComponent<NodeBehaviour>().NodeLinkList.Remove(linkObject);
            }

            GameObject.Destroy(linkObject);
        }

        NodeLinkList.Clear();

        foreach (GameObject linkedNode in LinkedNodeList)
        {
            NodeBehaviour nodeBehaviour = linkedNode.GetComponent<NodeBehaviour>();
            nodeBehaviour.LinkedNodeList.Remove(this.gameObject);
        }

        LinkedNodeList.Clear();
    }

    public string [] GetLinkedNodeNameList()
    {
        if (LinkedNodeList.Count == 0)
            return null;

        string[] nodeNames = new string[LinkedNodeList.Count];
        int counter = 0;

        foreach(GameObject node in LinkedNodeList)
        {
            nodeNames[counter] = node.name;
            counter++;
        }

        return nodeNames;
    }
}
