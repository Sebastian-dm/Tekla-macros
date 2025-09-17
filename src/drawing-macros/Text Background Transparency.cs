// ###########################################################################################
// ### Name           : Set Text Transparency
// ### Date           : 2022
// ### Author         : Sebastian Meier
// ### Description    : 
// ###########################################################################################


using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Tekla.Technology.Akit;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Dialog;


namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {
        
        private static DrawingHandler drawingHandler;
        private static Drawing activeDrawing;
        private static List<DrawingObject> drawingObjectsToModify;

        private const bool TransparencyGoal = true;

        private static int nText = 0;
        private static int nMark = 0;
        private static int nDim = 0;
        private static int nLevel = 0;

        private static int nTextUnmodified = 0;
        private static int nMarkUnmodified = 0;
        private static int nDimUnmodified = 0;
        private static int nLevelUnmodified = 0;

        public static void Run(Tekla.Technology.Akit.IScript akit)
        {
            Initialize();
            if (activeDrawing == null) {
                MessageBox.Show("You must have a drawing open first to use this tool.");
                return;
            }
            SetTransparencyOnAllObjcts();
            DisplayStatus();
        }

        private static void Initialize() {
            drawingHandler = new DrawingHandler();
            activeDrawing = drawingHandler.GetActiveDrawing();
        }

        private static void DisplayStatus() {
            MessageBox.Show(string.Concat("Made background transparent on:\n",
                "  " + nText.ToString() + " Text objects\t(" + nTextUnmodified+" already transparent)\n",
                "  " + nMark.ToString() + " Marks\t\t(" + nMarkUnmodified + " already transparent)\n",
                "  " + nLevel.ToString() + " Level marks\t(" + nLevelUnmodified + " already transparent)\n",
                "  " + nDim.ToString() + " Dimensions\t(" + nDimUnmodified + " already transparent)"));
        }

        private static void SetTransparencyOnAllObjcts() {
            DrawingObjectEnumerator DrawingObjectEnumerator = activeDrawing.GetSheet().GetAllObjects();
            while (DrawingObjectEnumerator.MoveNext()) {
                if (HasBackground(DrawingObjectEnumerator.Current))
                    SetTransparentBackground(DrawingObjectEnumerator.Current);
            }
        }

        private static bool WasObjectsModified() {
            if (nText + nMark + nDim + nLevel > 0)
                return true;
            else
                return false;
        }

        private static bool HasBackground(DrawingObject d) {
            if (d as Text != null ||
                d as MarkBase != null ||
                d as StraightDimensionSet != null ||
                d as LevelMark != null)
                return true;
            else
                return false;
        }

        private static void SetTransparentBackground(DrawingObject drawingObject) {
            if (drawingObject as Text != null)
                SetTransparentBackground(drawingObject as Text);
            else if (drawingObject as MarkBase != null)
                SetTransparentBackground(drawingObject as MarkBase);
            else if (drawingObject as StraightDimensionSet != null)
                SetTransparentBackground(drawingObject as StraightDimensionSet);
            else if (drawingObject as LevelMark != null)
                SetTransparentBackground(drawingObject as LevelMark);

            if (WasObjectsModified())
                activeDrawing.CommitChanges();
        }
        private static void SetTransparentBackground(Text d) {
            if (d.Attributes.TransparentBackground != TransparencyGoal) {
                d.Attributes.TransparentBackground = TransparencyGoal;
                d.Modify();
                nText++;
            }
            else
                nTextUnmodified++;
        }
        private static void SetTransparentBackground(MarkBase d) {
            if (d.Attributes.TransparentBackground != TransparencyGoal) {
                d.Attributes.TransparentBackground = TransparencyGoal;
                d.Modify();
                nMark++;
            }
            else
                nMarkUnmodified++;
        }
        private static void SetTransparentBackground(StraightDimensionSet d) {
            if (d.Attributes.TransparentBackground != TransparencyGoal) {
                d.Attributes.TransparentBackground = TransparencyGoal;
                d.Modify();
                nDim++;
            }
            else
                nDimUnmodified++;
        }
        private static void SetTransparentBackground(LevelMark d) {
            if (d.Attributes.TransparentBackground != TransparencyGoal) {
                d.Attributes.TransparentBackground = TransparencyGoal;
                d.Modify();
                nLevel++;
            }
            else
                nLevelUnmodified++;
        }

    }
}
