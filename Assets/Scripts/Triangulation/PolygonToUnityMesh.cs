using PolygonCuttingEar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Resources.Scripts.Triangulation
{
    public class PolygonToUnityMesh
    {
        public static Mesh Convert(List<Vector3> outline)
        {
            if (outline.Count < 3)
                return null;

            float yValue = outline[0].y;

            GeometryUtility.CPoint2D [] vertices = new GeometryUtility.CPoint2D[outline.Count];

            int counter = 0;

            foreach(Vector3 pos in outline)
            {
                vertices[counter] = new GeometryUtility.CPoint2D(pos.x, pos.z);

                counter++;
            }

            CPolygonShape cutPolygon = new CPolygonShape(vertices);

            cutPolygon.CutEar();

            List<Vector3> vertexList = new List<Vector3>();
            List<int> indices = new List<int>();

            int vertexCounter = 0;

            for (int i = 0; i < cutPolygon.NumberOfPolygons; i++)
            {
                int nPoints = cutPolygon.Polygons(i).Length;

                Vector3[] tempArray = new Vector3[nPoints];

                for (int j = 0; j < nPoints; j++)
                {
                    tempArray[j].x = (float) cutPolygon.Polygons(i)[j].X;
                    tempArray[j].y = yValue;
                    tempArray[j].z = (float) cutPolygon.Polygons(i)[j].Y;
                    

                    indices.Add(vertexCounter);
                    vertexCounter++;
                }

                vertexList.AddRange(tempArray);
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertexList.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
