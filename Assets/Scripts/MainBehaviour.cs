using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.IO;
using System;
using Assets.Scripts;
using System.Linq;

public class MainBehaviour : MonoBehaviour {

    [SerializeField]
    GameObject number = null;
    [SerializeField]
    GameObject target = null;
    [SerializeField]
    GameObject originalWallMeshObject = null;
    [SerializeField]
    GameObject basePlane = null;
    [SerializeField]
    GameObject buildingRoot = null;
    [SerializeField]
    Material wireframeMaterial = null;
    [SerializeField]
    GameObject currentPosition2D = null;
    [SerializeField]
    GameObject originalLine = null;
    [SerializeField]
    GameObject sphereRed = null;
    [SerializeField]
    GameObject originalPolygon = null;
    [SerializeField]
    GameObject originalRoom = null;
    [SerializeField]
    GameObject background2D = null;
    [SerializeField]
    GameObject mainCamera = null;
    [SerializeField]
    GameObject subCamera = null;
    [SerializeField]
    Material lineMaterial = null;
    [SerializeField]
    Transform grid2DTransform = null;
    [SerializeField]
    float gridZPos = 0.0f;
    [SerializeField]
    float gridStep = 1.0f;
    [SerializeField]
    GameObject doorBaseRoot = null;
    [SerializeField]
    GameObject originalNode = null;
    [SerializeField]
    GameObject nodeRoot = null;
    [SerializeField]
    GameObject laserLine = null;
    [SerializeField]
    GameObject moveGizmo = null;
    [SerializeField]
    GameObject topologyPanel = null;
    [SerializeField]
    Material nodeLinkMaterial = null;
    [SerializeField]
    Material selecteNodeLinkMaterial = null;
    [SerializeField]
    GameObject nodeLinkButton = null;
    [SerializeField]
    GameObject panelConfirm = null;
    [SerializeField]
    UnityEngine.UI.Text textResult = null;

    RoomBehaviour currentRoom = null;
    GameObject currentPolygon = null;

    GameObject selectedNode = null;
    GameObject selectedLink = null;

    List<RoomBehaviour> roomList = new List<RoomBehaviour>();

    List<GameObject> polygonList = new List<GameObject>();

    private GameObject selectedSphere = null;

    private Color gridLineColor = new Color(0f, 1f, 0f, 1f);

    public float speed = 2.0f;

    bool isDrawMode = false;

    string[] elementFilters = { "//bldg:Door", "//bldg:FloorSurface","//bldg:ClosureSurface",
        "//bldg:InteriorWallSurface","//bldg:CompositeSurface","//bldg:WallSurface" };
    string[] elementNames = { "Door Root", "Floor Root" ,"Closure Surface","Interior Wall Surface","Composite Surface","Wall Surface"};
    
    Dictionary<string, string> elementNameMap = new Dictionary<string, string>();
        
    //string[] elementFilters = { "//bldg:Door" };
    public enum EditMode
    {
        None,
        AddLink,
        AddNode
    }

    private EditMode currentEditMode = EditMode.None;

    // Use this for initialization
    void Start () {
        for(int i=0;i<elementFilters.Length;i++)
        {
            elementNameMap.Add(elementFilters[i], elementNames[i]);
        }

        //LoadCityGml(@"C:\Users\apple\Downloads\CityGML_2.0_Test_Dataset_2012-04-23\CityGML_2.0_Test_Dataset_2012-04-23\Part-6-Generics-V1.gml", originalWallMeshObject);
        //LoadCityGml(@"C:\Users\apple\Downloads\buildingslod3\geoRES_testdata_v1.0.0\geoRES_testdata_v1.0.0.xml", originalWallMeshObject);
        //StartCoroutine(LoadCityGml(@"D:\dell downloads\buildingslod3\geoRES_testdata_v1.0.0\building.xml", originalWallMeshObject, elementFilters));
        //LoadCityGml(@"C:\Users\apple\Downloads\Random3Dcity_2015-03-11\CityGML\LOD3_3.gml", originalWallMeshObject);        
        //LoadCityGml(@"C:\Users\apple\Downloads\Delft.gml\Delft_3dfier.gml", originalWallMeshObject);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string result = Application.absoluteURL;
            Debug.Log(result);
            StartCoroutine(LoadCityGml(result + @"M_B0010000000ELE8ML.xml", originalWallMeshObject, elementFilters));
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            StartCoroutine(LoadCityGml(@"Data\M_B0010000000ELE8ML", originalWallMeshObject, elementFilters));
        }
        else
        {
            //StartCoroutine(LoadCityGml(@"M_B0010000000ELE8ML.xml", originalWallMeshObject, elementFilters));
            //StartCoroutine(LoadCityGml(@"Lotte0603.gml", originalWallMeshObject, elementFilters));
        }

        createNewRoom();
    }

    Vector3 oldTargetPos = new Vector3();
    bool isFirstUpdate = true;

    // Update is called once per frame
    void Update () {

        if (mainCamera.activeSelf)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {

                isDrawMode = false;
                target.transform.position = new Vector3(1000.0f, 1000.0f, 1000.0f);

                if(selectedSphere != null)
                {
                    RoomBehaviour selectedRoom = selectedSphere.GetComponent<EditPointBehaviour>().RoomObject.GetComponent<RoomBehaviour>();

                    createOutlinePolygon(selectedRoom);
                    selectedRoom.RearrangePositionList();
                    selectedSphere = null;
                }
                    

                if(currentRoom.EditPointList.Count < 3)
                {
                    RemoveCurrentRoom();                    
                }

                createNewRoom();
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = 1 << 8;

            if (Physics.Raycast(ray, out hit, layerMask))
            {
                _processDrawRoom(hit);
            }

            if(!isDrawMode)
            {
                switch(currentEditMode)
                {
                    case EditMode.AddLink:
                        if(Input.GetMouseButtonDown(0))
                        {

                        }
                        break;
                    case EditMode.AddNode:
                        if(Input.GetMouseButtonDown(1))
                        {

                        }
                        break;
                }
                if (Input.GetMouseButtonDown(1)) //우클릭
                {
                    int nodeLayer = ~(1 << LayerMask.NameToLayer("DoorBase"));

                    if (Physics.Raycast(ray, out hit, 1000.0f, nodeLayer))
                    {
                        NodeBehaviour nodeBehaviour = hit.collider.gameObject.GetComponent<NodeBehaviour>();

                        if ( nodeBehaviour != null)
                        {
                            if (selectedNode != null)
                                clearNodeLinkListbox();

                            GameObject nodeObject = hit.collider.gameObject;
                            selectedNode = nodeObject;
                            //연결된 노드 리스트를 가져온다.
                            topologyPanel.SetActive(true);

                            //노드의 이름을 input field에 입력한다.
                            GameObject inputFieldNodeName = GameObject.Find("InputFieldNodeName");

                            inputFieldNodeName.GetComponent<UnityEngine.UI.InputField>().text = nodeObject.name;

                            moveGizmo.SetActive(true);
                            moveGizmo.transform.parent = nodeObject.transform;
                            moveGizmo.transform.localPosition = Vector3.zero;
                            moveGizmo.GetComponent<MoveGizmoBehaviour>().TargetObject = nodeObject;

                            CreateLinkButton(nodeBehaviour);
                        }
                        else if(selectedNode != null)
                        {
                            RestoreNodeLink();
                        }
                    }
                }
            }
        }

        //updateTargetPosition();
    }

    private void CreateLinkButton(NodeBehaviour nodeBehaviour)
    {
        foreach (GameObject linkedNode in nodeBehaviour.LinkedNodeList)
        {
            GameObject newNodeLinkButton = GameObject.Instantiate(nodeLinkButton);
            newNodeLinkButton.SetActive(true);

            //link를 찾는다.
            GameObject linkObject = nodeBehaviour.FindLink(linkedNode);

            //이벤트 연결
            newNodeLinkButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(()
                => NodeLinkButtonPressed(linkObject));

            UnityEngine.UI.Text text = newNodeLinkButton.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>();
            text.text = linkedNode.name;
            addButtonToContent(newNodeLinkButton);
        }
    }

    private void addButtonToContent(GameObject newNodeLinkButton)
    {
        GameObject content = GameObject.Find("ContentLinkList");

        newNodeLinkButton.transform.parent = content.transform;
    }

    private void clearNodeLinkListbox()
    {
        GameObject content = GameObject.Find("ContentLinkList");

        List<GameObject> buttonList = new List<GameObject>();

        foreach(Transform t in content.transform)
        {
            GameObject.Destroy(t.gameObject);
        }
    }

    private void _processDrawRoom(RaycastHit hit)
    {
        if (isDrawMode)
        {
            Vector3 toPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            if (isFirstUpdate)
            {
                oldTargetPos = hit.point;
                isFirstUpdate = false;
            }
            else
            {
                oldTargetPos = target.transform.position;

                Vector3 newTargetPos = getSnapPosition(Vector3.Lerp(oldTargetPos, hit.point, Time.deltaTime * 20.0f));

                target.transform.position = newTargetPos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    Add(target.transform.position + Vector3.up * 0.01f, currentRoom);
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    if (hit.collider.gameObject.tag == "EditPoint")
                        selectedSphere = hit.collider.gameObject;
                    else
                        selectedSphere = null;
                }
            }

            if (null != selectedSphere)
            {
                Vector3 realPosition = new Vector3(hit.point.x, selectedSphere.transform.position.y, hit.point.z);

                Vector3 snapPosition = getSnapPosition(realPosition);

                selectedSphere.transform.position = snapPosition;

                createOutlinePolygon(selectedSphere.GetComponent<EditPointBehaviour>().RoomObject.GetComponent<RoomBehaviour>());
            }
        }
    }

    private Vector3 handleMouseControl(Vector3 toPosition)
    {
        if (Input.GetMouseButton(0))
        {

            Ray ray = mainCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = 1 << 9;

            if (Physics.Raycast(ray, out hit, layerMask))
            {
                if (hit.collider.gameObject.tag == "EditPoint")
                    selectedSphere = hit.collider.gameObject;
                else
                    selectedSphere = null;
            }
            else
                selectedSphere = null;


            float oldY = transform.position.y;

            //Vector3 newForward = new Vector3(transform.forward.x, 0.0f, -transform.forward.y);

            //toPosition += transform.right * mouseX + newForward * mouseZ;

            if (null != selectedSphere)
            {

                Vector3 realPosition = new Vector3(hit.point.x, hit.point.y, selectedSphere.transform.position.z);

                Vector3 snapPosition = getSnapPosition(realPosition);


                selectedSphere.transform.position = snapPosition;

                createOutlinePolygon();
            }
            else
            {
                float mouseX = -Input.GetAxis("Mouse X") * speed;
                float mouseY = -Input.GetAxis("Mouse Y") * speed;

                toPosition = new Vector3(transform.position.x + mouseX, transform.position.y + mouseY, transform.position.z);
            }
        }

        return toPosition;
    }

    IEnumerator LoadCityGml(string fileName, GameObject originalMeshObject,string [] elementFilterList)
    {

        Dictionary<string, List<Vector2>> ringUvMap = new Dictionary<string, List<Vector2>>();
        Dictionary<string, string> polygonTextureMap = new Dictionary<string, string>();
        Dictionary<string, Texture2D> textureMap = new Dictionary<string, Texture2D>();

        string[] posSplitter = { " ","\r\n" };

        string text = "";

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WWW www = new WWW(fileName);

            yield return www;

            text = www.text;
        }
        else if(Application.platform == RuntimePlatform.Android)
        {
            TextAsset textAsset = UnityEngine.Resources.Load(fileName) as TextAsset;
            text = textAsset.text;
        }
        else
            text = File.ReadAllText(fileName);

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(text);
        XmlNamespaceManager ns = new XmlNamespaceManager(xmlDoc.NameTable);

        ns.AddNamespace("gml", "http://www.opengis.net/gml");
        ns.AddNamespace("app", "http://www.opengis.net/citygml/appearance/1.0");
        ns.AddNamespace("bldg", "http://www.opengis.net/citygml/building/2.0");


        XmlNodeList parameterizedTextureNodes = xmlDoc.SelectNodes("//app:ParameterizedTexture", ns);
        string rootPath = @"C:\Users\apple\Downloads\buildingslod3\geoRES_testdata_v1.0.0\textures\";
        //textrue 데이터를 미리 로딩
        foreach (XmlNode parameterizedTextureNode in parameterizedTextureNodes)
        {
            XmlNode imageUriNode = parameterizedTextureNode.SelectSingleNode("app:imageURI", ns);

            if(null != imageUriNode)
            {
                string imagePath = imageUriNode.InnerText;

                string imageFileName = System.IO.Path.GetFileName(imagePath);

                if(!textureMap.ContainsKey(imagePath))
                {
                    Texture2D texture2d = Assets.Scripts.Util.LoadPNG(rootPath + imageFileName);

                    if(null != texture2d)
                    {
                        textureMap.Add(imagePath, texture2d);
                    }
                    else
                    {
                        Debug.Log("Loading texture error:" + imageFileName);
                        continue;
                    }
                }

                XmlNodeList targetNodes = parameterizedTextureNode.SelectNodes("app:target", ns);

                foreach (XmlNode targetNode in targetNodes)
                {
                    string uri = targetNode.Attributes["uri"].Value; //polygon id

                    //# 제거
                    uri = uri.Replace("#", string.Empty);

                    polygonTextureMap.Add(uri, imagePath);

                    XmlNodeList texCoordListNodes = targetNode.SelectNodes("app:TexCoordList", ns);

                    foreach (XmlNode texCoordListNode in texCoordListNodes)
                    {
                        XmlNodeList uvNodes = texCoordListNode.SelectNodes("app:textureCoordinates", ns);
                        foreach (XmlNode uvNode in uvNodes)
                        {
                            string ringId = uvNode.Attributes["ring"].Value;

                            string coordText = uvNode.InnerText;

                            string[] coordinates = coordText.Split(posSplitter, System.StringSplitOptions.RemoveEmptyEntries);

                            if (coordinates.Length % 2 == 0) //2의 배수
                            {
                                List<Vector2> uvList = new List<Vector2>();

                                for (int i = 0; i < coordinates.Length; i += 2)
                                {
                                    float u = float.Parse(coordinates[i]);
                                    float v = float.Parse(coordinates[i + 1]);

                                    Vector2 uv = new Vector2(u, v);

                                    uvList.Add(uv);
                                }

                                ringUvMap.Add(ringId, uvList);
                            }
                        }
                    }
                }                
            }           
        }

        XmlNode lowerCornerNode = xmlDoc.SelectSingleNode("//gml:lowerCorner", ns);
        XmlNode upperCornerNode = xmlDoc.SelectSingleNode("//gml:upperCorner", ns);

        Vector3 centerPos = Vector3.zero;
        double[] centerPosDouble = new double[3];        

        if(lowerCornerNode != null && upperCornerNode != null)
        {
            Vector3 lowerCorner = getVector3FromPosNode(posSplitter,lowerCornerNode,null);
            Vector3 upperCorner = getVector3FromPosNode(posSplitter, upperCornerNode,null);
            centerPos = (upperCorner + lowerCorner) * 0.5f;

            double[] lowerCornerDouble = getVector3FromPosNode(posSplitter, lowerCornerNode);
            double[] upperCornerDouble = getVector3FromPosNode(posSplitter, upperCornerNode);
            
            for(int i=0;i<3;i++)
            {
                centerPosDouble[i] = (lowerCornerDouble[i] + upperCornerDouble[i]) * 0.5;
            }            
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                centerPosDouble[i] = 0.0;
            }
        }

        float lowestY = float.MaxValue;

        int objectCount = 0;

        XmlNodeList roomNodeList = xmlDoc.SelectNodes(".//bldg:Room",ns);

        int doorSurfaceCounter = 0;

        foreach (XmlNode roomNode in roomNodeList)
        {
            XmlAttribute roomIdAttr = roomNode.Attributes["gml:id"];

            GameObject roomRoot = new GameObject();

            if (null != roomIdAttr)
                roomRoot.name = roomIdAttr.Value;

            roomRoot.transform.parent = buildingRoot.transform;            

            foreach (string elementFilter in elementFilterList)
            {
                XmlNodeList filteredNodeList = roomNode.SelectNodes("." + elementFilter, ns);

                //int nodeNumber = filteredNodeList.Count;

                GameObject elementNodeRoot = new GameObject();
                elementNodeRoot.name = elementNameMap[elementFilter];
                elementNodeRoot.tag = elementNameMap[elementFilter];

                elementNodeRoot.transform.parent = roomRoot.transform;                

                if ("Door Root" == elementNodeRoot.name)
                {
                    doorSurfaceCounter += filteredNodeList.Count;
                }

                foreach (XmlNode filteredNode in filteredNodeList)
                {
                    XmlAttribute nodeIdAttr = filteredNode.Attributes["gml:id"];

                    string elementNodeId = elementFilter + objectCount;

                    if (null != nodeIdAttr)
                        elementNodeId = nodeIdAttr.Value;

                    XmlNodeList polygonNodes = filteredNode.SelectNodes(".//gml:Polygon", ns);

                    foreach (XmlNode polygonNode in polygonNodes)
                    {
                        List<Vector3> outline = new List<Vector3>();
                        List<Vector2> outlineUv = new List<Vector2>();
                        List<List<Vector3>> inline = new List<List<Vector3>>();
                        List<List<Vector2>> inlineUv = new List<List<Vector2>>();

                        var polygonIdAttribute = polygonNode.Attributes["gml:id"];

                        string polygonId = "";

                        if (null != polygonIdAttribute)
                        {
                            polygonId = polygonIdAttribute.Value;
                            elementNodeId = polygonId;
                        }

                        Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

                        XmlNodeList exteriorNodeList = polygonNode.SelectNodes("gml:exterior", ns);

                        foreach (XmlNode exteriorNode in exteriorNodeList)
                        {
                            XmlNode linerRingNode = exteriorNode.SelectSingleNode("gml:LinearRing", ns);

                            XmlAttribute idAttr = linerRingNode.Attributes["gml:id"];
                            string linerRingId = "_NONE";

                            if (idAttr != null)
                            {
                                linerRingId = linerRingNode.Attributes["gml:id"].Value;
                            }

                            if (null != linerRingNode)
                            {
                                XmlNodeList posNodes = linerRingNode.SelectNodes("gml:pos", ns);

                                if (0 == posNodes.Count)
                                {
                                    XmlNodeList posNodeLists = linerRingNode.SelectNodes("gml:posList", ns);

                                    foreach (XmlNode posList in posNodeLists)
                                    {
                                        List<Vector3> vectorList = getVector3FromPosListNode(posSplitter, posList, centerPosDouble);

                                        foreach (Vector3 v in vectorList)
                                        {
                                            Vector3 pos = v;

                                            pos -= centerPos;

                                            if (pos.magnitude > 10000.0f)
                                            {
                                                pos += centerPos;
                                            }

                                            lowestY = Mathf.Min(lowestY, pos.y);

                                            outline.Add(pos);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (XmlNode posNode in posNodes)
                                    {
                                        Vector3 pos = getVector3FromPosNode(posSplitter, posNode, centerPosDouble);

                                        pos -= centerPos;

                                        if (pos.magnitude > 10000.0f)
                                        {
                                            pos += centerPos;
                                        }

                                        lowestY = Mathf.Min(lowestY, pos.y);

                                        outline.Add(pos);
                                    }
                                }

                                outline.Reverse();
                                poly.outside = outline;

                                if (ringUvMap.ContainsKey(linerRingId))
                                {
                                    outlineUv = ringUvMap[linerRingId];
                                    outlineUv.Reverse();
                                    poly.outsideUVs = outlineUv;
                                }
                            }
                        }


                        XmlNodeList interiorNodeList = polygonNode.SelectNodes("gml:interior", ns);

                        foreach (XmlNode interiorNode in interiorNodeList)
                        {
                            XmlNode linerRingNode = interiorNode.SelectSingleNode("gml:LinearRing", ns);

                            if (null != linerRingNode)
                            {
                                XmlAttribute idAttr = linerRingNode.Attributes["gml:id"];

                                string linerRingId = "_NONE";

                                if (idAttr != null)
                                {
                                    linerRingId = linerRingNode.Attributes["gml:id"].Value;
                                }


                                XmlNodeList posNodes = linerRingNode.SelectNodes("gml:pos", ns);

                                List<Vector3> inlineRing = new List<Vector3>();
                                List<Vector2> inlineRingUv = new List<Vector2>();

                                if (0 == posNodes.Count)
                                {
                                    XmlNodeList posNodeLists = linerRingNode.SelectNodes("gml:posList", ns);

                                    foreach (XmlNode posList in posNodeLists)
                                    {
                                        List<Vector3> vectorList = getVector3FromPosListNode(posSplitter, posList, centerPosDouble);

                                        foreach (Vector3 v in vectorList)
                                        {
                                            Vector3 pos = v;

                                            pos -= centerPos;

                                            if (pos.magnitude > 10000.0f)
                                            {
                                                pos += centerPos;
                                            }

                                            lowestY = Mathf.Min(lowestY, pos.y);

                                            inlineRing.Add(pos);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (XmlNode posNode in posNodes)
                                    {
                                        Vector3 pos = getVector3FromPosNode(posSplitter, posNode, centerPosDouble);

                                        pos -= centerPos;

                                        if (pos.magnitude > 10000.0f)
                                        {
                                            pos += centerPos;
                                        }

                                        lowestY = Mathf.Min(lowestY, pos.y);

                                        inlineRing.Add(pos);
                                    }
                                }

                                inlineRing.Reverse();

                                inline.Add(inlineRing);

                                if (ringUvMap.ContainsKey(linerRingId))
                                {
                                    inlineRingUv = ringUvMap[linerRingId];
                                    inlineRingUv.Reverse();
                                    inlineUv.Add(inlineRingUv);
                                }
                            }
                        }

                        poly.holes = inline;

                        if (inlineUv.Count > 0)
                            poly.holesUVs = inlineUv;

                        GameObject newMeshObject = null;


                        //var polygonRingIdAttribute = exteriorNodeList

                        try
                        {
                            newMeshObject = Poly2Mesh.CreateGameObject(poly);
                        }
                        catch (Exception e)
                        {

                            if (null != polygonIdAttribute)
                                Debug.Log(string.Format("Polygon creation error:{0}", polygonIdAttribute.Value));
                            else
                                Debug.Log("Polygon creation error: unknown");
                        }

                        if (null != newMeshObject)
                        {
                            newMeshObject.AddComponent<SurfaceBehaviour>().Outline = poly.outside;

                            Mesh mesh = newMeshObject.GetComponent<MeshFilter>().mesh;

                            if (mesh.triangles.Length > 2)
                            {
                                if (Util.isCcw(mesh))
                                {
                                    int[] triangles = (int[])mesh.triangles.Clone();
                                    Array.Reverse(triangles);
                                    mesh.triangles = triangles;
                                }

                                mesh.RecalculateNormals();

                                newMeshObject.GetComponent<MeshRenderer>().material = originalMeshObject.GetComponent<MeshRenderer>().material;

                                if (polygonTextureMap.ContainsKey(polygonId))
                                {
                                    Material m = newMeshObject.GetComponent<MeshRenderer>().material;

                                    string textureId = polygonTextureMap[polygonId];

                                    m.mainTexture = textureMap[textureId];
                                }

                                if (null != polygonIdAttribute)
                                    newMeshObject.name = polygonIdAttribute.Value;

                                newMeshObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                                newMeshObject.transform.parent = elementNodeRoot.transform;
                                newMeshObject.name = elementNodeId;

                                objectCount++;

                                if (objectCount % 500 == 0)
                                    yield return null;
                            }
                            else
                            {
                                GameObject.Destroy(newMeshObject);
                            }
                        }
                    }
                }
            }
        }

        if(0 == doorSurfaceCounter)
        {
            XmlNodeList filteredNodeList = xmlDoc.SelectNodes(".//bldg:Door", ns);

            GameObject elementNodeRoot = new GameObject();
            elementNodeRoot.name = "Door Root";
            elementNodeRoot.tag = "Door Root";            

            foreach (XmlNode filteredNode in filteredNodeList)
            {
                XmlAttribute nodeIdAttr = filteredNode.Attributes["gml:id"];

                string elementNodeId = "Door" + objectCount;

                if (null != nodeIdAttr)
                    elementNodeId = nodeIdAttr.Value;

                XmlNodeList polygonNodes = filteredNode.SelectNodes(".//gml:Polygon", ns);

                foreach (XmlNode polygonNode in polygonNodes)
                {
                    List<Vector3> outline = new List<Vector3>();
                    List<Vector2> outlineUv = new List<Vector2>();
                    List<List<Vector3>> inline = new List<List<Vector3>>();
                    List<List<Vector2>> inlineUv = new List<List<Vector2>>();

                    var polygonIdAttribute = polygonNode.Attributes["gml:id"];

                    string polygonId = "";

                    if (null != polygonIdAttribute)
                    {
                        polygonId = polygonIdAttribute.Value;
                        elementNodeId = polygonId;
                    }

                    Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

                    XmlNodeList exteriorNodeList = polygonNode.SelectNodes("gml:exterior", ns);

                    foreach (XmlNode exteriorNode in exteriorNodeList)
                    {
                        XmlNode linerRingNode = exteriorNode.SelectSingleNode("gml:LinearRing", ns);

                        XmlAttribute idAttr = linerRingNode.Attributes["gml:id"];
                        string linerRingId = "_NONE";

                        if (idAttr != null)
                        {
                            linerRingId = linerRingNode.Attributes["gml:id"].Value;
                        }

                        if (null != linerRingNode)
                        {
                            XmlNodeList posNodes = linerRingNode.SelectNodes("gml:pos", ns);

                            if (0 == posNodes.Count)
                            {
                                XmlNodeList posNodeLists = linerRingNode.SelectNodes("gml:posList", ns);

                                foreach (XmlNode posList in posNodeLists)
                                {
                                    List<Vector3> vectorList = getVector3FromPosListNode(posSplitter, posList, centerPosDouble);

                                    foreach (Vector3 v in vectorList)
                                    {
                                        Vector3 pos = v;

                                        pos -= centerPos;

                                        if (pos.magnitude > 10000.0f)
                                        {
                                            pos += centerPos;
                                        }

                                        lowestY = Mathf.Min(lowestY, pos.y);

                                        outline.Add(pos);
                                    }
                                }
                            }
                            else
                            {
                                foreach (XmlNode posNode in posNodes)
                                {
                                    Vector3 pos = getVector3FromPosNode(posSplitter, posNode, centerPosDouble);

                                    pos -= centerPos;

                                    if (pos.magnitude > 10000.0f)
                                    {
                                        pos += centerPos;
                                    }

                                    lowestY = Mathf.Min(lowestY, pos.y);

                                    outline.Add(pos);
                                }
                            }

                            outline.Reverse();
                            poly.outside = outline;

                            if (ringUvMap.ContainsKey(linerRingId))
                            {
                                outlineUv = ringUvMap[linerRingId];
                                outlineUv.Reverse();
                                poly.outsideUVs = outlineUv;
                            }
                        }
                    }


                    XmlNodeList interiorNodeList = polygonNode.SelectNodes("gml:interior", ns);

                    foreach (XmlNode interiorNode in interiorNodeList)
                    {
                        XmlNode linerRingNode = interiorNode.SelectSingleNode("gml:LinearRing", ns);

                        if (null != linerRingNode)
                        {
                            XmlAttribute idAttr = linerRingNode.Attributes["gml:id"];

                            string linerRingId = "_NONE";

                            if (idAttr != null)
                            {
                                linerRingId = linerRingNode.Attributes["gml:id"].Value;
                            }


                            XmlNodeList posNodes = linerRingNode.SelectNodes("gml:pos", ns);

                            List<Vector3> inlineRing = new List<Vector3>();
                            List<Vector2> inlineRingUv = new List<Vector2>();

                            if (0 == posNodes.Count)
                            {
                                XmlNodeList posNodeLists = linerRingNode.SelectNodes("gml:posList", ns);

                                foreach (XmlNode posList in posNodeLists)
                                {
                                    List<Vector3> vectorList = getVector3FromPosListNode(posSplitter, posList, centerPosDouble);

                                    foreach (Vector3 v in vectorList)
                                    {
                                        Vector3 pos = v;

                                        pos -= centerPos;

                                        if (pos.magnitude > 10000.0f)
                                        {
                                            pos += centerPos;
                                        }

                                        lowestY = Mathf.Min(lowestY, pos.y);

                                        inlineRing.Add(pos);
                                    }
                                }
                            }
                            else
                            {
                                foreach (XmlNode posNode in posNodes)
                                {
                                    Vector3 pos = getVector3FromPosNode(posSplitter, posNode, centerPosDouble);

                                    pos -= centerPos;

                                    if (pos.magnitude > 10000.0f)
                                    {
                                        pos += centerPos;
                                    }

                                    lowestY = Mathf.Min(lowestY, pos.y);

                                    inlineRing.Add(pos);
                                }
                            }

                            inlineRing.Reverse();

                            inline.Add(inlineRing);

                            if (ringUvMap.ContainsKey(linerRingId))
                            {
                                inlineRingUv = ringUvMap[linerRingId];
                                inlineRingUv.Reverse();
                                inlineUv.Add(inlineRingUv);
                            }
                        }
                    }

                    poly.holes = inline;

                    if (inlineUv.Count > 0)
                        poly.holesUVs = inlineUv;

                    GameObject newMeshObject = null;


                    //var polygonRingIdAttribute = exteriorNodeList

                    try
                    {
                        newMeshObject = Poly2Mesh.CreateGameObject(poly);
                    }
                    catch (Exception e)
                    {

                        if (null != polygonIdAttribute)
                            Debug.Log(string.Format("Polygon creation error:{0}", polygonIdAttribute.Value));
                        else
                            Debug.Log("Polygon creation error: unknown");
                    }

                    if (null != newMeshObject)
                    {
                        newMeshObject.AddComponent<SurfaceBehaviour>().Outline = poly.outside;

                        Mesh mesh = newMeshObject.GetComponent<MeshFilter>().mesh;

                        if (mesh.triangles.Length > 2)
                        {
                            if (Util.isCcw(mesh))
                            {
                                int[] triangles = (int[])mesh.triangles.Clone();
                                Array.Reverse(triangles);
                                mesh.triangles = triangles;
                            }

                            mesh.RecalculateNormals();

                            newMeshObject.GetComponent<MeshRenderer>().material = originalMeshObject.GetComponent<MeshRenderer>().material;

                            if (polygonTextureMap.ContainsKey(polygonId))
                            {
                                Material m = newMeshObject.GetComponent<MeshRenderer>().material;

                                string textureId = polygonTextureMap[polygonId];

                                m.mainTexture = textureMap[textureId];
                            }

                            if (null != polygonIdAttribute)
                                newMeshObject.name = polygonIdAttribute.Value;

                            newMeshObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            newMeshObject.transform.parent = elementNodeRoot.transform;
                            newMeshObject.name = elementNodeId;

                            objectCount++;

                            if (objectCount % 500 == 0)
                                yield return null;
                        }
                        else
                        {
                            GameObject.Destroy(newMeshObject);
                        }
                    }
                }
            }
        }


        basePlane.transform.position = new Vector3(0.0f, lowestY*0.01f, 0.0f);

        ProcessDoorObjects();
        CreateNodes(doorBaseRoot); //Door base 생성뒤에 와야 한다.
        ProcessFloorObjects();
        CreateFloorGroup();
        AddFloorHeight();
        GlobalBuilding = CreateBuildingData();        

        yield return null;
    }

    

    //bool isCcw(Mesh mesh)
    //{
    //    if (mesh.triangles.Length < 3)
    //        return false;

    //    Vector3 v1 = mesh.vertices[mesh.triangles[0]];
    //    Vector3 v2 = mesh.vertices[mesh.triangles[1]];
    //    Vector3 v3 = mesh.vertices[mesh.triangles[2]];

    //    float x1 = v1.x, x2 = v2.x, x3 = v3.x;
    //    //float y1 = v1.y, y2 = v2.y, y3 = v3.y;
    //    //float z1 = -v1.z, z2 = -v2.z, z3 = -v3.z;

    //    float y1 = v1.z, y2 = v2.z, y3 = v3.z;
    //    float z1 = -v1.y, z2 = -v2.y, z3 = -v3.y;

    //    float temp = x1 * y2 + x2 * y3 + x3 * y1;
    //    temp = temp - y1 * x2 - y2 * x3 - y3 * x1;

    //    if (temp > 0.0f)
    //    {
    //        return true;
    //    }
    //    else if (temp < 0)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        return true;
    //    }
    //}

    Vector3 getSurfaceNormal(Mesh mesh)
    {
        if (mesh.triangles.Length < 3)
            return Vector3.up;

        Vector3[] vertices = new Vector3[3];

        vertices[0] = mesh.vertices[mesh.triangles[0]];
        vertices[1] = mesh.vertices[mesh.triangles[1]];
        vertices[2] = mesh.vertices[mesh.triangles[2]];

        var surfaceNormal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;



        return surfaceNormal;
    }

    double [] getVector3FromPosNode(string[] posSplitter, XmlNode posNode)
    {
        double[] pos = new double[3];

        string[] posElements = posNode.InnerText.Split(posSplitter, System.StringSplitOptions.RemoveEmptyEntries);

        if (posElements.Length == 3)
        {
            pos[0] = double.Parse(posElements[0]);
            pos[1] = double.Parse(posElements[1]);
            pos[2] = double.Parse(posElements[2]);
        }
        else
            return null;

        return pos;
    }

    private static List<Vector3> getVector3FromPosListNode(string[] posSplitter, XmlNode posListNode, double[] centerPos)
    {
        string[] posElements = posListNode.InnerText.Split(posSplitter, System.StringSplitOptions.RemoveEmptyEntries);

        

        double scale = 100.0f;

        List<Vector3> posList = new List<Vector3>();

        if (posElements.Length%3 == 0)
        {
            for(int i=0;i<posElements.Length;i+=3)
            {
                Vector3 pos = new Vector3();

                if (centerPos != null)
                {
                    double x = (double.Parse(posElements[i]) - centerPos[0]) * scale;
                    double z = (double.Parse(posElements[i+1]) - centerPos[1]) * scale;
                    double y = (double.Parse(posElements[i+2]) - centerPos[2]) * scale;

                    pos = new Vector3((float)x, (float)y, (float)z);
                }
                else
                {
                    float x = float.Parse(posElements[i]);
                    float z = float.Parse(posElements[i+1]);
                    float y = float.Parse(posElements[i+2]);

                    pos = new Vector3(x, y, z);
                }

                posList.Add(pos);
            }            
        }

        return posList;
    }

    private static Vector3 getVector3FromPosNode(string[] posSplitter, XmlNode posNode,double [] centerPos)
    {
        string[] posElements = posNode.InnerText.Split(posSplitter, System.StringSplitOptions.RemoveEmptyEntries);

        Vector3 pos = Vector3.zero;

        double scale = 100.0f;

        if (posElements.Length == 3)
        {
            if(centerPos != null)
            {
                double x = (double.Parse(posElements[0]) - centerPos[0]) * scale;
                double z = (double.Parse(posElements[1]) - centerPos[1]) * scale;
                double y = (double.Parse(posElements[2]) - centerPos[2]) * scale;

                pos = new Vector3((float)x, (float)y, (float)z);
            }
            else
            {
                float x = float.Parse(posElements[0]);
                float z = float.Parse(posElements[1]);
                float y = float.Parse(posElements[2]);

                pos = new Vector3(x, y, z);
            }            
        }

        return pos;
    }

    Dictionary<string, Material> materialMap = new Dictionary<string, Material>();

    bool isWireframeMode = false;
    public void ToggleBuildingWireframe()
    {
        if (isWireframeMode)
            RestoreMaterial(buildingRoot);
        else
            SetWireframeMaterial(buildingRoot);

        foreach(GameObject ob in polygonList)
        {
            if (isWireframeMode)
                RestoreMaterial(ob);
            else
                SetWireframeMaterial(ob);
        }

        isWireframeMode = !isWireframeMode;
    }

    bool showLine = true;

    public void ToggleLine()
    {
        ShowLine = !ShowLine;
    }

    void SetWireframeMaterial(GameObject root)
    {
        //materialMap.Clear();

        foreach(Transform child in root.transform)
        {
            MeshRenderer renderer = child.gameObject.GetComponent<MeshRenderer>();

            if (null != renderer)
            {
                if(!materialMap.ContainsKey(child.gameObject.name))
                    materialMap.Add(child.gameObject.name, renderer.material);

                renderer.material = wireframeMaterial;

                SetWireframeMaterial(child.gameObject);
            }
            else
            {
                SetWireframeMaterial(child.gameObject);
            }
        }
    }

    void RestoreMaterial(GameObject root)
    {
        foreach (Transform child in root.transform)
        {
            MeshRenderer renderer = child.gameObject.GetComponent<MeshRenderer>();
            string gameObjectName = child.gameObject.name;
            if (null != renderer)
            {
                if(materialMap.ContainsKey(gameObjectName))
                {
                    Material m = materialMap[gameObjectName];

                    renderer.material = m;
                }

                RestoreMaterial(child.gameObject);
            }
            else
            {
                RestoreMaterial(child.gameObject);
            }
        }
    }

    GameObject getPolygonObj(RoomBehaviour room)
    {
        int counter = 0;

        foreach(RoomBehaviour rb in roomList)
        {
            if (rb == room)
                return polygonList[counter];

            counter++;
        }

        return null;
    }

    public void createOutlinePolygon(RoomBehaviour room = null)
    {
        GameObject polygonObj = null;

        if (null == room)
        {
            room = currentRoom;
            polygonObj = currentPolygon;
        }
        else
        {
            polygonObj =  getPolygonObj(room);
        }

        List<Vector3> outline = new List<Vector3>();

        foreach (GameObject editPoint in room.EditPointList)
        {
            Vector3 pos = editPoint.transform.position;

            outline.Add(pos);
        }

        Mesh mesh = Assets.Resources.Scripts.Triangulation.PolygonToUnityMesh.Convert(outline);

        if (null != mesh)
        {
            //GameObject polygon = GameObject.Instantiate(originalPolygon);

            polygonObj.GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    private void createNewRoom()
    {
        GameObject currentRoomObj = GameObject.Instantiate(originalRoom);
        currentRoom = currentRoomObj.GetComponent<RoomBehaviour>();
        roomList.Add(currentRoom);

        currentPolygon = GameObject.Instantiate(originalPolygon);
        polygonList.Add(currentPolygon);
    }

    public void ClearAll()
    {
        foreach (RoomBehaviour roomBehaviour in roomList)
        {

            roomBehaviour.Clear();

            GameObject.Destroy(roomBehaviour.gameObject);
        }

        roomList.Clear();

        foreach (GameObject polygon in polygonList)
        {
            foreach (Transform t in polygon.transform)
            {
                if(null != t.GetComponent<MeshFilter>())
                {                    
                    GameObject.Destroy(t.gameObject.GetComponent<MeshFilter>().mesh);
                    GameObject.Destroy(t.gameObject);
                }                
            }

            GameObject.Destroy(polygon.GetComponent<MeshFilter>().mesh);
            GameObject.Destroy(polygon);
        }

        polygonList.Clear();
        createNewRoom();
    }

    public void Add(Vector3 position, RoomBehaviour roomBehaviour)
    {
        GameObject clone = GameObject.Instantiate(target);

        clone.transform.position = position;

        roomBehaviour.PositionList.Add(clone);

        if (roomBehaviour.PositionList.Count > 1)
            drawLine(roomBehaviour);
    }

    public void OnDoneButtonClicked()
    {
        //if (!mainCamera.activeSelf)
        //{
        //    mainCamera.SetActive(true);
        //    subCamera.SetActive(false);
        //    background2D.SetActive(false);
        //}
        //else
        //{
        //    mainCamera.SetActive(false);
        //    subCamera.SetActive(true);
        //    drawLine(currentRoom);
        //    //SendBasePoints();
        //    background2D.SetActive(true);
        //    background2D.transform.position = subCamera.transform.position + new Vector3(0.0f, 0.0f, 3.0f);
        //    //originalPolygon.transform.position = background2D.transform.position;
        //}
    }

    public void updateTargetPosition()
    {

        currentPosition2D.transform.position =
            subCamera.transform.position + new Vector3(target.transform.position.x, target.transform.position.z, 3.0f);
    }

    public void drawLine(RoomBehaviour roomBehaviour)
    {
        if (originalLine != null)
        {
            removeLines(roomBehaviour);

            Vector3 begin = new Vector3();
            Vector3 end = new Vector3();

            for (int i = 0; i < roomBehaviour.PositionList.Count; i++)
            {

                if (i == roomBehaviour.PositionList.Count - 1)
                {
                    begin = roomBehaviour.PositionList[i].transform.position;
                    end = roomBehaviour.PositionList[0].transform.position;
                }
                else
                {
                    begin = roomBehaviour.PositionList[i].transform.position;
                    end = roomBehaviour.PositionList[i + 1].transform.position;
                }

                //create sphere
                //GameObject beginSphere = GameObject.Instantiate(sphereRed);

                //beginSphere.transform.position = begin;

                GameObject endSphere = GameObject.Instantiate(sphereRed);

                endSphere.transform.position = end;

                GameObject newLine = GameObject.Instantiate(originalLine);

                LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();

                lineRenderer.SetPosition(0, begin);
                lineRenderer.SetPosition(1, end);

                Vector3 center = (begin + end) / 2.0f;
                center = new Vector3(center.x, center.y, center.z - 1.0f); //라인에 가리지 않게 앞으로 이동

                newLine.transform.position = center;

                //beginSphere.transform.parent = newLine.transform;
                endSphere.transform.parent = newLine.transform;

                //EditPointBehaviour editPointBehaviour = endSphere.GetComponent<EditPointBehaviour>();

                newLine.GetComponent<WallLineBehaviour>().EditPoint = endSphere;

                //float angle = Mathf.Atan2()
                //newLine.transform.LookAt(end);

                roomBehaviour.Lines.Add(newLine);
                roomBehaviour.EditPointList.Add(endSphere);
                endSphere.GetComponent<EditPointBehaviour>().RoomObject = roomBehaviour.gameObject;

                Debug.Log(string.Format("add {0}th line {1} to {2}", i, begin, end));
                Debug.Log(string.Format("Text pos: {0} rot: {1}",
                    newLine.transform.position.ToString(), newLine.transform.eulerAngles.ToString()));
            }

            for (int i = 0; i < roomBehaviour.Lines.Count; i++)
            {
                GameObject line = roomBehaviour.Lines[i];
                GameObject nextLine = null;

                if (i != (roomBehaviour.Lines.Count - 1))
                    nextLine = roomBehaviour.Lines[i + 1];
                else
                    nextLine = roomBehaviour.Lines[0];

                GameObject editPoint = line.GetComponent<WallLineBehaviour>().EditPoint;

                editPoint.GetComponent<EditPointBehaviour>().SetLinkedLine(line, nextLine);
            }

            createOutlinePolygon();
        }
    }

    public void removeLines(RoomBehaviour roomBehaviour)
    {
        foreach (GameObject line in roomBehaviour.Lines)
        {
            foreach (Transform child in line.transform)
            {
                Destroy(child.gameObject);
            }

            GameObject.Destroy(line);
        }

        roomBehaviour.Lines.Clear();
        roomBehaviour.EditPointList.Clear();
    }

    void RemoveCurrentRoom()
    {
        if(roomList.Count > 0)
        {
            currentRoom.Clear();
            GameObject.Destroy(currentRoom.gameObject);
            roomList.Remove(currentRoom);
        }
            

        if(polygonList.Count>0)
        {
            GameObject.Destroy(currentPolygon);
            polygonList.Remove(currentPolygon);
        }            
    }

    public void addSampleData()
    {
        float halfWidth = 5.0f;
        float halfHeight = 5.0f;

        Vector3 center = new Vector3(UnityEngine.Random.Range(-5.0f, 5.0f), 0.0f, UnityEngine.Random.Range(-5.0f, 5.0f));

        createNewRoom();

        Add(new Vector3(-halfWidth + center.x, 0.0f, -halfHeight + center.z), currentRoom);
        Add(new Vector3(-halfWidth + center.x, 0.0f, halfHeight + center.z), currentRoom);
        Add(new Vector3(halfWidth + center.x, 0.0f, halfHeight + center.z), currentRoom);
        Add(new Vector3(halfWidth + center.x, 0.0f, -halfHeight + center.z), currentRoom);
    }

    Vector3 getSnapPosition(Vector3 pos)
    {
        //transform to camera local pos
        Vector3 localPos = grid2DTransform.InverseTransformVector(pos);
        float localX = localPos.x;
        float localZ = localPos.z;

        Bounds bounds = grid2DTransform.gameObject.GetComponent<MeshRenderer>().bounds;

        float width = bounds.max.x - bounds.min.x;
        float halfWidth = width * 0.5f;
        float height = bounds.max.z - bounds.min.z;
        float halfHeight = height * 0.5f;
        float halfStep = gridStep * 0.5f;

        float newX = 0.0f;

        for (float x = -halfWidth; x <= halfWidth; x += gridStep)
        {
            if (Mathf.Abs(localX - x) <= halfStep)
            {
                newX = x;
                break;
            }
        }

        float newZ = 0.0f;

        for (float z = -halfHeight; z <= halfHeight; z += gridStep)
        {
            if (Mathf.Abs(localZ - z) <= halfStep)
            {
                newZ = z;
                break;
            }
        }


        //float newX = Mathf.Round((localX - halfWidth) / gridStep)*gridStep + halfWidth;
        //float newY = Mathf.Round((localY - halfHeight) / gridStep)*gridStep + halfHeight;

        //float newX = Mathf.Round((x) / gridStep) * gridStep ;
        //float newY = Mathf.Round((y ) / gridStep) * gridStep ;

        return grid2DTransform.TransformVector(new Vector3(newX,  localPos.y, newZ));
    }

    public void OnDrawButtonClicked()
    {
        isDrawMode = true;
    }

    public void Extrude(float height)
    {
        int counter = 0;        

        foreach(GameObject polygonObj in polygonList)
        {
            GameObject sidePolygon = GameObject.Instantiate(originalPolygon);

            List<Vector3> baseVertices = new List<Vector3>();
            List<Vector3> topVertices = new List<Vector3>();

            Mesh polygonMesh = polygonObj.GetComponent<MeshFilter>().mesh;

            RoomBehaviour roomBehaviour = roomList[counter];

            foreach (GameObject posObj in roomBehaviour.PositionList)
            {
                Vector3 v = posObj.transform.position;
                baseVertices.Add(v);
                Vector3 topVertex = new Vector3(v.x, v.y + height, v.z);

                topVertices.Add(topVertex);
            }

            List<Vector3> totalVertices = new List<Vector3>();

            baseVertices.Reverse();
            totalVertices.AddRange(baseVertices);
            topVertices.Reverse();
            totalVertices.AddRange(topVertices);

            int halfStartIndex = baseVertices.Count;

            List<int> indices = new List<int>();

            //side
            for(int i=0;i<halfStartIndex-1;i++)
            {
                indices.Add(i);                
                indices.Add(i+1);
                indices.Add(i + halfStartIndex + 1);
                indices.Add(i);
                indices.Add(i + halfStartIndex + 1);
                indices.Add(i + halfStartIndex);
            }

            int lastIndex = halfStartIndex - 1;

            if (lastIndex < 1)
                continue;

            //마지막 점 추가
            indices.Add(lastIndex);
            indices.Add(0);
            indices.Add(halfStartIndex);
            indices.Add(lastIndex);
            indices.Add(halfStartIndex);
            indices.Add(lastIndex + halfStartIndex);

            Mesh sideMesh = new Mesh();

            sideMesh.vertices = totalVertices.ToArray();
            sideMesh.triangles = indices.ToArray();
            sideMesh.RecalculateNormals();

            sidePolygon.GetComponent<MeshFilter>().mesh = sideMesh;            

            sidePolygon.name = "side_" + polygonObj.name;

            GameObject top = GameObject.Instantiate(polygonObj);
            top.name = "top_"+ polygonObj.name;
            Vector3 currentPos = top.transform.position;
            top.transform.position = new Vector3(currentPos.x, currentPos.y+ height, currentPos.z);

            sidePolygon.transform.parent = polygonObj.transform;
            top.transform.parent = polygonObj.transform;

            counter++;
        }
    }

    Dictionary<int, List<GameObject>> floorGroup = new Dictionary<int, List<GameObject>>();

    void CreateFloorGroup()
    {
        foreach(Transform t in nodeRoot.transform)
        {
            Vector3 center = t.position;

            int elevation = Mathf.RoundToInt(center.y);            

            if(!floorGroup.ContainsKey(elevation))
            {
                floorGroup.Add(elevation, new List<GameObject>());                
            }

            floorGroup[elevation].Add(t.gameObject);
        }             
    }

    bool isNodePlaceInSameFloor(GameObject node1,GameObject node2)
    {
        int elevation1 = Mathf.RoundToInt(node1.transform.position.y);
        int elevation2 = Mathf.RoundToInt(node2.transform.position.y);

        return elevation1 == elevation2;
    }

    void ProcessRoomObjects()
    {

    }

    bool isFloorVisible = true;

    private void OnGUI()
    {
        int counter = 0;

        foreach(KeyValuePair<int,List<GameObject>> kv in floorGroup.OrderBy(i => i.Key))
        {
            if (GUI.Button(new Rect(Screen.width - 120, 200 + counter * 30, 120, 30), "Floor:" + kv.Key))
            {
                foreach(GameObject obj in kv.Value)
                {
                    obj.SetActive(!obj.activeSelf);
                }
            }

            counter++;
        }

        if (GUI.Button(new Rect(Screen.width - 120, 170, 120, 30), "All"))
        {
            isFloorVisible = !isFloorVisible;

            foreach (KeyValuePair<int, List<GameObject>> kv in floorGroup)
            {
                foreach (GameObject obj in kv.Value)
                {
                    obj.SetActive(isFloorVisible);
                }
            }
        }
    }

    void CreateNodes(GameObject root)
    {
        foreach(Transform t in root.transform)
        {
            GameObject child = t.gameObject;

            Vector3 centerPos = Util.GetCenterPos(child);

            GameObject nodeChild = GameObject.Instantiate(originalNode);

            nodeChild.transform.position = centerPos;
            nodeChild.name = "node_" + t.gameObject.name;
            nodeChild.layer = LayerMask.NameToLayer("Node");

            nodeChild.transform.parent = nodeRoot.transform;

            nodeChild.GetComponent<NodeBehaviour>().ParentObject = t.gameObject;
        }
    }

    void ProcessFloorObjects()
    {
        GameObject[] floorRoots = GameObject.FindGameObjectsWithTag("Floor Root");

        if(null != floorRoots)
        {
            foreach(GameObject floorRoot in floorRoots)
            {
                if (null != floorRoot)
                {
                    foreach (Transform t in floorRoot.transform)
                    {
                        t.gameObject.tag = "Floor";
                        SphereCollider collider = t.gameObject.AddComponent<SphereCollider>();

                        collider.isTrigger = true;
                    }
                }

                CreateNodes(floorRoot);
            }
        }        
    }

    void ProcessDoorObjectsSingleSide()
    {
        //GameObject doorRoot = GameObject.Find("Door Root");        
        GameObject[] doorRoots = GameObject.FindGameObjectsWithTag("Door Root");

        int counter = 0;

        if (null != doorRoots)
        {
            foreach(GameObject doorRoot in doorRoots)
            {
                foreach (Transform t in doorRoot.transform)
                {
                    GameObject obj = t.gameObject;

                    Mesh frontMesh = obj.GetComponent<MeshFilter>().mesh;

                    Mesh mesh = Util.CreateDoorBase(frontMesh, 20f);
                    List<Vector3> outline = Util.GetDoorOutline(frontMesh, 20f);

                    outline.Add(outline[0]); //close 해준다.

                    GameObject doorBase = GameObject.Instantiate(originalPolygon);

                    doorBase.GetComponent<MeshFilter>().mesh = mesh;

                    doorBase.name = "Door_Base_" + counter;

                    doorBase.transform.parent = doorBaseRoot.transform;
                    doorBase.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    doorBase.tag = "DoorBase";
                    doorBase.layer = 10;
                    doorBase.AddComponent<SphereCollider>();
                    DoorBaseBehaviour doorBaseBehaviour = doorBase.AddComponent<DoorBaseBehaviour>();
                    doorBaseBehaviour.Outline = outline;
                    Rigidbody rigidbody = doorBase.AddComponent<Rigidbody>();
                    rigidbody.useGravity = false;
                    rigidbody.isKinematic = true;

                    counter++;
                }
            }            
        }
    }

    void ProcessDoorObjects()
    {
        GameObject[] doorRoots = GameObject.FindGameObjectsWithTag("Door Root");

        int counter = 0;

        if (null != doorRoots)
        {
            Dictionary<string, GameObject> nameMap = new Dictionary<string, GameObject>();

            int createCounter = 0;

            foreach(GameObject doorRoot in doorRoots)
            {
                if (null != doorRoot)
                {
                    foreach (Transform t in doorRoot.transform)
                    {
                        GameObject obj = t.gameObject;

                        if (nameMap.ContainsKey(obj.name))
                        {
                            GameObject firstDoorFace = nameMap[obj.name];
                            GameObject secondDoorFace = obj;

                            Mesh frontMesh = firstDoorFace.GetComponent<MeshFilter>().mesh;
                            Mesh backMesh = secondDoorFace.GetComponent<MeshFilter>().mesh;

                            Mesh mesh = Util.CreateDoorBase(frontMesh, backMesh);
                            List<Vector3> outline = Util.GetDoorOutline(frontMesh, backMesh);

                            GameObject doorBase = GameObject.Instantiate(originalPolygon);

                            doorBase.GetComponent<MeshFilter>().mesh = mesh;

                            doorBase.name = "Door_Base_" + createCounter;

                            doorBase.transform.parent = doorBaseRoot.transform;
                            doorBase.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                            doorBase.tag = "DoorBase";
                            doorBase.layer = 10;
                            doorBase.AddComponent<SphereCollider>();
                            DoorBaseBehaviour doorBaseBehaviour = doorBase.AddComponent<DoorBaseBehaviour>();
                            doorBaseBehaviour.Outline = outline;
                            Rigidbody rigidbody = doorBase.AddComponent<Rigidbody>();
                            rigidbody.useGravity = false;
                            rigidbody.isKinematic = true;

                            createCounter++;
                        }
                        else
                        {
                            nameMap.Add(obj.name, obj);
                        }
                    }                    
                }
            }

            if (0 == createCounter)
            {
                ProcessDoorObjectsSingleSide();
            }
        }           
    }

    public void ToggleBuilding(UnityEngine.UI.Toggle toggle)
    {
        buildingRoot.SetActive(toggle.isOn);
    }

    Building globalBuilding = null;

    public Building GlobalBuilding
    {
        get
        {
            return globalBuilding;
        }

        set
        {
            globalBuilding = value;
        }
    }

    public bool ShowLine
    {
        get
        {
            return showLine;
        }

        set
        {
            showLine = value;
        }
    }

    [SerializeField]
    int maxExportObjectNumber = 10000;

    public void SaveIndoorGML()
    {
        UniFileBrowser.use.SaveFileWindow(Export);
    }

    public void Export(string fileName)
    {
        //string rootPath = @"C:\Users\apple\Desktop\Data\";

        

        if (maxExportObjectNumber < 0)
            maxExportObjectNumber = int.MaxValue;

        Building building = CreateBuildingData();
        building.ExportIndoorGml(fileName, maxExportObjectNumber);        
    }

    public void CreateTopology()
    {
        CreateConnectionData();
    }

    void CreateConnectionData()
    {
        foreach(Transform t in doorBaseRoot.transform)
        {
            GameObject doorBase = t.gameObject;

            DoorBaseBehaviour doorBaseBehaviour = doorBase.GetComponent<DoorBaseBehaviour>();

            if(null != doorBaseBehaviour)
            {
                GameObject nodeObject = GetNode(doorBase);

                foreach (GameObject floorObj in doorBaseBehaviour.NeighborFloorList)
                {
                    GameObject targetNodeObject =  GetNode(floorObj);

                    if(null != targetNodeObject && isNodePlaceInSameFloor(nodeObject,targetNodeObject))
                    {
                        //라인 생성
                        GameObject cloneLine = GameObject.Instantiate(laserLine);

                        LineRenderer lineRenderer = cloneLine.GetComponent<LineRenderer>();

                        lineRenderer.positionCount = 2;
                        lineRenderer.SetPosition(0, nodeObject.transform.position);
                        lineRenderer.SetPosition(1, targetNodeObject.transform.position);

                        cloneLine.transform.parent = nodeObject.transform;

                        //노드끼리 연결
                        nodeObject.GetComponent<NodeBehaviour>().LinkedNodeList.Add(targetNodeObject);
                        nodeObject.GetComponent<NodeBehaviour>().NodeLinkList.Add(cloneLine);
                        targetNodeObject.GetComponent<NodeBehaviour>().LinkedNodeList.Add(nodeObject);
                        targetNodeObject.GetComponent<NodeBehaviour>().NodeLinkList.Add(cloneLine);

                        //링크에 노드 정보 입력
                        NodeLinkBehaviour nodeLinkBehaviour = cloneLine.GetComponent<NodeLinkBehaviour>();

                        nodeLinkBehaviour.StartNode = nodeObject;
                        nodeLinkBehaviour.EndNode = targetNodeObject;
                    }
                }
            }
        }
    }

    GameObject GetNode(GameObject parentObject)
    {
        foreach(Transform t in nodeRoot.transform)
        {
            NodeBehaviour nodeBehaviour = t.gameObject.GetComponent<NodeBehaviour>();

            if(null != nodeBehaviour)
            {
                if (nodeBehaviour.ParentObject == parentObject)
                    return t.gameObject;
            }
        }

        return null;
    }

    void NodeLinkButtonPressed(GameObject nodeLink)
    {
        if(null != selectedLink)
            selectedLink.GetComponent<LineRenderer>().material = nodeLinkMaterial;

        selectedLink = nodeLink;
        selectedLink.GetComponent<LineRenderer>().material = selecteNodeLinkMaterial;
    }

    private Building CreateBuildingData()
    {
        Building building = new Building();

        foreach (Transform t in nodeRoot.transform)
        {
            NodeBehaviour nodeBehaviour = t.gameObject.GetComponent<NodeBehaviour>();

            if (nodeBehaviour != null && nodeBehaviour.ParentObject != null)
            {
                GameObject parent = nodeBehaviour.ParentObject;
                List<Vector3> outline = null;

                Node node = new Node();
                Vector3 nodePos = t.gameObject.transform.position;

                node.name = t.gameObject.name;

                node.position[0] = nodePos.x * 100.0f;
                node.position[1] = nodePos.y * 100.0f;
                node.position[2] = nodePos.z * 100.0f;

                node.linkedNodes = nodeBehaviour.GetLinkedNodeNameList();

                switch (parent.tag)
                {
                    case "Floor":
                        Floor floor = new Floor();
                        floor.name = parent.name;
                        outline = parent.GetComponent<SurfaceBehaviour>().Outline;
                        floor.height = parent.GetComponent<SurfaceBehaviour>().Height * 100.0f;
                        floor.outline = Util.Vector3ListToArray(outline);
                        floor.node = node;
                        building.floorList.Add(floor);
                        break;
                    case "DoorBase":
                        DoorBase doorBase = new DoorBase();
                        doorBase.name = parent.name;
                        outline = parent.GetComponent<DoorBaseBehaviour>().Outline;
                        doorBase.outline = Util.Vector3ListToArray(outline);
                        doorBase.node = node;
                        building.doorBaseList.Add(doorBase);
                        break;
                }
            }
        }

        return building;
    }

    bool isDeleteNodeSelected = false;

    public void ButtonDeleteNodePressed()
    {
        if(null != selectedNode)
        {
            isDeleteNodeSelected = true;
            panelConfirm.SetActive(true);
        }
    }

    bool isDeleteLinkSelected = false;

    public void ButtonDeleteLinkPressed()
    {
        if(null != selectedLink)
        {
            isDeleteLinkSelected = true;
            panelConfirm.SetActive(true);
        }
    }

    

    public void RestoreNodeLink()
    {
        moveGizmo.SetActive(false);
        clearNodeLinkListbox();
        topologyPanel.SetActive(false);

        selectedNode = null;

        if (null != selectedLink)
            selectedLink.GetComponent<LineRenderer>().material = nodeLinkMaterial;

        selectedLink = null;
    }

    public void ConfirmPressed(bool yesOrNo)
    {
        if(yesOrNo)
        {
            //reply yes.
            if(isDeleteLinkSelected)
            {
                DeleteLink(selectedLink);
                selectedLink = null;

                //RestoreNodeLink();
                clearNodeLinkListbox();
                CreateLinkButton(selectedNode.GetComponent<NodeBehaviour>());

                isDeleteLinkSelected = false;
            }
            else if(isDeleteNodeSelected)
            {
                //gizmo를 우선 분리한다. (안그러면 노드를 제거할때 같이 삭제됨)
                moveGizmo.transform.parent = null;

                DeleteNode(selectedNode);
                selectedNode = null;

                RestoreNodeLink();

                isDeleteNodeSelected = false;
            }

            panelConfirm.SetActive(false);
        }
        else
        {
            //no
            panelConfirm.SetActive(false);
        }
    }

    void DeleteLink(GameObject link)
    {
        NodeLinkBehaviour nodeLinkBehaviour = link.GetComponent<NodeLinkBehaviour>();

        nodeLinkBehaviour.Clear();

        GameObject.Destroy(link);
    }

    public void LoadCityGml(string fileName)
    {
        UniFileBrowser.use.OpenFileWindow(OpenFile);        
    }

    public void OnButtonCheckIndoorGml()
    {
        UniFileBrowser.use.OpenFileWindow(CheckIndoorGml);
    }

    void ExecuteCommand(string command)
    {
        textResult.transform.parent.gameObject.SetActive(true);

        string result = "";

        var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/c " + command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        var process = System.Diagnostics.Process.Start(processInfo);

        process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
            result += "\n" + e.Data;
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
            Debug.Log("error>>" + e.Data);
        process.BeginErrorReadLine();

        process.WaitForExit();

        Debug.Log(string.Format("ExitCode: {0}", process.ExitCode));
        process.Close();

        textResult.text = result;
    }

    void CheckIndoorGml(string pathToFile)
    {
        string gmlReportPath = System.IO.Path.GetDirectoryName(pathToFile) + "\\report";

        if(!System.IO.Directory.Exists(gmlReportPath))
            System.IO.Directory.CreateDirectory(gmlReportPath);

        string runString = string.Format(Application.streamingAssetsPath + "\\val3dity.exe \"{0}\" -r \"{1}\"", pathToFile, gmlReportPath);

        //배치파일 작성
        System.IO.File.WriteAllText(Application.streamingAssetsPath + "\\run.bat", runString);

        String command = Application.streamingAssetsPath + "\\run.bat";

        //System.Diagnostics.ProcessStartInfo processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", command);
        //try
        //{
        //    System.Diagnostics.Process.Start(processInfo);
        //}
        //catch(Exception e)
        //{
        //    Debug.Log(e.Message);
        //}

        ExecuteCommand(command);
        ExecuteCommand(gmlReportPath + "\\report.html");

        //System.Diagnostics.Process.Start(Application.streamingAssetsPath + "\\run.bat");
    }    

    void OpenFile(string pathToFile)
    {
        StartCoroutine(LoadCityGml(pathToFile, originalWallMeshObject, elementFilters));
    }

    void DeleteNode(GameObject node)
    {
        NodeBehaviour nodeBehaviour = node.GetComponent<NodeBehaviour>();
        nodeBehaviour.ClearAllData();

        GameObject.Destroy(node);
    }

    void AddFloorHeight()
    {
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag("Floor Root");

        foreach(GameObject floor in floorObjects)
        {
            Bounds roomBounds = GetRoomNodeBounds(floor.transform.parent.gameObject);

            foreach(Transform t in floor.transform)
            {
                t.gameObject.GetComponent<SurfaceBehaviour>().Height = (roomBounds.max.y - roomBounds.min.y);
            }
            
        }
    }

    Bounds GetRoomNodeBounds(GameObject roomRoot)
    {
        Bounds totalBounds = new Bounds();

        bool isFirst = true;

        foreach(Transform element in roomRoot.transform)
        {
            foreach(Transform t in element)
            {
                if(isFirst)
                {
                    totalBounds = GetBounds(t.gameObject);

                    isFirst = false;
                }
                else
                {
                    totalBounds.Encapsulate(GetBounds(t.gameObject));
                }
            }
        }

        return totalBounds;
    }

    Bounds GetBounds(GameObject obj)
    {
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();

        if(null != meshRenderer)
        {
            return meshRenderer.bounds;
        }

        return new Bounds();
    }
}
