﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace K2Engineering
{
    public class LoadOutput : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CastLoadOutput class.
        /// </summary>
        public LoadOutput()
          : base("LoadOutput", "LoadOutput",
              "Extract the output of the Load goal",
              "K2Eng", "6 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("LoadData", "LD", "The LoadData from the output of the Load goal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "pos", "The position of the load", GH_ParamAccess.item);
            pManager.AddVectorParameter("Load", "load", "The nodal load in [kN]", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Input
            DataTypes.PointLoadData loadData = new DataTypes.PointLoadData();
            DA.GetData(0, ref loadData);

            //Extract properties
            Point3d pt = loadData.Location;
            Vector3d force = loadData.Load;

            //Output
            DA.SetData(0, pt);
            DA.SetData(1, force);
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
                return Properties.Resources.LoadOutput;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{fab9d682-f0bd-47fc-bbbe-4efd19cee19c}"); }
        }
    }
}