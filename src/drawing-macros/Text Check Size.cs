// Written by Sebasian Meier dec. 2021.
//
// Marks the text to print in red if the text size is smaller than specified

using Tekla.Structures.Drawing;
using Tekla.Technology.Akit;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;


namespace Tekla.Technology.Akit.UserScript
{
    public class Script
    {

        public static double textMinSize = 2.5;
        public static double textMaxSize = 99.0;

        public static void Run(IScript akit)
        {
            DrawingHandler dh = new DrawingHandler();
            DrawingObjectEnumerator objectsEnumerator = dh.GetActiveDrawing().GetSheet().GetAllObjects();

            int nText = 0;
            List<double> textSizes = new List<double>();
            
            int nMark = 0;
            List<double> markSizes = new List<double>();

            int nDim = 0;
            List<double> dimSizes = new List<double>();
            
            
            while (objectsEnumerator.MoveNext()) {
                DrawingObject obj = objectsEnumerator.Current;

                if (obj as Text != null) {
                    Text textObject = (Text)obj;
                    textSizes.Add(textObject.Attributes.Font.Height);
                    nText++;
                }
                //else if (obj as MarkBase != null) {
                //    MarkBase MarkObj = (MarkBase)obj;
                //    markSizes.Add(MarkObj.Attributes.Font.Height);
                //    nMark++;
                //}
                //else if (obj as DimensionBase != null) {
                //    DimensionBase DimObj = (DimensionBase)obj;
                //    dimSizes.Add(DimObj.Attributes.Font.Height);
                //    nDim++;
                //}
            }
            
            textSizes.Sort();
            MessageBox.Show("Checked:\n" +
                nText.ToString() + " text objects (Sizes used: " + string.Join(" ", textSizes.Distinct().ToArray()) + ").\n" +
                nMark.ToString() + " mark objects (Sizes used: " + string.Join(" ", markSizes.Distinct().ToArray()) + ").\n" +
                nDim.ToString() + " dimension objects (Sizes used: " + string.Join(" ", dimSizes.Distinct().ToArray()) + ").\n"
            );
        }
    }
}
