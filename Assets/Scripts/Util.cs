using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using ClipperLib;

namespace Assets.Scripts
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public class Util
    {
        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

        public static Vector3 GetCenterPos(GameObject meshObject)
        {
            MeshRenderer renderer =  meshObject.GetComponent<MeshRenderer>();

            if(null != renderer)
            {
                return renderer.bounds.center;
            }

            return meshObject.transform.localPosition;
        }

        public static Mesh CreateDoorBase(Mesh front,float depth)
        {
            List<Vector3> outline = GetDoorOutline(front, depth);

            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

            poly.outside = outline;

            Mesh mesh = Poly2Mesh.CreateMesh(poly);

            if (mesh != null && mesh.triangles.Length > 2)
            {
                if (isCcw(mesh))
                {
                    int[] triangles = (int[])mesh.triangles.Clone();
                    Array.Reverse(triangles);
                    mesh.triangles = triangles;
                }
            }

            return mesh;
        }

        public static Mesh CreateDoorBase(Mesh front,Mesh back)
        {
            List<Vector3> outline = GetDoorOutline(front, back);

            Poly2Mesh.Polygon poly = new Poly2Mesh.Polygon();

            poly.outside = outline;

            Mesh mesh = Poly2Mesh.CreateMesh(poly);

            if (mesh != null && mesh.triangles.Length > 2)
            {
                if (isCcw(mesh))
                {
                    int[] triangles = (int[])mesh.triangles.Clone();
                    Array.Reverse(triangles);
                    mesh.triangles = triangles;
                }
            }

            return mesh;
        }

        public static List<Vector3> GetDoorOutline(Mesh front, float depth)
        {
            Vector3 v1 = front.vertices[front.GetIndices(0)[0]];
            Vector3 v2 = front.vertices[front.GetIndices(0)[1]];
            Vector3 v3 = front.vertices[front.GetIndices(0)[2]];

            Vector3 side1 = v2 - v1;
            Vector3 side2 = v3 - v1;

            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            List<Vector3> frontLine = getBaseLine(front);
            List<Vector3> backLine = new List<Vector3>();

            backLine.Add(frontLine[1] + normal * depth);
            backLine.Add(frontLine[0] + normal * depth);

            Vector3 frontDir = (frontLine[1] - frontLine[0]).normalized;
            Vector3 backDir = (backLine[1] - backLine[0]).normalized;

            List<Vector3> outline = new List<Vector3>();

            if ((frontDir + backDir).magnitude >= 0.0001f) // 방향이 같으면 뒤집는다.
            {
                backLine.Reverse();
            }

            outline.AddRange(frontLine);
            outline.AddRange(backLine);

            if(Clipper.Orientation(ConvertVector3ToPath(outline)))
            {
                outline.Reverse();
            }

            return outline;
        }

        static Path ConvertVector3ToPath(List<Vector3> outline)
        {
            Path path = new Path();

            foreach(Vector3 v in outline)
            {
                path.Add(new IntPoint(v.x, v.z));
            }

            return path;
        }

        public static List<Vector3> GetDoorOutline(Mesh front, Mesh back)
        {
            List<Vector3> frontLine = getBaseLine(front);
            List<Vector3> backLine = getBaseLine(back);

            Vector3 frontDir = (frontLine[1] - frontLine[0]).normalized;
            Vector3 backDir = (backLine[1] - backLine[0]).normalized;

            List<Vector3> outline = new List<Vector3>();

            if ((frontDir + backDir).magnitude >= 0.0001f) // 방향이 같으면 뒤집는다.
            {
                backLine.Reverse();
            }

            outline.AddRange(frontLine);
            outline.AddRange(backLine);
            return outline;
        }

        public static bool isCcw(Mesh mesh)
        {
            if (mesh.triangles.Length < 3)
                return false;

            Vector3 v1 = mesh.vertices[mesh.triangles[0]];
            Vector3 v2 = mesh.vertices[mesh.triangles[1]];
            Vector3 v3 = mesh.vertices[mesh.triangles[2]];

            Vector3 axisX = (v1 - v2).normalized;
            Vector3 axisY = (v3 - v2).normalized;
            Vector3 axisZ = Vector3.Cross(axisX, axisY).normalized;

            if(axisZ.magnitude == 0.0)
            {
                Debug.Log(axisZ);
            }

            Quaternion q = Quaternion.LookRotation(axisZ);

            q = Quaternion.Inverse(q);

            Vector3 normal = q * axisZ;

            if (normal.z < 0.0f)
                return true;

            return false;
        }

        private static List<Vector3> getBaseLine(Mesh front)
        {
            //모든 vertex를 바닥 vertex로 생성
            List<Vector3> baseVertex = new List<Vector3>();

            float maxX = float.NegativeInfinity;
            float minX = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            float minY = float.PositiveInfinity;

            float minElevation = float.PositiveInfinity;

            foreach (Vector3 v in front.vertices)
            {
                maxX = Mathf.Max(v.x, maxX);
                maxY = Mathf.Max(v.z, maxY);
                minX = Mathf.Min(v.x, minX);
                minY = Mathf.Min(v.z, minY);
                minElevation = Mathf.Min(v.y, minElevation);

                baseVertex.Add(new Vector2(v.x, v.z));
            }

            Vector2 startPos = new Vector2(maxX, maxY);
            Vector2 endPos = new Vector2(minX, minY);

            bool startPosExist = false;

            foreach (Vector2 v in baseVertex)
            {
                if (v.Equals(startPos))
                {
                    startPosExist = true;
                    break;
                }
            }

            if (!startPosExist)
            {
                startPos = new Vector3(maxX, minY);
                endPos = new Vector3(minX, maxY);
            }

            List<Vector3> line = new List<Vector3>();

            line.Add(new Vector3(startPos.x,minElevation,startPos.y));
            line.Add(new Vector3(endPos.x,minElevation,endPos.y));

            return line;
        }

        public static Paths CreateFlatPolygon(Mesh mesh)
        {
            Paths paths = new Paths();

            //extract triangles



            //flatten


            return paths;
        }

        public static List<GameObject> FindGameObjects(string name)
        {
            List<GameObject> gameObjects = new List<GameObject>();

            var objects = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == name);

            foreach (GameObject obj in objects)
                gameObjects.Add(obj);

            return gameObjects;
        }

        public static float [] Vector3ListToArray(List<Vector3> vertexList)
        {
            if (0 == vertexList.Count)
                return null;

            float[] vertexArray = new float[vertexList.Count * 3];

            for(int i=0;i< vertexList.Count;i++)
            {
                Vector3 v = vertexList[i];
                vertexArray[i * 3] = v.x;
                vertexArray[i * 3+1] = v.y;
                vertexArray[i * 3+2] = v.z;
            }

            return vertexArray;
        }
    }
}
