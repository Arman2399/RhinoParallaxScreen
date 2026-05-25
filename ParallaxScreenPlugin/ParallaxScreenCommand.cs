using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;

namespace ParallaxScreenPlugin
{
    public class ParallaxScreenCommand : Command
    {
        public ParallaxScreenCommand()
        {
            // Rhino only creates one instance of each command class defined in a plug-in.
            Instance = this;
        }

        public static ParallaxScreenCommand Instance { get; private set; }

        public override string EnglishName => "ParallaxScreen";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // 1. Prompt for Base Surface (Screen Plane)
            GetObject goSurface = new GetObject();
            goSurface.SetCommandPrompt("Select base surface for the screen plane");
            goSurface.GeometryFilter = ObjectType.Surface;
            goSurface.SubObjectSelect = false;
            goSurface.Get();

            if (goSurface.CommandResult() != Result.Success)
                return goSurface.CommandResult();

            ObjRef surfaceRef = goSurface.Object(0);
            Surface baseSurface = surfaceRef.Surface();

            if (baseSurface == null)
            {
                RhinoApp.WriteLine("Failed to acquire a valid surface.");
                return Result.Failure;
            }

            // 2. Prompt for Observer Path (Curve)
            GetObject goCurve = new GetObject();
            goCurve.SetCommandPrompt("Select curve for the observer's path");
            goCurve.GeometryFilter = ObjectType.Curve;
            goCurve.SubObjectSelect = false;
            // Optionally disable selection of the previously selected surface edge if it acts as a curve
            goCurve.DisablePreSelect();
            goCurve.Get();

            if (goCurve.CommandResult() != Result.Success)
                return goCurve.CommandResult();

            ObjRef curveRef = goCurve.Object(0);
            Curve observerPath = curveRef.Curve();

            if (observerPath == null)
            {
                RhinoApp.WriteLine("Failed to acquire a valid curve.");
                return Result.Failure;
            }

            // 3. Command Logic
            RhinoApp.WriteLine("ParallaxScreen inputs successfully acquired.");
            RhinoApp.WriteLine($"Surface ID: {surfaceRef.ObjectId}");
            RhinoApp.WriteLine($"Curve ID: {curveRef.ObjectId}");

            // TODO: Implement parallax grid generation or screening logic here
            // --- Parallax Logic Implementation ---

            int uDivisions = 50; // Configurable density of the grid
            Interval uDomain = baseSurface.Domain(0);

            List<Curve> verticalLines = new List<Curve>();
            List<Vector3d> viewVectors = new List<Vector3d>();

            for (int i = 0; i <= uDivisions; i++)
            {
                // 1. Calculate U parameter
                double tParam = (double)i / uDivisions;
                double uVal = uDomain.ParameterAt(tParam);

                // 2. Extract vertical isocurve (V-direction = 1)
                Curve vIso = baseSurface.IsoCurve(1, uVal);
                if (vIso == null) continue;

                verticalLines.Add(vIso);

                // 3. Define the base of the vertical line (start point)
                Point3d basePoint = vIso.PointAtStart;

                // 4. Find closest point on observer curve
                if (observerPath.ClosestPoint(basePoint, out double tCurve))
                {
                    Point3d observerPoint = observerPath.PointAt(tCurve);

                    // 5. Calculate vector from observer to base point
                    Vector3d viewVector = basePoint - observerPoint;
                    viewVectors.Add(viewVector);

                    // Optional: Bake debug lines to visualize the vectors in Rhino
                    // doc.Objects.AddLine(observerPoint, basePoint); 
                }
            }

            RhinoApp.WriteLine($"Generated {verticalLines.Count} vertical lines and calculated corresponding view vectors.");

            // --- Consolidated Parallax Logic ---

            uDivisions = 100;
            double louverDepth = 0.40;
            double louverThickness = 0.01;

            uDomain = baseSurface.Domain(0);
            List<Brep> louverBreps = new List<Brep>();

            for (int i = 0; i <= uDivisions; i++)
            {
                double tParam = (double)i / uDivisions;
                double uVal = uDomain.ParameterAt(tParam);

                Curve vIso = baseSurface.IsoCurve(1, uVal);
                if (vIso == null) continue;

                Point3d basePoint = vIso.PointAtStart;

                if (observerPath.ClosestPoint(basePoint, out double tCurve))
                {
                    Point3d observerPoint = observerPath.PointAt(tCurve);
                    Vector3d viewVector = basePoint - observerPoint;

                    vIso.Domain = new Interval(0, 1);
                    Vector3d zAxis = vIso.TangentAt(0);

                    Vector3d xAxis = viewVector - (viewVector * zAxis) * zAxis;
                    if (!xAxis.Unitize()) continue;

                    Vector3d yAxis = Vector3d.CrossProduct(zAxis, xAxis);
                    yAxis.Unitize();

                    Plane profilePlane = new Plane(basePoint, xAxis, yAxis);

                    Interval xInterval = new Interval(-louverDepth / 2, louverDepth / 2);
                    Interval yInterval = new Interval(-louverThickness / 2, louverThickness / 2);
                    Rectangle3d rect = new Rectangle3d(profilePlane, xInterval, yInterval);
                    Curve profileCurve = rect.ToNurbsCurve();

                    Brep[] sweptBreps = Brep.CreateFromSweep(vIso, profileCurve, true, doc.ModelAbsoluteTolerance);
                    if (sweptBreps != null && sweptBreps.Length > 0)
                    {
                        Brep solidLouver = sweptBreps[0].CapPlanarHoles(doc.ModelAbsoluteTolerance);
                        Brep finalBrep = solidLouver ?? sweptBreps[0];

                        louverBreps.Add(finalBrep);
                        doc.Objects.AddBrep(finalBrep);
                    }
                }
            }

            RhinoApp.WriteLine($"Generated {louverBreps.Count} louver Breps.");

            doc.Views.Redraw();
            return Result.Success;
        }
    }
}