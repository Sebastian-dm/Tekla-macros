// ###########################################################################################
// ### Name           : Reopen_Drawing Macro
// ### Date           : 2022
// ### Author         : Sebastian Meier
// ### Description    : Attempts to reestablish the connection to the object level settings for
// ###                  a part that was previously modified through part representation settings.
// ###                  NOTE that for this to work, the dummy phase 16000 needs to be created and
// ###                  filtered out of drawings
// ###########################################################################################


using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;


namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        private static Tekla.Structures.Drawing.DrawingHandler drawingHandler;
        private static Tekla.Structures.Model.Phase dummyPhase;
        private static Tekla.Structures.Drawing.Drawing activeDrawing;

        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            drawingHandler = new DrawingHandler();
            activeDrawing = drawingHandler.GetActiveDrawing();
            var DrawingPartsToRegen = new List<Tekla.Structures.Drawing.Part>();

            //Check if drawing is open
            if (activeDrawing == null) {
                MessageBox.Show("You must have a drawing open first to use this tool.");
                return;
            }

            //Get model part from picked object
            DrawingPartsToRegen = GetUserSelectedParts();
            if (DrawingPartsToRegen == null) {
                MessageBox.Show("No drawing part found for picked object.");
                return;
            }

            bool dummyPhaseExists = false;
            dummyPhaseExists = FindDummyPhase(16000);
            if (!dummyPhaseExists) {
                MessageBox.Show("Phase 16000 must exist and be filtered out from view.");
            }

            // Call function to regenerate part representation
            RefreshParts(DrawingPartsToRegen);
        }


        private static List<Tekla.Structures.Drawing.Part> GetUserSelectedParts() {
            var UserSelectedParts = new List<Tekla.Structures.Drawing.Part>();

            // Try to get user selection
            bool selectionDone = false;
            var SelectedObjects = drawingHandler.GetDrawingObjectSelector().GetSelected();
            if (SelectedObjects.GetSize() > 0 && SelectedObjects != null) {
                foreach (var selectedObject in SelectedObjects) {
                    if (selectedObject is Tekla.Structures.Drawing.Part) {
                        UserSelectedParts.Add((Tekla.Structures.Drawing.Part) selectedObject);
                        selectionDone = true;
                    }
                }
            }

            // If no parts are selected, let user pick
            if (!selectionDone) {
                var picker = drawingHandler.GetPicker();
                Tekla.Structures.Drawing.ViewBase viewBase;
                Tekla.Structures.Drawing.DrawingObject pickedObject;
                picker.PickObject("Pick part to regenerate. (Note: phase 16000 must be filtered out of view).", out pickedObject, out viewBase);
                if (pickedObject != null && viewBase != null && (pickedObject is Tekla.Structures.Drawing.Part)) {
                    UserSelectedParts.Add(pickedObject as Tekla.Structures.Drawing.Part);
                }
            }

            return UserSelectedParts;
        }


        private static void RefreshParts(List<Tekla.Structures.Drawing.Part> DrawingPartsToRegen)
        {
            var PartPhases = new List<Tekla.Structures.Model.Phase>();
            var ModelParts = new List<Tekla.Structures.Model.Part>();
            var relevantViews = new List<Tekla.Structures.Drawing.ViewBase>();

            foreach (var drawingPart in DrawingPartsToRegen) {
                var ModelPart = new Model().SelectModelObject(drawingPart.ModelIdentifier);
                ModelParts.Add((Tekla.Structures.Model.Part) ModelPart);
                relevantViews.Add(drawingPart.GetView());

                // Save phase of part
                Tekla.Structures.Model.Phase PartPhase;
                ModelPart.GetPhase(out PartPhase);
                PartPhases.Add(PartPhase);
                ModelPart.SetPhase(dummyPhase);
                //drawingPart.Modify();
            }

            foreach (var view in relevantViews.Distinct()) {
                RedrawView((Tekla.Structures.Drawing.View) view);
            }
            
            // Reset
            int i = 0;
            foreach (var drawingPart in DrawingPartsToRegen) {
                ModelParts[i].SetPhase(PartPhases[i]);
                i++;
            }

            foreach (var view in relevantViews.Distinct()) {
                RedrawView((Tekla.Structures.Drawing.View)view);
            }
        }


        private static bool FindDummyPhase(int SoughtPhaseNumber = 16000) {
            var CurrentModel = new Tekla.Structures.Model.Model();

            dummyPhase = null;
            if (!CurrentModel.GetConnectionStatus())
                return false;

            PhaseCollection PhaseCollection = CurrentModel.GetPhases();
            foreach (Phase phase in PhaseCollection) {
                if (phase.PhaseNumber == SoughtPhaseNumber) {
                    dummyPhase = phase;
                    return true;
                }
            }

            return false;
        }


        private static void RedrawView(Tekla.Structures.Drawing.View view) {
            var maxPoint = view.RestrictionBox.MaxPoint;
            Tekla.Structures.Geometry3d.Point newMax = new Tekla.Structures.Geometry3d.Point(maxPoint.X+0.1, maxPoint.Y, maxPoint.Z);
            view.RestrictionBox.MaxPoint = newMax;
            view.Modify();
            view.RestrictionBox.MaxPoint = maxPoint;
            view.Modify();
        }
    }
}
