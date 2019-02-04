namespace VelocityMap
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Renci.SshNet;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.DataVisualization.Charting;

    /// <summary>
    /// Defines the <see cref="Form1" />
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Defines the fieldHeight
        /// </summary>
        private int fieldHeight = 8230;

        /// <summary>
        /// Defines the fieldWidth
        /// </summary>
        private int fieldWidth = 8230;

        /// <summary>
        /// Defines the padding
        /// </summary>
        internal int padding = 1;

        /// <summary>
        /// Defines the baseFieldImage
        /// </summary>
        private Bitmap baseFieldImage;

        /// <summary>
        /// Defines the paths
        /// </summary>
        private MotionProfile.Trajectory paths;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1()
        {
            //Create the window with all the fancy buttons.
            InitializeComponent();
        }

        /// <summary>
        /// The Form1_Load
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //Put all of the points from the fieldpoints.txt and put them on the field
            SetupMainField();
            SetupPlots();

            DistancePlot.Dock = DockStyle.Fill;
            VelocityPlot.Dock = DockStyle.Fill;

            splitContainer1.SplitterDistance = splitContainer1.Height / 2;
        }

        /// <summary>
        /// Configures what the main field looks like.
        /// </summary>
        private void SetupMainField()
        {

            baseFieldImage = buildField();
            //convert the bitmap to an image that we can put on the field.
            Image b = new Bitmap(baseFieldImage, 1000, 1000);
            //Flip the image because someone created the field upside down....
            b.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //Give the background image a name...
            NamedImage backImage = new NamedImage("Background", b);
            //the test series is really the background series.
            mainField.Series.Add("test");
            mainField.Series["test"].ChartType = SeriesChartType.Point;
            //mainField.Series["test"].Points.AddXY(0, 0);
            mainField.Series["test"].Points.AddXY(fieldWidth, fieldHeight);
            //add different lines to the main field chart.
            mainField.Series.Add("path");
            mainField.Series.Add("left");
            mainField.Series.Add("right");
            mainField.Series.Add("cp");
            //setup the point size(the dot size on the graph)
            mainField.Series["cp"].MarkerSize = 10;
            mainField.Series["path"].MarkerSize = 4;
            mainField.Series["left"].MarkerSize = 2;
            mainField.Series["right"].MarkerSize = 2;

            //set what the points/dots look like
            mainField.Series["cp"].MarkerStyle = MarkerStyle.Diamond;
            //set what the different lines on the graph look like.
            mainField.Series["cp"].ChartType = SeriesChartType.Point;
            mainField.Series["path"].ChartType = SeriesChartType.Point;
            mainField.Series["left"].ChartType = SeriesChartType.Point;
            mainField.Series["right"].ChartType = SeriesChartType.Point;
            //set what the seperate lines color.
            mainField.Series["cp"].Color = Color.ForestGreen;
            mainField.Series["path"].Color = Color.Lime;
            mainField.Series["left"].Color = Color.Blue;
            mainField.Series["right"].Color = Color.Red;

            //this sets up what the graph domain and range is and what our increments are.
            mainField.ChartAreas[0].Axes[0].Maximum = fieldWidth;
            mainField.ChartAreas[0].Axes[0].Interval = 1000;
            mainField.ChartAreas[0].Axes[0].Minimum = 0;

            mainField.ChartAreas[0].Axes[1].Maximum = fieldHeight;
            mainField.ChartAreas[0].Axes[1].Interval = 1000;
            mainField.ChartAreas[0].Axes[1].Minimum = 0;


            //set the background to the background.
            mainField.Images.Add(backImage);
            mainField.ChartAreas[0].BackImageWrapMode = ChartImageWrapMode.Scaled;
            mainField.ChartAreas[0].BackImage = "Background";
        }

        /// <summary>
        /// Configure what the velocity chart and the distance chart look like
        /// </summary>
        private void SetupPlots()
        {
            //set the minimum for the domaine
            VelocityPlot.ChartAreas[0].Axes[0].Minimum = 0;
            //set the lables on the graph
            VelocityPlot.ChartAreas[0].Axes[0].Title = "Distance (mm)";

            VelocityPlot.ChartAreas[0].Axes[1].Title = "Velocity (mm/s)";

            //add different point types for our main path and our right and left paths.
            VelocityPlot.Series.Add("path");
            VelocityPlot.Series.Add("left");
            VelocityPlot.Series.Add("right");

            //Set what the velocity chart should look like.
            VelocityPlot.Series["path"].ChartType = SeriesChartType.FastLine;
            VelocityPlot.Series["left"].ChartType = SeriesChartType.FastLine;
            VelocityPlot.Series["right"].ChartType = SeriesChartType.FastLine;

            //Sets what the point color should look like on the velocity map.
            VelocityPlot.Series["path"].Color = Color.Lime;
            VelocityPlot.Series["left"].Color = Color.Blue;
            VelocityPlot.Series["right"].Color = Color.Red;

            //set the minimium x axis value on the distance graph
            DistancePlot.ChartAreas[0].Axes[0].Minimum = 0;
            //set the amount the x axis increases distance graph
            DistancePlot.ChartAreas[0].Axes[0].Interval = .5;
            //set the title of the x axis distance graph
            DistancePlot.ChartAreas[0].Axes[0].Title = "Time (s)";
            //set the interval of the y axis
            DistancePlot.ChartAreas[0].Axes[1].Interval = 500;
            //set the title of the y axis
            DistancePlot.ChartAreas[0].Axes[1].Title = "Distance (mm)";

            //add the seperate lines to the distance plot.
            DistancePlot.Series.Add("path");
            DistancePlot.Series.Add("left");
            DistancePlot.Series.Add("right");

            //set the type of lines
            DistancePlot.Series["path"].ChartType = SeriesChartType.FastLine;
            DistancePlot.Series["left"].ChartType = SeriesChartType.FastLine;
            DistancePlot.Series["right"].ChartType = SeriesChartType.FastLine;


            //set the color of the lines.
            DistancePlot.Series["path"].Color = Color.Lime;
            DistancePlot.Series["left"].Color = Color.Blue;
            DistancePlot.Series["right"].Color = Color.Red;

            //set the minimium x axis value on the distance graph
            AnglePlot.ChartAreas[0].Axes[0].Minimum = 0;
            //set the amount the x axis increases distance graph
            AnglePlot.ChartAreas[0].Axes[0].Interval = 1000;
            //set the title of the x axis distance graph
            AnglePlot.ChartAreas[0].Axes[0].Title = "Distance (mm)";
            //set the interval of the y axis
            AnglePlot.ChartAreas[0].Axes[1].Interval = 20;
            //set the title of the y axis
            AnglePlot.ChartAreas[0].Axes[1].Title = "Degrees";

            //add the seperate lines to the distance plot.
            AnglePlot.Series.Add("angle");


            //set the type of lines
            AnglePlot.Series["angle"].ChartType = SeriesChartType.FastLine;

            //set the color of the lines.
            AnglePlot.Series["angle"].Color = Color.White;
        }

        /// <summary>
        /// Remove all of the points from the specified chart.
        /// </summary>
        /// <param name="chart">The chart<see cref="Chart"/></param>
        private void ClearChart(Chart chart)
        {
            foreach (Series s in chart.Series)
            {
                s.Points.Clear();
            }

            mainField.Series["test"].Points.AddXY(0, 0);
            mainField.Series["test"].Points.AddXY(fieldWidth, fieldHeight);
        }


        /// <summary>
        /// Used to draw the points from the fieldpoints.txt on the field
        /// </summary>
        /// <returns> The bitmap of the background for the field. </returns>
        private Bitmap buildField()
        {
            Pen bluePen = new Pen(Color.Red, 10);

            //create the drawing bitmap
            Bitmap b = new Bitmap(VelocityMap.Properties.Resources._2019_field);
            b.RotateFlip(RotateFlipType.Rotate90FlipNone);

            //draw the field size on the bitmap

            return b;
        }

        /// <summary>
        /// The event that is called when the user clicks on the main field chart.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="MouseEventArgs"/></param>
        private void mainField_MouseClick(object sender, MouseEventArgs e)
        {
            //if the button click is a left mouse click then add a positive point to the field chart.
            if (e.Button == MouseButtons.Left)
            {
                if (dp != null)
                {
                    dp.Color = Color.Yellow;
                    dp = null;
                    if ((controlPoints.RowCount - 2 > 0))
                    {
                        Apply_Click(null, null);
                    }
                    
                    return;
                }
                Chart c = (Chart)sender;

                double x = c.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double y = c.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

                if (x > 0 && y > 0 && x <= fieldWidth && y <= fieldHeight)
                {
                    c.Series["cp"].Points.AddXY(x, y);

                    controlPoints.Rows[controlPoints.Rows.Add((int)x, (int)y, "+", "")].Selected = true;
                }
            }
            //if the button click is a right mouse click then add a negative point to the field chart.

            if (e.Button == MouseButtons.Right)
            {
                if (dp != null)
                {
                    dp = null;
                    if ((controlPoints.RowCount - 2 > 0))
                    {
                        Apply_Click(null, null);
                    }
                    Apply_Click(null, null);
                    return;
                }
                Chart c = (Chart)sender;

                double x = c.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                double y = c.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);


                if (x > 0 && y > 0 && x <= fieldWidth && y <= fieldHeight)
                {
                    c.Series["cp"].Points.AddXY(x, y);
                    mainField.Series["cp"].Points.Last().Color = Color.Red;

                    controlPoints.Rows[controlPoints.Rows.Add((int)x, (int)y, "-", Int32.Parse(maxVelocity.Text))].Selected = true;
                }

            }
            if (e.Button == MouseButtons.Middle)
            {
                DataPoint p;
                HitTestResult hit = mainField.HitTest(e.X, e.Y);
                //get the point the user is clicking on.
                if (hit.PointIndex >= 0)
                {
                    //check to see if the point is part of the controlpoints because we have more than just controlpoints on the field chart
                    if (hit.Series == null)
                        return;
                    if (hit.Series.ToString() != "Series-path")
                        return;

                    if (hit.Series.Points[hit.PointIndex] == null)
                        return;
                    //if the point is real and exists then set dp to the point.
                    p = hit.Series.Points[hit.PointIndex];
                    p.Color = Color.Red;
                    p.MarkerStyle = MarkerStyle.Triangle;
                    p.MarkerSize = 10;

                    commandPointsList.Rows[commandPointsList.Rows.Add(mainField.Series["path"].Points.IndexOf(p), "")].Selected=true;
                }
            }
        }

        /// <summary>
        /// The event that is called when the user clicks and holds on the main field chart.
        /// </summary>
        private void mainField_MouseDown(object sender, MouseEventArgs e)
        {
            //if the user is holding their left mouse button
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                Chart c = (Chart)sender;

                ChartArea ca = c.ChartAreas[0];
                Axis ax = ca.AxisX;
                Axis ay = ca.AxisY;
                if (dp != null)
                {
                    return;
                }
                else
                {
                    HitTestResult hit = mainField.HitTest(e.X, e.Y);
                    //get the point the user is clicking on.
                    if (hit.PointIndex >= 0)
                    {
                        //check to see if the point is part of the controlpoints because we have more than just controlpoints on the field chart
                        if (hit.Series == null)
                            return;
                        if (hit.Series.ToString() != "Series-cp")
                            return;

                        if (hit.Series.Points[hit.PointIndex] == null)
                            return;
                        //if the point is real and exists then set dp to the point.
                        dp = hit.Series.Points[hit.PointIndex];
                        foreach (DataGridViewRow row in controlPoints.Rows)
                        {
                            if (row.Cells[0].Value != null)
                            {
                                // Debug.Print(row.Cells[0].Value.ToString() + ":" + ((int)dp.XValue).ToString() + ":" + row.Cells[1].Value.ToString() + ":" + ((int)dp.YValues[0]).ToString());
                                if (row.Cells[0].Value.ToString() == ((int)dp.XValue).ToString() && row.Cells[1].Value.ToString() == ((int)dp.YValues[0]).ToString())
                                {
                                    //move the point
                                    double dx = (int)ax.PixelPositionToValue(e.X);
                                    double dy = (int)ay.PixelPositionToValue(e.Y);

                                    dp.XValue = dx;
                                    dp.YValues[0] = dy;
                                    row.Cells[0].Value = dx;
                                    row.Cells[1].Value = dy;


                                    rowIndex = row.Index;
                                    row.Selected = true;


                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The event that is called when the user mouse while above the main field.
        /// </summary>
        private void mainField_MouseMove(object sender, MouseEventArgs e)
        {
            //if the user is holding the left button while moving the mouse allow them to move the point.
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                Chart c = (Chart)sender;

                ChartArea ca = c.ChartAreas[0];
                Axis ax = ca.AxisX;
                Axis ay = ca.AxisY;
                if (dp != null)
                {
                    double dx = (int)ax.PixelPositionToValue(e.X);
                    double dy = (int)ay.PixelPositionToValue(e.Y);

                    dp.XValue = dx;
                    dp.YValues[0] = dy;
                    controlPoints.Rows[rowIndex].Cells[0].Value = dx;
                    controlPoints.Rows[rowIndex].Cells[1].Value = dy;

                    c.Invalidate();
                }

            }
        }

        /// <summary>
        /// The currently selected row from the controlpoint table.
        /// </summary>
        private int rowIndex;
        /// <summary>
        /// The currently selected row from the commandPointList table.
        /// </summary>
        private int commandRowIndex;

        /// <summary>
        /// The currently selected point.
        /// </summary>
        internal DataPoint dp;

        /// <summary>
        /// The event that is called when the user clicks the invert button.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void Invert_Click(object sender, EventArgs e)
        {
            //goes though ever row and changes the x value from the left side to the right side by taking the field width and subracting the current x value.
            foreach (DataGridViewRow row in controlPoints.Rows)
            {
                if (row.Cells[0].Value != null)
                    row.Cells[0].Value = this.fieldWidth - float.Parse(row.Cells[0].Value.ToString());
            }
            Apply_Click(sender, e);
        }

        /// <summary>
        /// The event that is called when a rows state is changed ex: the row is selected.
        /// </summary>
        private void controlPoints_RowStateChange(object sender, DataGridViewRowStateChangedEventArgs e)
        {

            if (e.Row.Cells[0].Value == null && e.Row.Cells[1].Value == null && e.Row.Cells[1].Value == null)
            {
                return;
            }
            if (e.Row.Cells[0].Value == null || e.Row.Cells[0].Value.ToString() == "")
            {
                e.Row.Cells[0].Value = 0;
            }
            if (e.Row.Cells[1].Value == null || e.Row.Cells[1].Value.ToString() == "")
            {
                e.Row.Cells[1].Value = 0;
            }
            if (e.Row.Cells[2].Value == null || e.Row.Cells[2].Value.ToString() == "")
            {
                e.Row.Cells[2].Value = "+";
            }
            //If the state change is not a selection we don't care about it.
            if (e.StateChanged != DataGridViewElementStates.Selected)
            {
                return;
            }
            //Check to see if the row is selected because the selected event contains both unselecting and selecting.
            if (e.Row.Selected == true)
            {
                //Make sure that we at least have 1 point otherwise don't run this.
                if (controlPoints.Rows.Count - 2 != 0)
                {
                    //Go though each row.
                    foreach (DataGridViewRow row in controlPoints.Rows)
                    {
                        //Make sure that the row that is being selected is one of the ones that might have data.
                        if (row.Index >= 0 && row.Index <= controlPoints.Rows.Count - 2)
                        {
                            //Make sure that the cell is not blank so we dont get an error.
                            if (row.Cells[2].Value != null)
                            {
                                //Make sure that the cell is not blank so we dont get an error.
                                if (row.Cells[2].Value.ToString() != "")
                                {
                                    //If the third row contains a - then change the corresponding point on the graph to red.
                                    if (row.Cells[2].Value.ToString() == "-")
                                    {
                                        mainField.Series["cp"].Points[row.Index].Color = Color.Red;
                                    }
                                    //If the third row contains a + then change the corresponding point on the graph to green.
                                    if (row.Cells[2].Value.ToString() == "+")
                                    {
                                        mainField.Series["cp"].Points[row.Index].Color = Color.Green;

                                    }
                                }
                            }
                        }
                    }
                }

                //Make sure that we at least have 1 point otherwise don't run this.
                if (controlPoints.Rows.Count - 2 != 0)
                {
                    //Make sure that the row that is being selected is one of the ones that might have data.
                    if (e.Row.Index >= 0 && e.Row.Index <= controlPoints.Rows.Count - 2)
                    {
                        //Change the selected point to the color yellow.
                        mainField.Series["cp"].Points[e.Row.Index].Color = Color.Yellow;
                    }
                }
            }
        }

        /// <summary>
        /// The event that is called when a rows state is changed ex: the row is selected.
        /// </summary>
        private void commandPoints_RowStateChange(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
            {
                return;
            }

            foreach(DataGridViewRow row in commandPointsList.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    if (row.Cells[0].Value.ToString() != "")
                    {
                        if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                        {
                            DataPoint p = mainField.Series["path"].Points[int.Parse(row.Cells[0].Value.ToString())];
                            p.Color = Color.Red;
                        }
                    }
                }
            }

            if (e.Row.Cells[0].Value != null)
            {
                if (e.Row.Cells[0].Value.ToString() != "")
                {
                    if (mainField.Series["path"].Points.Count >= int.Parse(e.Row.Cells[0].Value.ToString()))
                    {
                        DataPoint p = mainField.Series["path"].Points[int.Parse(e.Row.Cells[0].Value.ToString())];
                        p.Color = Color.Blue;
                        p.MarkerStyle = MarkerStyle.Triangle;
                        p.MarkerSize =10;
                    }
                }
            }
        }

        /// <summary>
        /// The event that is called when the user stopes editing a cell.
        /// </summary>
        private void controlPoints_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //Check to see if the user is editing a cell that is in the third column.

            if (controlPoints.CurrentRow.Cells[0].Value == null && controlPoints.CurrentRow.Cells[1].Value == null && controlPoints.CurrentRow.Cells[1].Value == null)
            {
                return;
            }
            if (controlPoints.CurrentRow.Cells[0].Value == null || controlPoints.CurrentRow.Cells[0].Value.ToString() == "")
            {
                controlPoints.CurrentRow.Cells[0].Value = 0;
            }
            if (controlPoints.CurrentRow.Cells[1].Value == null || controlPoints.CurrentRow.Cells[1].Value.ToString() == "")
            {
                controlPoints.CurrentRow.Cells[1].Value = 0;
            }
            if (controlPoints.CurrentRow.Cells[2].Value == null || controlPoints.CurrentRow.Cells[2].Value.ToString() == "")
            {
                controlPoints.CurrentRow.Cells[2].Value = "+";
            }

            try
            {
                float.Parse(controlPoints.CurrentRow.Cells[0].Value.ToString());
            }
            catch (Exception)
            {
                controlPoints.CurrentRow.Cells[0].Value = 0;
            }
            try
            {
                float.Parse(controlPoints.CurrentRow.Cells[1].Value.ToString());
            }
            catch (Exception)
            {
                controlPoints.CurrentRow.Cells[1].Value = 0;
            }

            if (e.ColumnIndex == 2)
            {
                //If the cell contains a + or a - the ignore it. Else change the cell text to be a + signs.
                if (controlPoints.CurrentCell.Value.ToString() == "+" || controlPoints.CurrentCell.Value.ToString() == "-")
                {
                }
                else
                {
                    controlPoints.CurrentCell.Value = "+";
                }
            }
        }
        private void commandPoints_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int.Parse(commandPointsList.CurrentRow.Cells[0].Value.ToString());
            }
            catch (Exception)
            {
                commandPointsList.CurrentRow.Cells[0].Value = 0;
            }

            Apply_Click(null, null);
        }

        /// <summary>
        /// The event that is called when the user releases the mouse button while above the controlpoints cell.
        /// </summary>
        private void controlPoints_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            //make sure that the button that was released was the right mouse button.
            if (e.Button == MouseButtons.Right)
            {
                //Make sure that the cell that was selected was a cell that is real
                if (e.RowIndex >= 0)
                {
                    //on mouse up select that row.
                    this.controlPoints.Rows[e.RowIndex].Selected = true;
                    //When the row is selected set the rowindex to the index of the row that was just selected. (aka update the rowIndex value)
                    this.rowIndex = e.RowIndex;
                    //set the tables currentcell to the cell we just clicked.
                    this.controlPoints.CurrentCell = this.controlPoints.Rows[e.RowIndex].Cells[1];
                    //since we right clicked we open a context strip with things that allow us to delete and move the current row.
                    var relativeMousePosition = this.controlPoints.PointToClient(System.Windows.Forms.Cursor.Position);
                    this.contextMenuStrip2.Show(this.controlPoints, relativeMousePosition);
                }


            }
        }
        private void commandPoints_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            //make sure that the button that was released was the right mouse button.
            if (e.Button == MouseButtons.Right)
            {
                //Make sure that the cell that was selected was a cell that is real
                if (e.RowIndex >= 0)
                {
                    //on mouse up select that row.
                    this.commandPointsList.Rows[e.RowIndex].Selected = true;
                    //When the row is selected set the rowindex to the index of the row that was just selected. (aka update the rowIndex value)
                    this.commandRowIndex = e.RowIndex;
                    //set the tables currentcell to the cell we just clicked.
                    this.commandPointsList.CurrentCell = this.commandPointsList.Rows[e.RowIndex].Cells[1];
                    //since we right clicked we open a context strip with things that allow us to delete and move the current row.
                    var relativeMousePosition = this.commandPointsList.PointToClient(System.Windows.Forms.Cursor.Position);
                    this.commandPointListMenuStrip.Show(this.commandPointsList, relativeMousePosition);
                }


            }
        }

        /// <summary>
        /// The event that is called when the user clicks the delete button in the context strip.
        /// </summary>
        private void Delete_Click(object sender, EventArgs e)
        {
            //Make sure we are not deleting the always blank last row.
            if (rowIndex != controlPoints.RowCount - 1)
            {
                //Delete the row that is selected.
                controlPoints.Rows.RemoveAt(rowIndex);
            }
            //Reload the points because we just deleted one and we need the rest of the program to know.
            Apply_Click(null, null);
        }

        private void Delete_Click_commandPoints(object sender, EventArgs e)
        {
            //Make sure we are not deleting the always blank last row.
            if (commandRowIndex != commandPointsList.RowCount - 1)
            {
                //Delete the row that is selected.
                commandPointsList.Rows.RemoveAt(commandRowIndex);
            }
            //Reload the points because we just deleted one and we need the rest of the program to know.
            Apply_Click(null, null);
        }

        /// <summary>
        /// The event that is called when the user clicks the clear button.
        /// </summary>
        private void ClearCP_Click(object sender, EventArgs e)
        {
            //Clear all of the rows in the controlpoints table.
            controlPoints.Rows.Clear();
            commandPointsList.Rows.Clear();
            //Clear all of the plots.
            mainField.Series["cp"].Points.Clear();
            mainField.Series["path"].Points.Clear();
            mainField.Series["left"].Points.Clear();
            mainField.Series["right"].Points.Clear();

            VelocityPlot.Series["path"].Points.Clear();
            VelocityPlot.Series["right"].Points.Clear();
            VelocityPlot.Series["left"].Points.Clear();

            DistancePlot.Series["path"].Points.Clear();
            DistancePlot.Series["right"].Points.Clear();
            DistancePlot.Series["left"].Points.Clear();

            AnglePlot.Series["angle"].Points.Clear();
            

        }

        /// <summary>
        /// The event that is called when the user clicks the insert above button in the context stip.
        /// </summary>
        private void insertAbove_Click(object sender, EventArgs e)
        {
            //insert a new row at the selected index. (this will push the current index down one.)
            controlPoints.Rows.Insert(rowIndex);
        }

        /// <summary>
        /// The event that is called when the user clicks the insert above button in the context stip.
        /// </summary>
        private void insertAbove_Click_commandPoints(object sender, EventArgs e)
        {
            //insert a new row at the selected index. (this will push the current index down one.)
            commandPointsList.Rows.Insert(commandRowIndex);
        }

        /// <summary>
        /// The event that is called when the user clicks the insert below button in the context stip.
        /// </summary>

        private void insertBelow_Click(object sender, EventArgs e)
        {
            //insert a new row at the selected index plus one.
            controlPoints.Rows.Insert(rowIndex + 1);
        }

        /// <summary>
        /// The event that is called when the user clicks the insert below button in the context stip.
        /// </summary>

        private void insertBelow_Click_commandPoints(object sender, EventArgs e)
        {
            //insert a new row at the selected index plus one.
            if(!(commandPointsList.Rows.Count >= commandRowIndex))
                commandPointsList.Rows.Insert(commandRowIndex+1);

        }

        /// <summary>
        /// The event that is called when the user clicks the move up button in the context stip.
        /// </summary>
        private void btnUp_Click(object sender, EventArgs e)
        {
            //lets convert our object name because I copied this from the internet and am to lazy to change it.
            DataGridView dgv = controlPoints;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (rowIndex == 0)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex - 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        /// <summary>
        /// The event that is called when the user clicks the move up button in the context stip.
        /// </summary>
        private void btnUp_Click_commandPoints(object sender, EventArgs e)
        {
            //lets convert our object name because I copied this from the internet and am to lazy to change it.
            DataGridView dgv = commandPointsList;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int commandRowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (commandRowIndex == 0)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[commandRowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(commandRowIndex - 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[commandRowIndex - 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        /// <summary>
        /// The event that is called when the user clicks the move down button in the context stip.
        /// </summary>
        private void btnDown_Click(object sender, EventArgs e)
        {
            DataGridView dgv = controlPoints;
            try
            {
                //lets convert our object name because I copied this from the internet and am to lazy to change it.

                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (rowIndex == totalRows - 1)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex + 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        /// <summary>
        /// The event that is called when the user clicks the move down button in the context stip.
        /// </summary>
        private void btnDown_Click_commandPoints(object sender, EventArgs e)
        {
            DataGridView dgv = commandPointsList;
            try
            {
                //lets convert our object name because I copied this from the internet and am to lazy to change it.

                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int commandRowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (commandRowIndex == totalRows - 1)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[commandRowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(commandRowIndex + 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[commandRowIndex + 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        /// <summary>
        /// Converts our field points to a rectangle that can be drawn on a bitmap.
        /// </summary>
        /// <param name="array">The array that contains the x and y values of the rectangle.</param>
        /// <param name="adjustToScreen">If true will adjust the box to the screen.<see cref="bool"/></param>
        /// <returns>A rectangle that can be drawn on a bitmap.</returns>
        private Rectangle makeRectangle(int[] array, bool adjustToScreen = false)
        {
            Rectangle rec = new Rectangle();
            rec.X = array[0] + padding - 1;
            if (rec.X < 0) rec.X = padding - 1;

            rec.Width = array[2];
            if (array[0] < 0) rec.Width = rec.Width + array[0];

            rec.Y = array[1] - padding - 1;
            if (rec.Y < 0) rec.Y = 0;

            rec.Height = array[3];
            if (array[1] < 0) rec.Height = rec.Height + array[1];

            if (adjustToScreen)
                rec.Y = fieldWidth - rec.Y - rec.Height;

            return rec;
        }

        /// <summary>
        /// A method that reloads the control points and redraws them on the main field plot.
        /// </summary>
        /*private void ReloadControlPoints()
        {
            //Clear all of the points from the main field controlpoint series.
            mainField.Series["cp"].Points.Clear();
            //Go though each of the rows and if the x value is not blank then add the point to the main field plot.
            foreach (DataGridViewRow row in controlPoints.Rows)
            {
                //If the x cell is not empty.
                if (row.Cells[0].Value == null && row.Cells[1].Value == null && row.Cells[1].Value == null)
                {
                    continue;
                }
                if (row.Cells[0].Value == null || row.Cells[0].Value.ToString() == "")
                {
                    row.Cells[0].Value = 0;
                }
                if (row.Cells[1].Value == null || row.Cells[1].Value.ToString() == "")
                {
                    row.Cells[1].Value = 0;
                }
                if (row.Cells[2].Value == null || row.Cells[2].Value.ToString() == "")
                {
                    row.Cells[2].Value = "+";
                }
                if (row.Cells[0].Value != null)
                {
                    mainField.Series["cp"].Points.AddXY(float.Parse(row.Cells[0].Value.ToString()), float.Parse(row.Cells[1].Value.ToString()));
                }
                if (row.Cells[2].Value.ToString() == "-")
                {
                    mainField.Series["cp"].Points[row.Index].Color = Color.Red;
                }
                //If the third row contains a + then change the corresponding point on the graph to green.
                if (row.Cells[2].Value.ToString() == "+")
                {
                    mainField.Series["cp"].Points[row.Index].Color = Color.Green;

                }
            }
        }*/

        /// <summary>
        /// The method that is called when we want create a new path containing all of the information that we can input.
        /// </summary>
        /// <returns>The <see cref="MotionProfile.Path"/></returns>
        private MotionProfile.Path CreateNewPath()
        {
            //New path.
            MotionProfile.Path path = new MotionProfile.Path();
            //New VelocityMap for the path.
            path.velocityMap = new MotionProfile.VelocityMap();
            //Set the new VelocityMap's max velocity.
            path.velocityMap.vMax = int.Parse(maxVelocity.Text);
            //Set the new VelocityMap's max acceleration.
            path.velocityMap.FL1 = int.Parse(AccelRate.Text);
            //Set the new VelocityMap's time sampling rate.
            path.velocityMap.time = float.Parse(timeSample.Text) / 1000;
            //Set the new VelocityMap's boolean if the velocity should be instant.
            path.velocityMap.instVelocity = isntaVel.Checked;
            //Set the paths tolerance.
            path.tolerence = float.Parse(tolerence.Text);
            //Set the paths speed limit/max speed.
            path.speedLimit = float.Parse(SpeedLimit.Text);

            //Return this new path.
            return path;
        }

        /// <summary>
        /// The event that is called when the user clicks that apply button.
        /// </summary>
        private void Apply_Click(object sender, EventArgs e)
        {
            //Make sure that we have at least two points that we can actually make a path between.
            if (!(controlPoints.RowCount - 2 > 0))
            {
                //If not cancel this and show an error stating so.
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            //User the CreateNewPath method to get a path that we can use.
            MotionProfile.Path path = CreateNewPath();

            //A value that contains our "radius" of our track width
            int trackwidth = (int)((int.Parse(trackWidth.Text)) / 2);

            //Clear the main field because we are about to add a bunch of points.
            ClearChart(mainField);

            //Make the paths object to a new trajectory class.
            paths = new MotionProfile.Trajectory();

            //A value that will contain what our last cell in the third row contained.
            string last = "";
            //A value that will contain our last row from our control points.
            DataGridViewRow lastrow = controlPoints.Rows[0];
            //Lets run though each row of the controlpoints table
            foreach (DataGridViewRow row in controlPoints.Rows)
            {
                //If the x cell is not empty.
                if (row.Cells[0].Value == null && row.Cells[1].Value == null && row.Cells[1].Value == null)
                {
                    continue;
                }
                if(row.Cells[0].Value == null || row.Cells[0].Value.ToString() == "")
                {
                    row.Cells[0].Value = 0;
                }
                if (row.Cells[1].Value == null || row.Cells[1].Value.ToString() == "")
                {
                    row.Cells[1].Value = 0;
                }
                if (row.Cells[2].Value == null || row.Cells[2].Value.ToString() == "")
                {
                    row.Cells[2].Value = "+";
                }
                //update the last row.
                lastrow = row;
                    //since we believe that this row is not blank then we should put this point on the chart.
                    mainField.Series["cp"].Points.AddXY(float.Parse(row.Cells[0].Value.ToString()), float.Parse(row.Cells[1].Value.ToString()));
                    //Make sure that the direction cell is not empty so that we dont get an error.
                    if(row.Cells[2].Value==null)
                    {
                        row.Cells[2].Value = "+";
                    }
                    //If the direction cell contains a negative then we should turn the corresponding point to red and set that path direction to true.
                    if (row.Cells[2].Value.ToString() == "-")
                    {
                        mainField.Series["cp"].Points.Last().Color = Color.Red;
                        path.direction = true;
                    }
                    //If the direction cell contains a positive then set that path direction to false.
                    if (row.Cells[2].Value.ToString() == "+")
                    {
                        path.direction = false;
                    }

                    //Add our controlpoint to our path.
                    path.addControlPoint(float.Parse(row.Cells[1].Value.ToString()), float.Parse(row.Cells[0].Value.ToString()));
                Console.WriteLine(row.Cells[2].Value.ToString());

                //used to split our main path into seperate paths when we have a split in our negative and positive points.
                if (last != "" && last != row.Cells[2].Value.ToString())
                    {
                        Console.WriteLine("WTF");
                        if (row.Cells[2].Value.ToString() == "+")
                            path.direction = false;

                        if (row.Cells[2].Value.ToString() == "-")
                            path.direction = true;

                        if (path.controlPoints.Count >= 2)
                            paths.Add(path);

                        path = CreateNewPath();
                        path.addControlPoint(float.Parse(row.Cells[1].Value.ToString()), float.Parse(row.Cells[0].Value.ToString()));

                    }

                if (row.Selected)
                {
                    //Make sure that we at least have 1 point otherwise don't run this.
                    if (controlPoints.Rows.Count - 2 != 0)
                    {
                        //Make sure that the row that is being selected is one of the ones that might have data.
                        if (row.Index >= 0 && row.Index <= controlPoints.Rows.Count - 2)
                        {
                            //Change the selected point to the color yellow.
                            mainField.Series["cp"].Points[row.Index].Color = Color.Yellow;
                        }
                    }
                    
                }
                last = row.Cells[2].Value.ToString();

            }
            //if we have no controlpoints in our path then something is wrong and return.
            if (path.controlPoints.Count() == 0)
                return;


            if (lastrow != null && lastrow.Cells[2].Value.ToString() != "+")
                path.direction = false;

            if (lastrow != null && lastrow.Cells[2].Value.ToString() != "-")
                path.direction = true;

            //if our path contains more than or equal to 2 add the path to paths.
            if (path.controlPoints.Count >= 2)
                paths.Add(path);
            //Create the path.
            paths.Create(0);

            //Clear all of the data plots.
            ClearChart(VelocityPlot);
            ClearChart(DistancePlot);
            ClearChart(AnglePlot);

            //create a bunch of float arrays that will hold our data.
            float[] t, d, v, l, r, ld, rd, c, cd, h;



            //load the path information into the float arrays that we just created.
            t = paths.getTimeProfile();
            d = paths.getDistanceProfile();
            v = paths.getVelocityProfile();
            l = paths.getOffsetVelocityProfile(trackwidth).ToArray();
            ld = paths.getOffsetDistanceProfile(trackwidth).ToArray();
            c = paths.getOffsetVelocityProfile(0).ToArray();
            cd = paths.getOffsetDistanceProfile(0).ToArray();
            h = paths.getHeadingProfile();
            //Smooth our our offset velocity array.
            l.NoiseReduction(int.Parse(smoothness.Text));

            r = paths.getOffsetVelocityProfile(-trackwidth).ToArray();
            rd = paths.getOffsetDistanceProfile(-trackwidth).ToArray();
            //Smooth out the rest of our arrays.
            //h.NoiseReduction(int.Parse(smoothness.Text));
            r.NoiseReduction(int.Parse(smoothness.Text));
            rd.NoiseReduction(int.Parse(smoothness.Text));
            l.NoiseReduction(int.Parse(smoothness.Text));
            rd.NoiseReduction(int.Parse(smoothness.Text));
            c.NoiseReduction(int.Parse(smoothness.Text));
            cd.NoiseReduction(int.Parse(smoothness.Text));

            //temp values for holding values that will be put on our plots.
            double ldv = 0;// Right Distance.
            double rdv = 0;// Left Distance.
            double heading = 0; //Heading.

            //run though all of the values in the array and put them on the plot.
            for (int i = 0; i < ld.Length; i++)
            {
                ldv += ld[i];
                rdv += rd[i];
                
                DistancePlot.Series["left"].Points.AddXY(t[i], ldv);
                DistancePlot.Series["right"].Points.AddXY(t[i], rdv);


            }
            //run though all of the values in the array and put them on the plot.
            for (int i = 0; i < Math.Min(d.Length, r.Length); i++)
            {
                heading = h[i];

                VelocityPlot.Series["path"].Points.AddXY(d[i], v[i + 2]);
                VelocityPlot.Series["left"].Points.AddXY(d[i], l[i]);
                VelocityPlot.Series["right"].Points.AddXY(d[i], r[i]);

                AnglePlot.Series["angle"].Points.AddXY(d[i], heading);
            }

            //clear off the main field minus the controlpoints.
            mainField.Series["path"].Points.Clear();
            mainField.Series["left"].Points.Clear();
            mainField.Series["right"].Points.Clear();


            //Build the path and use the controlPoints that are returned to plot.
            foreach (ControlPoint p in paths.BuildPath())
            {
                for(int i = 0; i<p.point.Length-2; i++)
                {
                    PointF p1 = p.point[i];
                    mainField.Series["path"].Points.AddXY(p1.Y, p1.X);

                }
            }
            //Build the path and use the controlPoints that are returned to plot the offset.

            foreach (ControlPoint p in paths.BuildPath(trackwidth))
            {
                foreach (PointF p1 in p.point)
                {
                    mainField.Series["left"].Points.AddXY(p1.Y, p1.X);
                }
            }

            //Build the path and use the controlPoints that are returned to plot the offset.

            foreach (ControlPoint p in paths.BuildPath(-trackwidth))
            {
                foreach (PointF p1 in p.point)
                {
                    mainField.Series["right"].Points.AddXY(p1.Y, p1.X);
                }
            }
            foreach (DataGridViewRow row in commandPointsList.Rows)
            {
                if (row.Cells[0].Value != null)
                {
                    if(row.Cells[0].Value.ToString() != "")
                    {
                        if(mainField.Series["path"].Points.Count>= int.Parse(row.Cells[0].Value.ToString()))
                        {
                            DataPoint p = mainField.Series["path"].Points[int.Parse(row.Cells[0].Value.ToString())];
                            p.Color = Color.Red;
                            p.MarkerStyle = MarkerStyle.Triangle;
                            p.MarkerSize = 10;
                            if(row.Selected)
                            {
                                p.Color = Color.Blue;
                            }
                        }
                    }
                }

            }
                
        }

        /// <summary>
        /// The event that is called when the save button is clicked.
        /// </summary>
        private void Save_Click(object sender, EventArgs e)
        {
            //Make sure that we have at least two points that we can actually make a path between.
            if (!(controlPoints.RowCount - 2 > 0))
            {
                //If not cancel this and show an error stating so.
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            //We are going to apply before we save so that we have the newest data.
            Apply_Click(null, null);
            //Double check that we have more than 1 point for our calculation.
            if ((controlPoints.RowCount - 2 > 0))
            {
                //Create a save dialog window that allows the user to select where they want us to save the information to.
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //Limit the user to that can only see .mp and .json files.
                saveFileDialog1.Filter = "Motion Profile|*.mp;*.json";
                //Set the window title.
                saveFileDialog1.Title = "Save an MP File";
                //Actually open the dialog so that user can see it.
                saveFileDialog1.ShowDialog();


                //If the save dialog file name is not blank then continue.
                if (saveFileDialog1.FileName != "")
                {
                    String DirPath = System.IO.Path.GetDirectoryName(saveFileDialog1.FileName);    // Used for storing the directory path of the saved file.
                    String JSONPath = Path.Combine(DirPath, Path.GetFileNameWithoutExtension(saveFileDialog1.FileName) + ".json");     // Used for storing the json saved file directory path.
                    String MPPath = Path.Combine(DirPath, Path.GetFileNameWithoutExtension(saveFileDialog1.FileName) + ".mp");      // Used for storing the mp saved file directory path.


                    //open a new writer to the JSONPath.
                    using (var writer = new System.IO.StreamWriter(JSONPath))
                    {
                        //similiar to the apply stuff where we load our data from our paths into arrays then we us the arrays to write into the file.
                        writer.WriteLine("{");
                        writer.WriteLine("  \"Data\":[ ");

                        List<string> left = new List<string>();
                        List<string> right = new List<string>();
                        List<string> center = new List<string>();

                        List<string> line = new List<string>();

                        int trackwidth = (int)((int.Parse(trackWidth.Text)) / 2);

                        float[] l = paths.getOffsetVelocityProfile(trackwidth).ToArray();
                        List<float> ld = paths.getOffsetDistanceProfile(trackwidth);

                        float[] r;
                        List<float> rd = new List<float>(); ;

                        float[] c = paths.getOffsetVelocityProfile(0).ToArray();
                        List<float> cd = paths.getOffsetDistanceProfile(0);

                        float[] angles = paths.getHeadingProfile();




                        r = paths.getOffsetVelocityProfile(-trackwidth).ToArray();
                        rd = paths.getOffsetDistanceProfile(-trackwidth);


                        //angles.NoiseReduction(int.Parse(smoothness.Text));
                        r.NoiseReduction(int.Parse(smoothness.Text));
                        rd.NoiseReduction(int.Parse(smoothness.Text));
                        l.NoiseReduction(int.Parse(smoothness.Text));
                        ld.NoiseReduction(int.Parse(smoothness.Text));
                        c.NoiseReduction(int.Parse(smoothness.Text));
                        cd.NoiseReduction(int.Parse(smoothness.Text));
                        //write the information to the json file.
                        Dictionary<int, String> commandPoints = new Dictionary<int, String>();
                        foreach (DataGridViewRow row in commandPointsList.Rows)
                        {
                            if (row.Cells[0].Value != null)
                            {
                                if (row.Cells[0].Value.ToString() != "")
                                {
                                    if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                                    {
                                        if(row.Cells[1].Value!=null)
                                            commandPoints[int.Parse(row.Cells[0].Value.ToString())] = row.Cells[1].Value.ToString();
                                    }
                                }
                            }
                        }
                        for (int i = 0; i < l.Length; i++)
                        {
                            String text = "";
                            if (commandPoints.ContainsKey(i))
                            {
                                text = commandPoints[i];
                            }
                            if (CTRE.Checked)
                            {
                                double dConvert = Math.PI * double.Parse(wheel.Text) * 25.4;

                                line.Add("  {   \"Rotation\":" + cd.Take(i).Sum() / dConvert + " , " + "\"Velocity\":" + (c[i] / dConvert * 60).ToString() + " , " + "\"Time\":" + paths[0].velocityMap.time * 1000 + " , " + "\"Angle\":" + angles[i] + " , " + "\"State\":" + "\"" + text + "\"" + "}");

                            }
                            else
                            {
                                line.Add("  {   \"Rotation\":" + cd.Take(i).Sum().ToString() + " , " + "\"Velocity\":" + c[i].ToString() + " , " + "\"Time\":" + paths[0].velocityMap.time * 1000 + " , " + "\"Angle\":" + angles[i] + " , " + "\"State\":" +"\"" +text +"\""+"}");
                            }
                        }
                        right.Add(string.Join(",\n", line));

                        foreach (string ret in right)
                        {
                            writer.WriteLine(ret);
                        }
                        writer.WriteLine("  ] ");
                        writer.WriteLine("} ");
                    }
                    //Call the WriteSetupFile that will write a file so that we can go back and load these points again.
                    WriteSetupFile(MPPath);

                }

            }
        }

        /// <summary>
        /// A Method that will allow us to write a file that we can later load.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        private void WriteSetupFile(string path)
        {
            var writer1 = new System.IO.StreamWriter(path);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            //Create a writer  that goes to the path.
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                //set our formatting type for our writer
                writer.Formatting = Formatting.Indented;

                //start the writer
                writer.WriteStartObject();
                //start inputing our values into the json objects.
                writer.WritePropertyName("Max Velocity");
                writer.WriteValue(maxVelocity.Text);

                writer.WritePropertyName("Track Width");
                writer.WriteValue(trackWidth.Text);

                writer.WritePropertyName("Accel Rate");
                writer.WriteValue(AccelRate.Text);

                writer.WritePropertyName("Time Sample");
                writer.WriteValue(timeSample.Text);

                writer.WritePropertyName("Wheel Diameter");
                writer.WriteValue(wheel.Text);

                writer.WritePropertyName("Speed Limit");
                writer.WriteValue(SpeedLimit.Text);

                writer.WritePropertyName("Smoothness");
                writer.WriteValue(smoothness.Text);

                writer.WritePropertyName("CTRE");
                writer.WriteValue(CTRE.Checked.ToString());

                writer.WritePropertyName("isntaVel");
                writer.WriteValue(isntaVel.Checked.ToString());

                writer.WritePropertyName("Profile Name");
                writer.WriteValue(profilename.Text);

                writer.WritePropertyName("Username");
                writer.WriteValue(user.Text);

                writer.WritePropertyName("Ip-Address");
                writer.WriteValue(ipadd.Text);
                //put our points in as an array.
                writer.WritePropertyName("Points");
                writer.WriteStartArray();

                foreach (DataGridViewRow row in controlPoints.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {

                        writer.WriteStartArray();
                        writer.WriteValue(string.Concat(row.Cells[0].Value.ToString()));
                        writer.WriteValue(row.Cells[1].Value.ToString());
                        writer.WriteValue(row.Cells[2].Value.ToString());
                        writer.WriteEndArray();
                    }
                }
                //close our writer up
                writer.WriteEndArray();
                writer.WritePropertyName("CommandPoints");
                writer.WriteStartArray();

                foreach (DataGridViewRow row in commandPointsList.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        writer.WriteStartArray();
                        writer.WriteValue(string.Concat(row.Cells[0].Value.ToString()));
                        writer.WriteValue(row.Cells[1].Value.ToString());
                        writer.WriteEndArray();
                    }
                }
                //close our writer up
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            writer1.WriteLine(sb.ToString());
            writer1.Close();
        }

        /// <summary>
        /// The event that will be called when the user clicks the load button.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void Load_Click(object sender, EventArgs e)
        {
            //Opens a dialog that will allow the user to only select a .mp file and load it into the program.
            openFileDialog1.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "MotionProfile Data (*.mp)|*.mp";

            DialogResult results = openFileDialog1.ShowDialog();
            if (results == DialogResult.OK)
            {
                using (var reader1 = new System.IO.StreamReader(openFileDialog1.FileName))
                {
                    //First clear out our points.
                    controlPoints.Rows.Clear();
                    commandPointsList.Rows.Clear();
                    //Read the file and load our points and other variables.
                    string json = reader1.ReadToEnd();

                    JObject o = JObject.Parse(json);

                    maxVelocity.Text = (string)o["Max Velocity"];
                    trackWidth.Text = (string)o["Track Width"];
                    AccelRate.Text = (string)o["Accel Rate"];
                    timeSample.Text = (string)o["Time Sample"];
                    SpeedLimit.Text = (string)o["Speed Limit"];
                    wheel.Text = (string)o["Wheel Diameter"];
                    smoothness.Text = (string)o["Smoothness"];
                    CTRE.Checked = Boolean.Parse((string)o["CTRE"]);
                    isntaVel.Checked = Boolean.Parse((string)o["isntaVel"]);

                    profilename.Text = (string)o["Profile Name"];
                    user.Text = (string)o["Username"];
                    ipadd.Text = (string)o["Ip-Address"];

                    JArray a = (JArray)o["Points"];

                    for (int x = 0; x <= a.Count - 1; x++)
                    {
                        controlPoints.Rows.Add(float.Parse((string)a[x][0]), float.Parse((string)a[x][1]), (string)a[x][2]);
                    }

                    JArray CommandPointsArray = (JArray)o["CommandPoints"];

                    for (int x = 0; x <= CommandPointsArray.Count - 1; x++)
                    {
                        commandPointsList.Rows.Add(int.Parse((string)CommandPointsArray[x][0]), (string)CommandPointsArray[x][1]);
                    }
                }
            }
            //Run the apply so that it looks like where we left off.
            Apply_Click(null, null);
        }

        /// <summary>
        /// The CalCheck_CheckedChanged
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        /// 

            //HARDLY USED!
        private void CalCheck_CheckedChanged(object sender, EventArgs e)
        {
            offset.Text = "0";
            offset.Enabled = false;
            ClearCP_Click(null, null);
        }


        /// <summary>
        /// The fpstodps
        /// </summary>
        /// <param name="Vel">The Vel<see cref="float"/></param>
        /// <returns>The <see cref="float"/></returns>
        /// HARDLY USED
        public float fpstodps(float Vel)
        {
            
            float dgps = (float)((87.92 / 360.0) * (int.Parse(wheel.Text) * Math.PI * Vel / 60));

            return (float)(dgps * .02199);
        }

        /// <summary>
        /// The button4_Click
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void deploy_Click(object sender, EventArgs e)
        {
            //Check to make sure that the user have given this profile a name.
            if (profilename.Text == "")
            {
                MessageBox.Show("You must give this profile a name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Check to make sure that the user has given us a valid ip for the robot.
            if (!ValidateIPv4(ipadd.Text))
            {
                MessageBox.Show("This ip address is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Make sure that we have at least two points that we can actually make a path between.
            if (!(controlPoints.RowCount - 2 > 0))
            {
                //If not cancel this and show an error stating so.
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            //Create a temp file where we can write this information then upload it to the robot.
            String DirPath = Path.GetTempPath();    // Used for storing the directory path of the saved file.
            String JSONPath = Path.Combine(DirPath, profilename.Text + ".json");     // Used for storing the json saved file directory path.
            //This is almost the same as saving the file however this one will be a temp file which will be deleted after deploying.
            Apply_Click(null, null);
                using (var writer = new System.IO.StreamWriter(JSONPath))
                {
                    writer.WriteLine("{");
                    writer.WriteLine("  \"Data\":[ ");

                    List<string> left = new List<string>();
                    List<string> right = new List<string>();
                    List<string> center = new List<string>();

                    List<string> line = new List<string>();

                    int trackwidth = (int)((int.Parse(trackWidth.Text)) / 2);

                    float[] l = paths.getOffsetVelocityProfile(trackwidth).ToArray();
                    List<float> ld = paths.getOffsetDistanceProfile(trackwidth);

                    float[] r;
                    List<float> rd = new List<float>(); ;

                    float[] c = paths.getOffsetVelocityProfile(0).ToArray();
                    List<float> cd = paths.getOffsetDistanceProfile(0);

                    float[] angles = paths.getHeadingProfile();

                    r = paths.getOffsetVelocityProfile(-trackwidth).ToArray();
                    rd = paths.getOffsetDistanceProfile(-trackwidth);


                r.NoiseReduction(int.Parse(smoothness.Text));
                    rd.NoiseReduction(int.Parse(smoothness.Text));
                    l.NoiseReduction(int.Parse(smoothness.Text));
                    ld.NoiseReduction(int.Parse(smoothness.Text));
                    c.NoiseReduction(int.Parse(smoothness.Text));
                    cd.NoiseReduction(int.Parse(smoothness.Text));





                Dictionary<int, String> commandPoints = new Dictionary<int, String>();
                foreach (DataGridViewRow row in commandPointsList.Rows)
                {
                    if (row.Cells[0].Value != null)
                    {
                        if (row.Cells[0].Value.ToString() != "")
                        {
                            if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                            {
                                if (row.Cells[1].Value != null)
                                    commandPoints[int.Parse(row.Cells[0].Value.ToString())] = row.Cells[1].Value.ToString();
                            }
                        }
                    }
                }
                for (int i = 0; i < l.Length; i++)
                {
                    String text = "";
                    if (commandPoints.ContainsKey(i))
                    {
                        text = commandPoints[i];
                    }
                    if (CTRE.Checked)
                    {
                        double dConvert = Math.PI * double.Parse(wheel.Text) * 25.4;

                        line.Add("  {   \"Rotation\":" + cd.Take(i).Sum() / dConvert + " , " + "\"Velocity\":" + (c[i] / dConvert * 60).ToString() + " , " + "\"Time\":" + paths[0].velocityMap.time * 1000 + " , " + "\"Angle\":" + angles[i] + " , " + "\"State\":" + "\"" + text + "\"" + "}");

                    }
                    else
                    {
                        line.Add("  {   \"Rotation\":" + cd.Take(i).Sum().ToString() + " , " + "\"Velocity\":" + c[i].ToString() + " , " + "\"Time\":" + paths[0].velocityMap.time * 1000 + " , " + "\"Angle\":" + angles[i] + " , " + "\"State\":" + "\"" + text + "\"" + "}");
                    }
                }
                right.Add(string.Join(",\n", line));

                    foreach (string ret in right)
                    {
                        writer.WriteLine(ret);
                    }
                    writer.WriteLine("  ] ");
                    writer.WriteLine("} ");
                }

            //Create a sftp client that we will use to upload the file to the robot.
            SftpClient sftp = new SftpClient(ipadd.Text, user.Text, pass.Text);

            try
            {
                //Change the user cursor to a wait cursor because this process can take a minute.
                this.Cursor = Cursors.WaitCursor;
                //Connect to the sftp
                try
                {
                    sftp.Connect();
                }
                catch (Renci.SshNet.Common.SshConnectionException e1)
                {
                    //Make sure that we are connected to the robot.
                    Console.WriteLine("IOException source: {0}", e1.StackTrace);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (System.Net.Sockets.SocketException e1)
                {
                    //Make sure that we are connected to the robot.
                    Console.WriteLine("IOException source: {0}", e1.StackTrace);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    //try to create a new directory it will fail if it already exists which is ok.
                    sftp.CreateDirectory("/home/lvuser/Motion_Profiles");

                }
                catch (Renci.SshNet.Common.SftpPermissionDeniedException e1)
                {
                    //Make sure that the user has the access to make/put a file here.
                    Console.WriteLine("IOException source: {0}", e1.StackTrace);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Permission Denied By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Renci.SshNet.Common.SftpPathNotFoundException e1)
                {
                    //Make sure that the main directory they gave us actually exists.
                    Console.WriteLine("IOException source: {0}", e1.StackTrace);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Path Not Found By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (System.Net.Sockets.SocketException)
                {

                }
                catch (Exception e1)
                {

                }
                //Open that file that we just saved to a temp file.
                using (FileStream fileStream = File.OpenRead(JSONPath))
                {
                    //Load and upload the file.
                    MemoryStream memStream = new MemoryStream();
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                    sftp.UploadFile(memStream, Path.Combine("/home/lvuser/Motion_Profiles/", profilename.Text + ".json"));
                }
            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException e1)
            {
                //Make sure that the user has the access to make/put a file here.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Permission Denied By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException e1)
            {
                //Make sure that the main directory they gave us actually exists.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Path Not Found By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Renci.SshNet.Common.SshConnectionException e1)
            {
                //Make sure that we are connected to the robot.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //The process is done so change the cursor back.
            this.Cursor = Cursors.Default;
            //Good the program did not fail and tell the user.
            MessageBox.Show("Success", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //We are done with the upload so lets disconnect the sftp client.
            sftp.Disconnect();
            //Sleep a second before deleting the temp json file.
            System.Threading.Thread.Sleep(100);
            File.Delete(JSONPath);
        }

        /// <summary>
        /// Used to validate the ip address of the robot to make sure that it is in an ipv4 format.
        /// </summary>
        /// <param name="ipString">The ip string value.</param>
        /// <returns>a boolean that tells you if the ip is in ipv4 format.</returns>
        public bool ValidateIPv4(string ipString)
        {
            // if the text contains a whitespace/space or a null value then it is clearly not a ip address.
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }
            //Split the ip address into different parts
            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;
            //check to see if all of the values are bytes.
            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        private void refresh_button_Click(object sender, EventArgs e)
        {
            RioFiles.Items.Clear();
            //RioFiles.Items.Add("TTTTTTTTTTTTTESttttttttttttttttttttttttT");
            //Check to make sure that the user has given us a valid ip for the robot.
            if (!ValidateIPv4(ipadd.Text))
            {
                MessageBox.Show("This ip address is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //Create a sftp client that we will use to get the file list from the robot.
            SftpClient sftp = new SftpClient(ipadd.Text, user.Text, pass.Text);
            try
            {
                sftp.Connect();
                if(!sftp.Exists("/home/lvuser/Motion_Profiles/"))
                {
                    MessageBox.Show("Motion_Profiles Folder Not Found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                List<Renci.SshNet.Sftp.SftpFile> files = sftp.ListDirectory("/home/lvuser/Motion_Profiles/").ToList();

                foreach(Renci.SshNet.Sftp.SftpFile file in files)
                {
                    if(!file.Name.Equals("..") && !file.Name.Equals("."))
                    {
                        RioFiles.Items.Add(file.Name);
                    }
                }

                sftp.Disconnect();

            }
            catch (Renci.SshNet.Common.SftpPermissionDeniedException e1)
            {
                //Make sure that the user has the access to make/put a file here.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Permission Denied By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Renci.SshNet.Common.SftpPathNotFoundException e1)
            {
                //Make sure that the main directory they gave us actually exists.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Path Not Found By Host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Renci.SshNet.Common.SshConnectionException e1)
            {
                //Make sure that we are connected to the robot.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (System.Net.Sockets.SocketException e1)
            {
                //Make sure that we are connected to the robot.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                this.Cursor = Cursors.Default;
                MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            


        }

        private void GridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            switch (GridCheckBox.CheckState)
            {
                case CheckState.Checked:
                    mainField.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                    mainField.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
                    break;
                case CheckState.Unchecked:
                    mainField.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                    mainField.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
                    break;
                case CheckState.Indeterminate:
                    mainField.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                    mainField.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
                    break;
            }
        }


    }
}
