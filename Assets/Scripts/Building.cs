using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Assets.Scripts
{
    [Serializable()]
    public class Building
    {
        public List<Floor> floorList = new List<Floor>();
        public List<DoorBase> doorBaseList = new List<DoorBase>();

        string xmlnsUri = "http://www.w3.org/2000/xmlns/";
        string coreUri = "http://www.opengis.net/indoorgml/1.0/core";
        string gmlUri = "http://www.opengis.net/gml/3.2";
        string xlinkUri = "http://www.w3.org/1999/xlink";
        string xsiUri = "http://www.w3.org/2001/XMLSchema-instance";
        string schemaLocationUri = "http://www.opengis.net/indoorgml/1.0/core/indoorgmlcore.xsd";
        string gmlIdUri = "http://www.gml.com/test"; // fake

        Dictionary<string, Node> nodeMap = new Dictionary<string, Node>();

        public void SaveXml(string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Building));
            using (StreamWriter wr = new StreamWriter(fileName))
            {
                xmlSerializer.Serialize(wr, this);
            }
        }

        public Building LoadXml(string fileName)
        {
            Building b;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Building));
            using (StreamReader rdr = new StreamReader(fileName))
            {
                b = (Building)xmlSerializer.Deserialize(rdr);
            }

            return b;
        }

        public void ExportIndoorGml(string fileName,int maxObjectNum = int.MaxValue)
        {
            //node map을 만든다.
            nodeMap.Clear();

            foreach(Floor floor in floorList)
            {
                nodeMap.Add(floor.node.name, floor.node);
            }

            foreach(DoorBase doorBase in doorBaseList)
            {
                nodeMap.Add(doorBase.node.name, doorBase.node);
            }

            XmlDocument doc = new XmlDocument();


            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);

            doc.AppendChild(docNode);

            XmlElement indoorFeatures = doc.CreateElement("IndoorFeatures");



            //indoorFeatures.SetAttribute("xmlns", "http://www.opengis.net/indoorgml/1.0/core");

            AppendNamespace(doc, indoorFeatures, "xmlns", "core", xmlnsUri, coreUri);
            AppendNamespace(doc, indoorFeatures, "xmlns", "gml", xmlnsUri, gmlUri);
            AppendNamespace(doc, indoorFeatures, "xmlns", "xsi", xmlnsUri, xsiUri);
            AppendNamespace(doc, indoorFeatures, "xmlns", "xlink", xmlnsUri, xlinkUri);
            AppendNamespace(doc, indoorFeatures, "xsi", "schemaLocation", xsiUri, schemaLocationUri);
            AppendNamespace(doc, indoorFeatures, "gml", "id", gmlUri, gmlIdUri);

            doc.AppendChild(indoorFeatures);

            XmlElement primalSpaceFeatures = doc.CreateElement("primalSpaceFeatures",coreUri);            

            //primalSpaceFeatures.Prefix = "core";
            indoorFeatures.AppendChild(primalSpaceFeatures);

            XmlElement primalSpaceFeatures2 = doc.CreateElement("PrimalSpaceFeatures", coreUri);
            //primalSpaceFeatures2.Prefix = "core";
            primalSpaceFeatures2.SetAttribute("id",gmlUri, "PS1");

            primalSpaceFeatures.AppendChild(primalSpaceFeatures2);

            int counter = 0;
            //int maxObjectNum = 2;

            foreach(Floor floor in floorList)
            {
                if (maxObjectNum == counter)
                    break;

                counter++;

                processFloorSurface(doc, primalSpaceFeatures2, floor);
            }

            counter = 0;

            foreach (DoorBase doorBase in doorBaseList)
            {
                processDoorBaseSurface(doc, primalSpaceFeatures2, doorBase);
            }

            XmlElement multiLayeredGraph = doc.CreateElement("multiLayeredGraph",coreUri);
            //multiLayeredGraph.Prefix = "core";
            indoorFeatures.AppendChild(multiLayeredGraph);

            XmlElement multiLayeredGraph2 = doc.CreateElement("MultiLayeredGraph", coreUri); //대문자!
            //multiLayeredGraph2.Prefix = "core";
            multiLayeredGraph2.SetAttribute("id", gmlUri, "MG1");

            multiLayeredGraph.AppendChild(multiLayeredGraph2);

            XmlElement spaceLayers = doc.CreateElement("spaceLayers", coreUri);
            //spaceLayers.Prefix = "core";
            spaceLayers.SetAttribute("id", gmlUri, "SL1");

            multiLayeredGraph2.AppendChild(spaceLayers);
            XmlElement spaceLayerMember = doc.CreateElement("spaceLayerMember", coreUri);
            //spaceLayerMember.Prefix = "core";
            spaceLayers.AppendChild(spaceLayerMember);

            XmlElement spaceLayer = doc.CreateElement("SpaceLayer", coreUri);
            //spaceLayer.Prefix = "core";
            spaceLayer.SetAttribute("id", gmlUri, "IS1");
            spaceLayerMember.AppendChild(spaceLayer);

            XmlElement nodes = doc.CreateElement("nodes", coreUri);
            //nodes.Prefix = "core";
            nodes.SetAttribute("id", gmlUri, "N1");
            spaceLayer.AppendChild(nodes);

            

            foreach (Floor floor in floorList)
            {
                if (maxObjectNum == counter)
                    break;

                counter++;

                processNodes(doc, nodes, floor);                
            }

            counter = 0;

            foreach (DoorBase doorBase in doorBaseList)
            {
                if (maxObjectNum == counter)
                    break;

                processNodes(doc, nodes, doorBase);

                counter++;
            }

            counter = 0;

            XmlElement edges = doc.CreateElement("edges");
            edges.SetAttribute("id", gmlUri, "E1");
            spaceLayer.AppendChild(edges);

            foreach (Floor floor in floorList)
            {
                if (maxObjectNum == counter)
                    break;

                counter++;

                processEdges(doc, edges, floor);

            }

            counter = 0;

            foreach (DoorBase door in doorBaseList)
            {
                if (maxObjectNum == counter)
                    break;

                counter++;

                processEdges(doc, edges, door);

            }

            doc.Save(fileName);
        }

        int pointCounter = 0;
        int lineCounter = 0;

        private void processEdges(XmlDocument doc, XmlElement edges, string id,Node node)        
        {
            if (null != node.linkedNodes)
            {
                XmlElement transitionMember = doc.CreateElement("transitionMember", coreUri);
                //transitionMember.Prefix = "core";
                edges.AppendChild(transitionMember);

                foreach (string linkedNodeName in node.linkedNodes)
                {
                    XmlElement transition = doc.CreateElement("Transition", coreUri);
                    //transition.Prefix = "core";
                    transitionMember.AppendChild(transition);

                    string transitionName = transitionMap[node.name + linkedNodeName];
                    transition.SetAttribute("id", gmlUri, transitionName);

                    XmlElement weight = doc.CreateElement("weight");

                    weight.InnerText = "1";
                    transition.AppendChild(weight);

                    XmlElement connects1 = doc.CreateElement("connects", coreUri);
                    //connects1.Prefix = "core";
                    connects1.SetAttribute("href", xlinkUri, "state_" + node.name);
                    transition.AppendChild(connects1);

                    XmlElement connects2 = doc.CreateElement("connects", coreUri);
                    //connects2.Prefix = "core";
                    connects2.SetAttribute("href", xlinkUri, "state_" + linkedNodeName);
                    transition.AppendChild(connects2);

                    XmlElement geometry = doc.CreateElement("geometry", coreUri);
                    //geometry.Prefix = "core";
                    transition.AppendChild(geometry);

                    XmlElement gmlLineString = doc.CreateElement("LineString", gmlUri);
                    gmlLineString.Prefix = "gml";
                    gmlLineString.SetAttribute("id", gmlUri, "L" + lineCounter);
                    lineCounter++;
                    geometry.AppendChild(gmlLineString);

                    addPosElement(doc, node.position, gmlLineString);
                    addPosElement(doc, nodeMap[linkedNodeName].position, gmlLineString);
                }
            }
        }

        private void processEdges(XmlDocument doc, XmlElement edges, DoorBase door)
        {
            processEdges(doc, edges, door.name, door.node);
        }

        private void processEdges(XmlDocument doc,XmlElement edges,Floor floor)
        {
            processEdges(doc, edges,floor.name, floor.node);                     
        }

        private void addPosElement(XmlDocument doc, float [] position, XmlElement parent)
        {
            XmlElement pos = doc.CreateElement("pos", gmlUri);
            pos.Prefix = "gml";
            pos.InnerText = string.Format("{0} {1} {2}", position[0], position[2], position[1]);
            parent.AppendChild(pos);
        }

        Dictionary<string, string> transitionMap = new Dictionary<string, string>();

        int transitionCounter = 0;

        private void processNodes(XmlDocument doc, XmlElement nodes,string id,Node node)
        {
            XmlElement stateMember = doc.CreateElement("stateMember", coreUri);
            //stateMember.Prefix = "core";
            nodes.AppendChild(stateMember);

            XmlElement state = doc.CreateElement("State", coreUri);
            //state.Prefix = "core";
            state.SetAttribute("id", gmlUri, "state_" + node.name);
            stateMember.AppendChild(state);

            XmlElement gmlName = doc.CreateElement("name", gmlUri);
            gmlName.Prefix = "gml";
            gmlName.InnerText = id;

            state.AppendChild(gmlName);

            XmlElement duality = doc.CreateElement("duality", coreUri);
            //duality.Prefix = "core";
            duality.SetAttribute("href", xlinkUri, "cell_" + id);
            state.AppendChild(duality);

            if (null != node.linkedNodes)
            {
                foreach (string linkedNodeName in node.linkedNodes)
                {
                    XmlElement connects = doc.CreateElement("connects", coreUri);
                    //connects.Prefix = "core";
                    string transitionName = "T" + transitionCounter;
                    transitionMap.Add(node.name + linkedNodeName, transitionName);

                    transitionCounter++;
                    connects.SetAttribute("href", xlinkUri, transitionName);
                    state.AppendChild(connects);
                }
            }


            XmlElement geometry = doc.CreateElement("geometry", coreUri);
            //geometry.Prefix = "core";
            state.AppendChild(geometry);

            XmlElement gmlPoint = doc.CreateElement("Point", gmlUri);
            gmlPoint.Prefix = "gml";
            gmlPoint.SetAttribute("id", "P" + pointCounter);
            pointCounter++;

            geometry.AppendChild(gmlPoint);

            XmlElement pos = doc.CreateElement("pos", gmlUri);
            pos.Prefix = "gml";
            pos.InnerText = string.Format("{0} {1} {2}", node.position[0], node.position[2], node.position[1]);
            gmlPoint.AppendChild(pos);
        }

        private void processNodes(XmlDocument doc, XmlElement nodes, DoorBase door)
        {
            processNodes(doc, nodes, door.name, door.node);
        }

        private void processNodes(XmlDocument doc, XmlElement nodes, Floor floor)
        {
            processNodes(doc, nodes, floor.name, floor.node); 
        }

        private void processDoorBaseSurface(XmlDocument doc, XmlElement primalSpaceFeatures, DoorBase doorBase)
        {
            processSurface(doc, primalSpaceFeatures, doorBase.name, doorBase.outline, 200.0f);            
        }

        private void processSurface(XmlDocument doc, 
            XmlElement primalSpaceFeatures, string id,float [] outline,float height)
        {
            int polygonCounter = 0;
            XmlElement cellSpaceMember = doc.CreateElement("cellSpaceMember", coreUri);
            //cellSpaceMember.Prefix = "core";
            primalSpaceFeatures.AppendChild(cellSpaceMember);

            XmlElement cellSpace = doc.CreateElement("CellSpace", coreUri);
            //cellSpace.Prefix = "core";
            cellSpace.SetAttribute("id", gmlUri, "cell_" + id);
            cellSpaceMember.AppendChild(cellSpace);

            XmlElement cellSpaceGeometry = doc.CreateElement("CellSpaceGeometry", coreUri);
            //cellSpaceGeometry.Prefix = "core";
            cellSpace.AppendChild(cellSpaceGeometry);

            XmlElement geometry3D = doc.CreateElement("Geometry3D", coreUri);
            //geometry3D.Prefix = "core";
            cellSpaceGeometry.AppendChild(geometry3D);

            XmlElement gmlSolid = doc.CreateElement("Solid", gmlUri);
            gmlSolid.Prefix = "gml";
            gmlSolid.SetAttribute("id", gmlUri, "solid_" + id);
            geometry3D.AppendChild(gmlSolid);

            XmlElement gmlExterior = doc.CreateElement("exterior", gmlUri);
            gmlExterior.Prefix = "gml";
            gmlSolid.AppendChild(gmlExterior);

            XmlElement gmlShell = doc.CreateElement("Shell", gmlUri);
            gmlShell.Prefix = "gml";
            gmlExterior.AppendChild(gmlShell);

            List<List<float>> extrudeMesh = Extrude(outline, height, true);

            foreach (List<float> ring in extrudeMesh)
            {
                XmlElement gmlSurfaceMember = doc.CreateElement("surfaceMember", gmlUri);
                gmlSurfaceMember.Prefix = "gml";
                gmlShell.AppendChild(gmlSurfaceMember);

                XmlElement gmlPolygon = doc.CreateElement("Polygon", gmlUri);
                gmlPolygon.Prefix = "gml";
                gmlPolygon.SetAttribute("id", gmlUri, string.Format("polygon_{0}_{1}", polygonCounter, id));
                gmlSurfaceMember.AppendChild(gmlPolygon);

                XmlElement gmlExterior2 = doc.CreateElement("exterior", gmlUri);
                gmlExterior2.Prefix = "gml";
                gmlPolygon.AppendChild(gmlExterior2);

                XmlElement gmlLinearRing = doc.CreateElement("LinearRing", gmlUri);
                gmlLinearRing.Prefix = "gml";


                


                for (int i = 0; i < ring.Count; i += 3)
                {
                    XmlElement pos = doc.CreateElement("pos", gmlUri);
                    pos.Prefix = "gml";

                    pos.InnerText = string.Format("{0} {1} {2}", ring[i], ring[i + 2], ring[i+1]);

                    gmlLinearRing.AppendChild(pos);
                }

                gmlExterior2.AppendChild(gmlLinearRing);

                polygonCounter++;
            }
        }
        

        private void processFloorSurface(XmlDocument doc, XmlElement primalSpaceFeatures, Floor floor)
        {
            processSurface(doc, primalSpaceFeatures,floor.name, floor.outline, floor.height);
        }

        private void AppendNamespace(XmlDocument doc, XmlElement element, string prefix, string domain, string prefixUri, string domainUri)
        {
            XmlAttribute xmlnsAttr = doc.CreateAttribute(
            prefix, domain, prefixUri);
            string acpNamespace = domainUri;
            xmlnsAttr.Value = acpNamespace;

            element.Attributes.Append(xmlnsAttr);
        }

        List<List<float>> Extrude(float[] baseLine, float height,bool reverse = false)
        {
            float[] newBaseLine = null;

            if(reverse)
            {
                newBaseLine = new float[baseLine.Length];

                int counter = 0;

                for(int i=baseLine.Length-3;i > -1;i-=3)
                {
                    newBaseLine[counter] = baseLine[i];
                    newBaseLine[counter+1] = baseLine[i+1];
                    newBaseLine[counter+2] = baseLine[i+2];

                    counter += 3;
                }
            }
            else
            {
                newBaseLine = baseLine;
            }

            Dictionary<float, int> yPosMap = new Dictionary<float, int>();

            for (int i = 0; i < newBaseLine.Length; i += 3)
            {
                float z = newBaseLine[i + 1];

                if (yPosMap.ContainsKey(z))
                {
                    yPosMap[z]++;
                }
                else
                {
                    yPosMap.Add(z, 1);
                }
            }

            int maxPosNum = 0;
            float yPos = 0.0f;

            foreach (KeyValuePair<float, int> kv in yPosMap)
            {
                if (kv.Value > maxPosNum)
                {
                    yPos = kv.Key;
                    maxPosNum = kv.Value;
                }
            }


            List<List<float>> mesh = new List<List<float>>();

            int vertexCount = newBaseLine.Length / 3;

            List<float> baseVertices = new List<float>();
            //바닥
            for(int i= vertexCount-1; i > -1; i--)
            {
                baseVertices.Add(newBaseLine[i*3]);
                baseVertices.Add(yPos);
                baseVertices.Add(newBaseLine[i * 3 + 2]);
            }            

            mesh.Add(baseVertices);

            List<float> topVertices = new List<float>();
            //윗면
            for (int i = 0; i < vertexCount; i++)
            {
                topVertices.Add(newBaseLine[i*3]);
                topVertices.Add(yPos + height);
                topVertices.Add(newBaseLine[i*3+2]);
            }

            mesh.Add(topVertices);

            //옆면               
            for (int i = 0; i < vertexCount - 1; i++)
            {
                List<float> sideVertices = new List<float>();

                float x1 = newBaseLine[i * 3];
                float y1 = yPos;
                float z1 = newBaseLine[i * 3 + 2];

                sideVertices.Add(x1);
                sideVertices.Add(y1);
                sideVertices.Add(z1);

                float x2 = newBaseLine[i * 3 + 3];
                float y2 = yPos;
                float z2 = newBaseLine[i * 3 + 5];

                sideVertices.Add(x2);
                sideVertices.Add(y2);
                sideVertices.Add(z2);

                float x3 = x2;
                float y3 = y2 + height;
                float z3 = z2;

                sideVertices.Add(x3);
                sideVertices.Add(y3);
                sideVertices.Add(z3);

                float x4 = x1;
                float y4 = y1 + height;
                float z4 = z1;

                sideVertices.Add(x4);
                sideVertices.Add(y4);
                sideVertices.Add(z4);

                //close 하기 위해 첫번째 vertex를 추가

                sideVertices.Add(x1);
                sideVertices.Add(y1);
                sideVertices.Add(z1);

                mesh.Add(sideVertices);
            }

            return mesh;
        }
    }




    [Serializable()]
    public class Floor
    {
        public string name = "";
        public float[] outline = null;
        public Node node = null;
        public float height = 350.0f;
    }

    [Serializable()]
    public class DoorBase
    {
        public string name = "";
        public float[] outline = null;

        public Node node = null;
    }

    [Serializable()]
    public class Node
    {
        public string name = "";
        public float[] position = new float[3];
        public string[] linkedNodes = null;
    }
}
