using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Structural
{
    public class WarpAlignedPMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WarpAlignedPMesh class.
        /// </summary>
        public WarpAlignedPMesh()
          : base("WarpAlignedPMesh", "pMeshW",
              "A plankton mesh with the first edge of each triangle following the warp direction",
              "K2Structural", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PlanktonMesh", "pMesh", "The plankton mesh that represents the soap film (triangulated)", GH_ParamAccess.item);
            pManager.AddCurveParameter("WarpPolylines", "warpPl", "Specify the warp polylines", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("PMeshWarp", "pMeshW", "A plankton mesh with the first edge in each face following the warp direction", GH_ParamAccess.item);
            pManager.AddIntegerParameter("faceIndexes", "fIndex", "The face indexes to check for correct sorting", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //INPUT
            PlanktonMesh pMesh = new PlanktonMesh();
            DA.GetData(0, ref pMesh);

            //Test that the mesh is triangulated
            for (int i = 0; i < pMesh.Faces.Count; i++)
            {
                int[] faceIndexes = pMesh.Faces.GetFaceVertices(i);
                if (faceIndexes.Length != 3)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The mesh has to be triangulated");
                }
            }

            //convert from curve to polyline
            List<Polyline> warpPl = new List<Polyline>();
            List<Curve> warpCrvs = new List<Curve>();
            DA.GetDataList(1, warpCrvs);

            foreach (Curve crv in warpCrvs)
            {
                Polyline pl;
                crv.TryGetPolyline(out pl);
                warpPl.Add(pl);
            }


            //CALCULATE
            List<int> warpIndexList = createWarpIndexListFromPolylines(pMesh, warpPl);
            PlanktonMesh pMeshW = createWarpPMesh(pMesh, warpIndexList);

            //check mesh
            DataTree<int> faceVerticesTree = extractFaceVertices(pMeshW);


            //OUTPUT
            DA.SetData(0, pMeshW);
            DA.SetDataTree(1, faceVerticesTree);
        }


        //METHODS

        //Check: Extract the face vertices of a mesh to check sorting is correct
        DataTree<int> extractFaceVertices(PlanktonMesh wPMesh)
        {
            DataTree<int> faceVerticesTree = new DataTree<int>();

            for (int i = 0; i < wPMesh.Faces.Count; i++)
            {
                GH_Path path = new GH_Path(i);
                int[] faceVertices = wPMesh.Faces.GetFaceVertices(i);

                foreach (int index in faceVertices)
                {
                    faceVerticesTree.Add(index, path);
                }
            }
            return faceVerticesTree;
        }

        //Create new plankton mesh from sorted face topology according to warp direction
        PlanktonMesh createWarpPMesh(PlanktonMesh pMesh, List<int> warpIndexList)
        {
            PlanktonMesh pMeshW = new PlanktonMesh();

            //Add vertices from original pMesh
            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                pMeshW.Vertices.Add(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z);
            }

            //Add faces with new topology
            for (int i = 0; i < pMesh.Faces.Count; i++)
            {
                int[] faceVerticesSorted = calcFaceVerticesOrder(pMesh, i, warpIndexList);
                pMeshW.Faces.AddFace(faceVerticesSorted[0], faceVerticesSorted[1], faceVerticesSorted[2]);
            }
            return pMeshW;
        }


        //Sort face vertices according to warp direction
        int[] calcFaceVerticesOrder(PlanktonMesh pMesh, int faceIndex, List<int> warpIndexList)
        {
            int[] faceVerticesSorted = new int[3];

            int[] faceHalfedges = pMesh.Faces.GetHalfedges(faceIndex);
            int halfedgeWarp = -1;
            for (int i = 0; i < faceHalfedges.Length; i++)
            {
                int start = pMesh.Halfedges[faceHalfedges[i]].StartVertex;
                int end = pMesh.Halfedges[pMesh.Halfedges[faceHalfedges[i]].NextHalfedge].StartVertex;

                int wIndexStart = warpIndexList.IndexOf(start);
                if (wIndexStart == 0)
                {
                    if (end == warpIndexList[wIndexStart + 1])
                    {
                        halfedgeWarp = faceHalfedges[i];
                    }
                }
                else
                {
                    if (end == warpIndexList[wIndexStart + 1] || end == warpIndexList[wIndexStart - 1])
                    {
                        halfedgeWarp = faceHalfedges[i];
                    }
                }
            }

            if (halfedgeWarp == -1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Face " + faceIndex + " does not have a warp direction specified");
            }
            else
            {
                int index0 = pMesh.Halfedges[halfedgeWarp].StartVertex;
                int index1 = pMesh.Halfedges[pMesh.Halfedges[halfedgeWarp].NextHalfedge].StartVertex;
                int index2 = pMesh.Halfedges[pMesh.Halfedges[halfedgeWarp].PrevHalfedge].StartVertex;

                faceVerticesSorted[0] = index0;
                faceVerticesSorted[1] = index1;
                faceVerticesSorted[2] = index2;
            }

            return faceVerticesSorted;
        }


        //Create one list of warp vertex indexes
        List<int> createWarpIndexListFromPolylines(PlanktonMesh pMesh, List<Polyline> pl)
        {
            List<int> warpIndexes = new List<int>();

            foreach (Polyline p in pl)
            {
                List<int> polyIndexes = convertPolyToMeshIndexes(pMesh, p);
                foreach (int i in polyIndexes)
                {
                    warpIndexes.Add(i);
                }
                warpIndexes.Add(-1);            //to seperate between polylines
            }
            return warpIndexes;
        }


        //Convert polyline points to mesh indexes
        List<int> convertPolyToMeshIndexes(PlanktonMesh pMesh, Polyline pl)
        {
            List<int> plIndexes = new List<int>();

            List<Point3d> verticesXYZ = extractMeshVerticesXYZ(pMesh);
            List<Point3d> polylinePts = new List<Point3d>();
            for (int i = 0; i < pl.Count; i++)
            {
                polylinePts.Add(pl[i]);
            }

            foreach (Point3d pt in polylinePts)
            {
                int index = verticesXYZ.IndexOf(pt);

                if (index == -1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Was not able to find point P(" + pt.X + "," + pt.Y + "," + pt.Z + ") amongst the vertices in the mesh");
                }
                else
                {
                    plIndexes.Add(index);
                }
            }

            return plIndexes;
        }


        //Convert mesh vertices to verticesXYZ
        List<Point3d> extractMeshVerticesXYZ(PlanktonMesh pMesh)
        {
            List<Point3d> verticesXYZ = new List<Point3d>();

            for (int i = 0; i < pMesh.Vertices.Count; i++)
            {
                Point3d pt = new Point3d(pMesh.Vertices[i].X, pMesh.Vertices[i].Y, pMesh.Vertices[i].Z);
                verticesXYZ.Add(pt);
            }
            return verticesXYZ;
        }



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{03309ed6-ee3c-43ed-a9d6-e6afebc2378e}"); }
        }
    }
}