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
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.DataVisualization.Charting;
    using MotionProfile;
    using static MotionProfile.ControlPoint;
    using MotionProfile.Spline;

    /// <summary>
    /// Defines the <see cref="MainForm" />
    /// </summary>
    public partial class MainForm : Form
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

        public List<ControlPoint> controlPointArray = new List<ControlPoint>();

        public OutputPoints outputPoints = new OutputPoints();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            //Create the window with all the fancy buttons.
            InitializeComponent();


        }

        /// <summary>
        /// The Form1_Load
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            //TODO: Remove fieldpoints.txt comment and review methods it describes

            //Put all of the points from the fieldpoints.txt and put them on the field
            SetupMainField();
            SetupPlots();

        }

        /// <summary>
        /// Configures what the main field looks like.
        /// </summary>
        private void SetupMainField()
        {

            //this sets up what the graph domain and range is and what our increments are.
            mainField.ChartAreas["field"].Axes[0].Maximum = fieldWidth;
            mainField.ChartAreas["field"].Axes[0].Interval = 1000;
            mainField.ChartAreas["field"].Axes[0].Minimum = 0;

            mainField.ChartAreas["field"].Axes[1].Maximum = fieldHeight;
            mainField.ChartAreas["field"].Axes[1].Interval = 1000;
            mainField.ChartAreas["field"].Axes[1].Minimum = 0;


            mainField.Series["background"].Points.AddXY(0, 0);
            mainField.Series["background"].Points.AddXY(fieldWidth, fieldHeight);


        }

        /// <summary>
        /// Configure what the velocity chart and the distance chart look like
        /// </summary>
        private void SetupPlots()
        {

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

            SetupMainField();
        }


        /// <summary>
        /// The event that is called when the user clicks on the main field chart.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="MouseEventArgs"/></param>
        /// 
        /// <summary>
        /// The currently selected point.
        /// </summary>
        private DataPoint clickedPoint;
        private Boolean skipNextClick = false;

        private void MainField_MouseClick(object sender, MouseEventArgs e)
        {
            if (!skipNextClick)
            {
                //if the button click is a left mouse click then add a positive point to the field chart.
                if (e.Button == MouseButtons.Left)
                {
                    Chart c = (Chart)sender;

                    double x = c.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    double y = c.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);

                    if (x > 0 && y > 0 && x <= fieldWidth && y <= fieldHeight)
                    {
                        ControlPointTable.Rows[ControlPointTable.Rows.Add((int)x, (int)y, "+", "")].Selected = true;
                    }
                    DrawControlPoints();
                }

                //if the button click is a right mouse click then add a negative point to the field chart.

                if (e.Button == MouseButtons.Right)
                {
                    Chart c = (Chart)sender;

                    double x = c.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                    double y = c.ChartAreas[0].AxisY.PixelPositionToValue(e.Y);


                    if (x > 0 && y > 0 && x <= fieldWidth && y <= fieldHeight)
                    {
                        c.Series["cp"].Points.AddXY(x, y);
                        mainField.Series["cp"].Points.Last().Color = Color.Red;

                        ControlPointTable.Rows[ControlPointTable.Rows.Add((int)x, (int)y, "-", Int32.Parse(maxVelocity.Text))].Selected = true;
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

                        commandPointsList.Rows[commandPointsList.Rows.Add(mainField.Series["path"].Points.IndexOf(p), "")].Selected = true;
                    }
                }
            }
            skipNextClick = false;
            clickedPoint = null;
        }

        /// <summary>
        /// The event that is called when the user clicks and holds on the main field chart.
        /// </summary>
        private void MainField_MouseDown(object sender, MouseEventArgs e)
        {
            //runs when the mouse is pressed down.
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                Chart c = (Chart)sender;

                ChartArea ca = c.ChartAreas["field"];
                Axis ax = ca.AxisX;
                Axis ay = ca.AxisY;
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
                    clickedPoint = hit.Series.Points[hit.PointIndex];
                    foreach (DataGridViewRow row in ControlPointTable.Rows)
                    {
                        if (RowContainData(row, true))
                        {
                            skipNextClick = true;

                            // Debug.Print(row.Cells[0].Value.ToString() + ":" + ((int)dp.XValue).ToString() + ":" + row.Cells[1].Value.ToString() + ":" + ((int)dp.YValues[0]).ToString());
                            if (row.Cells[0].Value.ToString() == ((int)clickedPoint.XValue).ToString() && row.Cells[1].Value.ToString() == ((int)clickedPoint.YValues[0]).ToString())
                            {
                                //move the point
                                double dx = (int)ax.PixelPositionToValue(e.X);
                                double dy = (int)ay.PixelPositionToValue(e.Y);

                                clickedPoint.XValue = dx;
                                clickedPoint.YValues[0] = dy;
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

        /// <summary>
        /// The event that is called when the user mouse while above the main field.
        /// </summary>
        private void MainField_MouseMove(object sender, MouseEventArgs e)
        {
            //if the user is holding the left button while moving the mouse allow them to move the point.
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                Chart c = (Chart)sender;

                ChartArea ca = c.ChartAreas[0];
                Axis ax = ca.AxisX;
                Axis ay = ca.AxisY;
                if (clickedPoint != null)
                {
                    double dx = (int)ax.PixelPositionToValue(e.X);
                    double dy = (int)ay.PixelPositionToValue(e.Y);

                    clickedPoint.XValue = dx;
                    clickedPoint.YValues[0] = dy;
                    ControlPointTable.Rows[rowIndex].Cells[0].Value = dx;
                    ControlPointTable.Rows[rowIndex].Cells[1].Value = dy;

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
        /// The currently selected row from the RioFilesList table.
        /// </summary>
        private int RioFilesRowIndex;


        /// <summary>
        /// The event that is called when the user clicks the invert button.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        private void Invert_Click(object sender, EventArgs e)
        {
            //goes though ever row and changes the x value from the left side to the right side by taking the field width and subracting the current x value.
            foreach (DataGridViewRow row in ControlPointTable.Rows)
            {
                if (RowContainData(row, true))
                    row.Cells[0].Value = this.fieldWidth - float.Parse(row.Cells[0].Value.ToString());
            }
            Apply_Click(sender, e);
        }

       /// <summary>
        /// The event that is called when a rows state is changed ex: the row is selected.
        /// </summary>
        private void ControlPointsTable_RowStateChange(object sender, DataGridViewRowStateChangedEventArgs e)
        {

            if (e.Row.Cells[0].Value == null && e.Row.Cells[1].Value == null && e.Row.Cells[1].Value == null)
            {
                return;
            }
            if (e.Row.Cells[0].Value == null || e.Row.Cells[0].Value.ToString() == "")
            {
                e.Row.Cells[0].Value = 100;
            }
            if (e.Row.Cells[1].Value == null || e.Row.Cells[1].Value.ToString() == "")
            {
                e.Row.Cells[1].Value = 100;
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
                if (ControlPointTable.Rows.Count - 2 != 0)
                {
                    //Go though each row.
                    foreach (DataGridViewRow row in ControlPointTable.Rows)
                    {
                        //Make sure that the row that is being selected is one of the ones that might have data.
                        if (RowContainData(row, true))
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
                            controlPointArray[row.Index].Selected = false;
                        }


                    }
                }

                //Make sure that we at least have 1 point otherwise don't run this.
                if (ControlPointTable.Rows.Count - 2 != 0)
                {
                    //Make sure that the row that is being selected is one of the ones that might have data.
                    if (e.Row.Index >= 0 && e.Row.Index <= ControlPointTable.Rows.Count - 2)
                    {
                        //Change the selected point to the color yellow.
                        mainField.Series["cp"].Points[e.Row.Index].Color = Color.Yellow;
                        controlPointArray[e.Row.Index].Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// The event that is called when a rows state is changed ex: the row is selected.
        /// </summary>
        private void CommandPoints_RowStateChange(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
            {
                return;
            }

            foreach (DataGridViewRow row in commandPointsList.Rows)
            {
                if (RowContainData(row, true))
                {

                    if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                    {
                        DataPoint p = mainField.Series["path"].Points[int.Parse(row.Cells[0].Value.ToString())];
                        p.Color = Color.Red;
                    }

                }
            }

            if (RowContainData(e.Row, true))
            {

                if (mainField.Series["path"].Points.Count >= int.Parse(e.Row.Cells[0].Value.ToString()))
                {
                    DataPoint p = mainField.Series["path"].Points[int.Parse(e.Row.Cells[0].Value.ToString())];
                    p.Color = Color.Blue;
                    p.MarkerStyle = MarkerStyle.Triangle;
                    p.MarkerSize = 10;
                }

            }
        }

        /// <summary>
        /// The event that is called when the user stopes editing a cell.
        /// </summary>
        private void ControlPoints_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            //Check to see if the user is editing a cell that is in the third column.

            if (ControlPointTable.CurrentRow.Cells[0].Value == null && ControlPointTable.CurrentRow.Cells[1].Value == null && ControlPointTable.CurrentRow.Cells[1].Value == null)
            {
                return;
            }
            if (ControlPointTable.CurrentRow.Cells[0].Value == null || ControlPointTable.CurrentRow.Cells[0].Value.ToString() == "")
            {
                ControlPointTable.CurrentRow.Cells[0].Value = 100;
            }
            if (ControlPointTable.CurrentRow.Cells[1].Value == null || ControlPointTable.CurrentRow.Cells[1].Value.ToString() == "")
            {
                ControlPointTable.CurrentRow.Cells[1].Value = 100;
            }
            if (ControlPointTable.CurrentRow.Cells[2].Value == null || ControlPointTable.CurrentRow.Cells[2].Value.ToString() == "")
            {
                ControlPointTable.CurrentRow.Cells[2].Value = "+";
            }

            try
            {
                float.Parse(ControlPointTable.CurrentRow.Cells[0].Value.ToString());
            }
            catch (Exception)
            {
                ControlPointTable.CurrentRow.Cells[0].Value = 100;
            }
            try
            {
                float.Parse(ControlPointTable.CurrentRow.Cells[1].Value.ToString());
            }
            catch (Exception)
            {
                ControlPointTable.CurrentRow.Cells[1].Value = 100;
            }

            if (e.ColumnIndex == 2)
            {
                //If the cell contains a + or a - the ignore it. Else change the cell text to be a + signs.
                if (ControlPointTable.CurrentCell.Value.ToString() == "+" || ControlPointTable.CurrentCell.Value.ToString() == "-")
                {
                }
                else
                {
                    ControlPointTable.CurrentCell.Value = "+";
                }
            }
            DrawControlPoints();
        }
        private void CommandPoints_CellEndEdit(object sender, DataGridViewCellEventArgs e)
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
        private void ControlPoints_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            //make sure that the button that was released was the right mouse button.
            if (e.Button == MouseButtons.Right)
            {
                //Make sure that the cell that was selected was a cell that is real
                if (e.RowIndex >= 0)
                {
                    //on mouse up select that row.
                    this.ControlPointTable.Rows[e.RowIndex].Selected = true;
                    //When the row is selected set the rowindex to the index of the row that was just selected. (aka update the rowIndex value)
                    this.rowIndex = e.RowIndex;
                    //set the tables currentcell to the cell we just clicked.
                    this.ControlPointTable.CurrentCell = this.ControlPointTable.Rows[e.RowIndex].Cells[1];
                    //since we right clicked we open a context strip with things that allow us to delete and move the current row.
                    var relativeMousePosition = this.ControlPointTable.PointToClient(System.Windows.Forms.Cursor.Position);
                    this.contextMenuStrip2.Show(this.ControlPointTable, relativeMousePosition);
                }


            }

        }
        private void CommandPoints_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
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
            if (rowIndex != ControlPointTable.RowCount - 1)
            {
                //Delete the row that is selected.
                ControlPointTable.Rows.RemoveAt(rowIndex);
            }
            //Reload the points because we just deleted one and we need the rest of the program to know.
            DrawControlPoints();
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
            DrawControlPoints();
            Apply_Click(null, null);
        }

        /// <summary>
        /// The event that is called when the user clicks the clear button.
        /// </summary>
        private void ClearCP_Click(object sender, EventArgs e)
        {
            //Clear all of the rows in the controlpoints table.
            ControlPointTable.Rows.Clear();
            commandPointsList.Rows.Clear();
            //Clear all of the plots.
            mainField.Series["cp"].Points.Clear();
            mainField.Series["path"].Points.Clear();
            mainField.Series["left"].Points.Clear();
            mainField.Series["right"].Points.Clear();

            kinematicsChart.Series["Position"].Points.Clear();
            kinematicsChart.Series["Velocity"].Points.Clear();
            kinematicsChart.Series["Acceleration"].Points.Clear();



            AnglePlot.Series["angle"].Points.Clear();


        }

        /// <summary>
        /// The event that is called when the user clicks the insert above button in the context stip.
        /// </summary>
        private void InsertAbove_Click(object sender, EventArgs e)
        {
            //insert a new row at the selected index. (this will push the current index down one.)
            mainField.Series["cp"].Points.AddXY(100, 100);
            ControlPointTable.Rows.Insert(rowIndex, 100, 100, "+");
            DrawControlPoints();

        }

        /// <summary>
        /// The event that is called when the user clicks the insert above button in the context stip.
        /// </summary>
        private void InsertAbove_Click_commandPoints(object sender, EventArgs e)
        {
            //insert a new row at the selected index. (this will push the current index down one.)
            commandPointsList.Rows.Insert(commandRowIndex);

        }

        /// <summary>
        /// The event that is called when the user clicks the insert below button in the context stip.
        /// </summary>

        private void InsertBelow_Click(object sender, EventArgs e)
        {
            //insert a new row at the selected index plus one.
            ControlPointTable.Rows.Insert(rowIndex + 1, 100, 100, "+");
            mainField.Series["cp"].Points.AddXY(100, 100);

            DrawControlPoints();
        }

        /// <summary>
        /// The event that is called when the user clicks the insert below button in the context stip.
        /// </summary>

        private void InsertBelow_Click_commandPoints(object sender, EventArgs e)
        {
            //insert a new row at the selected index plus one.
            if (!(commandPointsList.Rows.Count >= commandRowIndex))
                commandPointsList.Rows.Insert(commandRowIndex + 1);

        }

        /// <summary>
        /// The event that is called when the user clicks the move up button in the context stip.
        /// </summary>
        private void BtnUp_Click(object sender, EventArgs e)
        {
            //lets convert our object name because I copied this from the internet and am to lazy to change it.
            DataGridView dgv = ControlPointTable;
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
            DrawControlPoints();
        }

        /// <summary>
        /// The event that is called when the user clicks the move up button in the context stip.
        /// </summary>
        private void BtnUp_Click_commandPoints(object sender, EventArgs e)
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
        private void BtnDown_Click(object sender, EventArgs e)
        {
            DataGridView dgv = ControlPointTable;
            try
            {
                //lets convert our object name because I copied this from the internet and am to lazy to change it.

                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (rowIndex == totalRows - 2)
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
            DrawControlPoints();
        }

        /// <summary>
        /// The event that is called when the user clicks the move down button in the context stip.
        /// </summary>
        private void BtnDown_Click_commandPoints(object sender, EventArgs e)
        {
            DataGridView dgv = commandPointsList;
            try
            {
                //lets convert our object name because I copied this from the internet and am to lazy to change it.

                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int commandRowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (commandRowIndex == totalRows - 2)
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
        private Rectangle MakeRectangle(int[] array, bool adjustToScreen = false)
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
        private void DrawControlPoints()
        {
            //Clear all of the points from the main field controlpoint series.
            mainField.Series["cp"].Points.Clear();

            foreach(ControlPoint controlpoint in controlPointArray)
            {

                controlpoint.setGraphIndex(mainField.Series["cp"].Points.AddXY(controlpoint.X, controlpoint.Y));
                if (controlpoint.Selected == true)
                {
                    mainField.Series["cp"].Points[controlpoint.getGraphIndex()].Color = Color.Yellow;

                }
                else if (controlpoint.isReverse())
                {
                    mainField.Series["cp"].Points.Last().Color = Color.Red;
                }
                
            }
        }

        /// <summary>
        /// The event that is called when the user clicks that apply button.
        /// </summary>
        private void Apply_Click(object sender, EventArgs e)
        {
            double maxV = 0;
            double maxA = 0;
            double maxJ = 0;

            updateControlPointArray();
            if (!(controlPointArray.Count >1))
            {
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (maxVelocity.Text==null || maxVelocity.Text == "" || !double.TryParse(maxVelocity.Text.ToString(), out maxV))
            {
                MessageBox.Show("Max Velocity Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (maxAcc.Text == null || maxAcc.Text == "" || !double.TryParse(maxAcc.Text.ToString(), out maxA))
            {
                MessageBox.Show("Max Acceleration Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (maxJerk.Text == null || maxJerk.Text == "" || !double.TryParse(maxJerk.Text.ToString(), out maxJ))
            {
                MessageBox.Show("Max Jerk Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            mainField.Series["path"].Points.Clear();
            kinematicsChart.Series["Position"].Points.Clear();
            kinematicsChart.Series["Velocity"].Points.Clear();
            kinematicsChart.Series["Acceleration"].Points.Clear();
            Random rnd = new Random();

            outputPoints = new OutputPoints();

            List<ControlPoints> directionPoints = new List<ControlPoints>();
            ControlPointDirection lastDirection = controlPointArray.First().Direction;
            ControlPoints points = new ControlPoints();
            points.direction = controlPointArray.First().Direction;


            foreach (ControlPoint point in controlPointArray)
            {
                points.points.Add(point);
                if (point.Direction != lastDirection)
                {
                    if (points.points.Count>=2)
                    {
                        directionPoints.Add(points);
                    }
                    points = new ControlPoints();
                    points.direction = point.Direction;
                    points.points.Add(point);

                }
                lastDirection = point.Direction;
            }
            if (points.points.Count >= 2)
            {
                directionPoints.Add(points);
            }

            double Posoffset = 0;
            double Timeoffset = 0;
            double angleOffset = 0;
            Boolean isFirst = true;
            List<SplinePoint> pointList = new List<SplinePoint>();
            int count = 0;

            foreach (ControlPoints ps in directionPoints)
            {
                SplinePath.GenSpline(ps.points);
                VelocityGenerator test = new VelocityGenerator(maxV, maxA, maxJ, ps.direction, .01);
                List<VelocityPoint> velocityPoints = test.GeneratePoints(SplinePath.getLength());

                List<ControlPointSegment> spline = SplinePath.GenSpline(ps.points, velocityPoints);


                foreach (ControlPointSegment seg in spline)
                {
                    Color randomColor;

                    randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));


                    foreach (SplinePoint point in seg.points)
                    {
                        mainField.Series["path"].Points.AddXY(point.X, point.Y);
                        point.Direction = ps.direction;
                        pointList.Add(point);
                        mainField.Series["path"].Points.Last().Color = randomColor;
                    }
                }
               
                foreach (VelocityPoint point in velocityPoints)
                {
                    outputPoints.position.Add(point.Pos + Posoffset);
                    outputPoints.time.Add(point.Time + Timeoffset);
                    outputPoints.velocity.Add(point.Vel);
                    outputPoints.acceleration.Add(point.Acc);

                }
                Posoffset = outputPoints.position.Last();
                Timeoffset = outputPoints.time.Last();
                angleOffset = outputPoints.time.Last();

            }
            Console.WriteLine(pointList.Count);

            Console.WriteLine(outputPoints.position.Count);

            for (int x = 0; x < outputPoints.position.Count; x++)
            {
                kinematicsChart.Series["Position"].Points.AddXY(outputPoints.time[x], outputPoints.position[x]);
                kinematicsChart.Series["Velocity"].Points.AddXY(outputPoints.time[x], outputPoints.velocity[x]);
                kinematicsChart.Series["Acceleration"].Points.AddXY(outputPoints.time[x], outputPoints.acceleration[x]);
                //AnglePlot.Series["Angle"].Points.AddXY(outputPoints.time[x], outputPoints.angle[x]);
            }



        }

        private float findStartAngle(double x2, double x1, double y2, double y1)
        {
            double CONVERT = 180.0 / Math.PI;
            float ang = 0;
            float chx = (float)(x2 - x1);
            float chy = (float)(y2 - y1);
            if (chy == 0)
            {
                if (chx >= 0) ang = 0;
                else ang = 180;
            }
            else if (chy > 0)
            {                         // X AND Y ARE REVERSED BECAUSE OF MOTION PROFILER STUFF
                if (chx > 0)
                {
                    // positive x, positive y, 90 - ang, quad 1
                    ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 1; // represents quadrants.
                }
                else
                {
                    // positive x, negative y, 90 + ang, quad 2
                    ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 2;
                }
            }
            else
            {
                if (chx > 0)
                {
                    // negative x, positive y, 270 + ang, quad 4
                    ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 4;
                }
                else
                {
                    // negative x, negative y, 270 - ang, quad 3
                    ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 3;
                }
            }
            return ang;
        }

        private double findAngleChange(double x2, double x1, double y2, double y1, double prevAngle, ControlPointDirection direction)
        {
            double CONVERT = 180.0 / Math.PI;
            float ang = 0;
            float chx = (float)(x2 - x1);
            float chy = (float)(y2 - y1);
            if (chy == 0)
            {
                if (chx >= 0) ang = 0;
                else ang = 180;
            }
            else if (chy > 0)
            {                         // X AND Y ARE REVERSED BECAUSE OF MOTION PROFILER STUFF
                if (chx > 0)
                {
                    // positive x, positive y, 90 - ang, quad 1
                    ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 1; // represents quadrants.
                }
                else
                {
                    // positive x, negative y, 90 + ang, quad 2
                    ang = (float)(90 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 2;
                }
            }
            else
            {
                if (chx > 0)
                {
                    // negative x, positive y, 270 + ang, quad 4
                    ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 4;
                }
                else
                {
                    // negative x, negative y, 270 - ang, quad 3
                    ang = (float)(270 - CONVERT * (Math.Atan(chx / chy)));
                    //ang = (float)(CONVERT * Math.Atan(chx / chy));
                    //ang = 3;
                }
            }

            if (direction == ControlPointDirection.REVERSE)
            {
                int add = 0;
                if (ang > 0)
                    add = -180;
                if (ang < 0)
                    add = 180;
                ang = ang + add;
            }

            double angleChange = ang - prevAngle;
            if (angleChange > 300) angleChange -= 360;
            if (angleChange < -300) angleChange += 360;
            return (prevAngle + angleChange);
        }

        /// <summary>
        /// The event that is called when the save button is clicked.
        /// </summary>
        /*private void Save_Click(object sender, EventArgs e)
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

                        float[] l = paths.GetOffsetVelocityProfile(trackwidth).ToArray();
                        List<float> ld = paths.GetOffsetDistanceProfile(trackwidth);

                        float[] r;
                        List<float> rd = new List<float>(); ;

                        float[] c = paths.GetOffsetVelocityProfile(0).ToArray();
                        List<float> cd = paths.GetOffsetDistanceProfile(0);

                        float[] angles = paths.GetHeadingProfile();




                        r = paths.GetOffsetVelocityProfile(-trackwidth).ToArray();
                        rd = paths.GetOffsetDistanceProfile(-trackwidth);


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
                            if (RowContainData(row, true))
                            {

                                if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                                {
                                        commandPoints[int.Parse(row.Cells[0].Value.ToString())] = row.Cells[1].Value.ToString();
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
                    //Call the WriteSetupFile that will write a file so that we can go back and load these points again.
                    WriteSetupFile(MPPath);

                }

            }
        }*/

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

                writer.WritePropertyName("CTRE");
                writer.WriteValue(CTRE.Checked.ToString());

                writer.WritePropertyName("isntaVel");
                writer.WriteValue(isntaVel.Checked.ToString());

                writer.WritePropertyName("Profile Name");
                writer.WriteValue(profilename.Text);



                //put our points in as an array.
                writer.WritePropertyName("Points");
                writer.WriteStartArray();

                foreach (DataGridViewRow row in ControlPointTable.Rows)
                {
                    if (RowContainData(row, false))
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
                    if (RowContainData(row, false))
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
                    ControlPointTable.Rows.Clear();
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
                    CTRE.Checked = Boolean.Parse((string)o["CTRE"]);
                    isntaVel.Checked = Boolean.Parse((string)o["isntaVel"]);

                    profilename.Text = (string)o["Profile Name"];

                    JArray a = (JArray)o["Points"];

                    for (int x = 0; x <= a.Count - 1; x++)
                    {
                        ControlPointTable.Rows.Add(float.Parse((string)a[x][0]), float.Parse((string)a[x][1]), (string)a[x][2]);
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
        /// The fpstodps
        /// </summary>
        /// <param name="Vel">The Vel<see cref="float"/></param>
        /// <returns>The <see cref="float"/></returns>
        /// HARDLY USED
        public float Fpstodps(float Vel)
        {

            float dgps = (float)((87.92 / 360.0) * (int.Parse(wheel.Text) * Math.PI * Vel / 60));

            return (float)(dgps * .02199);
        }

        /// <summary>
        /// The button4_Click
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        /*private void Deploy_Click(object sender, EventArgs e)
        {
            //Check to make sure that the user have given this profile a name.
            if (profilename.Text == "")
            {
                MessageBox.Show("You must give this profile a name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            String MPPath = Path.Combine(DirPath, profilename.Text + ".mp");         // Used for storing the mp saved file directory path.
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

                float[] l = paths.GetOffsetVelocityProfile(trackwidth).ToArray();
                List<float> ld = paths.GetOffsetDistanceProfile(trackwidth);

                float[] r;
                List<float> rd = new List<float>(); ;

                float[] c = paths.GetOffsetVelocityProfile(0).ToArray();
                List<float> cd = paths.GetOffsetDistanceProfile(0);

                float[] angles = paths.GetHeadingProfile();               r = paths.GetOffsetVelocityProfile(-trackwidth).ToArray();
                rd = paths.GetOffsetDistanceProfile(-trackwidth);


                r.NoiseReduction(int.Parse(smoothness.Text));
                rd.NoiseReduction(int.Parse(smoothness.Text));
                l.NoiseReduction(int.Parse(smoothness.Text));
                ld.NoiseReduction(int.Parse(smoothness.Text));
                c.NoiseReduction(int.Parse(smoothness.Text));
                cd.NoiseReduction(int.Parse(smoothness.Text));





                Dictionary<int, String> commandPoints = new Dictionary<int, String>();
                foreach (DataGridViewRow row in commandPointsList.Rows)
                {
                    if (RowContainData(row, true))
                    {

                        if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                        {

                            commandPoints[int.Parse(row.Cells[0].Value.ToString())] = row.Cells[1].Value.ToString();
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
            WriteSetupFile(MPPath);

            //Create a sftp client that we will use to upload the file to the robot.
            SftpClient sftp = new SftpClient(Properties.Settings.Default.IpAddress, Properties.Settings.Default.Username, Properties.Settings.Default.Password);
            

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
                    sftp.CreateDirectory(Properties.Settings.Default.RioMPPath);

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
                    MessageBox.Show("An Error Has Occured", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                catch(Exception)
                {

                }
                //Open that file that we just saved to a temp file.
                using (FileStream fileStream = File.OpenRead(JSONPath))
                {
                    //Load and upload the file.
                    MemoryStream memStream = new MemoryStream();
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                    sftp.UploadFile(memStream, Path.Combine(Properties.Settings.Default.RioMPPath, profilename.Text + ".json"));
                }

                //Open that file that we just saved to a temp file.
                using (FileStream fileStream = File.OpenRead(MPPath))
                {
                    //Load and upload the file.
                    MemoryStream memStream = new MemoryStream();
                    memStream.SetLength(fileStream.Length);
                    fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                    sftp.UploadFile(memStream, Path.Combine(Properties.Settings.Default.RioMPPath, profilename.Text + ".mp"));
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
            MessageBox.Show("Success", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            //We are done with the upload so lets disconnect the sftp client.
            sftp.Disconnect();
            //Sleep a second before deleting the temp json file.
            System.Threading.Thread.Sleep(100);

            File.Delete(JSONPath);
            File.Delete(MPPath);
        }*/



        private void Refresh_button_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            RioFiles.Rows.Clear();
            //RioFiles.Rows.Add("TESTTTT", DateTime.Now.ToString("HH:mm:ss dd/MM/yy"));

            //Create a sftp client that we will use to get the file list from the robot.
            SftpClient sftp = new SftpClient(Properties.Settings.Default.IpAddress, Properties.Settings.Default.Username, Properties.Settings.Default.Password);
            try
            {
                sftp.Connect();
                if (!sftp.Exists(Properties.Settings.Default.RioMPPath))
                {
                    MessageBox.Show("Motion Profiles Folder Not Found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                List<Renci.SshNet.Sftp.SftpFile> files = sftp.ListDirectory(Properties.Settings.Default.RioMPPath).ToList();


                foreach (Renci.SshNet.Sftp.SftpFile file in files)
                {
                    if (!file.Name.Equals("..") && !file.Name.Equals(".") && !file.Name.Contains(".mp"))
                    {
                        RioFiles.Rows.Add(file.Name, file.LastWriteTime.ToString("HH:mm:ss dd/MM/yy"));
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
            this.Cursor = Cursors.Default;


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
        Boolean isFileMenuItemOpen = false;

        private void FileToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            ToolStripMenuItem TSMI = sender as ToolStripMenuItem;
            TSMI.ForeColor = Color.Black;
            isFileMenuItemOpen = true;
        }

        private void FileToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            ToolStripMenuItem TSMI = sender as ToolStripMenuItem;
            TSMI.ForeColor = Color.White;
            isFileMenuItemOpen = false;
        }

        private void FileToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            ToolStripMenuItem TSMI = sender as ToolStripMenuItem;
            TSMI.ForeColor = Color.Black;
        }

        private void FileToolStripMenuItem_MouseLeave(object sender, EventArgs e)
        {
            ToolStripMenuItem TSMI = sender as ToolStripMenuItem;
            if (isFileMenuItemOpen)
                TSMI.ForeColor = Color.Black;
            else
                TSMI.ForeColor = Color.White;
        }

        private void RioFiles_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            //make sure that the button that was released was the right mouse button.
            if (e.Button == MouseButtons.Right)
            {
                //Make sure that the cell that was selected was a cell that is real
                if (e.RowIndex >= 0)
                {
                    //on mouse up select that row.
                    this.RioFiles.Rows[e.RowIndex].Selected = true;
                    //When the row is selected set the rowindex to the index of the row that was just selected. (aka update the rowIndex value)
                    this.rowIndex = e.RowIndex;
                    //set the tables currentcell to the cell we just clicked.
                    this.RioFiles.CurrentCell = this.RioFiles.Rows[e.RowIndex].Cells[1];
                    //since we right clicked we open a context strip with things that allow us to delete and move the current row.
                    var relativeMousePosition = this.ControlPointTable.PointToClient(System.Windows.Forms.Cursor.Position);
                    this.rioFilesContextMenuStrip.Show(this.ControlPointTable, relativeMousePosition);
                }


            }
        }

        private void RioFilesLoad(object sender, EventArgs e)
        {

            if (RioFiles.Rows[RioFilesRowIndex].Cells[0].Value == null)
            {
                return;
            }
            if (RioFiles.Rows[RioFilesRowIndex].Cells[0].Value.ToString().Equals(""))
            {
                return;
            }
            if (!MessageBox.Show("Your current profile will be over written are you sure you would like to contine?", "Warning!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning).Equals(DialogResult.Yes))
                return;
            SftpClient sftp = new SftpClient(Properties.Settings.Default.IpAddress, Properties.Settings.Default.Username, Properties.Settings.Default.Password);

            try
            {
                sftp.Connect();
            }
            catch (Exception e1)
            {
                //Make sure that we are connected to the robot.
                Console.WriteLine("IOException source: {0}", e1.StackTrace);
                MessageBox.Show("Unable to connect to host!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            String RioProfilePath = Path.Combine(Properties.Settings.Default.RioMPPath, RioFiles.Rows[RioFilesRowIndex].Cells[0].Value.ToString().Replace(".json", ".mp"));
            String tempFileName = Path.Combine(Path.GetTempPath(), RioFiles.Rows[RioFilesRowIndex].Cells[0].Value.ToString());
            if (!sftp.Exists(Properties.Settings.Default.RioMPPath))
            {
                MessageBox.Show("Could not find motion profile path on rio!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!sftp.Exists(RioProfilePath))
            {
                MessageBox.Show("Could not find specified motion profile on rio!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                using (var file = File.OpenWrite(tempFileName))
                {
                    sftp.DownloadFile(RioProfilePath, file);

                }
                using (var reader1 = new System.IO.StreamReader(tempFileName))
                {
                    //First clear out our points.
                    ControlPointTable.Rows.Clear();
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
                    CTRE.Checked = Boolean.Parse((string)o["CTRE"]);
                    isntaVel.Checked = Boolean.Parse((string)o["isntaVel"]);

                    profilename.Text = (string)o["Profile Name"];

                    JArray a = (JArray)o["Points"];

                    for (int x = 0; x <= a.Count - 1; x++)
                    {
                        ControlPointTable.Rows.Add(float.Parse((string)a[x][0]), float.Parse((string)a[x][1]), (string)a[x][2]);
                    }

                    JArray CommandPointsArray = (JArray)o["CommandPoints"];

                    for (int x = 0; x <= CommandPointsArray.Count - 1; x++)
                    {
                        commandPointsList.Rows.Add(int.Parse((string)CommandPointsArray[x][0]), (string)CommandPointsArray[x][1]);
                    }
                }
                //Run the apply so that it looks like where we left off.
                Apply_Click(null, null);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not download specified motion profile on rio!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

        }

        private void RioFiles_RowStateChange(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            if (e.StateChanged != DataGridViewElementStates.Selected)
            {
                return;
            }
            if (e.Row.Selected == true)
            {
                RioFilesRowIndex = e.Row.Index;
            }
        }

        private void About_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void OptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private Boolean RowContainData(DataGridViewRow row, Boolean scanWholeRow)
        {
            if (!scanWholeRow)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                        if (!cell.Value.ToString().Equals(""))
                            return true;
                }
            }
            else
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null)
                    {
                        return false;
                    }
                    if (cell.Value.ToString().Equals(""))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private void ControlPointTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            updateControlPointArray();
        }

        private void ControlPointTable_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            updateControlPointArray();
        }
        private void updateControlPointArray()
        {
            controlPointArray.Clear();
            foreach (DataGridViewRow row in ControlPointTable.Rows)
            {
                //Make sure that the row contains something that we care about.
                //If the x cell is not empty.
                if (row.Cells[0].Value == null && row.Cells[1].Value == null && row.Cells[2].Value == null)
                {
                    continue;
                }
                if (row.Cells[0].Value == null || row.Cells[0].Value.ToString().Equals(""))
                {
                    row.Cells[0].Value = 0;
                }
                if (row.Cells[1].Value == null || row.Cells[1].Value.ToString().Equals(""))
                {
                    row.Cells[1].Value = 0;
                }
                if (row.Cells[2].Value == null || row.Cells[2].Value.ToString().Equals(""))
                {
                    row.Cells[2].Value = "+";
                }

                ControlPointDirection direction = ControlPoint.ControlPointDirection.FORWARD;

                if (row.Cells[2].Value.ToString() == "-")
                {
                    direction = ControlPoint.ControlPointDirection.REVERSE;
                }
                //Add the data to the control point array.
                controlPointArray.Add(new ControlPoint(float.Parse(row.Cells[0].Value.ToString()), float.Parse(row.Cells[1].Value.ToString()), direction, row.Selected));
            }
            DrawControlPoints();
        }
    }
}

