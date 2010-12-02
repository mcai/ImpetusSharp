using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Gtk;
using MinCai.Simulators.Impetus;

namespace ImpetusSharp.Demo1
{
	class MainClass
	{
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

