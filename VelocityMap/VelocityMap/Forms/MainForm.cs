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

        private double CONVERT = 180.0 / Math.PI;

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

            Bitmap b = new Bitmap(VelocityMap.Properties.Resources._2019_field);
            NamedImage backImage = new NamedImage("Background", b);
            mainField.Images.Add(backImage);
            mainField.ChartAreas["field"].BackImageWrapMode = ChartImageWrapMode.Scaled;
            mainField.ChartAreas["field"].BackImage = "Background";
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
                clickedPoint = null;
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
        }

        private void mainField_MouseUp(object sender, MouseEventArgs e)
        {
            if(clickedPoint!=null)
            {
                UpdateField();
            }
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
            if (!UpdateField())
            {
                return;
            }
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

            if (!UpdateField())
            {
                return;
            }
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
            if (!UpdateField())
            {
                return;
            }
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
            if (!UpdateField())
            {
                return;
            }
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

            AngleChart.Series["Angle"].Points.Clear();


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
        /// A method that reloads the control points and redraws them on the main field plot.
        /// </summary>
        private void DrawControlPoints()
        {
            //Clear all of the points from the main field controlpoint series.
            mainField.Series["cp"].Points.Clear();

            foreach (ControlPoint controlpoint in controlPointArray)
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
            UpdateField();
        }

        private Boolean UpdateField()
        {
            double maxV = 0;
            double maxA = 0;
            double maxJ = 0;

            updateControlPointArray();
            if (!(controlPointArray.Count > 1))
            {
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (maxVelocity.Text == null || maxVelocity.Text == "" || !double.TryParse(maxVelocity.Text.ToString(), out maxV))
            {
                MessageBox.Show("Max Velocity Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (maxAcc.Text == null || maxAcc.Text == "" || !double.TryParse(maxAcc.Text.ToString(), out maxA))
            {
                MessageBox.Show("Max Acceleration Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (maxJerk.Text == null || maxJerk.Text == "" || !double.TryParse(maxJerk.Text.ToString(), out maxJ))
            {
                MessageBox.Show("Max Jerk Not Specified", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            mainField.Series["left"].Points.Clear();
            mainField.Series["right"].Points.Clear();


            mainField.Series["path"].Points.Clear();
            kinematicsChart.Series["Position"].Points.Clear();
            kinematicsChart.Series["Velocity"].Points.Clear();
            kinematicsChart.Series["Acceleration"].Points.Clear();
            AngleChart.Series["Angle"].Points.Clear();

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
                    if (points.points.Count >= 2)
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
            List<SplinePoint> pointList = new List<SplinePoint>();

            int segmentCount = 0;

            foreach (ControlPoints ps in directionPoints)
            {
                SplinePath.GenSpline(ps.points);
                VelocityGenerator test = new VelocityGenerator(maxV, maxA, maxJ, ps.direction, .01);
                List<VelocityPoint> velocityPoints = test.GeneratePoints(SplinePath.getLength());

                List<ControlPointSegment> spline = SplinePath.GenSpline(ps.points, velocityPoints);



                foreach (ControlPointSegment seg in spline)
                {
                    Color randomColor;

                    seg.PathNum = segmentCount;

                    randomColor = System.Drawing.ColorTranslator.FromHtml(indexcolors[seg.PathNum]);
                    if(seg.points.Count==0)
                    {
                        MessageBox.Show("Generation Error", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    foreach (SplinePoint point in seg.points)
                    {
                        mainField.Series["path"].Points.AddXY(point.X, point.Y);
                        point.Direction = ps.direction;
                        pointList.Add(point);
                        mainField.Series["path"].Points.Last().Color = randomColor;
                    }
                    segmentCount++;

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

            outputPoints.angle.AddRange(getHeadingList(pointList));

            for (int x = 0; x < outputPoints.position.Count; x++)
            {
                kinematicsChart.Series["Position"].Points.AddXY(outputPoints.time[x], outputPoints.position[x]);
                kinematicsChart.Series["Velocity"].Points.AddXY(outputPoints.time[x], outputPoints.velocity[x]);
                kinematicsChart.Series["Acceleration"].Points.AddXY(outputPoints.time[x], outputPoints.acceleration[x]);
                AngleChart.Series["Angle"].Points.AddXY(outputPoints.time[x], outputPoints.angle[x]);
            }

            
            foreach (SplinePoint point in buildOffsetPoints(Properties.Settings.Default.TrackWidth, pointList))
            {
                mainField.Series["right"].Points.AddXY(point.X, point.Y);
            }
            foreach (SplinePoint point in buildOffsetPoints(-Properties.Settings.Default.TrackWidth, pointList))
            {
                mainField.Series["left"].Points.AddXY(point.X, point.Y);
            }
            foreach (DataGridViewRow row in commandPointsList.Rows)
            {
                if (RowContainData(row, true))
                {
                    if (mainField.Series["path"].Points.Count >= int.Parse(row.Cells[0].Value.ToString()))
                    {
                        DataPoint p = mainField.Series["path"].Points[int.Parse(row.Cells[0].Value.ToString())];
                        p.Color = Color.Red;
                        p.MarkerStyle = MarkerStyle.Triangle;
                        p.MarkerSize = 10;
                        if (row.Selected)
                        {
                            p.Color = Color.Blue;
                        }
                    }
                }

            }

            return true;
        }

        public List<SplinePoint> buildOffsetPoints(float offset, List<SplinePoint> pointList)
        {
            List<SplinePoint> ret = new List<SplinePoint>();
            SplinePoint p1 = new SplinePoint(0, 0, 0);
            OffsetSegment prevSeg = new OffsetSegment(p1, p1);

            foreach (SplinePoint p in pointList)
            {
                SplinePoint p2 = p;

                if (p.Direction == ControlPointDirection.REVERSE)
                {
                    if (p1.X != 0 && p1.Y != 0)
                    {
                        ret.Add(new OffsetSegment(p1, p2).perp(-offset/2));
                    }
                }
                else
                {
                    if (p1.X != 0 && p1.Y != 0)
                    {
                        ret.Add(new OffsetSegment(p1, p2).perp(offset/2));
                    }

                }

                p1 = p2;

            }

            return ret;

        }

        private List<double> getHeadingList(List<SplinePoint> pointList)
        {
            List<double> headings = new List<double>();



            double startAngle = findStartAngle(pointList[2].X, pointList[1].X, pointList[2].Y, pointList[1].Y);

            for (int i = 0; i < (pointList.Count); i++) //for not zeroing the angle after each path.
            {

                if (i == 0)
                {
                    headings.Add(findStartAngle(pointList[2].X, pointList[1].X, pointList[2].Y, pointList[1].Y));
                }
                else
                {

                    headings.Add(findAngleChange(pointList[i].X, pointList[i - 1].X, pointList[i].Y, pointList[i - 1].Y, headings[headings.Count - 1], pointList[i-1].Direction));
                }
            }

            for (int i = 0; i < (pointList.Count); i++) //converts the values from raw graph angles to angles the robot can use.
            {
                double angle = headings[i];
                angle = (angle - startAngle);
                angle = -angle;

                headings[i] = angle;
            }
            //headings.NoiseReduction(10);
            return headings;
        }

        private double findStartAngle(double x2, double x1, double y2, double y1)
        {
            double xDiff = x2 - x1;
            double yDiff = y2 - y1;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }
        /// <summary>
        /// Returns the angle of this point by adding the angle change to the prevAngle.
        /// </summary>

        private double findAngleChange(double x2, double x1, double y2, double y1, double prevAngle, ControlPointDirection direction)
        {
            double target = findStartAngle(x2, x1, y2, y1);

            double a = target - prevAngle;
            double b = target - prevAngle + 360;
            double y = target - prevAngle - 360;

            double dir = a;

            double directionOffset = 0;


            if (Math.Abs(a)>Math.Abs(b))
            {
                dir = b;
                if (Math.Abs(b) > Math.Abs(y))
                {
                    dir = y;
                }
            }
            if(Math.Abs(a)>Math.Abs(y))
            {
                dir = y;
            }

            if (direction == ControlPointDirection.REVERSE)
            {
                if (dir > 0)
                {
                    directionOffset = -180;
                }
                if (dir < 0)
                {
                    directionOffset = 180;
                }
            }

            return (prevAngle + dir+ directionOffset);
        }


        /// <summary>
        /// The event that is called when the save button is clicked.
        /// </summary>
        private void Save_Click(object sender, EventArgs e)
        {
            //We are going to apply before we save so that we have the newest data.
            if(!UpdateField())
            {
                return;
            }
            //Double check that we have more than 1 point for our calculation.

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
                    for (int i = 0; i < outputPoints.position.Count; i++)
                    {
                        String text = "";
                        if (commandPoints.ContainsKey(i))
                        {
                            text = commandPoints[i];
                        }
                        double dConvert = Math.PI * double.Parse(wheel.Text) * 25.4;

                        line.Add("  {   \"Rotation\":" + outputPoints.position[i] / dConvert + " , " + "\"Velocity\":" + (outputPoints.velocity[i] / dConvert * 60).ToString() + " , " + "\"Time\":" + 10 + " , " + "\"Angle\":" + outputPoints.angle[i] + " , " + "\"State\":" + "\"" + text + "\"" + "}");

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
                    try
                    {
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

                        //Clear out our points.
                        ControlPointTable.Rows.Clear();
                        commandPointsList.Rows.Clear();

                        ClearCP_Click(null, null);

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
                    catch
                    {
                        MessageBox.Show("Error loading file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                   
                }
            }
            //Run the apply so that it looks like where we left off.
            if (!UpdateField())
            {
                return;
            }
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
        private void Deploy_Click(object sender, EventArgs e)
        {
            //Check to make sure that the user have given this profile a name.
            if (profilename.Text == "")
            {
                MessageBox.Show("You must give this profile a name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Make sure that we have at least two points that we can actually make a path between.
            if (!(controlPointArray.Count > 1))
            {
                MessageBox.Show("Not enought points!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Create a temp file where we can write this information then upload it to the robot.
            String DirPath = Path.GetTempPath();    // Used for storing the directory path of the saved file.
            String JSONPath = Path.Combine(DirPath, profilename.Text + ".json");     // Used for storing the json saved file directory path.
            String MPPath = Path.Combine(DirPath, profilename.Text + ".mp");         // Used for storing the mp saved file directory path.
                                                                                     //This is almost the same as saving the file however this one will be a temp file which will be deleted after deploying.
            if (!UpdateField())
            {
                return;
            }
            using (var writer = new System.IO.StreamWriter(JSONPath))
            {
                writer.WriteLine("{");
                writer.WriteLine("  \"Data\":[ ");



                List<string> left = new List<string>();
                List<string> right = new List<string>();
                List<string> center = new List<string>();

                List<string> line = new List<string>();

                int trackwidth = (int)((int.Parse(trackWidth.Text)) / 2);


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
                for (int i = 0; i < outputPoints.position.Count; i++)
                {
                    String text = "";
                    if (commandPoints.ContainsKey(i))
                    {
                        text = commandPoints[i];
                    }
                    double dConvert = Math.PI * double.Parse(wheel.Text) * 25.4;

                    line.Add("  {   \"Rotation\":" + outputPoints.position[i] / dConvert + " , " + "\"Velocity\":" + (outputPoints.velocity[i] / dConvert * 60).ToString() + " , " + "\"Time\":" + 10 + " , " + "\"Angle\":" + outputPoints.angle[i] + " , " + "\"State\":" + "\"" + text + "\"" + "}");

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
        }



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
                if (!UpdateField())
                {
                    return;
                }
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
        public String[] indexcolors = new String[]{
        "#000000", "#FFFF00", "#1CE6FF", "#FF34FF", "#FF4A46", "#008941", "#006FA6", "#A30059",
        "#FFDBE5", "#7A4900", "#0000A6", "#63FFAC", "#B79762", "#004D43", "#8FB0FF", "#997D87",
        "#5A0007", "#809693", "#FEFFE6", "#1B4400", "#4FC601", "#3B5DFF", "#4A3B53", "#FF2F80",
        "#61615A", "#BA0900", "#6B7900", "#00C2A0", "#FFAA92", "#FF90C9", "#B903AA", "#D16100",
        "#DDEFFF", "#000035", "#7B4F4B", "#A1C299", "#300018", "#0AA6D8", "#013349", "#00846F",
        "#372101", "#FFB500", "#C2FFED", "#A079BF", "#CC0744", "#C0B9B2", "#C2FF99", "#001E09",
        "#00489C", "#6F0062", "#0CBD66", "#EEC3FF", "#456D75", "#B77B68", "#7A87A1", "#788D66",
        "#885578", "#FAD09F", "#FF8A9A", "#D157A0", "#BEC459", "#456648", "#0086ED", "#886F4C",
        "#34362D", "#B4A8BD", "#00A6AA", "#452C2C", "#636375", "#A3C8C9", "#FF913F", "#938A81",
        "#575329", "#00FECF", "#B05B6F", "#8CD0FF", "#3B9700", "#04F757", "#C8A1A1", "#1E6E00",
        "#7900D7", "#A77500", "#6367A9", "#A05837", "#6B002C", "#772600", "#D790FF", "#9B9700",
        "#549E79", "#FFF69F", "#201625", "#72418F", "#BC23FF", "#99ADC0", "#3A2465", "#922329",
        "#5B4534", "#FDE8DC", "#404E55", "#0089A3", "#CB7E98", "#A4E804", "#324E72", "#6A3A4C",
        "#83AB58", "#001C1E", "#D1F7CE", "#004B28", "#C8D0F6", "#A3A489", "#806C66", "#222800",
        "#BF5650", "#E83000", "#66796D", "#DA007C", "#FF1A59", "#8ADBB4", "#1E0200", "#5B4E51",
        "#C895C5", "#320033", "#FF6832", "#66E1D3", "#CFCDAC", "#D0AC94", "#7ED379", "#012C58",
        "#7A7BFF", "#D68E01", "#353339", "#78AFA1", "#FEB2C6", "#75797C", "#837393", "#943A4D",
        "#B5F4FF", "#D2DCD5", "#9556BD", "#6A714A", "#001325", "#02525F", "#0AA3F7", "#E98176",
        "#DBD5DD", "#5EBCD1", "#3D4F44", "#7E6405", "#02684E", "#962B75", "#8D8546", "#9695C5",
        "#E773CE", "#D86A78", "#3E89BE", "#CA834E", "#518A87", "#5B113C", "#55813B", "#E704C4",
        "#00005F", "#A97399", "#4B8160", "#59738A", "#FF5DA7", "#F7C9BF", "#643127", "#513A01",
        "#6B94AA", "#51A058", "#A45B02", "#1D1702", "#E20027", "#E7AB63", "#4C6001", "#9C6966",
        "#64547B", "#97979E", "#006A66", "#391406", "#F4D749", "#0045D2", "#006C31", "#DDB6D0",
        "#7C6571", "#9FB2A4", "#00D891", "#15A08A", "#BC65E9", "#FFFFFE", "#C6DC99", "#203B3C",
        "#671190", "#6B3A64", "#F5E1FF", "#FFA0F2", "#CCAA35", "#374527", "#8BB400", "#797868",
        "#C6005A", "#3B000A", "#C86240", "#29607C", "#402334", "#7D5A44", "#CCB87C", "#B88183",
        "#AA5199", "#B5D6C3", "#A38469", "#9F94F0", "#A74571", "#B894A6", "#71BB8C", "#00B433",
        "#789EC9", "#6D80BA", "#953F00", "#5EFF03", "#E4FFFC", "#1BE177", "#BCB1E5", "#76912F",
        "#003109", "#0060CD", "#D20096", "#895563", "#29201D", "#5B3213", "#A76F42", "#89412E",
        "#1A3A2A", "#494B5A", "#A88C85", "#F4ABAA", "#A3F3AB", "#00C6C8", "#EA8B66", "#958A9F",
        "#BDC9D2", "#9FA064", "#BE4700", "#658188", "#83A485", "#453C23", "#47675D", "#3A3F00",
        "#061203", "#DFFB71", "#868E7E", "#98D058", "#6C8F7D", "#D7BFC2", "#3C3E6E", "#D83D66",
        "#2F5D9B", "#6C5E46", "#D25B88", "#5B656C", "#00B57F", "#545C46", "#866097", "#365D25",
        "#252F99", "#00CCFF", "#674E60", "#FC009C", "#92896B", "#1E2324", "#DEC9B2", "#9D4948",
        "#85ABB4", "#342142", "#D09685", "#A4ACAC", "#00FFFF", "#AE9C86", "#742A33", "#0E72C5",
        "#AFD8EC", "#C064B9", "#91028C", "#FEEDBF", "#FFB789", "#9CB8E4", "#AFFFD1", "#2A364C",
        "#4F4A43", "#647095", "#34BBFF", "#807781", "#920003", "#B3A5A7", "#018615", "#F1FFC8",
        "#976F5C", "#FF3BC1", "#FF5F6B", "#077D84", "#F56D93", "#5771DA", "#4E1E2A", "#830055",
        "#02D346", "#BE452D", "#00905E", "#BE0028", "#6E96E3", "#007699", "#FEC96D", "#9C6A7D",
        "#3FA1B8", "#893DE3", "#79B4D6", "#7FD4D9", "#6751BB", "#B28D2D", "#E27A05", "#DD9CB8",
        "#AABC7A", "#980034", "#561A02", "#8F7F00", "#635000", "#CD7DAE", "#8A5E2D", "#FFB3E1",
        "#6B6466", "#C6D300", "#0100E2", "#88EC69", "#8FCCBE", "#21001C", "#511F4D", "#E3F6E3",
        "#FF8EB1", "#6B4F29", "#A37F46", "#6A5950", "#1F2A1A", "#04784D", "#101835", "#E6E0D0",
        "#FF74FE", "#00A45F", "#8F5DF8", "#4B0059", "#412F23", "#D8939E", "#DB9D72", "#604143",
        "#B5BACE", "#989EB7", "#D2C4DB", "#A587AF", "#77D796", "#7F8C94", "#FF9B03", "#555196",
        "#31DDAE", "#74B671", "#802647", "#2A373F", "#014A68", "#696628", "#4C7B6D", "#002C27",
        "#7A4522", "#3B5859", "#E5D381", "#FFF3FF", "#679FA0", "#261300", "#2C5742", "#9131AF",
        "#AF5D88", "#C7706A", "#61AB1F", "#8CF2D4", "#C5D9B8", "#9FFFFB", "#BF45CC", "#493941",
        "#863B60", "#B90076", "#003177", "#C582D2", "#C1B394", "#602B70", "#887868", "#BABFB0",
        "#030012", "#D1ACFE", "#7FDEFE", "#4B5C71", "#A3A097", "#E66D53", "#637B5D", "#92BEA5",
        "#00F8B3", "#BEDDFF", "#3DB5A7", "#DD3248", "#B6E4DE", "#427745", "#598C5A", "#B94C59",
        "#8181D5", "#94888B", "#FED6BD", "#536D31", "#6EFF92", "#E4E8FF", "#20E200", "#FFD0F2",
        "#4C83A1", "#BD7322", "#915C4E", "#8C4787", "#025117", "#A2AA45", "#2D1B21", "#A9DDB0",
        "#FF4F78", "#528500", "#009A2E", "#17FCE4", "#71555A", "#525D82", "#00195A", "#967874",
        "#555558", "#0B212C", "#1E202B", "#EFBFC4", "#6F9755", "#6F7586", "#501D1D", "#372D00",
        "#741D16", "#5EB393", "#B5B400", "#DD4A38", "#363DFF", "#AD6552", "#6635AF", "#836BBA",
        "#98AA7F", "#464836", "#322C3E", "#7CB9BA", "#5B6965", "#707D3D", "#7A001D", "#6E4636",
        "#443A38", "#AE81FF", "#489079", "#897334", "#009087", "#DA713C", "#361618", "#FF6F01",
        "#006679", "#370E77", "#4B3A83", "#C9E2E6", "#C44170", "#FF4526", "#73BE54", "#C4DF72",
        "#ADFF60", "#00447D", "#DCCEC9", "#BD9479", "#656E5B", "#EC5200", "#FF6EC2", "#7A617E",
        "#DDAEA2", "#77837F", "#A53327", "#608EFF", "#B599D7", "#A50149", "#4E0025", "#C9B1A9",
        "#03919A", "#1B2A25", "#E500F1", "#982E0B", "#B67180", "#E05859", "#006039", "#578F9B",
        "#305230", "#CE934C", "#B3C2BE", "#C0BAC0", "#B506D3", "#170C10", "#4C534F", "#224451",
        "#3E4141", "#78726D", "#B6602B", "#200441", "#DDB588", "#497200", "#C5AAB6", "#033C61",
        "#71B2F5", "#A9E088", "#4979B0", "#A2C3DF", "#784149", "#2D2B17", "#3E0E2F", "#57344C",
        "#0091BE", "#E451D1", "#4B4B6A", "#5C011A", "#7C8060", "#FF9491", "#4C325D", "#005C8B",
        "#E5FDA4", "#68D1B6", "#032641", "#140023", "#8683A9", "#CFFF00", "#A72C3E", "#34475A",
        "#B1BB9A", "#B4A04F", "#8D918E", "#A168A6", "#813D3A", "#425218", "#DA8386", "#776133",
        "#563930", "#8498AE", "#90C1D3", "#B5666B", "#9B585E", "#856465", "#AD7C90", "#E2BC00",
        "#E3AAE0", "#B2C2FE", "#FD0039", "#009B75", "#FFF46D", "#E87EAC", "#DFE3E6", "#848590",
        "#AA9297", "#83A193", "#577977", "#3E7158", "#C64289", "#EA0072", "#C4A8CB", "#55C899",
        "#E78FCF", "#004547", "#F6E2E3", "#966716", "#378FDB", "#435E6A", "#DA0004", "#1B000F",
        "#5B9C8F", "#6E2B52", "#011115", "#E3E8C4", "#AE3B85", "#EA1CA9", "#FF9E6B", "#457D8B",
        "#92678B", "#00CDBB", "#9CCC04", "#002E38", "#96C57F", "#CFF6B4", "#492818", "#766E52",
        "#20370E", "#E3D19F", "#2E3C30", "#B2EACE", "#F3BDA4", "#A24E3D", "#976FD9", "#8C9FA8",
        "#7C2B73", "#4E5F37", "#5D5462", "#90956F", "#6AA776", "#DBCBF6", "#DA71FF", "#987C95",
        "#52323C", "#BB3C42", "#584D39", "#4FC15F", "#A2B9C1", "#79DB21", "#1D5958", "#BD744E",
        "#160B00", "#20221A", "#6B8295", "#00E0E4", "#102401", "#1B782A", "#DAA9B5", "#B0415D",
        "#859253", "#97A094", "#06E3C4", "#47688C", "#7C6755", "#075C00", "#7560D5", "#7D9F00",
        "#C36D96", "#4D913E", "#5F4276", "#FCE4C8", "#303052", "#4F381B", "#E5A532", "#706690",
        "#AA9A92", "#237363", "#73013E", "#FF9079", "#A79A74", "#029BDB", "#FF0169", "#C7D2E7",
        "#CA8869", "#80FFCD", "#BB1F69", "#90B0AB", "#7D74A9", "#FCC7DB", "#99375B", "#00AB4D",
        "#ABAED1", "#BE9D91", "#E6E5A7", "#332C22", "#DD587B", "#F5FFF7", "#5D3033", "#6D3800",
        "#FF0020", "#B57BB3", "#D7FFE6", "#C535A9", "#260009", "#6A8781", "#A8ABB4", "#D45262",
        "#794B61", "#4621B2", "#8DA4DB", "#C7C890", "#6FE9AD", "#A243A7", "#B2B081", "#181B00",
        "#286154", "#4CA43B", "#6A9573", "#A8441D", "#5C727B", "#738671", "#D0CFCB", "#897B77",
        "#1F3F22", "#4145A7", "#DA9894", "#A1757A", "#63243C", "#ADAAFF", "#00CDE2", "#DDBC62",
        "#698EB1", "#208462", "#00B7E0", "#614A44", "#9BBB57", "#7A5C54", "#857A50", "#766B7E",
        "#014833", "#FF8347", "#7A8EBA", "#274740", "#946444", "#EBD8E6", "#646241", "#373917",
        "#6AD450", "#81817B", "#D499E3", "#979440", "#011A12", "#526554", "#B5885C", "#A499A5",
        "#03AD89", "#B3008B", "#E3C4B5", "#96531F", "#867175", "#74569E", "#617D9F", "#E70452",
        "#067EAF", "#A697B6", "#B787A8", "#9CFF93", "#311D19", "#3A9459", "#6E746E", "#B0C5AE",
        "#84EDF7", "#ED3488", "#754C78", "#384644", "#C7847B", "#00B6C5", "#7FA670", "#C1AF9E",
        "#2A7FFF", "#72A58C", "#FFC07F", "#9DEBDD", "#D97C8E", "#7E7C93", "#62E674", "#B5639E",
        "#FFA861", "#C2A580", "#8D9C83", "#B70546", "#372B2E", "#0098FF", "#985975", "#20204C",
        "#FF6C60", "#445083", "#8502AA", "#72361F", "#9676A3", "#484449", "#CED6C2", "#3B164A",
        "#CCA763", "#2C7F77", "#02227B", "#A37E6F", "#CDE6DC", "#CDFFFB", "#BE811A", "#F77183",
        "#EDE6E2", "#CDC6B4", "#FFE09E", "#3A7271", "#FF7B59", "#4E4E01", "#4AC684", "#8BC891",
        "#BC8A96", "#CF6353", "#DCDE5C", "#5EAADD", "#F6A0AD", "#E269AA", "#A3DAE4", "#436E83",
        "#002E17", "#ECFBFF", "#A1C2B6", "#50003F", "#71695B", "#67C4BB", "#536EFF", "#5D5A48",
        "#890039", "#969381", "#371521", "#5E4665", "#AA62C3", "#8D6F81", "#2C6135", "#410601",
        "#564620", "#E69034", "#6DA6BD", "#E58E56", "#E3A68B", "#48B176", "#D27D67", "#B5B268",
        "#7F8427", "#FF84E6", "#435740", "#EAE408", "#F4F5FF", "#325800", "#4B6BA5", "#ADCEFF",
        "#9B8ACC", "#885138", "#5875C1", "#7E7311", "#FEA5CA", "#9F8B5B", "#A55B54", "#89006A",
        "#AF756F", "#2A2000", "#7499A1", "#FFB550", "#00011E", "#D1511C", "#688151", "#BC908A",
        "#78C8EB", "#8502FF", "#483D30", "#C42221", "#5EA7FF", "#785715", "#0CEA91", "#FFFAED",
        "#B3AF9D", "#3E3D52", "#5A9BC2", "#9C2F90", "#8D5700", "#ADD79C", "#00768B", "#337D00",
        "#C59700", "#3156DC", "#944575", "#ECFFDC", "#D24CB2", "#97703C", "#4C257F", "#9E0366",
        "#88FFEC", "#B56481", "#396D2B", "#56735F", "#988376", "#9BB195", "#A9795C", "#E4C5D3",
        "#9F4F67", "#1E2B39", "#664327", "#AFCE78", "#322EDF", "#86B487", "#C23000", "#ABE86B",
        "#96656D", "#250E35", "#A60019", "#0080CF", "#CAEFFF", "#323F61", "#A449DC", "#6A9D3B",
        "#FF5AE4", "#636A01", "#D16CDA", "#736060", "#FFBAAD", "#D369B4", "#FFDED6", "#6C6D74",
        "#927D5E", "#845D70", "#5B62C1", "#2F4A36", "#E45F35", "#FF3B53", "#AC84DD", "#762988",
        "#70EC98", "#408543", "#2C3533", "#2E182D", "#323925", "#19181B", "#2F2E2C", "#023C32",
        "#9B9EE2", "#58AFAD", "#5C424D", "#7AC5A6", "#685D75", "#B9BCBD", "#834357", "#1A7B42",
        "#2E57AA", "#E55199", "#316E47", "#CD00C5", "#6A004D", "#7FBBEC", "#F35691", "#D7C54A",
        "#62ACB7", "#CBA1BC", "#A28A9A", "#6C3F3B", "#FFE47D", "#DCBAE3", "#5F816D", "#3A404A",
        "#7DBF32", "#E6ECDC", "#852C19", "#285366", "#B8CB9C", "#0E0D00", "#4B5D56", "#6B543F",
        "#E27172", "#0568EC", "#2EB500", "#D21656", "#EFAFFF", "#682021", "#2D2011", "#DA4CFF",
        "#70968E", "#FF7B7D", "#4A1930", "#E8C282", "#E7DBBC", "#A68486", "#1F263C", "#36574E",
        "#52CE79", "#ADAAA9", "#8A9F45", "#6542D2", "#00FB8C", "#5D697B", "#CCD27F", "#94A5A1",
        "#790229", "#E383E6", "#7EA4C1", "#4E4452", "#4B2C00", "#620B70", "#314C1E", "#874AA6",
        "#E30091", "#66460A", "#EB9A8B", "#EAC3A3", "#98EAB3", "#AB9180", "#B8552F", "#1A2B2F",
        "#94DDC5", "#9D8C76", "#9C8333", "#94A9C9", "#392935", "#8C675E", "#CCE93A", "#917100",
        "#01400B", "#449896", "#1CA370", "#E08DA7", "#8B4A4E", "#667776", "#4692AD", "#67BDA8",
        "#69255C", "#D3BFFF", "#4A5132", "#7E9285", "#77733C", "#E7A0CC", "#51A288", "#2C656A",
        "#4D5C5E", "#C9403A", "#DDD7F3", "#005844", "#B4A200", "#488F69", "#858182", "#D4E9B9",
        "#3D7397", "#CAE8CE", "#D60034", "#AA6746", "#9E5585", "#BA6200"

    };

    }
}

