using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Gtk;
using MinCai.Simulators.Impetus;
using ZedGraph;

namespace ImpetusSharp.Demo1
{
	class MainClass
	{
		void xx ()
		{
			GraphPane myPane = new GraphPane (new RectangleF (0, 0, 640, 480), "Graph Title", "X Title", "Y Title");
			
			////////////////////////////////////////////////
			
			// Set the title and axis labels
			myPane.Title.Text = "Line Graph with Band Demo";
			myPane.XAxis.Title.Text = "Sequence";
			myPane.YAxis.Title.Text = "Temperature, C";
			
			// Enter some random data values
			double[] y = { 100, 115, 75, 22, 98, 40, 10 };
			double[] y2 = { 90, 100, 95, 35, 80, 35, 35 };
			double[] y3 = { 80, 110, 65, 15, 54, 67, 18 };
			double[] x = { 100, 200, 300, 400, 500, 600, 700 };
			
			// Fill the axis background with a color gradient
			myPane.Chart.Fill = new Fill (Color.FromArgb (255, 255, 245), Color.FromArgb (255, 255, 190), 90f);
			
			// Generate a red curve with "Curve 1" in the legend
			LineItem myCurve = myPane.AddCurve ("Curve 1", x, y, Color.Red);
			// Make the symbols opaque by filling them with white
			myCurve.Symbol.Fill = new Fill (Color.White);
			
			// Generate a blue curve with "Curve 2" in the legend
			myCurve = myPane.AddCurve ("Curve 2", x, y2, Color.Blue);
			// Make the symbols opaque by filling them with white
			myCurve.Symbol.Fill = new Fill (Color.White);
			
			// Generate a green curve with "Curve 3" in the legend
			myCurve = myPane.AddCurve ("Curve 3", x, y3, Color.Green);
			// Make the symbols opaque by filling them with white
			myCurve.Symbol.Fill = new Fill (Color.White);
			
			// Manually set the x axis range
			myPane.XAxis.Scale.Min = 0;
			myPane.XAxis.Scale.Max = 800;
			// Display the Y axis grid lines
			myPane.YAxis.MajorGrid.IsVisible = true;
			myPane.YAxis.MinorGrid.IsVisible = true;
			
			// Draw a box item to highlight a value range
			BoxObj box = new BoxObj (0, 100, 1, 30, Color.Empty, Color.FromArgb (150, Color.LightGreen));
			box.Fill = new Fill (Color.White, Color.FromArgb (200, Color.LightGreen), 45.0f);
			// Use the BehindAxis zorder to draw the highlight beneath the grid lines
			box.ZOrder = ZOrder.E_BehindCurves;
			// Make sure that the boxObj does not extend outside the chart rect if the chart is zoomed
			box.IsClippedToChartRect = true;
			// Use a hybrid coordinate system so the X axis always covers the full x range
			// from chart fraction 0.0 to 1.0
			box.Location.CoordinateFrame = CoordType.XChartFractionYScale;
			myPane.GraphObjList.Add (box);
			
			// Add a text item to label the highlighted range
			TextObj text = new TextObj ("Optimal\nRange", 0.95f, 85, CoordType.AxisXYScale, AlignH.Right, AlignV.Center);
			text.FontSpec.Fill.IsVisible = false;
			text.FontSpec.Border.IsVisible = false;
			text.FontSpec.IsBold = true;
			text.FontSpec.IsItalic = true;
			text.Location.CoordinateFrame = CoordType.XChartFractionYScale;
			text.IsClippedToChartRect = true;
			myPane.GraphObjList.Add (text);
			
			// Fill the pane background with a gradient
			myPane.Fill = new Fill (Color.WhiteSmoke, Color.Lavender, 0f);
			
			// Calculate the Axis Scale Ranges
			myPane.AxisChange ();
			
			////////////////////////////////////////////////
			
			Bitmap bm = new Bitmap (600, 800);
			using (Graphics g = Graphics.FromImage (bm))
				myPane.AxisChange (g);
			
			myPane.GetImage ().Save (@"zedgraph.png", ImageFormat.Png);
		}

		public static void Main (string[] args)
		{
//			Application.Init ();
			
			Simulator simulator = new Simulator ();
			
//			string simulationsCwd = "/home/itecgo/Julie/Results/" + DateTime.Now.ToString ("yyyyMMdd_HHmmss");
//			simulator.DoFunctionalSimulation(simulator.WorkloadSetCPU2006["482.sphinx3"]);
			
//			bool cacheProfilerEnabled = false;
			
//			cDoDetailedSimulation (simulationsCwd, WorkloadSet.OldenCustom1, cacheProfilerEnabled);
//			simulator.DoDetailedSimulation (simulationsCwd, WorkloadSet.CPU2006, cacheProfilerEnabled);
			
//			MainWindow win = new MainWindow ();
//			win.Show ();
//			Application.Run ();
			
			List<Simulation> simulations = new List<Simulation> ();
			
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/mst_original_Q6600.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/mst_original_Corei7_930.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/mst_prepush_Q6600.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/mst_prepush_Corei7_930.xml"));
			
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/em3d_original_Q6600.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/em3d_original_Corei7_930.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/em3d_prepush_Q6600.xml"));
			simulations.Add (Simulation.Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/Simulations/Step2/em3d_prepush_Corei7_930.xml"));
			
			simulator.DoBatchExecute (simulations, false, false);
			
			Console.WriteLine("################### Simulation Results Summary ###################\n");
			
			foreach (Simulation simulation in simulations) {
				Console.WriteLine ("Simulation of {0:s}", simulation.Title);
				
				Console.WriteLine ("\tTime used: {0:f} seconds", simulation.PipelineReport.Global.Time);
				Console.WriteLine ("\tCycles spent: {0:d} cycles", simulation.PipelineReport.Global.Cycles);
				Console.WriteLine ("\tCycles per second during simulation: {0:d} cycles", simulation.PipelineReport.Global.CyclesPerSecond);
				Console.WriteLine ("\tTotal instructions committed on all threads: {0:d}", simulation.PipelineReport.Global.UopReportCommitted.Total);
				Console.WriteLine ("\tTotal instructions committed on thread c0t0: {0:d}", simulation.PipelineReport.Threads["c0t0"].UopReportCommitted.Total);
				
				Console.WriteLine ();
			}
		}
	}
}

