using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace GraphicsBook
{
    public class EdgeIDs
    {
        public int[] vertices;
        
        public EdgeIDs(int a, int b) { vertices = new int[] { a, b }; }

        public int this[int i]
        {
            get { return vertices[i]; }
            set { vertices[i] = value; }
        }
    }

    public class FaceIDs {
        public int[] vertices;

        public FaceIDs(int a, int b, int c, int d) {
            vertices = new int[] { a, b, c, d };
        }

        public int this[int i]
        {
            get { return vertices[i]; }
            set { vertices[i] = value; }
        }
    }

    public class Object3D
    {
        public Object3D(Point3D[] vertices, EdgeIDs[] edges, FaceIDs[] faces) {
            this.vertices = vertices;
            this.edges = edges;
            this.faces = faces;
        }

        public Point3D[] vertices;
        public EdgeIDs[] edges;
        public FaceIDs[] faces;

        public Point3D GetFaceVertex(int faceID, int vertex) {
            return vertices[faces[faceID].vertices[vertex]];
        }

        public Point3D GetEdgeVertex(int edgeID, int vertex) {
            return vertices[edges[edgeID].vertices[vertex]];
        }

        public Object3D rotatedParametric(double t) {
            // WORKS ONLY FOR DEFAULT CUBE!
            Object3D copy = this;
            double r = Math.Sqrt(2) / 2;

            for(int i = 0; i < 4; i++) {
                double trigArg = (2 * i + 1) * Math.PI / 4 + t;
                // rotate front face
                copy.vertices[copy.faces[0][i]] = new Point3D(
                    r * Math.Cos(trigArg),
                    r * Math.Sin(trigArg),
                    copy.GetFaceVertex(0, i).Z
                );

                // rotate back face
                copy.vertices[copy.faces[5][i]] = new Point3D(
                    r * Math.Cos(trigArg),
                    r * Math.Sin(trigArg),
                    copy.GetFaceVertex(5, i).Z
                );
            }

            return copy;
        }

        public static Object3D CreateCube(Point3D center, double xSize, double ySize, double zSize) {

            Vector3D toLeftTopBack = new Vector3D(-xSize / 2, ySize / 2, zSize / 2);

            Point3D[] vtable = new Point3D[]
            {
           /*0*/center + new Vector3D(toLeftTopBack.X, -toLeftTopBack.Y, -toLeftTopBack.Z),
           /*1*/center + new Vector3D(toLeftTopBack.X, toLeftTopBack.Y, -toLeftTopBack.Z),
           /*2*/center + new Vector3D(-toLeftTopBack.X, toLeftTopBack.Y, -toLeftTopBack.Z),
           /*3*/center + new Vector3D(-toLeftTopBack.X, -toLeftTopBack.Y, -toLeftTopBack.Z),
           /*4*/center + new Vector3D(toLeftTopBack.X, -toLeftTopBack.Y, toLeftTopBack.Z),
           /*5*/center + toLeftTopBack,
           /*6*/center + new Vector3D(-toLeftTopBack.X, toLeftTopBack.Y, toLeftTopBack.Z),
           /*7*/center + new Vector3D(-toLeftTopBack.X, -toLeftTopBack.Y, toLeftTopBack.Z),
            };
            EdgeIDs[] etable = new EdgeIDs[]{
                new EdgeIDs(0, 1), new EdgeIDs(1, 2), new EdgeIDs(2, 3), new EdgeIDs(3, 0), // one face
                new EdgeIDs(0, 4), new EdgeIDs(1, 5), new EdgeIDs(2, 6), new EdgeIDs(3, 7),  // joining edges
                new EdgeIDs(4, 5), new EdgeIDs(5, 6), new EdgeIDs(6, 7), new EdgeIDs(7, 4)}; // opposite face
            FaceIDs[] ftable = new FaceIDs[]
            {
                new FaceIDs(3, 2, 1, 0),   // front face
                new FaceIDs(4, 0, 1, 5),   // left face
                new FaceIDs(3, 7, 6, 2),   // right face
                new FaceIDs(1, 2, 6, 5),   // top face
                new FaceIDs(4, 7, 3, 0),   // bottom face
                new FaceIDs(5, 6, 7, 4)    // back face
            };

            return new Object3D(vtable, etable, ftable);
        }

        public static Object3D CreatePrism(Point3D center, double xSize, double ySize, double zSize) {

            Vector3D toLeftTopBack = new Vector3D(-xSize / 2, ySize / 2, zSize / 2);

            Point3D[] vtable = new Point3D[]
            {
           /*0*/center + new Vector3D(-xSize / 2, ySize / 2, -zSize / 3 * 2),
           /*1*/center + new Vector3D(0, ySize / 2, zSize / 3),
           /*2*/center + new Vector3D(xSize / 2, ySize / 2, -zSize / 3 * 2),
           /*3*/center + new Vector3D(-xSize / 2, -ySize / 2, -zSize / 3 * 2),
           /*4*/center + new Vector3D(0, -ySize / 2, zSize / 3),
           /*5*/center + new Vector3D(xSize / 2, -ySize / 2, -zSize / 3 * 2),
            };
            EdgeIDs[] etable = new EdgeIDs[]{
                new EdgeIDs(0, 1), new EdgeIDs(1, 2), new EdgeIDs(2, 0),    // top face
                new EdgeIDs(0, 3), new EdgeIDs(1, 4), new EdgeIDs(2, 5),    // joining edges
                new EdgeIDs(3, 4), new EdgeIDs(4, 5), new EdgeIDs(5, 3)};   // bottom face
            FaceIDs[] ftable = new FaceIDs[]
            {
                new FaceIDs(1, 0, 2, 2),   // top face
                new FaceIDs(0, 1, 4, 3),   // left face
                new FaceIDs(1, 2, 5, 4),   // right face
                new FaceIDs(0, 3, 5, 2),   // back face
                new FaceIDs(4, 5, 3, 3),   // bottom face
            };

            return new Object3D(vtable, etable, ftable);
        }
    }
}
