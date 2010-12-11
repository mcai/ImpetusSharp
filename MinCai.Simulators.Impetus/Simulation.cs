/*
 * Simulation.cs
 * 
 * Copyright (c) 2010 Min Cai <itecgo@163.com>. 
 * 
 * This file is part of ImpetusSharp - a driver program written in C# for Multi2Sim.
 * 
 * Flexim is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Flexim is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ImpetusSharp.  If not, see <http ://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using MinCai.Common;
using Mono.Unix.Native;

namespace MinCai.Simulators.Impetus
{
	#region Configuration

	public sealed class Workload
	{
		public sealed class Serializer : XmlConfigSerializer<Workload>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (Workload workload)
			{
				XmlConfig xmlConfig = new XmlConfig ("Workload");
				xmlConfig["num"] = workload.Num + "";
				xmlConfig["title"] = workload.Title;
				xmlConfig["cwd"] = workload.Cwd;
				xmlConfig["exe"] = workload.Exe;
				xmlConfig["argsLiteral"] = workload.Args;
				xmlConfig["stdin"] = workload.Stdin;
				xmlConfig["stdout"] = workload.Stdout;
				xmlConfig["numThreadsNeeded"] = workload.NumThreadsNeeded + "";
				
				return xmlConfig;
			}

			public override Workload Load (XmlConfig xmlConfig)
			{
				uint num = uint.Parse (xmlConfig["num"]);
				string title = xmlConfig["title"];
				string cwd = xmlConfig["cwd"];
				string exe = xmlConfig["exe"];
				string args = xmlConfig["argsLiteral"];
				string stdin = xmlConfig["stdin"];
				string stdout = xmlConfig["stdout"];
				uint numThreadsNeeded = uint.Parse (xmlConfig["numThreadsNeeded"]);
				
				Workload workload = new Workload (num, title, cwd, exe, args, stdin, stdout, numThreadsNeeded);
				
				return workload;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public Workload (uint num, string title, string cwd, string exe, string args, string stdin, string stdout, uint numThreadsNeeded)
		{
			this.Num = num;
			this.Title = title;
			this.Cwd = cwd;
			this.Exe = exe;
			this.Args = args;
			this.Stdin = stdin;
			this.Stdout = stdout;
			this.NumThreadsNeeded = numThreadsNeeded;
		}

		public uint Num { get; set; }
		public string Title { get; set; }
		public string Cwd { get; set; }
		public string Exe { get; set; }
		public string Args { get; set; }
		public string Stdin { get; set; }
		public string Stdout { get; set; }
		public uint NumThreadsNeeded { get; set; }
	}

//	public sealed class WorkloadSet
//	{
//		public sealed class Serializer : XmlConfigFileSerializer<WorkloadSet>
//		{
//			public Serializer ()
//			{
//			}
//
//			public override XmlConfigFile Save (WorkloadSet workloadSet)
//			{
//				XmlConfigFile xmlConfigFile = new XmlConfigFile ("WorkloadSet");
//				xmlConfigFile["title"] = workloadSet.Title;
//				
//				foreach (KeyValuePair<string, Workload> pair in workloadSet.Workloads) {
//					Workload workload = pair.Value;
//					xmlConfigFile.Entries.Add (Workload.Serializer.SingleInstance.Save (workload));
//				}
//				
//				return xmlConfigFile;
//			}
//
//			public override WorkloadSet Load (XmlConfigFile xmlConfigFile)
//			{
//				string workloadSetTitle = xmlConfigFile["title"];
//				
//				WorkloadSet workloadSet = new WorkloadSet (workloadSetTitle);
//				
//				foreach (XmlConfig entry in xmlConfigFile.Entries) {
//					Workload workload = Workload.Serializer.SingleInstance.Load (entry);
//					workloadSet.Register (workload);
//				}
//				
//				return workloadSet;
//			}
//
//			public static Serializer SingleInstance = new Serializer ();
//		}
//
//		public WorkloadSet (string title)
//		{
//			this.Title = title;
//			this.Workloads = new SortedDictionary<string, Workload> ();
//		}
//
//		public void Register (Workload workload)
//		{
//			this[workload.Title] = workload;
//		}
//
//		public Workload this[string title] {
//			get { return this.Workloads[title]; }
//			set { this.Workloads[title] = value; }
//		}
//
//		public string Title { get; set; }
//
//		public SortedDictionary<string, Workload> Workloads { get; private set; }
//
//		static WorkloadSet ()
//		{
//			OldenCustom1 = Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/ImpetusSharp/configs/workloads", "Olden_Custom1.xml");
//			CPU2006Custom1 = Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/ImpetusSharp/configs/workloads", "CPU2006_Custom1.xml");
//			CPU2006 = Serializer.SingleInstance.LoadXML ("/home/itecgo/Julie/ImpetusSharp/configs/workloads", "CPU2006.xml");
//		}
//
//		public static WorkloadSet OldenCustom1 { get; set; }
//		public static WorkloadSet CPU2006Custom1 { get; set; }
//		public static WorkloadSet CPU2006 { get; set; }
//	}

	public sealed class PipelineConfig
	{
		public enum RecoverKinds
		{
			[StringValue("writeback")]
			WRITEBACK,

			[StringValue("commit")]
			COMMIT
		}

		public enum FetchKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("timeslice")]
			TIMESLICE,

			[StringValue("switchonevent")]
			SWITCHONEVENT
		}

		public enum DispatchKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("timeslice")]
			TIMESLICE
		}

		public enum IssueKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("timeslice")]
			TIMESLICE
		}

		public enum CommitKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("timeslice")]
			TIMESLICE
		}

		public enum RfKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("private")]
			PRIVATE
		}

		public enum ROBKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("private")]
			PRIVATE
		}

		public enum IQKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("private")]
			PRIVATE
		}

		public enum LSQKinds
		{
			[StringValue("shared")]
			SHARED,

			[StringValue("private")]
			PRIVATE
		}

		public sealed class ReportConfig
		{
			public sealed class Serializer : XmlConfigSerializer<ReportConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (ReportConfig reportConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("ReportConfig");
					xmlConfig["pipeline"] = reportConfig.Pipeline;
					xmlConfig["cache"] = reportConfig.Cache;
					xmlConfig["cacheProfiler"] = reportConfig.CacheProfiler;
					
					return xmlConfig;
				}

				public override ReportConfig Load (XmlConfig xmlConfig)
				{
					string pipeline = xmlConfig["pipeline"];
					string cache = xmlConfig["cache"];
					string cacheProfiler = xmlConfig["cacheProfiler"];
					
					ReportConfig reportConfig = new ReportConfig ();
					reportConfig.Pipeline = pipeline;
					reportConfig.Cache = cache;
					reportConfig.CacheProfiler = cacheProfiler;
					
					return reportConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public ReportConfig ()
			{
				this.Pipeline = this.Cache = this.CacheProfiler = "";
			}

			public string Pipeline { get; set; }
			public string Cache { get; set; }
			public string CacheProfiler { get; set; }
		}

		public sealed class DebugConfig
		{
			public sealed class Serializer : XmlConfigSerializer<DebugConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (DebugConfig debugConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("DebugConfig");
					xmlConfig["ctx"] = debugConfig.Ctx;
					xmlConfig["syscall"] = debugConfig.Syscall;
					xmlConfig["loader"] = debugConfig.Loader;
					xmlConfig["inst"] = debugConfig.Inst;
					xmlConfig["cache"] = debugConfig.Cache;
					xmlConfig["pipeline"] = debugConfig.Pipeline;
					xmlConfig["error"] = debugConfig.Error;
					
					return xmlConfig;
				}

				public override DebugConfig Load (XmlConfig xmlConfig)
				{
					string ctx = xmlConfig["ctx"];
					string syscall = xmlConfig["syscall"];
					string loader = xmlConfig["loader"];
					string inst = xmlConfig["inst"];
					string cache = xmlConfig["cache"];
					string pipeline = xmlConfig["pipeline"];
					string error = xmlConfig["error"];
					
					DebugConfig debugConfig = new DebugConfig ();
					debugConfig.Ctx = ctx;
					debugConfig.Syscall = syscall;
					debugConfig.Loader = loader;
					debugConfig.Inst = inst;
					debugConfig.Cache = cache;
					debugConfig.Pipeline = pipeline;
					debugConfig.Error = error;
					
					return debugConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public DebugConfig ()
			{
				this.Ctx = this.Syscall = this.Loader = this.Inst = this.Cache = this.Pipeline = this.Error = "";
			}

			public string Ctx { get; set; }
			public string Syscall { get; set; }
			public string Loader { get; set; }
			public string Call { get; set; }
			public string Inst { get; set; }
			public string Cache { get; set; }
			public string Pipeline { get; set; }
			public string Error { get; set; }
		}

		public sealed class JulieConfig
		{
			public sealed class CtxToCpuMapping
			{
				public sealed class Serializer : XmlConfigSerializer<CtxToCpuMapping>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (CtxToCpuMapping ctxToCpuMapping)
					{
						XmlConfig xmlConfig = new XmlConfig ("CtxToCpuMapping");
						xmlConfig["contextId"] = ctxToCpuMapping.ContextId + "";
						xmlConfig["cpuId"] = ctxToCpuMapping.CpuId + "";
						
						return xmlConfig;
					}

					public override CtxToCpuMapping Load (XmlConfig xmlConfig)
					{
						uint contextId = uint.Parse (xmlConfig["contextId"]);
						uint cpuId = uint.Parse (xmlConfig["cpuId"]);
						
						CtxToCpuMapping ctxToCpuMapping = new CtxToCpuMapping (contextId, cpuId);
						
						return ctxToCpuMapping;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public CtxToCpuMapping (uint contextId, uint cpuId)
				{
					this.ContextId = contextId;
					this.CpuId = cpuId;
				}

				public uint ContextId { get; set; }
				public uint CpuId { get; set; }
			}

			public sealed class Serializer : XmlConfigSerializer<JulieConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (JulieConfig julieConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("JulieConfig");
					
					xmlConfig.Entries.Add (SaveList<CtxToCpuMapping> ("CtxToCpuMappings", julieConfig.CtxToCpuMappings, new SaveEntryDelegate<CtxToCpuMapping> (CtxToCpuMapping.Serializer.SingleInstance.Save)));
					
					return xmlConfig;
				}

				public override JulieConfig Load (XmlConfig xmlConfig)
				{
					List<CtxToCpuMapping> ctxToCpuMappings = LoadList<CtxToCpuMapping> (xmlConfig.Entries[0], new LoadEntryDelegate<CtxToCpuMapping> (CtxToCpuMapping.Serializer.SingleInstance.Load));
					
					JulieConfig julieConfig = new JulieConfig ();
					julieConfig.CtxToCpuMappings = ctxToCpuMappings;
					
					return julieConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public JulieConfig ()
			{
				this.CtxToCpuMappings = new List<CtxToCpuMapping> ();
			}

			public void WriteTo (string fileName)
			{
				StreamWriter sw = new StreamWriter (fileName);
				
				foreach (CtxToCpuMapping mapping in this.CtxToCpuMappings) {
					sw.WriteLine (mapping.ContextId + "," + mapping.CpuId);
				}
				
				sw.Close ();
			}

			public List<CtxToCpuMapping> CtxToCpuMappings { get; set; }

			public static string CTX_TO_MAPPINGS_FILE_NAME = "ctx_to_cpu_mappings.csv";
		}

		public sealed class BpredConfig
		{
			public enum Kinds
			{
				[StringValue("perfect")]
				PERFECT,

				[StringValue("taken")]
				TAKEN,

				[StringValue("nottaken")]
				NOTTAKEN,

				[StringValue("bimod")]
				BIMOD,

				[StringValue("twolevel")]
				TWOLEVEL,

				[StringValue("comb")]
				COMB
			}

			public sealed class BtbConfig
			{
				public sealed class Serializer : XmlConfigSerializer<BtbConfig>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (BtbConfig btbConfig)
					{
						XmlConfig xmlConfig = new XmlConfig ("BtbConfig");
						xmlConfig["sets"] = btbConfig.Sets + "";
						xmlConfig["assoc"] = btbConfig.Assoc + "";
						
						return xmlConfig;
					}

					public override BtbConfig Load (XmlConfig xmlConfig)
					{
						uint sets = uint.Parse (xmlConfig["sets"]);
						uint assoc = uint.Parse (xmlConfig["assoc"]);
						
						BtbConfig btbConfig = new BtbConfig ();
						btbConfig.Sets = sets;
						btbConfig.Assoc = assoc;
						
						return btbConfig;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public BtbConfig ()
				{
					this.Sets = 256;
					this.Assoc = 4;
				}

				public uint Sets { get; set; }
				public uint Assoc { get; set; }
			}

			public sealed class BimodConfig
			{
				public sealed class Serializer : XmlConfigSerializer<BimodConfig>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (BimodConfig bimodConfig)
					{
						XmlConfig xmlConfig = new XmlConfig ("BimodConfig");
						xmlConfig["size"] = bimodConfig.Size + "";
						
						return xmlConfig;
					}

					public override BimodConfig Load (XmlConfig xmlConfig)
					{
						uint size = uint.Parse (xmlConfig["size"]);
						
						BimodConfig bimodConfig = new BimodConfig ();
						bimodConfig.Size = size;
						
						return bimodConfig;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public BimodConfig ()
				{
					this.Size = 1024;
				}

				public uint Size { get; set; }
			}

			public sealed class TwoLevelConfig
			{
				public sealed class Serializer : XmlConfigSerializer<TwoLevelConfig>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (TwoLevelConfig twoLevelConfig)
					{
						XmlConfig xmlConfig = new XmlConfig ("TwoLevelConfig");
						xmlConfig["l1Size"] = twoLevelConfig.L1Size + "";
						xmlConfig["l2Size"] = twoLevelConfig.L2Size + "";
						xmlConfig["histSize"] = twoLevelConfig.HistSize + "";
						
						return xmlConfig;
					}

					public override TwoLevelConfig Load (XmlConfig xmlConfig)
					{
						uint l1Size = uint.Parse (xmlConfig["l1Size"]);
						uint l2Size = uint.Parse (xmlConfig["l2Size"]);
						uint histSize = uint.Parse (xmlConfig["histSize"]);
						
						TwoLevelConfig twoLevelConfig = new TwoLevelConfig ();
						twoLevelConfig.L1Size = l1Size;
						twoLevelConfig.L2Size = l2Size;
						twoLevelConfig.HistSize = histSize;
						
						return twoLevelConfig;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public TwoLevelConfig ()
				{
					this.L1Size = 1;
					this.L2Size = 1024;
					this.HistSize = 8;
				}

				public uint L1Size { get; set; }
				public uint L2Size { get; set; }
				public uint HistSize { get; set; }
			}

			public sealed class ChoiceConfig
			{
				public sealed class Serializer : XmlConfigSerializer<ChoiceConfig>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (ChoiceConfig choiceConfig)
					{
						XmlConfig xmlConfig = new XmlConfig ("ChoiceConfig");
						xmlConfig["size"] = choiceConfig.Size + "";
						
						return xmlConfig;
					}

					public override ChoiceConfig Load (XmlConfig xmlConfig)
					{
						uint size = uint.Parse (xmlConfig["size"]);
						
						ChoiceConfig choiceConfig = new ChoiceConfig ();
						choiceConfig.Size = size;
						
						return choiceConfig;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public ChoiceConfig ()
				{
					this.Size = 1024;
				}

				public uint Size { get; set; }
			}

			public sealed class Serializer : XmlConfigSerializer<BpredConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (BpredConfig bpredConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("BpredConfig");
					xmlConfig["kind"] = EnumUtils.ToStringValue (bpredConfig.Kind);
					xmlConfig["ras"] = bpredConfig.Ras + "";
					
					xmlConfig.Entries.Add (BtbConfig.Serializer.SingleInstance.Save (bpredConfig.Btb));
					xmlConfig.Entries.Add (BimodConfig.Serializer.SingleInstance.Save (bpredConfig.Bimod));
					xmlConfig.Entries.Add (TwoLevelConfig.Serializer.SingleInstance.Save (bpredConfig.TwoLevel));
					xmlConfig.Entries.Add (ChoiceConfig.Serializer.SingleInstance.Save (bpredConfig.Choice));
					
					return xmlConfig;
				}

				public override BpredConfig Load (XmlConfig xmlConfig)
				{
					BpredConfig.Kinds kind = EnumUtils.Parse<BpredConfig.Kinds> (xmlConfig["kind"]);
					uint ras = uint.Parse (xmlConfig["ras"]);
					BtbConfig btb = BtbConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[0]);
					BimodConfig bimod = BimodConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[1]);
					TwoLevelConfig twoLevel = TwoLevelConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[2]);
					ChoiceConfig choice = ChoiceConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[3]);
					
					BpredConfig bpredConfig = new BpredConfig ();
					bpredConfig.Kind = kind;
					bpredConfig.Btb = btb;
					bpredConfig.Ras = ras;
					bpredConfig.Bimod = bimod;
					bpredConfig.TwoLevel = twoLevel;
					bpredConfig.Choice = choice;
					
					return bpredConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public BpredConfig ()
			{
				this.Kind = Kinds.TWOLEVEL;
				this.Btb = new BtbConfig ();
				this.Ras = 32;
				this.Bimod = new BimodConfig ();
				this.TwoLevel = new TwoLevelConfig ();
				this.Choice = new ChoiceConfig ();
			}

			public Kinds Kind { get; set; }
			public BtbConfig Btb { get; set; }

			public uint Ras { get; set; }
			public BimodConfig Bimod { get; set; }
			public TwoLevelConfig TwoLevel { get; set; }
			public ChoiceConfig Choice { get; set; }
		}

		public sealed class TCacheConfig
		{
			public sealed class TopologyConfig
			{
				public sealed class Serializer : XmlConfigSerializer<TopologyConfig>
				{
					public Serializer ()
					{
					}

					public override XmlConfig Save (TopologyConfig topologyConfig)
					{
						XmlConfig xmlConfig = new XmlConfig ("TopologyConfig");
						xmlConfig["sets"] = topologyConfig.Sets + "";
						xmlConfig["assoc"] = topologyConfig.Assoc + "";
						
						return xmlConfig;
					}

					public override TopologyConfig Load (XmlConfig xmlConfig)
					{
						uint sets = uint.Parse (xmlConfig["sets"]);
						uint assoc = uint.Parse (xmlConfig["assoc"]);
						
						TopologyConfig topologyConfig = new TopologyConfig ();
						topologyConfig.Sets = sets;
						topologyConfig.Assoc = assoc;
						
						return topologyConfig;
					}

					public static Serializer SingleInstance = new Serializer ();
				}

				public TopologyConfig ()
				{
					this.Sets = 64;
					this.Assoc = 4;
				}

				public uint Sets { get; set; }
				public uint Assoc { get; set; }
			}

			public sealed class Serializer : XmlConfigSerializer<TCacheConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (TCacheConfig tCacheConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("TCacheConfig");
					xmlConfig["enabled"] = tCacheConfig.Enabled + "";
					xmlConfig["traceSize"] = tCacheConfig.TraceSize + "";
					xmlConfig["branchMax"] = tCacheConfig.BranchMax + "";
					xmlConfig["queueSize"] = tCacheConfig.QueueSize + "";
					
					xmlConfig.Entries.Add (TopologyConfig.Serializer.SingleInstance.Save (tCacheConfig.Topology));
					
					return xmlConfig;
				}

				public override TCacheConfig Load (XmlConfig xmlConfig)
				{
					bool enabled = bool.Parse (xmlConfig["enabled"]);
					uint traceSize = uint.Parse (xmlConfig["traceSize"]);
					uint branchMax = uint.Parse (xmlConfig["branchMax"]);
					uint queueSize = uint.Parse (xmlConfig["queueSize"]);
					
					TopologyConfig topology = TopologyConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[0]);
					
					TCacheConfig tCacheConfig = new TCacheConfig ();
					tCacheConfig.Enabled = enabled;
					tCacheConfig.Topology = topology;
					tCacheConfig.TraceSize = traceSize;
					tCacheConfig.BranchMax = branchMax;
					tCacheConfig.QueueSize = queueSize;
					
					return tCacheConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public TCacheConfig ()
			{
				this.Enabled = false;
				this.Topology = new TopologyConfig ();
				this.TraceSize = 16;
				this.BranchMax = 3;
				this.QueueSize = 32;
			}

			public bool Enabled { get; set; }
			public TopologyConfig Topology { get; set; }
			public uint TraceSize { get; set; }
			public uint BranchMax { get; set; }
			public uint QueueSize { get; set; }
		}

		public sealed class FuConfig
		{
			public sealed class Serializer : XmlConfigSerializer<FuConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (FuConfig fuConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("FuConfig");
					xmlConfig["name"] = fuConfig.Name;
					xmlConfig["count"] = fuConfig.Count + "";
					xmlConfig["opLat"] = fuConfig.OpLat + "";
					xmlConfig["issueLat"] = fuConfig.IssueLat + "";
					
					return xmlConfig;
				}

				public override FuConfig Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					uint count = uint.Parse (xmlConfig["count"]);
					uint opLat = uint.Parse (xmlConfig["opLat"]);
					uint issueLat = uint.Parse (xmlConfig["issueLat"]);
					
					FuConfig fuConfig = new FuConfig (name, count, opLat, issueLat);
					
					return fuConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public FuConfig (string name, uint count, uint opLat, uint issueLat)
			{
				this.Name = name;
				this.Count = count;
				this.OpLat = opLat;
				this.IssueLat = issueLat;
			}

			public string Name { get; set; }
			public uint Count { get; set; }
			public uint OpLat { get; set; }
			public uint IssueLat { get; set; }
		}

		public sealed class Serializer : XmlConfigSerializer<PipelineConfig>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (PipelineConfig pipelineConfig)
			{
				XmlConfig xmlConfig = new XmlConfig ("PipelineConfig");
				xmlConfig["title"] = pipelineConfig.Title;
				xmlConfig["maxCycles"] = pipelineConfig.MaxCycles + "";
				xmlConfig["maxInst"] = pipelineConfig.MaxInst + "";
				xmlConfig["maxTime"] = pipelineConfig.MaxTime + "";
				xmlConfig["fastfwd"] = pipelineConfig.Fastfwd + "";
				xmlConfig["fastfwdInsts"] = pipelineConfig.FastfwdInsts + "";
				xmlConfig["cores"] = pipelineConfig.Cores + "";
				xmlConfig["threads"] = pipelineConfig.Threads + "";
				xmlConfig["contextSwitch"] = pipelineConfig.ContextSwitch + "";
				xmlConfig["contextQuantum"] = pipelineConfig.ContextQuantum + "";
				xmlConfig["stageTimeStats"] = pipelineConfig.StageTimeStats + "";
				xmlConfig["recoverKind"] = EnumUtils.ToStringValue (pipelineConfig.RecoverKind);
				xmlConfig["recoverPenalty"] = pipelineConfig.RecoverPenalty + "";
				xmlConfig["threadQuantum"] = pipelineConfig.ThreadQuantum + "";
				xmlConfig["threadSwitchPenalty"] = pipelineConfig.ThreadSwitchPenalty + "";
				xmlConfig["fetchKind"] = EnumUtils.ToStringValue (pipelineConfig.FetchKind);
				xmlConfig["decodeWidth"] = pipelineConfig.DecodeWidth + "";
				xmlConfig["dispatchKind"] = EnumUtils.ToStringValue (pipelineConfig.DispatchKind);
				xmlConfig["dispatchWidth"] = pipelineConfig.DispatchWidth + "";
				xmlConfig["issueKind"] = EnumUtils.ToStringValue (pipelineConfig.IssueKind);
				xmlConfig["issueWidth"] = pipelineConfig.IssueWidth + "";
				xmlConfig["commitKind"] = EnumUtils.ToStringValue (pipelineConfig.CommitKind);
				xmlConfig["commitWidth"] = pipelineConfig.CommitWidth + "";
				xmlConfig["fetchqSize"] = pipelineConfig.FetchqSize + "";
				xmlConfig["uopqSize"] = pipelineConfig.UopqSize + "";
				xmlConfig["robKind"] = EnumUtils.ToStringValue (pipelineConfig.RobKind);
				xmlConfig["robSize"] = pipelineConfig.RobSize + "";
				xmlConfig["rfKind"] = EnumUtils.ToStringValue (pipelineConfig.RfKind);
				xmlConfig["rfIntSize"] = pipelineConfig.RfIntSize + "";
				xmlConfig["rfFpSize"] = pipelineConfig.RfFpSize + "";
				xmlConfig["iqKind"] = EnumUtils.ToStringValue (pipelineConfig.IqKind);
				xmlConfig["iqSize"] = pipelineConfig.IqSize + "";
				xmlConfig["lsqKind"] = EnumUtils.ToStringValue (pipelineConfig.LsqKind);
				xmlConfig["lsqSize"] = pipelineConfig.LsqSize + "";
				xmlConfig["iPerfect"] = pipelineConfig.IPerfect + "";
				xmlConfig["dPerfect"] = pipelineConfig.DPerfect + "";
				xmlConfig["pageSize"] = pipelineConfig.PageSize + "";
				
				xmlConfig.Entries.Add (DebugConfig.Serializer.SingleInstance.Save (pipelineConfig.Dbg));
				xmlConfig.Entries.Add (JulieConfig.Serializer.SingleInstance.Save (pipelineConfig.Julie));
				xmlConfig.Entries.Add (ReportConfig.Serializer.SingleInstance.Save (pipelineConfig.Report));
				xmlConfig.Entries.Add (BpredConfig.Serializer.SingleInstance.Save (pipelineConfig.Bpred));
				xmlConfig.Entries.Add (TCacheConfig.Serializer.SingleInstance.Save (pipelineConfig.TCache));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.IntAdd));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.IntSub));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.IntMult));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.IntDiv));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.EffAddr));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.Logical));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpSimple));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpAdd));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpComp));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpMult));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpDiv));
				xmlConfig.Entries.Add (FuConfig.Serializer.SingleInstance.Save (pipelineConfig.FpComplex));
				
				return xmlConfig;
			}

			public override PipelineConfig Load (XmlConfig xmlConfig)
			{
				string title = xmlConfig["title"];
				uint maxCycles = uint.Parse (xmlConfig["maxCycles"]);
				uint maxInst = uint.Parse (xmlConfig["maxInst"]);
				uint maxTime = uint.Parse (xmlConfig["maxTime"]);
				uint fastfwd = uint.Parse (xmlConfig["fastfwd"]);
				uint fastfwdInsts = uint.Parse (xmlConfig["fastfwdInsts"]);
				uint cores = uint.Parse (xmlConfig["cores"]);
				uint threads = uint.Parse (xmlConfig["threads"]);
				bool contextSwitch = bool.Parse (xmlConfig["contextSwitch"]);
				uint contextQuantum = uint.Parse (xmlConfig["contextQuantum"]);
				bool stageTimeStats = bool.Parse (xmlConfig["stageTimeStats"]);
				RecoverKinds recoverKind = EnumUtils.Parse<RecoverKinds> (xmlConfig["recoverKind"]);
				uint recoverPenalty = uint.Parse (xmlConfig["recoverPenalty"]);
				uint threadQuantum = uint.Parse (xmlConfig["threadQuantum"]);
				uint threadSwitchPenalty = uint.Parse (xmlConfig["threadSwitchPenalty"]);
				FetchKinds fetchKind = EnumUtils.Parse<FetchKinds> (xmlConfig["fetchKind"]);
				uint decodeWidth = uint.Parse (xmlConfig["decodeWidth"]);
				DispatchKinds dispatchKind = EnumUtils.Parse<DispatchKinds> (xmlConfig["dispatchKind"]);
				uint dispatchWidth = uint.Parse (xmlConfig["dispatchWidth"]);
				IssueKinds issueKind = EnumUtils.Parse<IssueKinds> (xmlConfig["issueKind"]);
				uint issueWidth = uint.Parse (xmlConfig["issueWidth"]);
				CommitKinds commitKind = EnumUtils.Parse<CommitKinds> (xmlConfig["commitKind"]);
				uint commitWidth = uint.Parse (xmlConfig["commitWidth"]);
				uint fetchqSize = uint.Parse (xmlConfig["fetchqSize"]);
				uint uopqSize = uint.Parse (xmlConfig["uopqSize"]);
				ROBKinds robKind = EnumUtils.Parse<ROBKinds> (xmlConfig["robKind"]);
				uint robSize = uint.Parse (xmlConfig["robSize"]);
				RfKinds rfKind = EnumUtils.Parse<RfKinds> (xmlConfig["rfKind"]);
				uint rfIntSize = uint.Parse (xmlConfig["rfIntSize"]);
				uint rfFpSize = uint.Parse (xmlConfig["rfFpSize"]);
				IQKinds iqKind = EnumUtils.Parse<IQKinds> (xmlConfig["iqKind"]);
				uint iqSize = uint.Parse (xmlConfig["iqSize"]);
				LSQKinds lsqKind = EnumUtils.Parse<LSQKinds> (xmlConfig["lsqKind"]);
				uint lsqSize = uint.Parse (xmlConfig["lsqSize"]);
				bool iPerfect = bool.Parse (xmlConfig["iPerfect"]);
				bool dPerfect = bool.Parse (xmlConfig["dPerfect"]);
				uint pageSize = uint.Parse (xmlConfig["pageSize"]);
				
				int i = 0;
				
				DebugConfig dbg = DebugConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				JulieConfig julie = JulieConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				ReportConfig report = ReportConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				BpredConfig bpred = BpredConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				TCacheConfig tCache = TCacheConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig intAdd = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig intSub = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig intMult = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig intDiv = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig effAddr = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig logical = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpSimple = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpAdd = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpComp = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpMult = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpDiv = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				FuConfig fpComplex = FuConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
				
				PipelineConfig pipelineConfig = new PipelineConfig ();
				pipelineConfig.Title = title;
				pipelineConfig.MaxCycles = maxCycles;
				pipelineConfig.MaxInst = maxInst;
				pipelineConfig.MaxTime = maxTime;
				pipelineConfig.Fastfwd = fastfwd;
				pipelineConfig.FastfwdInsts = fastfwdInsts;
				pipelineConfig.Dbg = dbg;
				pipelineConfig.Julie = julie;
				pipelineConfig.Report = report;
				pipelineConfig.Cores = cores;
				pipelineConfig.Threads = threads;
				pipelineConfig.ContextSwitch = contextSwitch;
				pipelineConfig.ContextQuantum = contextQuantum;
				pipelineConfig.StageTimeStats = stageTimeStats;
				pipelineConfig.RecoverKind = recoverKind;
				pipelineConfig.RecoverPenalty = recoverPenalty;
				pipelineConfig.ThreadQuantum = threadQuantum;
				pipelineConfig.ThreadSwitchPenalty = threadSwitchPenalty;
				pipelineConfig.FetchKind = fetchKind;
				pipelineConfig.DecodeWidth = decodeWidth;
				pipelineConfig.DispatchKind = dispatchKind;
				pipelineConfig.DispatchWidth = dispatchWidth;
				pipelineConfig.IssueKind = issueKind;
				pipelineConfig.IssueWidth = issueWidth;
				pipelineConfig.CommitKind = commitKind;
				pipelineConfig.CommitWidth = commitWidth;
				pipelineConfig.Bpred = bpred;
				pipelineConfig.TCache = tCache;
				pipelineConfig.FetchqSize = fetchqSize;
				pipelineConfig.UopqSize = uopqSize;
				pipelineConfig.RobKind = robKind;
				pipelineConfig.RobSize = robSize;
				pipelineConfig.RfKind = rfKind;
				pipelineConfig.RfIntSize = rfIntSize;
				pipelineConfig.RfFpSize = rfFpSize;
				pipelineConfig.IqKind = iqKind;
				pipelineConfig.IqSize = iqSize;
				pipelineConfig.LsqKind = lsqKind;
				pipelineConfig.LsqSize = lsqSize;
				pipelineConfig.IntAdd = intAdd;
				pipelineConfig.IntSub = intSub;
				pipelineConfig.IntMult = intMult;
				pipelineConfig.IntDiv = intDiv;
				pipelineConfig.EffAddr = effAddr;
				pipelineConfig.Logical = logical;
				pipelineConfig.FpSimple = fpSimple;
				pipelineConfig.FpAdd = fpAdd;
				pipelineConfig.FpComp = fpComp;
				pipelineConfig.FpMult = fpMult;
				pipelineConfig.FpDiv = fpDiv;
				pipelineConfig.FpComplex = fpComplex;
				pipelineConfig.IPerfect = iPerfect;
				pipelineConfig.DPerfect = dPerfect;
				pipelineConfig.PageSize = pageSize;
				
				return pipelineConfig;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public PipelineConfig ()
		{
			this.Title = "";
			this.MaxInst = this.MaxTime = this.FastfwdInsts = 0;
			
			//fast forward 1 billion cycles
			this.Fastfwd = 1000000000;
			//detailed simulate 2 billion cycles
			this.MaxCycles = 2000000000;
			
			this.Fastfwd = 10000000;
			this.MaxCycles = 20000000;
			
			this.Fastfwd = 100000;
			this.MaxCycles = 200000;
			
			this.Dbg = new DebugConfig ();
			
			this.Julie = new JulieConfig ();
			
			this.Report = new ReportConfig ();
			
			this.Cores = this.Threads = 1;
			
			this.StageTimeStats = false;
			
			this.RecoverKind = RecoverKinds.WRITEBACK;
			this.RecoverPenalty = 0;
			
			this.ContextSwitch = true;
			this.ContextQuantum = 100000;
			
			this.ThreadQuantum = 1000;
			this.ThreadSwitchPenalty = 0;
			
			this.FetchKind = FetchKinds.TIMESLICE;
			
			this.DecodeWidth = 4;
			
			this.DispatchKind = DispatchKinds.TIMESLICE;
			this.DispatchWidth = 4;
			
			this.IssueKind = IssueKinds.TIMESLICE;
			this.IssueWidth = 4;
			
			this.CommitKind = CommitKinds.SHARED;
			this.CommitWidth = 4;
			
			this.Bpred = new BpredConfig ();
			
			this.TCache = new TCacheConfig ();
			
			this.FetchqSize = 64;
			
			this.UopqSize = 32;
			
			this.RobKind = ROBKinds.PRIVATE;
			this.RobSize = 64;
			
			this.RfKind = RfKinds.PRIVATE;
			this.RfIntSize = 80;
			this.RfFpSize = 40;
			
			this.IqKind = IQKinds.PRIVATE;
			this.IqSize = 40;
			
			this.LsqKind = LSQKinds.PRIVATE;
			this.LsqSize = 20;
			
			this.IntAdd = new FuConfig ("IntAdd", 4, 2, 1);
			this.IntSub = new FuConfig ("IntSub", 4, 2, 1);
			this.IntMult = new FuConfig ("IntMult", 1, 3, 1);
			this.IntDiv = new FuConfig ("IntDiv", 1, 20, 19);
			
			this.EffAddr = new FuConfig ("EffAddr", 4, 2, 1);
			
			this.Logical = new FuConfig ("Logical", 4, 1, 1);
			
			this.FpSimple = new FuConfig ("FpSimple", 2, 2, 2);
			this.FpAdd = new FuConfig ("FpAdd", 2, 5, 5);
			this.FpComp = new FuConfig ("FpComp", 2, 5, 5);
			this.FpMult = new FuConfig ("FpMult", 1, 10, 10);
			this.FpDiv = new FuConfig ("FpDiv", 1, 20, 20);
			this.FpComplex = new FuConfig ("FpComplex", 1, 40, 40);
			
			this.IPerfect = this.DPerfect = false;
			
			this.PageSize = 4096;
		}

		public string WriteTo (Simulation simulation)
		{
			StringBuilder sb = new StringBuilder ();
			
			sb.Append (ToString ("title", "\"" + this.Title + "\""));
			
			sb.Append (ToString ("max_cycles", this.MaxCycles));
			sb.Append (ToString ("max_inst", this.MaxInst));
			sb.Append (ToString ("max_time", this.MaxTime));
			sb.Append (ToString ("fastfwd", this.Fastfwd));
			sb.Append (ToString ("fastfwd_insts", this.FastfwdInsts));
			
			sb.Append (ToString ("debug:ctx", "\"" + this.Dbg.Ctx + "\""));
			sb.Append (ToString ("debug:syscall", "\"" + this.Dbg.Syscall + "\""));
			sb.Append (ToString ("debug:loader", "\"" + this.Dbg.Loader + "\""));
			sb.Append (ToString ("debug:call", "\"" + this.Dbg.Call + "\""));
			sb.Append (ToString ("debug:inst", "\"" + this.Dbg.Inst + "\""));
			sb.Append (ToString ("debug:cache", "\"" + this.Dbg.Cache + "\""));
			sb.Append (ToString ("debug:pipeline", "\"" + this.Dbg.Pipeline + "\""));
			sb.Append (ToString ("debug:error", "\"" + this.Dbg.Error + "\""));
			
			sb.Append (ToString ("julie:ctx_to_cpu_mappings", "\"" + JulieConfig.CTX_TO_MAPPINGS_FILE_NAME + "\""));
			
			sb.Append (ToString ("report:pipeline", "\"" + simulation.Cwd + Path.DirectorySeparatorChar + this.Report.Pipeline + "\""));
			sb.Append (ToString ("report:cache", "\"" + simulation.Cwd + Path.DirectorySeparatorChar + this.Report.Cache + "\""));
			sb.Append (ToString ("report:cache_profiler", "\"" + simulation.Cwd + Path.DirectorySeparatorChar + this.Report.CacheProfiler + "\""));
			
			sb.Append (ToString ("cores", this.Cores));
			sb.Append (ToString ("threads", this.Threads));
			
			sb.Append (ToString ("context_switch", this.ContextSwitch ? "t" : "f"));
			sb.Append (ToString ("context_quantum", this.ContextQuantum));
			
			sb.Append (ToString ("stage_time_stats", this.StageTimeStats ? "t" : "f"));
			
			sb.Append (ToString ("recover_kind", EnumUtils.ToStringValue (this.RecoverKind)));
			sb.Append (ToString ("recover_penalty", this.RecoverPenalty));
			
			sb.Append (ToString ("thread_quantum", this.ThreadQuantum));
			sb.Append (ToString ("thread_switch_penalty", this.ThreadSwitchPenalty));
			
			sb.Append (ToString ("fetch_kind", EnumUtils.ToStringValue (this.FetchKind)));
			
			sb.Append (ToString ("decode_width", this.DecodeWidth));
			
			sb.Append (ToString ("dispatch_kind", EnumUtils.ToStringValue (this.DispatchKind)));
			sb.Append (ToString ("dispatch_width", this.DispatchWidth));
			
			sb.Append (ToString ("issue_kind", EnumUtils.ToStringValue (this.IssueKind)));
			sb.Append (ToString ("issue_width", this.IssueWidth));
			
			sb.Append (ToString ("commit_kind", EnumUtils.ToStringValue (this.CommitKind)));
			sb.Append (ToString ("commit_width", this.CommitWidth));
			
			sb.Append (ToString ("bpred", EnumUtils.ToStringValue (this.Bpred.Kind)));
			sb.Append (ToString ("bpred:btb", this.Bpred.Btb.Sets + ":" + this.Bpred.Btb.Assoc));
			sb.Append (ToString ("bpred:ras", this.Bpred.Ras));
			sb.Append (ToString ("bpred:bimod", this.Bpred.Bimod.Size));
			sb.Append (ToString ("bpred:twolevel", this.Bpred.TwoLevel.L1Size + " " + this.Bpred.TwoLevel.L2Size + " " + this.Bpred.TwoLevel.HistSize));
			sb.Append (ToString ("bpred:choice", this.Bpred.Choice.Size));
			
			sb.Append (ToString ("tcache", this.TCache.Enabled ? "t" : "f"));
			sb.Append (ToString ("tcache:topo", this.TCache.Topology.Sets + ":" + this.TCache.Topology.Assoc));
			sb.Append (ToString ("tcache:trace_size", this.TCache.TraceSize));
			sb.Append (ToString ("tcache:branch_max", this.TCache.BranchMax));
			sb.Append (ToString ("tcache:queue_size", this.TCache.QueueSize));
			
			sb.Append (ToString ("fetchq_size", this.FetchqSize));
			
			sb.Append (ToString ("uopq_size", this.UopqSize));
			
			sb.Append (ToString ("rob_kind", EnumUtils.ToStringValue (this.RobKind)));
			sb.Append (ToString ("rob_size", this.RobSize));
			
			sb.Append (ToString ("rf_kind", EnumUtils.ToStringValue (this.RfKind)));
			sb.Append (ToString ("rf_int_size", this.RfIntSize));
			sb.Append (ToString ("rf_fp_size", this.RfFpSize));
			
			sb.Append (ToString ("iq_kind", EnumUtils.ToStringValue (this.IqKind)));
			sb.Append (ToString ("iq_size", this.IqSize));
			
			sb.Append (ToString ("lsq_kind", EnumUtils.ToStringValue (this.LsqKind)));
			sb.Append (ToString ("lsq_size", this.LsqSize));
			
			sb.Append (ToString ("fu:intadd", ToString (this.IntAdd)));
			sb.Append (ToString ("fu:intsub", ToString (this.IntSub)));
			sb.Append (ToString ("fu:intmult", ToString (this.IntMult)));
			sb.Append (ToString ("fu:intdiv", ToString (this.IntDiv)));
			
			sb.Append (ToString ("fu:effaddr", ToString (this.EffAddr)));
			
			sb.Append (ToString ("fu:logical", ToString (this.Logical)));
			
			sb.Append (ToString ("fu:fpsimple", ToString (this.FpSimple)));
			sb.Append (ToString ("fu:fpadd", ToString (this.FpAdd)));
			sb.Append (ToString ("fu:fpcomp", ToString (this.FpComp)));
			sb.Append (ToString ("fu:fpmult", ToString (this.FpMult)));
			sb.Append (ToString ("fu:fpdiv", ToString (this.FpDiv)));
			sb.Append (ToString ("fu:fpcomplex", ToString (this.FpComplex)));
			
			sb.Append (ToString ("iperfect", this.IPerfect ? "t" : "f"));
			sb.Append (ToString ("dperfect", this.DPerfect ? "t" : "f"));
			
			sb.Append (ToString ("page_size", this.PageSize));
			
			return sb.ToString ();
		}

		public string Title { get; set; }

		public uint MaxCycles { get; set; }
		public uint MaxInst { get; set; }
		public uint MaxTime { get; set; }
		public uint Fastfwd { get; set; }
		public uint FastfwdInsts { get; set; }

		public DebugConfig Dbg { get; set; }

		public JulieConfig Julie { get; set; }

		public ReportConfig Report { get; set; }

		public uint Cores { get; set; }
		public uint Threads { get; set; }

		public bool ContextSwitch { get; set; }
		public uint ContextQuantum { get; set; }

		public bool StageTimeStats { get; set; }

		public RecoverKinds RecoverKind { get; set; }
		public uint RecoverPenalty { get; set; }

		public uint ThreadQuantum { get; set; }
		public uint ThreadSwitchPenalty { get; set; }

		public FetchKinds FetchKind { get; set; }

		public uint DecodeWidth { get; set; }

		public DispatchKinds DispatchKind { get; set; }
		public uint DispatchWidth { get; set; }

		public IssueKinds IssueKind { get; set; }
		public uint IssueWidth { get; set; }

		public CommitKinds CommitKind { get; set; }
		public uint CommitWidth { get; set; }

		public BpredConfig Bpred { get; set; }

		public TCacheConfig TCache { get; set; }

		public uint FetchqSize { get; set; }

		public uint UopqSize { get; set; }

		public ROBKinds RobKind { get; set; }
		public uint RobSize { get; set; }

		public RfKinds RfKind { get; set; }
		public uint RfIntSize { get; set; }
		public uint RfFpSize { get; set; }

		public IQKinds IqKind { get; set; }
		public uint IqSize { get; set; }

		public LSQKinds LsqKind { get; set; }
		public uint LsqSize { get; set; }

		public FuConfig IntAdd { get; set; }
		public FuConfig IntSub { get; set; }
		public FuConfig IntMult { get; set; }
		public FuConfig IntDiv { get; set; }
		public FuConfig EffAddr { get; set; }
		public FuConfig Logical { get; set; }
		public FuConfig FpSimple { get; set; }
		public FuConfig FpAdd { get; set; }
		public FuConfig FpComp { get; set; }
		public FuConfig FpMult { get; set; }
		public FuConfig FpDiv { get; set; }
		public FuConfig FpComplex { get; set; }

		public bool IPerfect { get; set; }
		public bool DPerfect { get; set; }

		public uint PageSize { get; set; }

		private static string ToString<T> (string key, T val)
		{
			return "-" + key + " " + val + " ";
		}

		private static string ToString (FuConfig fu)
		{
			return "\"" + fu.Count + "\"" + " " + "\"" + fu.OpLat + "\"" + " " + "\"" + fu.IssueLat + "\"";
		}

		public static PipelineConfig DefaultValue (uint cores, uint threads, string cwd, bool cacheProfilerEnabled)
		{
			PipelineConfig pipelineConfig = new PipelineConfig ();
			pipelineConfig.Cores = cores;
			pipelineConfig.Threads = threads;
			
			pipelineConfig.Report.Pipeline = cwd + Path.DirectorySeparatorChar + INIFILE_REPORT_PIPELINE;
			pipelineConfig.Report.Cache = cwd + Path.DirectorySeparatorChar + INIFILE_REPORT_CACHE;
			pipelineConfig.Report.CacheProfiler = cacheProfilerEnabled ? (cwd + Path.DirectorySeparatorChar + INIFILE_REPORT_CACHE_PROFILER) : "";
			
			return pipelineConfig;
		}

		public static string INIFILE_REPORT_PIPELINE = "report.pipeline";
		public static string INIFILE_REPORT_CACHE = "report.cache";
		public static string INIFILE_REPORT_CACHE_PROFILER = "report.cacheProfiler";
	}

	public sealed class CacheSystemConfig
	{
		public sealed class CacheGeometry
		{
			public enum Policies
			{
				[StringValue("LRU")]
				LRU,
				[StringValue("FIFO")]
				FIFO,
				[StringValue("Random")]
				RANDOM
			}

			public sealed class Serializer : XmlConfigSerializer<CacheGeometry>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (CacheGeometry cacheGeometry)
				{
					XmlConfig xmlConfig = new XmlConfig ("CacheGeometry");
					xmlConfig["name"] = cacheGeometry.Name;
					xmlConfig["size"] = cacheGeometry.Size + "";
					xmlConfig["assoc"] = cacheGeometry.Assoc + "";
					xmlConfig["blockSize"] = cacheGeometry.BlockSize + "";
					xmlConfig["readPorts"] = cacheGeometry.ReadPorts + "";
					xmlConfig["writePorts"] = cacheGeometry.WritePorts + "";
					xmlConfig["latency"] = cacheGeometry.Latency + "";
					xmlConfig["policy"] = EnumUtils.ToStringValue (cacheGeometry.Policy);
					
					return xmlConfig;
				}

				public override CacheGeometry Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					uint size = uint.Parse (xmlConfig["size"]);
					uint assoc = uint.Parse (xmlConfig["assoc"]);
					uint blockSize = uint.Parse (xmlConfig["blockSize"]);
					uint readPorts = uint.Parse (xmlConfig["readPorts"]);
					uint writePorts = uint.Parse (xmlConfig["writePorts"]);
					uint latency = uint.Parse (xmlConfig["latency"]);
					Policies policy = EnumUtils.Parse<Policies> (xmlConfig["policy"]);
					
					CacheGeometry cacheGeometry = new CacheGeometry (name, size, assoc, blockSize, readPorts, writePorts, latency, policy);
					return cacheGeometry;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public CacheGeometry (string name, uint size, uint assoc, uint blockSize, uint readPorts, uint writePorts, uint latency, Policies policy)
			{
				this.Name = name;
				this.Size = size;
				this.Assoc = assoc;
				this.BlockSize = blockSize;
				this.ReadPorts = readPorts;
				this.WritePorts = writePorts;
				this.Latency = latency;
				this.Policy = policy;
			}

			public uint Sets {
				get { return this.Size / this.Assoc / this.BlockSize; }
			}

			public string Name { get; private set; }
			public uint Size { get; private set; }
			public uint Assoc { get; private set; }
			public uint BlockSize { get; private set; }
			public uint ReadPorts { get; private set; }
			public uint WritePorts { get; private set; }
			public uint Latency { get; private set; }
			public Policies Policy { get; private set; }
		}

		public sealed class NetConfig
		{
			public enum Topologies
			{
				[StringValue("Bus")]
				BUS,
				[StringValue("P2P")]
				P2P
			}

			public sealed class Serializer : XmlConfigSerializer<NetConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (NetConfig netConfig)
				{
					if (netConfig == null) {
						return XmlConfig.Null ("NetConfig");
					}
					
					XmlConfig xmlConfig = new XmlConfig ("NetConfig");
					xmlConfig["name"] = netConfig.Name;
					xmlConfig["linkWidth"] = netConfig.LinkWidth + "";
					xmlConfig["topology"] = EnumUtils.ToStringValue (netConfig.Topology);
					
					return xmlConfig;
				}

				public override NetConfig Load (XmlConfig xmlConfig)
				{
					if (xmlConfig.IsNull) {
						return null;
					}
					
					string name = xmlConfig["name"];
					uint linkWidth = uint.Parse (xmlConfig["linkWidth"]);
					Topologies topology = EnumUtils.Parse<Topologies> (xmlConfig["topology"]);
					
					NetConfig netConfig = new NetConfig (name, linkWidth, topology);
					return netConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public NetConfig (string name, uint linkWidth, Topologies topology)
			{
				this.Name = name;
				this.LinkWidth = linkWidth;
				this.Topology = topology;
			}

			public string Name { get; set; }
			public uint LinkWidth { get; set; }
			public Topologies Topology { get; set; }
		}

		public interface HiConnectable
		{
			string HiNetName { get; set; }
		}

		public interface LoConnectable
		{
			string LoNetName { get; set; }
		}

		public sealed class CacheConfig : HiConnectable, LoConnectable
		{
			public sealed class Serializer : XmlConfigSerializer<CacheConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (CacheConfig cacheConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("CacheConfig");
					xmlConfig["name"] = cacheConfig.Name;
					xmlConfig["geometryName"] = cacheConfig.GeometryName;
					xmlConfig["hiNetName"] = cacheConfig.HiNetName;
					xmlConfig["loNetName"] = cacheConfig.LoNetName;
					
					return xmlConfig;
				}

				public override CacheConfig Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					string geometryName = xmlConfig["geometryName"];
					string hiNetName = xmlConfig["hiNetName"];
					string loNetName = xmlConfig["loNetName"];
					
					CacheConfig cacheConfig = new CacheConfig (name, geometryName, hiNetName, loNetName);
					return cacheConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public CacheConfig (string name, string geometryName, string hiNetName, string loNetName)
			{
				this.Name = name;
				this.GeometryName = geometryName;
				this.HiNetName = hiNetName;
				this.LoNetName = loNetName;
			}

			public string Name { get; private set; }
			public string GeometryName { get; private set; }
			public string HiNetName { get; set; }
			public string LoNetName { get; set; }
		}

		public sealed class MainMemoryConfig : HiConnectable
		{
			public sealed class Serializer : XmlConfigSerializer<MainMemoryConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (MainMemoryConfig mainMemoryConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("MainMemoryConfig");
					xmlConfig["blockSize"] = mainMemoryConfig.BlockSize + "";
					xmlConfig["latency"] = mainMemoryConfig.Latency + "";
					xmlConfig["hiNetName"] = mainMemoryConfig.HiNetName;
					
					return xmlConfig;
				}

				public override MainMemoryConfig Load (XmlConfig xmlConfig)
				{
					uint blockSize = uint.Parse (xmlConfig["blockSize"]);
					uint latency = uint.Parse (xmlConfig["latency"]);
					string hiNetName = xmlConfig["hiNetName"];
					
					MainMemoryConfig mainMemoryConfig = new MainMemoryConfig (blockSize, latency, hiNetName);
					return mainMemoryConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public MainMemoryConfig (uint blockSize, uint latency, string hiNetName)
			{
				this.BlockSize = blockSize;
				this.Latency = latency;
				this.HiNetName = hiNetName;
			}

			public uint BlockSize { get; set; }
			public uint Latency { get; set; }
			public string HiNetName { get; set; }
		}

		public sealed class NodeConfig
		{
			public sealed class Serializer : XmlConfigSerializer<NodeConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (NodeConfig nodeConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("NodeConfig");
					xmlConfig["name"] = nodeConfig.Name;
					xmlConfig["core"] = nodeConfig.Core + "";
					xmlConfig["thread"] = nodeConfig.Thread + "";
					xmlConfig["iCacheName"] = nodeConfig.ICacheName;
					xmlConfig["dCacheName"] = nodeConfig.DCacheName;
					
					return xmlConfig;
				}

				public override NodeConfig Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					uint core = uint.Parse (xmlConfig["core"]);
					uint thread = uint.Parse (xmlConfig["thread"]);
					string iCacheName = xmlConfig["iCacheName"];
					string dCacheName = xmlConfig["dCacheName"];
					
					NodeConfig nodeConfig = new NodeConfig (name, core, thread, iCacheName, dCacheName);
					return nodeConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public NodeConfig (string name, uint core, uint thread, string iCacheName, string dCacheName)
			{
				this.Name = name;
				this.Core = core;
				this.Thread = thread;
				this.ICacheName = iCacheName;
				this.DCacheName = dCacheName;
			}

			public string Name { get; set; }
			public uint Core { get; set; }
			public uint Thread { get; set; }
			public string ICacheName { get; set; }
			public string DCacheName { get; set; }
		}

		public sealed class TlbConfig
		{
			public sealed class Serializer : XmlConfigSerializer<TlbConfig>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (TlbConfig tlbConfig)
				{
					XmlConfig xmlConfig = new XmlConfig ("TlbConfig");
					xmlConfig["sets"] = tlbConfig.Sets + "";
					xmlConfig["assoc"] = tlbConfig.Assoc + "";
					xmlConfig["hitLatency"] = tlbConfig.HitLatency + "";
					xmlConfig["missLatency"] = tlbConfig.MissLatency + "";
					
					return xmlConfig;
				}

				public override TlbConfig Load (XmlConfig xmlConfig)
				{
					uint sets = uint.Parse (xmlConfig["sets"]);
					uint assoc = uint.Parse (xmlConfig["assoc"]);
					uint hitLatency = uint.Parse (xmlConfig["hitLatency"]);
					uint missLatency = uint.Parse (xmlConfig["missLatency"]);
					
					TlbConfig tlbConfig = new TlbConfig (sets, assoc, hitLatency, missLatency);
					return tlbConfig;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public TlbConfig (uint sets, uint assoc, uint hitLatency, uint missLatency)
			{
				this.Sets = sets;
				this.Assoc = assoc;
				this.HitLatency = hitLatency;
				this.MissLatency = missLatency;
			}

			public uint Sets { get; set; }
			public uint Assoc { get; set; }
			public uint HitLatency { get; set; }
			public uint MissLatency { get; set; }
		}

		public sealed class Serializer : XmlConfigSerializer<CacheSystemConfig>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (CacheSystemConfig cacheSystemConfig)
			{
				XmlConfig xmlConfig = new XmlConfig ("CacheSystemConfig");
				
				xmlConfig.Entries.Add (SaveList<CacheGeometry> ("cacheGeometries", cacheSystemConfig.CacheGeometries, new SaveEntryDelegate<CacheGeometry> (CacheGeometry.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (SaveList<NetConfig> ("nets", cacheSystemConfig.Nets, new SaveEntryDelegate<NetConfig> (NetConfig.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (SaveList<CacheConfig> ("caches", cacheSystemConfig.Caches, new SaveEntryDelegate<CacheConfig> (CacheConfig.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (MainMemoryConfig.Serializer.SingleInstance.Save (cacheSystemConfig.MainMemory));
				xmlConfig.Entries.Add (SaveList<NodeConfig> ("nodes", cacheSystemConfig.Nodes, new SaveEntryDelegate<NodeConfig> (NodeConfig.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (TlbConfig.Serializer.SingleInstance.Save (cacheSystemConfig.Tlb));
				
				return xmlConfig;
			}

			public override CacheSystemConfig Load (XmlConfig xmlConfig)
			{
				List<CacheGeometry> cacheGeometries = LoadList<CacheGeometry> (xmlConfig.Entries[0], new LoadEntryDelegate<CacheGeometry> (CacheGeometry.Serializer.SingleInstance.Load));
				List<NetConfig> nets = LoadList<NetConfig> (xmlConfig.Entries[1], new LoadEntryDelegate<NetConfig> (NetConfig.Serializer.SingleInstance.Load));
				List<CacheConfig> caches = LoadList<CacheConfig> (xmlConfig.Entries[2], new LoadEntryDelegate<CacheConfig> (CacheConfig.Serializer.SingleInstance.Load));
				MainMemoryConfig mainMemory = MainMemoryConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[3]);
				List<NodeConfig> nodes = LoadList<NodeConfig> (xmlConfig.Entries[4], new LoadEntryDelegate<NodeConfig> (NodeConfig.Serializer.SingleInstance.Load));
				TlbConfig tlb = TlbConfig.Serializer.SingleInstance.Load (xmlConfig.Entries[5]);
				
				CacheSystemConfig cacheSystemConfig = new CacheSystemConfig ();
				cacheSystemConfig.CacheGeometries = cacheGeometries;
				cacheSystemConfig.Nets = nets;
				cacheSystemConfig.Caches = caches;
				cacheSystemConfig.MainMemory = mainMemory;
				cacheSystemConfig.Nodes = nodes;
				cacheSystemConfig.Tlb = tlb;
				
				return cacheSystemConfig;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public CacheSystemConfig ()
		{
			this.CacheGeometries = new List<CacheGeometry> ();
			this.Nets = new List<NetConfig> ();
			this.Caches = new List<CacheConfig> ();
			this.MainMemory = new MainMemoryConfig (200, 64, null);
			this.Nodes = new List<NodeConfig> ();
			this.Tlb = new TlbConfig (64, 4, 2, 30);
		}

		public void WriteTo (string fileName)
		{
			IniFile iniFile = new IniFile ();
			
			foreach (CacheGeometry geometry in this.CacheGeometries) {
				IniFile.Section section = new IniFile.Section ("CacheGeometry " + geometry.Name);
				iniFile[section.Name] = section;
				
				section.Register (new IniFile.Property ("Sets", geometry.Sets + ""));
				section.Register (new IniFile.Property ("Assoc", geometry.Assoc + ""));
				section.Register (new IniFile.Property ("BlockSize", geometry.BlockSize + ""));
				section.Register (new IniFile.Property ("ReadPorts", geometry.ReadPorts + ""));
				section.Register (new IniFile.Property ("WritePorts", geometry.WritePorts + ""));
				section.Register (new IniFile.Property ("Latency", geometry.Latency + ""));
				section.Register (new IniFile.Property ("Policy", EnumUtils.ToStringValue (geometry.Policy)));
			}
			
			foreach (CacheConfig cache in this.Caches) {
				IniFile.Section section = new IniFile.Section ("Cache " + cache.Name);
				iniFile.Register (section);
				
				section.Register (new IniFile.Property ("Geometry", cache.GeometryName));
				section.Register (new IniFile.Property ("HiNet", cache.HiNetName));
				section.Register (new IniFile.Property ("loNet", cache.LoNetName));
			}
			
			IniFile.Section sectionMainMemory = new IniFile.Section ("MainMemory");
			iniFile.Register (sectionMainMemory);
			
			sectionMainMemory.Register (new IniFile.Property ("HiNet", this.MainMemory.HiNetName));
			sectionMainMemory.Register (new IniFile.Property ("BlockSize", this.MainMemory.BlockSize + ""));
			sectionMainMemory.Register (new IniFile.Property ("Latency", this.MainMemory.Latency + ""));
			
			foreach (NodeConfig node in this.Nodes) {
				IniFile.Section section = new IniFile.Section ("Node " + node.Name);
				iniFile.Register (section);
				
				section.Register (new IniFile.Property ("Core", node.Core + ""));
				section.Register (new IniFile.Property ("Thread", node.Thread + ""));
				section.Register (new IniFile.Property ("DCache", node.DCacheName));
				section.Register (new IniFile.Property ("ICache", node.ICacheName));
			}
			
			foreach (NetConfig net in this.Nets) {
				IniFile.Section section = new IniFile.Section ("Net " + net.Name);
				iniFile.Register (section);
				
				section.Register (new IniFile.Property ("LinkWidth", net.LinkWidth + ""));
				section.Register (new IniFile.Property ("Topology", EnumUtils.ToStringValue (net.Topology)));
			}
			
			IniFile.Section sectionTlb = new IniFile.Section ("Tlb");
			iniFile.Register (sectionTlb);
			
			sectionTlb.Register (new IniFile.Property ("Sets", this.Tlb.Sets + ""));
			sectionTlb.Register (new IniFile.Property ("Assoc", this.Tlb.Assoc + ""));
			sectionTlb.Register (new IniFile.Property ("HitLatency", this.Tlb.HitLatency + ""));
			sectionTlb.Register (new IniFile.Property ("MissLatency", this.Tlb.MissLatency + ""));
			
			iniFile.save (fileName);
		}

		public List<CacheGeometry> CacheGeometries { get; set; }
		public List<NetConfig> Nets { get; set; }
		public List<CacheConfig> Caches { get; set; }
		public MainMemoryConfig MainMemory { get; set; }
		public List<NodeConfig> Nodes { get; set; }
		public TlbConfig Tlb { get; set; }

		public static void connect (LoConnectable nodeA, HiConnectable nodeB, NetConfig net)
		{
			nodeA.LoNetName = net.Name;
			nodeB.HiNetName = net.Name;
		}

		public static CacheSystemConfig Q6600 ()
		{
			CacheSystemConfig cacheSystemConfig = new CacheSystemConfig ();
			
			//nets
			NetConfig net0 = new NetConfig ("net-0", 32, NetConfig.Topologies.BUS);
			NetConfig net1 = new NetConfig ("net-1", 32, NetConfig.Topologies.BUS);
			NetConfig net2 = new NetConfig ("net-2", 32, NetConfig.Topologies.BUS);
			
			cacheSystemConfig.Nets.Add (net0);
			cacheSystemConfig.Nets.Add (net1);
			cacheSystemConfig.Nets.Add (net2);
			
			//main memory
			MainMemoryConfig mainMemory = new MainMemoryConfig (64, 200, null);
			cacheSystemConfig.MainMemory = mainMemory;
			
			//cache geometries: ref, approx.: http://tech.icrontic.com/articles/core_2_duo/
			//32K bytes (x4)
			CacheGeometry geometryL1 = new CacheGeometry ("l1geo", 64, 8, 64, 2, 1, 3, CacheGeometry.Policies.LRU);
			
			//4096K bytes (x2)
			CacheGeometry geometryL2 = new CacheGeometry ("l2geo", 4096, 16, 64, 2, 1, 10, CacheGeometry.Policies.LRU);
			
			cacheSystemConfig.CacheGeometries.Add (geometryL1);
			cacheSystemConfig.CacheGeometries.Add (geometryL2);
			
			//l2 cache
			CacheConfig cacheL2_0 = new CacheConfig ("l2-0", geometryL2.Name, null, null);
			cacheSystemConfig.Caches.Add (cacheL2_0);
			
			CacheConfig cacheL2_1 = new CacheConfig ("l2-1", geometryL2.Name, null, null);
			cacheSystemConfig.Caches.Add (cacheL2_1);
			
			//connect l2 cache with main memory
			CacheSystemConfig.connect (cacheL2_0, mainMemory, net2);
			CacheSystemConfig.connect (cacheL2_1, mainMemory, net2);
			
			//l1 cache
			for (uint core = 0; core < 2; core++) {
				CacheConfig cacheDL1 = new CacheConfig ("dl1-" + core, geometryL1.Name, null, null);
				CacheConfig cacheIL1 = new CacheConfig ("il1-" + core, geometryL1.Name, null, null);
				
				cacheSystemConfig.Caches.Add (cacheDL1);
				cacheSystemConfig.Caches.Add (cacheIL1);
				
				//connect l1 cache with l2 cache
				CacheSystemConfig.connect (cacheDL1, cacheL2_0, net0);
				CacheSystemConfig.connect (cacheIL1, cacheL2_0, net0);
				
				//connect node with l1 cache
				NodeConfig node = new NodeConfig (core + "", core, 0, cacheIL1.Name, cacheDL1.Name);
				cacheSystemConfig.Nodes.Add (node);
			}
			
			//l1 cache
			for (uint core = 2; core < 4; core++) {
				CacheConfig cacheDL1 = new CacheConfig ("dl1-" + core, geometryL1.Name, null, null);
				CacheConfig cacheIL1 = new CacheConfig ("il1-" + core, geometryL1.Name, null, null);
				
				cacheSystemConfig.Caches.Add (cacheDL1);
				cacheSystemConfig.Caches.Add (cacheIL1);
				
				//connect l1 cache with l2 cache
				CacheSystemConfig.connect (cacheDL1, cacheL2_1, net1);
				CacheSystemConfig.connect (cacheIL1, cacheL2_1, net1);
				
				//connect node with l1 cache
				NodeConfig node = new NodeConfig (core + "", core, 0, cacheIL1.Name, cacheDL1.Name);
				cacheSystemConfig.Nodes.Add (node);
			}
			
			//tlb
			cacheSystemConfig.Tlb = new TlbConfig (64, 4, 2, 30);
			
			return cacheSystemConfig;
		}

		public static CacheSystemConfig corei7_930 ()
		{
			CacheSystemConfig cacheSystemConfig = new CacheSystemConfig ();
			
			//nets
			NetConfig net0 = new NetConfig ("net-0", 32, NetConfig.Topologies.BUS);
			NetConfig net1 = new NetConfig ("net-1", 32, NetConfig.Topologies.BUS);
			NetConfig net2 = new NetConfig ("net-2", 32, NetConfig.Topologies.BUS);
			NetConfig net3 = new NetConfig ("net-3", 32, NetConfig.Topologies.BUS);
			NetConfig net4 = new NetConfig ("net-4", 32, NetConfig.Topologies.BUS);
			NetConfig net5 = new NetConfig ("net-5", 32, NetConfig.Topologies.BUS);
			
			cacheSystemConfig.Nets.Add (net0);
			cacheSystemConfig.Nets.Add (net1);
			cacheSystemConfig.Nets.Add (net2);
			cacheSystemConfig.Nets.Add (net3);
			cacheSystemConfig.Nets.Add (net4);
			cacheSystemConfig.Nets.Add (net5);
			
			//main memory, ref: http://www.anandtech.com/show/2658/4
			MainMemoryConfig mainMemory = new MainMemoryConfig (64, 107, null);
			cacheSystemConfig.MainMemory = mainMemory;
			
			//cache geometries, ref: http://www.anandtech.com/show/2658/4
			//32K bytes (x4)
			CacheGeometry geometryL1I = new CacheGeometry ("l1igeo", 128, 4, 64, 2, 1, 4, CacheGeometry.Policies.LRU);
			//32K bytes (x4)
			CacheGeometry geometryL1D = new CacheGeometry ("l1dgeo", 64, 8, 64, 2, 1, 4, CacheGeometry.Policies.LRU);
			
			//256K bytes (x4)
			CacheGeometry geometryL2 = new CacheGeometry ("l2geo", 512, 8, 64, 2, 1, 11, CacheGeometry.Policies.LRU);
			
			//8192K bytes (x1)
			CacheGeometry geometryL3 = new CacheGeometry ("l3geo", 8192, 16, 64, 2, 1, 39, CacheGeometry.Policies.LRU);
			
			cacheSystemConfig.CacheGeometries.Add (geometryL1I);
			cacheSystemConfig.CacheGeometries.Add (geometryL1D);
			cacheSystemConfig.CacheGeometries.Add (geometryL2);
			cacheSystemConfig.CacheGeometries.Add (geometryL3);
			
			//l3 cache
			CacheConfig cacheL3 = new CacheConfig ("l3", geometryL3.Name, null, null);
			cacheSystemConfig.Caches.Add (cacheL3);
			
			//connect l3 cache with main memory
			CacheSystemConfig.connect (cacheL3, mainMemory, net5);
			
			for (uint core = 0; core < 4; core++) {
				//l1 cache
				CacheConfig cacheIL1 = new CacheConfig ("il1-" + core, geometryL1I.Name, null, null);
				CacheConfig cacheDL1 = new CacheConfig ("dl1-" + core, geometryL1D.Name, null, null);
				
				cacheSystemConfig.Caches.Add (cacheIL1);
				cacheSystemConfig.Caches.Add (cacheDL1);
				
				//l2 cache
				CacheConfig cacheL2 = new CacheConfig ("l2-" + core, geometryL2.Name, null, null);
				cacheSystemConfig.Caches.Add (cacheL2);
				
				//connect l1 cache with l2 cache
				CacheSystemConfig.connect (cacheIL1, cacheL2, cacheSystemConfig.Nets[(int)core]);
				CacheSystemConfig.connect (cacheDL1, cacheL2, cacheSystemConfig.Nets[(int)core]);
				
				//connect l2 cache with l3 cache
				CacheSystemConfig.connect (cacheL2, cacheL3, net4);
				
				//connect node with l1 cache
				for (uint thread = 0; thread < 2; thread++) {
					NodeConfig node = new NodeConfig ((core * 2 + thread) + "", core, thread, cacheIL1.Name, cacheDL1.Name);
					cacheSystemConfig.Nodes.Add (node);
				}
			}
			
			//tlb
			cacheSystemConfig.Tlb = new TlbConfig (64, 4, 2, 30);
			
			return cacheSystemConfig;
		}

		public static CacheSystemConfig DefaultValue (uint cores, uint threads)
		{
			CacheSystemConfig cacheSystemConfig = new CacheSystemConfig ();
			
			//nets
			NetConfig net0 = new NetConfig ("net-0", 32, NetConfig.Topologies.BUS);
			NetConfig net1 = new NetConfig ("net-1", 32, NetConfig.Topologies.BUS);
			
			cacheSystemConfig.Nets.Add (net0);
			cacheSystemConfig.Nets.Add (net1);
			
			//main memory
			MainMemoryConfig mainMemory = new MainMemoryConfig (64, 200, null);
			cacheSystemConfig.MainMemory = mainMemory;
			
			//cache geometries
			CacheGeometry geometryL1 = new CacheGeometry ("l1geo", 256, 2, 64, 2, 1, 2, CacheGeometry.Policies.LRU);
			CacheGeometry geometryL2 = new CacheGeometry ("l2geo", 1024, 8, 64, 2, 1, 20, CacheGeometry.Policies.LRU);
			
			cacheSystemConfig.CacheGeometries.Add (geometryL1);
			cacheSystemConfig.CacheGeometries.Add (geometryL2);
			
			//l2 cache
			CacheConfig cacheL2 = new CacheConfig ("l2", geometryL2.Name, null, null);
			cacheSystemConfig.Caches.Add (cacheL2);
			
			//connect l2 cache with main memory
			CacheSystemConfig.connect (cacheL2, mainMemory, net1);
			
			//l1 cache
			for (uint core = 0; core < cores; core++) {
				CacheConfig cacheIL1 = new CacheConfig ("il1-" + core, geometryL1.Name, null, null);
				CacheConfig cacheDL1 = new CacheConfig ("dl1-" + core, geometryL1.Name, null, null);
				
				cacheSystemConfig.Caches.Add (cacheIL1);
				cacheSystemConfig.Caches.Add (cacheDL1);
				
				//connect l1 cache with l2 cache
				connect (cacheIL1, cacheL2, net0);
				connect (cacheDL1, cacheL2, net0);
				
				//connect node with l1 cache
				for (uint thread = 0; thread < threads; thread++) {
					NodeConfig node = new NodeConfig ((core * threads + thread) + "", core, thread, cacheIL1.Name, cacheDL1.Name);
					cacheSystemConfig.Nodes.Add (node);
				}
			}
			
			//tlb
			cacheSystemConfig.Tlb = new TlbConfig (64, 4, 2, 30);
			
			return cacheSystemConfig;
		}
	}

	public sealed class ContextConfig
	{
		public sealed class Serializer : XmlConfigSerializer<ContextConfig>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (ContextConfig contextConfig)
			{
				XmlConfig xmlConfig = new XmlConfig ("ContextConfig");
				
				xmlConfig.Entries.Add (SaveUintDictionary<Workload> ("Contexts", contextConfig.Contexts, new SaveEntryDelegate<Workload> (Workload.Serializer.SingleInstance.Save)));
				
				return xmlConfig;
			}

			public override ContextConfig Load (XmlConfig xmlConfig)
			{
				SortedDictionary<uint, Workload> contexts = LoadUintDictionary<Workload> (xmlConfig.Entries[0], new LoadEntryDelegate<Workload> (Workload.Serializer.SingleInstance.Load), workload => workload.Num);
				
				ContextConfig contextConfig = new ContextConfig ();
				contextConfig.Contexts = contexts;
				
				return contextConfig;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public ContextConfig ()
		{
			this.Contexts = new SortedDictionary<uint, Workload> ();
		}

		public void WriteTo (string fileName)
		{
			IniFile iniFile = new IniFile ();
			
			foreach (KeyValuePair<uint, Workload> pair in this.Contexts) {
				uint num = pair.Key;
				Workload context = pair.Value;
				
				IniFile.Section section = new IniFile.Section ("Context " + num);
				iniFile.Register (section);
				
				section.Register (new IniFile.Property ("Exe", context.Exe));
				section.Register (new IniFile.Property ("Args", context.Args));
				
				if (!String.IsNullOrEmpty (context.Cwd)) {
					section.Register (new IniFile.Property ("Cwd", context.Cwd));
				}
				
				if (!String.IsNullOrEmpty (context.Stdin)) {
					section.Register (new IniFile.Property ("Stdin", context.Stdin));
				}
				
				if (!String.IsNullOrEmpty (context.Stdout)) {
					section.Register (new IniFile.Property ("Stdout", context.Stdout));
				}
			}
			
			iniFile.save (fileName);
		}

		public SortedDictionary<uint, Workload> Contexts { get; set; }
	}

	#endregion

	#region Report

	public sealed class PipelineReport
	{
		public sealed class UopReport
		{
			public sealed class Serializer : XmlConfigSerializer<UopReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (UopReport uopReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("UopReport");
					xmlConfig["name"] = uopReport.Name;
					xmlConfig["uopNop"] = uopReport.UopNop + "";
					xmlConfig["uopMove"] = uopReport.UopMove + "";
					xmlConfig["uopAdd"] = uopReport.UopAdd + "";
					xmlConfig["uopSub"] = uopReport.UopSub + "";
					xmlConfig["uopMult"] = uopReport.UopMult + "";
					xmlConfig["uopDiv"] = uopReport.UopDiv + "";
					xmlConfig["uopEffAddr"] = uopReport.UopEffAddr + "";
					xmlConfig["uopAnd"] = uopReport.UopAnd + "";
					xmlConfig["uopOr"] = uopReport.UopOr + "";
					xmlConfig["uopXor"] = uopReport.UopXor + "";
					xmlConfig["uopNot"] = uopReport.UopNot + "";
					xmlConfig["uopShift"] = uopReport.UopShift + "";
					xmlConfig["uopSign"] = uopReport.UopSign + "";
					xmlConfig["uopFpMove"] = uopReport.UopFpMove + "";
					xmlConfig["uopFpSimple"] = uopReport.UopFpSimple + "";
					xmlConfig["uopFpAdd"] = uopReport.UopFpAdd + "";
					xmlConfig["uopFpComp"] = uopReport.UopFpComp + "";
					xmlConfig["uopFpMult"] = uopReport.UopFpMult + "";
					xmlConfig["uopFpDiv"] = uopReport.UopFpDiv + "";
					xmlConfig["uopFpComplex"] = uopReport.UopFpComplex + "";
					xmlConfig["uopLoad"] = uopReport.UopLoad + "";
					xmlConfig["uopStore"] = uopReport.UopStore + "";
					xmlConfig["uopCall"] = uopReport.UopCall + "";
					xmlConfig["uopRet"] = uopReport.UopRet + "";
					xmlConfig["uopJump"] = uopReport.UopJump + "";
					xmlConfig["uopBranch"] = uopReport.UopBranch + "";
					xmlConfig["uopSyscall"] = uopReport.UopSyscall + "";
					
					xmlConfig["simpleInteger"] = uopReport.SimpleInteger + "";
					xmlConfig["complexInteger"] = uopReport.ComplexInteger + "";
					xmlConfig["integer"] = uopReport.Integer + "";
					xmlConfig["logical"] = uopReport.Logical + "";
					xmlConfig["floatingPoint"] = uopReport.FloatingPoint + "";
					xmlConfig["memory"] = uopReport.Memory + "";
					xmlConfig["ctrl"] = uopReport.Ctrl + "";
					xmlConfig["wndSwitch"] = uopReport.WndSwitch + "";
					xmlConfig["total"] = uopReport.Total + "";
					xmlConfig["ipc"] = uopReport.IPC + "";
					xmlConfig["dutyCycle"] = uopReport.DutyCycle + "";
					
					return xmlConfig;
				}

				public override UopReport Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong uopNop = ulong.Parse (xmlConfig["uopNop"]);
					ulong uopMove = ulong.Parse (xmlConfig["uopMove"]);
					ulong uopAdd = ulong.Parse (xmlConfig["uopAdd"]);
					ulong uopSub = ulong.Parse (xmlConfig["uopSub"]);
					ulong uopMult = ulong.Parse (xmlConfig["uopMult"]);
					ulong uopDiv = ulong.Parse (xmlConfig["uopDiv"]);
					ulong uopEffAddr = ulong.Parse (xmlConfig["uopEffAddr"]);
					ulong uopAnd = ulong.Parse (xmlConfig["uopAnd"]);
					ulong uopOr = ulong.Parse (xmlConfig["uopOr"]);
					ulong uopXor = ulong.Parse (xmlConfig["uopXor"]);
					ulong uopNot = ulong.Parse (xmlConfig["uopNot"]);
					ulong uopShift = ulong.Parse (xmlConfig["uopShift"]);
					ulong uopSign = ulong.Parse (xmlConfig["uopSign"]);
					ulong uopFpMove = ulong.Parse (xmlConfig["uopFpMove"]);
					ulong uopFpSimple = ulong.Parse (xmlConfig["uopFpSimple"]);
					ulong uopFpAdd = ulong.Parse (xmlConfig["uopFpAdd"]);
					ulong uopFpComp = ulong.Parse (xmlConfig["uopFpComp"]);
					ulong uopFpMult = ulong.Parse (xmlConfig["uopFpMult"]);
					ulong uopFpDiv = ulong.Parse (xmlConfig["uopFpDiv"]);
					ulong uopFpComplex = ulong.Parse (xmlConfig["uopFpComplex"]);
					ulong uopLoad = ulong.Parse (xmlConfig["uopLoad"]);
					ulong uopStore = ulong.Parse (xmlConfig["uopStore"]);
					ulong uopCall = ulong.Parse (xmlConfig["uopCall"]);
					ulong uopRet = ulong.Parse (xmlConfig["uopRet"]);
					ulong uopJump = ulong.Parse (xmlConfig["uopJump"]);
					ulong uopBranch = ulong.Parse (xmlConfig["uopBranch"]);
					ulong uopSyscall = ulong.Parse (xmlConfig["uopSyscall"]);
					
					ulong simpleInteger = ulong.Parse (xmlConfig["simpleInteger"]);
					ulong complexInteger = ulong.Parse (xmlConfig["complexInteger"]);
					ulong integer = ulong.Parse (xmlConfig["integer"]);
					ulong logical = ulong.Parse (xmlConfig["logical"]);
					ulong floatingPoint = ulong.Parse (xmlConfig["floatingPoint"]);
					ulong memory = ulong.Parse (xmlConfig["memory"]);
					ulong ctrl = ulong.Parse (xmlConfig["ctrl"]);
					ulong wndSwitch = ulong.Parse (xmlConfig["wndSwitch"]);
					ulong total = ulong.Parse (xmlConfig["total"]);
					double ipc = double.Parse (xmlConfig["ipc"]);
					double dutyCycle = double.Parse (xmlConfig["dutyCycle"]);
					
					UopReport uopReport = new UopReport ();
					uopReport.Name = name;
					
					uopReport.UopNop = uopNop;
					uopReport.UopMove = uopMove;
					uopReport.UopAdd = uopAdd;
					uopReport.UopSub = uopSub;
					uopReport.UopMult = uopMult;
					uopReport.UopDiv = uopDiv;
					uopReport.UopEffAddr = uopEffAddr;
					uopReport.UopAnd = uopAnd;
					uopReport.UopOr = uopOr;
					uopReport.UopXor = uopXor;
					uopReport.UopNot = uopNot;
					uopReport.UopShift = uopShift;
					uopReport.UopSign = uopSign;
					uopReport.UopFpMove = uopFpMove;
					uopReport.UopFpSimple = uopFpSimple;
					uopReport.UopFpAdd = uopFpAdd;
					uopReport.UopFpComp = uopFpComp;
					uopReport.UopFpMult = uopFpMult;
					uopReport.UopFpDiv = uopFpDiv;
					uopReport.UopFpComplex = uopFpComplex;
					uopReport.UopLoad = uopLoad;
					uopReport.UopStore = uopStore;
					uopReport.UopCall = uopCall;
					uopReport.UopRet = uopRet;
					uopReport.UopJump = uopJump;
					uopReport.UopBranch = uopBranch;
					uopReport.UopSyscall = uopSyscall;
					
					uopReport.SimpleInteger = simpleInteger;
					uopReport.ComplexInteger = complexInteger;
					uopReport.Integer = integer;
					uopReport.Logical = logical;
					uopReport.FloatingPoint = floatingPoint;
					uopReport.Memory = memory;
					uopReport.Ctrl = ctrl;
					uopReport.WndSwitch = wndSwitch;
					uopReport.Total = total;
					uopReport.IPC = ipc;
					uopReport.DutyCycle = dutyCycle;
					
					return uopReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public UopReport ()
			{
			}

			public void Load (IniFile iniFile, string sectionName, string prefix)
			{
				this.Name = prefix;
				
				this.UopNop = ulong.Parse (iniFile[sectionName][prefix + ".Uop.nop"].Value);
				this.UopMove = ulong.Parse (iniFile[sectionName][prefix + ".Uop.move"].Value);
				this.UopAdd = ulong.Parse (iniFile[sectionName][prefix + ".Uop.add"].Value);
				this.UopSub = ulong.Parse (iniFile[sectionName][prefix + ".Uop.sub"].Value);
				this.UopMult = ulong.Parse (iniFile[sectionName][prefix + ".Uop.mult"].Value);
				this.UopDiv = ulong.Parse (iniFile[sectionName][prefix + ".Uop.div"].Value);
				this.UopEffAddr = ulong.Parse (iniFile[sectionName][prefix + ".Uop.effaddr"].Value);
				this.UopAnd = ulong.Parse (iniFile[sectionName][prefix + ".Uop.and"].Value);
				this.UopOr = ulong.Parse (iniFile[sectionName][prefix + ".Uop.or"].Value);
				this.UopXor = ulong.Parse (iniFile[sectionName][prefix + ".Uop.xor"].Value);
				this.UopNot = ulong.Parse (iniFile[sectionName][prefix + ".Uop.not"].Value);
				this.UopShift = ulong.Parse (iniFile[sectionName][prefix + ".Uop.shift"].Value);
				this.UopSign = ulong.Parse (iniFile[sectionName][prefix + ".Uop.sign"].Value);
				this.UopFpMove = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpmove"].Value);
				this.UopFpSimple = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpsimple"].Value);
				this.UopFpAdd = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpadd"].Value);
				this.UopFpComp = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpcomp"].Value);
				this.UopFpMult = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpmult"].Value);
				this.UopFpDiv = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpdiv"].Value);
				this.UopFpComplex = ulong.Parse (iniFile[sectionName][prefix + ".Uop.fpcomplex"].Value);
				this.UopLoad = ulong.Parse (iniFile[sectionName][prefix + ".Uop.load"].Value);
				this.UopStore = ulong.Parse (iniFile[sectionName][prefix + ".Uop.store"].Value);
				this.UopCall = ulong.Parse (iniFile[sectionName][prefix + ".Uop.call"].Value);
				this.UopRet = ulong.Parse (iniFile[sectionName][prefix + ".Uop.ret"].Value);
				this.UopJump = ulong.Parse (iniFile[sectionName][prefix + ".Uop.jump"].Value);
				this.UopBranch = ulong.Parse (iniFile[sectionName][prefix + ".Uop.branch"].Value);
				this.UopSyscall = ulong.Parse (iniFile[sectionName][prefix + ".Uop.syscall"].Value);
				
				this.SimpleInteger = ulong.Parse (iniFile[sectionName][prefix + ".SimpleInteger"].Value);
				this.ComplexInteger = ulong.Parse (iniFile[sectionName][prefix + ".ComplexInteger"].Value);
				this.Integer = ulong.Parse (iniFile[sectionName][prefix + ".Integer"].Value);
				this.Logical = ulong.Parse (iniFile[sectionName][prefix + ".Logical"].Value);
				this.FloatingPoint = ulong.Parse (iniFile[sectionName][prefix + ".FloatingPoint"].Value);
				this.Memory = ulong.Parse (iniFile[sectionName][prefix + ".Memory"].Value);
				this.Ctrl = ulong.Parse (iniFile[sectionName][prefix + ".Ctrl"].Value);
				this.WndSwitch = ulong.Parse (iniFile[sectionName][prefix + ".WndSwitch"].Value);
				this.Total = ulong.Parse (iniFile[sectionName][prefix + ".Total"].Value);
				this.IPC = double.Parse (iniFile[sectionName][prefix + ".IPC"].Value);
				this.DutyCycle = double.Parse (iniFile[sectionName][prefix + ".DutyCycle"].Value);
			}

			public string Name { get; set; }

			public ulong UopNop { get; set; }
			public ulong UopMove { get; set; }
			public ulong UopAdd { get; set; }
			public ulong UopSub { get; set; }
			public ulong UopMult { get; set; }
			public ulong UopDiv { get; set; }
			public ulong UopEffAddr { get; set; }
			public ulong UopAnd { get; set; }
			public ulong UopOr { get; set; }
			public ulong UopXor { get; set; }
			public ulong UopNot { get; set; }
			public ulong UopShift { get; set; }
			public ulong UopSign { get; set; }
			public ulong UopFpMove { get; set; }
			public ulong UopFpSimple { get; set; }
			public ulong UopFpAdd { get; set; }
			public ulong UopFpComp { get; set; }
			public ulong UopFpMult { get; set; }
			public ulong UopFpDiv { get; set; }
			public ulong UopFpComplex { get; set; }
			public ulong UopLoad { get; set; }
			public ulong UopStore { get; set; }
			public ulong UopCall { get; set; }
			public ulong UopRet { get; set; }
			public ulong UopJump { get; set; }
			public ulong UopBranch { get; set; }
			public ulong UopSyscall { get; set; }

			public ulong SimpleInteger { get; set; }
			public ulong ComplexInteger { get; set; }
			public ulong Integer { get; set; }
			public ulong Logical { get; set; }
			public ulong FloatingPoint { get; set; }
			public ulong Memory { get; set; }
			public ulong Ctrl { get; set; }
			public ulong WndSwitch { get; set; }
			public ulong Total { get; set; }
			public double IPC { get; set; }
			public double DutyCycle { get; set; }
		}

		public sealed class FuStat
		{
			public sealed class Serializer : XmlConfigSerializer<FuStat>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (FuStat fuStat)
				{
					XmlConfig xmlConfig = new XmlConfig ("FuStat");
					xmlConfig["name"] = fuStat.Name;
					xmlConfig["accesses"] = fuStat.Accesses + "";
					xmlConfig["denied"] = fuStat.Denied + "";
					xmlConfig["waitingTime"] = fuStat.WaitingTime + "";
					
					return xmlConfig;
				}

				public override FuStat Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong accesses = ulong.Parse (xmlConfig["accesses"]);
					ulong denied = ulong.Parse (xmlConfig["denied"]);
					double waitingTime = double.Parse (xmlConfig["waitingTime"]);
					
					FuStat fuStat = new FuStat ();
					fuStat.Name = name;
					fuStat.Accesses = accesses;
					fuStat.Denied = denied;
					fuStat.WaitingTime = waitingTime;
					
					return fuStat;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public FuStat ()
			{
			}

			public void Load (IniFile iniFile, string sectionName, string fuName)
			{
				this.Name = fuName;
				
				this.Accesses = ulong.Parse (iniFile[sectionName]["fu." + fuName + ".Accesses"].Value);
				this.Denied = ulong.Parse (iniFile[sectionName]["fu." + fuName + ".Denied"].Value);
				this.WaitingTime = double.Parse (iniFile[sectionName]["fu." + fuName + ".WaitingTime"].Value);
			}

			public string Name { get; set; }
			public ulong Accesses { get; set; }
			public ulong Denied { get; set; }
			public double WaitingTime { get; set; }
		}

		public sealed class DispatchStat
		{
			public sealed class Serializer : XmlConfigSerializer<DispatchStat>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (DispatchStat dispatchStat)
				{
					XmlConfig xmlConfig = new XmlConfig ("DispatchStat");
					xmlConfig["name"] = dispatchStat.Name;
					xmlConfig["dispatchStall"] = dispatchStat.DispatchStall + "";
					
					return xmlConfig;
				}

				public override DispatchStat Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong dispatchStall = ulong.Parse (xmlConfig["dispatchStall"]);
					
					DispatchStat dispatchStat = new DispatchStat ();
					dispatchStat.Name = name;
					dispatchStat.DispatchStall = dispatchStall;
					
					return dispatchStat;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public DispatchStat ()
			{
			}

			public void Load (IniFile iniFile, string sectionName, string postfix)
			{
				this.Name = postfix;
				
				this.DispatchStall = ulong.Parse (iniFile[sectionName]["Dispatch.Stall." + postfix].Value);
			}

			public string Name { get; set; }
			public ulong DispatchStall { get; set; }
		}

		public sealed class CoreStructStat
		{
			public sealed class Serializer : XmlConfigSerializer<CoreStructStat>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (CoreStructStat coreStructStat)
				{
					if (coreStructStat == null) {
						return XmlConfig.Null ("CoreStructStat");
					}
					
					XmlConfig xmlConfig = new XmlConfig ("CoreStructStat");
					xmlConfig["size"] = coreStructStat.Size + "";
					xmlConfig["occupancy"] = coreStructStat.Occupancy + "";
					xmlConfig["full"] = coreStructStat.Full + "";
					xmlConfig["reads"] = coreStructStat.Reads + "";
					xmlConfig["writes"] = coreStructStat.Writes + "";
					
					return xmlConfig;
				}

				public override CoreStructStat Load (XmlConfig xmlConfig)
				{
					if (xmlConfig.IsNull) {
						return null;
					}
					
					uint size = uint.Parse (xmlConfig["size"]);
					double occupancy = double.Parse (xmlConfig["occupancy"]);
					ulong full = ulong.Parse (xmlConfig["full"]);
					ulong reads = ulong.Parse (xmlConfig["reads"]);
					ulong writes = ulong.Parse (xmlConfig["writes"]);
					
					CoreStructStat coreStructStat = new CoreStructStat ();
					coreStructStat.Size = size;
					coreStructStat.Occupancy = occupancy;
					coreStructStat.Full = full;
					coreStructStat.Reads = reads;
					coreStructStat.Writes = writes;
					
					return coreStructStat;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public CoreStructStat ()
			{
			}

			public void Load (IniFile iniFile, string sectionName, string prefix)
			{
				this.Size = uint.Parse (iniFile[sectionName][prefix + ".Size"].Value);
				this.Occupancy = double.Parse (iniFile[sectionName][prefix + ".Occupancy"].Value);
				this.Full = ulong.Parse (iniFile[sectionName][prefix + ".Full"].Value);
				this.Reads = ulong.Parse (iniFile[sectionName][prefix + ".Reads"].Value);
				this.Writes = ulong.Parse (iniFile[sectionName][prefix + ".Writes"].Value);
			}

			public uint Size { get; set; }
			public double Occupancy { get; set; }
			public ulong Full { get; set; }
			public ulong Reads { get; set; }
			public ulong Writes { get; set; }
		}

		public sealed class ThreadStructStat
		{
			public sealed class Serializer : XmlConfigSerializer<ThreadStructStat>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (ThreadStructStat threadStructStat)
				{
					XmlConfig xmlConfig = new XmlConfig ("ThreadStructStat");
					xmlConfig["name"] = threadStructStat.Name;
					xmlConfig["size"] = threadStructStat.Size + "";
					xmlConfig["occupancy"] = threadStructStat.Occupancy + "";
					xmlConfig["full"] = threadStructStat.Full + "";
					xmlConfig["reads"] = threadStructStat.Reads + "";
					xmlConfig["writes"] = threadStructStat.Writes + "";
					
					return xmlConfig;
				}

				public override ThreadStructStat Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					uint size = uint.Parse (xmlConfig["size"]);
					double occupancy = double.Parse (xmlConfig["occupancy"]);
					ulong full = ulong.Parse (xmlConfig["full"]);
					ulong reads = ulong.Parse (xmlConfig["reads"]);
					ulong writes = ulong.Parse (xmlConfig["writes"]);
					
					ThreadStructStat threadStructStat = new ThreadStructStat ();
					threadStructStat.Name = name;
					threadStructStat.Size = size;
					threadStructStat.Occupancy = occupancy;
					threadStructStat.Full = full;
					threadStructStat.Reads = reads;
					threadStructStat.Writes = writes;
					
					return threadStructStat;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public ThreadStructStat ()
			{
			}

			public void Load (IniFile iniFile, string sectionName, string prefix)
			{
				this.Name = prefix;
				
				this.Size = uint.Parse (iniFile[sectionName][prefix + ".Size"].Value);
				this.Occupancy = double.Parse (iniFile[sectionName][prefix + ".Occupancy"].Value);
				this.Full = ulong.Parse (iniFile[sectionName][prefix + ".Full"].Value);
				this.Reads = ulong.Parse (iniFile[sectionName][prefix + ".Reads"].Value);
				this.Writes = ulong.Parse (iniFile[sectionName][prefix + ".Writes"].Value);
			}

			public string Name { get; set; }
			public uint Size { get; set; }
			public double Occupancy { get; set; }
			public ulong Full { get; set; }
			public ulong Reads { get; set; }
			public ulong Writes { get; set; }
		}

		public sealed class TCacheReport
		{
			public sealed class Serializer : XmlConfigSerializer<TCacheReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (TCacheReport tCacheReport)
				{
					if (tCacheReport == null) {
						return XmlConfig.Null ("TCacheReport");
					}
					
					XmlConfig xmlConfig = new XmlConfig ("TCacheReport");
					xmlConfig["accesses"] = tCacheReport.Accesses + "";
					xmlConfig["hits"] = tCacheReport.Hits + "";
					xmlConfig["hitRatio"] = tCacheReport.HitRatio + "";
					xmlConfig["committed"] = tCacheReport.Committed + "";
					xmlConfig["squashed"] = tCacheReport.Squashed + "";
					xmlConfig["traceLength"] = tCacheReport.TraceLength + "";
					
					return xmlConfig;
				}

				public override TCacheReport Load (XmlConfig xmlConfig)
				{
					if (xmlConfig.IsNull) {
						return null;
					}
					
					ulong accesses = ulong.Parse (xmlConfig["accesses"]);
					ulong hits = ulong.Parse (xmlConfig["hits"]);
					double hitRatio = double.Parse (xmlConfig["hitRatio"]);
					ulong committed = ulong.Parse (xmlConfig["committed"]);
					ulong squashed = ulong.Parse (xmlConfig["squashed"]);
					double traceLength = double.Parse (xmlConfig["traceLength"]);
					
					TCacheReport tCacheReport = new TCacheReport ();
					tCacheReport.Accesses = accesses;
					tCacheReport.Hits = hits;
					tCacheReport.HitRatio = hitRatio;
					tCacheReport.Committed = committed;
					tCacheReport.Squashed = squashed;
					tCacheReport.TraceLength = traceLength;
					
					return tCacheReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public TCacheReport ()
			{
			}

			public void Load (IniFile iniFile, string sectionName)
			{
				this.Accesses = ulong.Parse (iniFile[sectionName]["Accesses"].Value);
				this.Hits = ulong.Parse (iniFile[sectionName]["Hits"].Value);
				this.HitRatio = double.Parse (iniFile[sectionName]["HitRatio"].Value);
				this.Committed = ulong.Parse (iniFile[sectionName]["Committed"].Value);
				this.Squashed = ulong.Parse (iniFile[sectionName]["Squashed"].Value);
				this.TraceLength = double.Parse (iniFile[sectionName]["TraceLength"].Value);
			}

			public ulong Accesses { get; set; }
			public ulong Hits { get; set; }
			public double HitRatio { get; set; }
			public ulong Committed { get; set; }
			public ulong Squashed { get; set; }
			public double TraceLength { get; set; }
		}

		public sealed class GlobalReport
		{
			public sealed class Serializer : XmlConfigSerializer<GlobalReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (GlobalReport globalReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("GlobalReport");
					xmlConfig["cycles"] = globalReport.Cycles + "";
					xmlConfig["time"] = globalReport.Time + "";
					xmlConfig["cyclesPerSecond"] = globalReport.CyclesPerSecond + "";
					xmlConfig["memoryUsed"] = globalReport.MemoryUsed + "";
					xmlConfig["memoryUsedMax"] = globalReport.MemoryUsedMax + "";
					xmlConfig["commitBranches"] = globalReport.CommitBranches + "";
					xmlConfig["commitSquashed"] = globalReport.CommitSquashed + "";
					xmlConfig["commitMispred"] = globalReport.CommitMispred + "";
					xmlConfig["commitPredAcc"] = globalReport.CommitPredAcc + "";
					
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (globalReport.UopReportDispatched));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (globalReport.UopReportIssued));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (globalReport.UopReportCommitted));
					
					return xmlConfig;
				}

				public override GlobalReport Load (XmlConfig xmlConfig)
				{
					ulong cycles = ulong.Parse (xmlConfig["cycles"]);
					double time = double.Parse (xmlConfig["time"]);
					ulong cyclesPerSecond = ulong.Parse (xmlConfig["cyclesPerSecond"]);
					ulong memoryUsed = ulong.Parse (xmlConfig["memoryUsed"]);
					ulong memoryUsedMax = ulong.Parse (xmlConfig["memoryUsedMax"]);
					ulong commitBranches = ulong.Parse (xmlConfig["commitBranches"]);
					ulong commitSquashed = ulong.Parse (xmlConfig["commitSquashed"]);
					ulong commitMispred = ulong.Parse (xmlConfig["commitMispred"]);
					double commitPredAcc = double.Parse (xmlConfig["commitPredAcc"]);
					
					UopReport uopReportDispatched = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[0]);
					UopReport uopReportIssued = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[1]);
					UopReport uopReportCommitted = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[2]);
					
					GlobalReport globalReport = new GlobalReport ();
					globalReport.Cycles = cycles;
					globalReport.Time = time;
					globalReport.CyclesPerSecond = cyclesPerSecond;
					globalReport.MemoryUsed = memoryUsed;
					globalReport.MemoryUsedMax = memoryUsedMax;
					
					globalReport.UopReportDispatched = uopReportDispatched;
					globalReport.UopReportIssued = uopReportIssued;
					globalReport.UopReportCommitted = uopReportCommitted;
					
					globalReport.CommitBranches = commitBranches;
					globalReport.CommitSquashed = commitSquashed;
					globalReport.CommitMispred = commitMispred;
					globalReport.CommitPredAcc = commitPredAcc;
					
					return globalReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public GlobalReport ()
			{
			}

			public void Load (IniFile iniFile)
			{
				this.Cycles = ulong.Parse (iniFile["global"]["Cycles"].Value);
				this.Time = double.Parse (iniFile["global"]["Time"].Value);
				this.CyclesPerSecond = ulong.Parse (iniFile["global"]["CyclesPerSecond"].Value);
				this.MemoryUsed = ulong.Parse (iniFile["global"]["MemoryUsed"].Value);
				this.MemoryUsedMax = ulong.Parse (iniFile["global"]["MemoryUsedMax"].Value);
				
				this.UopReportDispatched = new UopReport ();
				this.UopReportDispatched.Load (iniFile, "global", "Dispatch");
				this.UopReportIssued = new UopReport ();
				this.UopReportIssued.Load (iniFile, "global", "Issue");
				this.UopReportCommitted = new UopReport ();
				this.UopReportCommitted.Load (iniFile, "global", "Commit");
				
				this.CommitBranches = ulong.Parse (iniFile["global"]["Commit.Branches"].Value);
				this.CommitSquashed = ulong.Parse (iniFile["global"]["Commit.Squashed"].Value);
				this.CommitMispred = ulong.Parse (iniFile["global"]["Commit.Mispred"].Value);
				this.CommitPredAcc = double.Parse (iniFile["global"]["Commit.PredAcc"].Value);
			}

			public ulong Cycles { get; set; }
			public double Time { get; set; }
			public ulong CyclesPerSecond { get; set; }
			public ulong MemoryUsed { get; set; }
			public ulong MemoryUsedMax { get; set; }

			public UopReport UopReportDispatched { get; set; }
			public UopReport UopReportIssued { get; set; }
			public UopReport UopReportCommitted { get; set; }

			public ulong CommitBranches { get; set; }
			public ulong CommitSquashed { get; set; }
			public ulong CommitMispred { get; set; }
			public double CommitPredAcc { get; set; }
		}

		public sealed class CoreReport
		{
			public sealed class Serializer : XmlConfigSerializer<CoreReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (CoreReport coreReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("CoreReport");
					xmlConfig["name"] = coreReport.Name;
					xmlConfig["commitBranches"] = coreReport.CommitBranches + "";
					xmlConfig["commitSquashed"] = coreReport.CommitSquashed + "";
					xmlConfig["commitMispred"] = coreReport.CommitMispred + "";
					xmlConfig["commitPredAcc"] = coreReport.CommitPredAcc + "";
					xmlConfig["iqWakeupAccesses"] = coreReport.IQWakeupAccesses + "";
					
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatUsed));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatSpec));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatUopq));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatRob));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatIq));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatLsq));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatRename));
					xmlConfig.Entries.Add (DispatchStat.Serializer.SingleInstance.Save (coreReport.DispatchStatCtx));
					
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuIntAdd));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuIntSub));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuIntMult));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuIntDiv));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuEffAddr));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuLogical));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpSimple));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpAdd));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpComp));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpMult));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpDiv));
					xmlConfig.Entries.Add (FuStat.Serializer.SingleInstance.Save (coreReport.FuFpComplex));
					
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (coreReport.UopReportDispatched));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (coreReport.UopReportIssued));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (coreReport.UopReportCommitted));
					
					xmlConfig.Entries.Add (CoreStructStat.Serializer.SingleInstance.Save (coreReport.CoreStructStatRob));
					xmlConfig.Entries.Add (CoreStructStat.Serializer.SingleInstance.Save (coreReport.CoreStructStatIq));
					xmlConfig.Entries.Add (CoreStructStat.Serializer.SingleInstance.Save (coreReport.CoreStructStatLsq));
					xmlConfig.Entries.Add (CoreStructStat.Serializer.SingleInstance.Save (coreReport.CoreStructStatRfInt));
					xmlConfig.Entries.Add (CoreStructStat.Serializer.SingleInstance.Save (coreReport.CoreStructStatRfFp));
					
					return xmlConfig;
				}

				public override CoreReport Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong commitBranches = ulong.Parse (xmlConfig["commitBranches"]);
					ulong commitSquashed = ulong.Parse (xmlConfig["commitSquashed"]);
					ulong commitMispred = ulong.Parse (xmlConfig["commitMispred"]);
					double commitPredAcc = double.Parse (xmlConfig["commitPredAcc"]);
					ulong iqWakeupAccesses = ulong.Parse (xmlConfig["iqWakeupAccesses"]);
					
					int i = 0;
					
					DispatchStat dispatchStatUsed = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatSpec = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatUopq = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatRob = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatIq = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatLsq = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatRename = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					DispatchStat dispatchStatCtx = DispatchStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					FuStat fuIntAdd = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuIntSub = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuIntMult = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuIntDiv = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuEffAddr = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuLogical = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpSimple = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpAdd = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpComp = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpMult = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpDiv = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					FuStat fuFpComplex = FuStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					UopReport uopReportDispatched = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					UopReport uopReportIssued = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					UopReport uopReportCommitted = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					CoreStructStat coreStructStatRob = CoreStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					CoreStructStat coreStructStatIq = CoreStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					CoreStructStat coreStructStatLsq = CoreStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					CoreStructStat coreStructStatRfInt = CoreStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					CoreStructStat coreStructStatRfFp = CoreStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					CoreReport coreReport = new CoreReport ();
					coreReport.Name = name;
					coreReport.DispatchStatUsed = dispatchStatUsed;
					coreReport.DispatchStatSpec = dispatchStatSpec;
					coreReport.DispatchStatUopq = dispatchStatUopq;
					coreReport.DispatchStatRob = dispatchStatRob;
					coreReport.DispatchStatIq = dispatchStatIq;
					coreReport.DispatchStatLsq = dispatchStatLsq;
					coreReport.DispatchStatRename = dispatchStatRename;
					coreReport.DispatchStatCtx = dispatchStatCtx;
					
					coreReport.FuIntAdd = fuIntAdd;
					coreReport.FuIntSub = fuIntSub;
					coreReport.FuIntMult = fuIntMult;
					coreReport.FuIntDiv = fuIntDiv;
					coreReport.FuEffAddr = fuEffAddr;
					coreReport.FuLogical = fuLogical;
					coreReport.FuFpSimple = fuFpSimple;
					coreReport.FuFpAdd = fuFpAdd;
					coreReport.FuFpComp = fuFpComp;
					coreReport.FuFpMult = fuFpMult;
					coreReport.FuFpDiv = fuFpDiv;
					coreReport.FuFpComplex = fuFpComplex;
					
					coreReport.UopReportDispatched = uopReportDispatched;
					coreReport.UopReportIssued = uopReportIssued;
					coreReport.UopReportCommitted = uopReportCommitted;
					
					coreReport.CommitBranches = commitBranches;
					coreReport.CommitSquashed = commitSquashed;
					coreReport.CommitMispred = commitMispred;
					coreReport.CommitPredAcc = commitPredAcc;
					
					coreReport.CoreStructStatRob = coreStructStatRob;
					coreReport.CoreStructStatIq = coreStructStatIq;
					coreReport.CoreStructStatLsq = coreStructStatLsq;
					coreReport.IQWakeupAccesses = iqWakeupAccesses;
					coreReport.CoreStructStatLsq = coreStructStatLsq;
					coreReport.CoreStructStatRfInt = coreStructStatRfInt;
					coreReport.CoreStructStatRfFp = coreStructStatRfFp;
					
					return coreReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public CoreReport ()
			{
			}

			public CoreReport (Simulation simulation, IniFile iniFile, string sectionName)
			{
				this.Name = sectionName;
				
				this.FuIntAdd = new FuStat ();
				this.FuIntAdd.Load (iniFile, sectionName, "IntAdd");
				this.FuIntSub = new FuStat ();
				this.FuIntSub.Load (iniFile, sectionName, "IntSub");
				this.FuIntMult = new FuStat ();
				this.FuIntMult.Load (iniFile, sectionName, "IntMult");
				this.FuIntDiv = new FuStat ();
				this.FuIntDiv.Load (iniFile, sectionName, "IntDiv");
				this.FuEffAddr = new FuStat ();
				this.FuEffAddr.Load (iniFile, sectionName, "Effaddr");
				this.FuLogical = new FuStat ();
				this.FuLogical.Load (iniFile, sectionName, "Logical");
				this.FuFpSimple = new FuStat ();
				this.FuFpSimple.Load (iniFile, sectionName, "FPSimple");
				this.FuFpAdd = new FuStat ();
				this.FuFpAdd.Load (iniFile, sectionName, "FPAdd");
				this.FuFpComp = new FuStat ();
				this.FuFpComp.Load (iniFile, sectionName, "FPComp");
				this.FuFpMult = new FuStat ();
				this.FuFpMult.Load (iniFile, sectionName, "FPMult");
				this.FuFpDiv = new FuStat ();
				this.FuFpDiv.Load (iniFile, sectionName, "FPDiv");
				this.FuFpComplex = new FuStat ();
				this.FuFpComplex.Load (iniFile, sectionName, "FPComplex");
				
				if (simulation.PipelineConfig.DispatchKind == PipelineConfig.DispatchKinds.TIMESLICE) {
					this.DispatchStatUsed = new DispatchStat ();
					this.DispatchStatUsed.Load (iniFile, sectionName, "used");
					this.DispatchStatSpec = new DispatchStat ();
					this.DispatchStatSpec.Load (iniFile, sectionName, "spec");
					this.DispatchStatUopq = new DispatchStat ();
					this.DispatchStatUopq.Load (iniFile, sectionName, "uopq");
					this.DispatchStatRob = new DispatchStat ();
					this.DispatchStatRob.Load (iniFile, sectionName, "rob");
					this.DispatchStatIq = new DispatchStat ();
					this.DispatchStatIq.Load (iniFile, sectionName, "iq");
					this.DispatchStatLsq = new DispatchStat ();
					this.DispatchStatLsq.Load (iniFile, sectionName, "lsq");
					this.DispatchStatRename = new DispatchStat ();
					this.DispatchStatRename.Load (iniFile, sectionName, "rename");
					this.DispatchStatCtx = new DispatchStat ();
					this.DispatchStatCtx.Load (iniFile, sectionName, "ctx");
				}
				
				this.UopReportDispatched = new UopReport ();
				this.UopReportDispatched.Load (iniFile, sectionName, "Dispatch");
				this.UopReportIssued = new UopReport ();
				this.UopReportIssued.Load (iniFile, sectionName, "Issue");
				this.UopReportCommitted = new UopReport ();
				this.UopReportCommitted.Load (iniFile, sectionName, "Commit");
				
				this.CommitBranches = ulong.Parse (iniFile[sectionName]["Commit.Branches"].Value);
				this.CommitSquashed = ulong.Parse (iniFile[sectionName]["Commit.Squashed"].Value);
				this.CommitMispred = ulong.Parse (iniFile[sectionName]["Commit.Mispred"].Value);
				this.CommitPredAcc = double.Parse (iniFile[sectionName]["Commit.PredAcc"].Value);
				
				if (simulation.PipelineConfig.RobKind == PipelineConfig.ROBKinds.SHARED) {
					this.CoreStructStatRob = new CoreStructStat ();
					this.CoreStructStatRob.Load (iniFile, sectionName, "ROB");
				}
				
				if (simulation.PipelineConfig.IqKind == PipelineConfig.IQKinds.SHARED) {
					this.CoreStructStatIq = new CoreStructStat ();
					this.CoreStructStatIq.Load (iniFile, sectionName, "IQ");
					this.IQWakeupAccesses = ulong.Parse (iniFile[sectionName]["IQ.WakeupAccesses"].Value);
				}
				
				if (simulation.PipelineConfig.LsqKind == PipelineConfig.LSQKinds.SHARED) {
					this.CoreStructStatLsq = new CoreStructStat ();
					this.CoreStructStatLsq.Load (iniFile, sectionName, "LSQ");
				}
				
				if (simulation.PipelineConfig.RfKind == PipelineConfig.RfKinds.SHARED) {
					this.CoreStructStatRfInt = new CoreStructStat ();
					this.CoreStructStatRfInt.Load (iniFile, sectionName, "RF_Int");
					this.CoreStructStatRfFp = new CoreStructStat ();
					this.CoreStructStatRfFp.Load (iniFile, sectionName, "RF_Fp");
				}
			}

			public string Name { get; set; }

			public DispatchStat DispatchStatUsed { get; set; }
			public DispatchStat DispatchStatSpec { get; set; }
			public DispatchStat DispatchStatUopq { get; set; }
			public DispatchStat DispatchStatRob { get; set; }
			public DispatchStat DispatchStatIq { get; set; }
			public DispatchStat DispatchStatLsq { get; set; }
			public DispatchStat DispatchStatRename { get; set; }
			public DispatchStat DispatchStatCtx { get; set; }

			public FuStat FuIntAdd { get; set; }
			public FuStat FuIntSub { get; set; }
			public FuStat FuIntMult { get; set; }
			public FuStat FuIntDiv { get; set; }
			public FuStat FuEffAddr { get; set; }
			public FuStat FuLogical { get; set; }
			public FuStat FuFpSimple { get; set; }
			public FuStat FuFpAdd { get; set; }
			public FuStat FuFpComp { get; set; }
			public FuStat FuFpMult { get; set; }
			public FuStat FuFpDiv { get; set; }
			public FuStat FuFpComplex { get; set; }

			public UopReport UopReportDispatched { get; set; }
			public UopReport UopReportIssued { get; set; }
			public UopReport UopReportCommitted { get; set; }

			public ulong CommitBranches { get; set; }
			public ulong CommitSquashed { get; set; }
			public ulong CommitMispred { get; set; }
			public double CommitPredAcc { get; set; }

			public CoreStructStat CoreStructStatRob { get; set; }
			public CoreStructStat CoreStructStatIq { get; set; }
			public ulong IQWakeupAccesses { get; set; }
			public CoreStructStat CoreStructStatLsq { get; set; }
			public CoreStructStat CoreStructStatRfInt { get; set; }
			public CoreStructStat CoreStructStatRfFp { get; set; }
		}

		public sealed class ThreadReport
		{
			public sealed class Serializer : XmlConfigSerializer<ThreadReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (ThreadReport threadReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("ThreadReport");
					xmlConfig["name"] = threadReport.Name;
					xmlConfig["commitBranches"] = threadReport.CommitBranches + "";
					xmlConfig["commitSquashed"] = threadReport.CommitSquashed + "";
					xmlConfig["commitMispred"] = threadReport.CommitMispred + "";
					xmlConfig["commitPredAcc"] = threadReport.CommitPredAcc + "";
					xmlConfig["iqWakeupAccesses"] = threadReport.IQWakeupAccesses + "";
					
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (threadReport.UopReportDispatched));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (threadReport.UopReportIssued));
					xmlConfig.Entries.Add (UopReport.Serializer.SingleInstance.Save (threadReport.UopReportCommitted));
					
					xmlConfig.Entries.Add (ThreadStructStat.Serializer.SingleInstance.Save (threadReport.ThreadStructStatRob));
					xmlConfig.Entries.Add (ThreadStructStat.Serializer.SingleInstance.Save (threadReport.ThreadStructStatIq));
					xmlConfig.Entries.Add (ThreadStructStat.Serializer.SingleInstance.Save (threadReport.ThreadStructStatLsq));
					xmlConfig.Entries.Add (ThreadStructStat.Serializer.SingleInstance.Save (threadReport.ThreadStructStatRfInt));
					xmlConfig.Entries.Add (ThreadStructStat.Serializer.SingleInstance.Save (threadReport.ThreadStructStatRfFp));
					
					xmlConfig.Entries.Add (TCacheReport.Serializer.SingleInstance.Save (threadReport.TCacheReport));
					
					return xmlConfig;
				}

				public override ThreadReport Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong commitBranches = ulong.Parse (xmlConfig["commitBranches"]);
					ulong commitSquashed = ulong.Parse (xmlConfig["commitSquashed"]);
					ulong commitMispred = ulong.Parse (xmlConfig["commitMispred"]);
					double commitPredAcc = double.Parse (xmlConfig["commitPredAcc"]);
					ulong iqWakeupAccesses = ulong.Parse (xmlConfig["iqWakeupAccesses"]);
					
					int i = 0;
					
					UopReport uopReportDispatched = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					UopReport uopReportIssued = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					UopReport uopReportCommitted = UopReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					ThreadStructStat threadStructStatRob = ThreadStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					ThreadStructStat threadStructStatIq = ThreadStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					ThreadStructStat threadStructStatLsq = ThreadStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					ThreadStructStat threadStructStatRfInt = ThreadStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					ThreadStructStat threadStructStatRfFp = ThreadStructStat.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					TCacheReport tCacheReport = TCacheReport.Serializer.SingleInstance.Load (xmlConfig.Entries[i++]);
					
					ThreadReport threadReport = new ThreadReport ();
					threadReport.Name = name;
					threadReport.UopReportDispatched = uopReportDispatched;
					threadReport.UopReportIssued = uopReportIssued;
					threadReport.UopReportCommitted = uopReportCommitted;
					
					threadReport.CommitBranches = commitBranches;
					threadReport.CommitSquashed = commitSquashed;
					threadReport.CommitMispred = commitMispred;
					threadReport.CommitPredAcc = commitPredAcc;
					
					threadReport.ThreadStructStatRob = threadStructStatRob;
					threadReport.ThreadStructStatIq = threadStructStatIq;
					threadReport.IQWakeupAccesses = iqWakeupAccesses;
					threadReport.ThreadStructStatLsq = threadStructStatLsq;
					threadReport.ThreadStructStatRfInt = threadStructStatRfInt;
					threadReport.ThreadStructStatRfFp = threadStructStatRfFp;
					
					threadReport.TCacheReport = tCacheReport;
					
					return threadReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public ThreadReport ()
			{
			}

			public ThreadReport (Simulation simulation, IniFile iniFile, string sectionName)
			{
				this.Name = sectionName;
				
				this.UopReportDispatched = new UopReport ();
				this.UopReportDispatched.Load (iniFile, sectionName, "Dispatch");
				this.UopReportIssued = new UopReport ();
				this.UopReportIssued.Load (iniFile, sectionName, "Issue");
				this.UopReportCommitted = new UopReport ();
				this.UopReportCommitted.Load (iniFile, sectionName, "Commit");
				
				this.CommitBranches = ulong.Parse (iniFile[sectionName]["Commit.Branches"].Value);
				this.CommitSquashed = ulong.Parse (iniFile[sectionName]["Commit.Squashed"].Value);
				this.CommitMispred = ulong.Parse (iniFile[sectionName]["Commit.Mispred"].Value);
				this.CommitPredAcc = double.Parse (iniFile[sectionName]["Commit.PredAcc"].Value);
				
				if (simulation.PipelineConfig.RobKind == PipelineConfig.ROBKinds.PRIVATE) {
					this.ThreadStructStatRob = new ThreadStructStat ();
					this.ThreadStructStatRob.Load (iniFile, sectionName, "ROB");
				}
				
				if (simulation.PipelineConfig.IqKind == PipelineConfig.IQKinds.PRIVATE) {
					this.ThreadStructStatIq = new ThreadStructStat ();
					this.ThreadStructStatIq.Load (iniFile, sectionName, "IQ");
					this.IQWakeupAccesses = ulong.Parse (iniFile[sectionName]["IQ.WakeupAccesses"].Value);
				}
				
				if (simulation.PipelineConfig.LsqKind == PipelineConfig.LSQKinds.PRIVATE) {
					this.ThreadStructStatLsq = new ThreadStructStat ();
					this.ThreadStructStatLsq.Load (iniFile, sectionName, "LSQ");
				}
				
				if (simulation.PipelineConfig.RfKind == PipelineConfig.RfKinds.PRIVATE) {
					this.ThreadStructStatRfInt = new ThreadStructStat ();
					this.ThreadStructStatRfInt.Load (iniFile, sectionName, "RF_Int");
					this.ThreadStructStatRfFp = new ThreadStructStat ();
					this.ThreadStructStatRfFp.Load (iniFile, sectionName, "RF_Fp");
				}
				
				if (simulation.PipelineConfig.TCache.Enabled) {
					this.TCacheReport = new TCacheReport ();
					this.TCacheReport.Load (iniFile, sectionName);
				}
			}

			public string Name { get; set; }

			public UopReport UopReportDispatched { get; set; }
			public UopReport UopReportIssued { get; set; }
			public UopReport UopReportCommitted { get; set; }

			public ulong CommitBranches { get; set; }
			public ulong CommitSquashed { get; set; }
			public ulong CommitMispred { get; set; }
			public double CommitPredAcc { get; set; }

			public ThreadStructStat ThreadStructStatRob { get; set; }
			public ThreadStructStat ThreadStructStatIq { get; set; }
			public ulong IQWakeupAccesses { get; set; }
			public ThreadStructStat ThreadStructStatLsq { get; set; }
			public ThreadStructStat ThreadStructStatRfInt { get; set; }
			public ThreadStructStat ThreadStructStatRfFp { get; set; }

			public TCacheReport TCacheReport { get; set; }
		}

		public sealed class Serializer : XmlConfigSerializer<PipelineReport>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (PipelineReport pipelineReport)
			{
				XmlConfig xmlConfig = new XmlConfig ("PipelineReport");
				
				xmlConfig.Entries.Add (GlobalReport.Serializer.SingleInstance.Save (pipelineReport.Global));
				xmlConfig.Entries.Add (SaveStringDictionary<CoreReport> ("Cores", pipelineReport.Cores, new SaveEntryDelegate<CoreReport> (CoreReport.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (SaveStringDictionary<ThreadReport> ("Threads", pipelineReport.Threads, new SaveEntryDelegate<ThreadReport> (ThreadReport.Serializer.SingleInstance.Save)));
				
				return xmlConfig;
			}

			public override PipelineReport Load (XmlConfig xmlConfig)
			{
				GlobalReport glb = GlobalReport.Serializer.SingleInstance.Load (xmlConfig.Entries[0]);
				SortedDictionary<string, CoreReport> cores = LoadStringDictionary<CoreReport> (xmlConfig.Entries[1], new LoadEntryDelegate<CoreReport> (CoreReport.Serializer.SingleInstance.Load), core => core.Name);
				SortedDictionary<string, ThreadReport> threads = LoadStringDictionary<ThreadReport> (xmlConfig.Entries[2], new LoadEntryDelegate<ThreadReport> (ThreadReport.Serializer.SingleInstance.Load), thread => thread.Name);
				
				PipelineReport pipelineReport = new PipelineReport ();
				pipelineReport.Global = glb;
				pipelineReport.Cores = cores;
				pipelineReport.Threads = threads;
				
				return pipelineReport;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public PipelineReport ()
		{
		}

		public PipelineReport (Simulation simulation)
		{
			IniFile iniFile = new IniFile ();
			iniFile.Load (simulation.Cwd + Path.DirectorySeparatorChar + POSTFIX_PIPELINE_REPORT_FILE);
			
			this.Global = new GlobalReport ();
			this.Global.Load (iniFile);
			
			this.Cores = new SortedDictionary<string, CoreReport> ();
			
			this.Threads = new SortedDictionary<string, ThreadReport> ();
			
			for (uint core = 0; core < simulation.PipelineConfig.Cores; core++) {
				string coreName = "c" + core;
				this.Cores[coreName] = new CoreReport (simulation, iniFile, coreName);
				
				for (uint thread = 0; thread < simulation.PipelineConfig.Threads; thread++) {
					string threadName = "c" + core + "t" + thread;
					this.Threads[threadName] = new PipelineReport.ThreadReport (simulation, iniFile, threadName);
				}
			}
		}

		public GlobalReport Global { get; set; }

		public SortedDictionary<string, CoreReport> Cores { get; set; }

		public SortedDictionary<string, ThreadReport> Threads { get; set; }

		public static string POSTFIX_PIPELINE_REPORT_FILE = "report.pipeline";
	}

	public sealed class CacheSystemReport
	{
		public class CacheReport
		{
			public sealed class Serializer : XmlConfigSerializer<CacheReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (CacheReport cacheReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("CacheReport");
					xmlConfig["name"] = cacheReport.Name;
					
					xmlConfig["accesses"] = cacheReport.Accesses + "";
					xmlConfig["hits"] = cacheReport.Hits + "";
					xmlConfig["misses"] = cacheReport.Misses + "";
					xmlConfig["hitRatio"] = cacheReport.HitRatio + "";
					xmlConfig["evictions"] = cacheReport.Evictions + "";
					xmlConfig["retries"] = cacheReport.Retries + "";
					xmlConfig["readRetries"] = cacheReport.ReadRetries + "";
					xmlConfig["writeRetries"] = cacheReport.WriteRetries + "";
					
					xmlConfig["noRetryAccesses"] = cacheReport.NoRetryAccesses + "";
					xmlConfig["noRetryHits"] = cacheReport.NoRetryHits + "";
					xmlConfig["noRetryMisses"] = cacheReport.NoRetryMisses + "";
					xmlConfig["noRetryHitRatio"] = cacheReport.NoRetryHitRatio + "";
					xmlConfig["noRetryReads"] = cacheReport.NoRetryReads + "";
					xmlConfig["noRetryReadHits"] = cacheReport.NoRetryReadHits + "";
					xmlConfig["noRetryReadMisses"] = cacheReport.NoRetryReadMisses + "";
					xmlConfig["noRetryWrites"] = cacheReport.NoRetryWrites + "";
					xmlConfig["noRetryWriteHits"] = cacheReport.NoRetryWriteHits + "";
					xmlConfig["noRetryWriteMisses"] = cacheReport.NoRetryWriteMisses + "";
					
					xmlConfig["reads"] = cacheReport.Reads + "";
					xmlConfig["blockingReads"] = cacheReport.BlockingReads + "";
					xmlConfig["nonBlockingReads"] = cacheReport.NonBlockingReads + "";
					xmlConfig["readHits"] = cacheReport.ReadHits + "";
					xmlConfig["readMisses"] = cacheReport.ReadMisses + "";
					
					xmlConfig["writes"] = cacheReport.Writes + "";
					xmlConfig["blockingWrites"] = cacheReport.BlockingWrites + "";
					xmlConfig["nonBlockingWrites"] = cacheReport.NonBlockingWrites + "";
					xmlConfig["writeHits"] = cacheReport.WriteHits + "";
					xmlConfig["writeMisses"] = cacheReport.WriteMisses + "";
					
					return xmlConfig;
				}

				public override CacheReport Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					
					ulong accesses = ulong.Parse (xmlConfig["accesses"]);
					ulong hits = ulong.Parse (xmlConfig["hits"]);
					ulong misses = ulong.Parse (xmlConfig["misses"]);
					double hitRatio = double.Parse (xmlConfig["hitRatio"]);
					ulong evictions = ulong.Parse (xmlConfig["evictions"]);
					ulong retries = ulong.Parse (xmlConfig["retries"]);
					ulong readRetries = ulong.Parse (xmlConfig["readRetries"]);
					ulong writeRetries = ulong.Parse (xmlConfig["writeRetries"]);
					
					ulong noRetryAccesses = ulong.Parse (xmlConfig["noRetryAccesses"]);
					ulong noRetryHits = ulong.Parse (xmlConfig["noRetryHits"]);
					ulong noRetryMisses = ulong.Parse (xmlConfig["noRetryMisses"]);
					double noRetryHitRatio = double.Parse (xmlConfig["noRetryHitRatio"]);
					ulong noRetryReads = ulong.Parse (xmlConfig["noRetryReads"]);
					ulong noRetryReadHits = ulong.Parse (xmlConfig["noRetryReadHits"]);
					ulong noRetryReadMisses = ulong.Parse (xmlConfig["noRetryReadMisses"]);
					ulong noRetryWrites = ulong.Parse (xmlConfig["noRetryWrites"]);
					ulong noRetryWriteHits = ulong.Parse (xmlConfig["noRetryWriteHits"]);
					ulong noRetryWriteMisses = ulong.Parse (xmlConfig["noRetryWriteMisses"]);
					
					ulong reads = ulong.Parse (xmlConfig["reads"]);
					ulong blockingReads = ulong.Parse (xmlConfig["blockingReads"]);
					ulong nonBlockingReads = ulong.Parse (xmlConfig["nonBlockingReads"]);
					ulong readHits = ulong.Parse (xmlConfig["readHits"]);
					ulong readMisses = ulong.Parse (xmlConfig["readMisses"]);
					
					ulong writes = ulong.Parse (xmlConfig["writes"]);
					ulong blockingWrites = ulong.Parse (xmlConfig["blockingWrites"]);
					ulong nonBlockingWrites = ulong.Parse (xmlConfig["nonBlockingWrites"]);
					ulong writeHits = ulong.Parse (xmlConfig["writeHits"]);
					ulong writeMisses = ulong.Parse (xmlConfig["writeMisses"]);
					
					CacheReport cacheReport = new CacheReport ();
					cacheReport.Name = name;
					
					cacheReport.Accesses = accesses;
					cacheReport.Hits = hits;
					cacheReport.Misses = misses;
					cacheReport.HitRatio = hitRatio;
					cacheReport.Evictions = evictions;
					cacheReport.Retries = retries;
					cacheReport.ReadRetries = readRetries;
					cacheReport.WriteRetries = writeRetries;
					
					cacheReport.NoRetryAccesses = noRetryAccesses;
					cacheReport.NoRetryHits = noRetryHits;
					cacheReport.NoRetryMisses = noRetryMisses;
					cacheReport.NoRetryHitRatio = noRetryHitRatio;
					cacheReport.NoRetryReads = noRetryReads;
					cacheReport.NoRetryReadHits = noRetryReadHits;
					cacheReport.NoRetryReadMisses = noRetryReadMisses;
					cacheReport.NoRetryWrites = noRetryWrites;
					cacheReport.NoRetryWriteHits = noRetryWriteHits;
					cacheReport.NoRetryWriteMisses = noRetryWriteMisses;
					
					cacheReport.Reads = reads;
					cacheReport.BlockingReads = blockingReads;
					cacheReport.NonBlockingReads = nonBlockingReads;
					cacheReport.ReadHits = readHits;
					cacheReport.ReadMisses = readMisses;
					
					cacheReport.Writes = writes;
					cacheReport.BlockingWrites = blockingWrites;
					cacheReport.NonBlockingWrites = nonBlockingWrites;
					cacheReport.WriteHits = writeHits;
					cacheReport.WriteMisses = writeMisses;
					
					return cacheReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public CacheReport ()
			{
			}

			public void Load (IniFile iniFile, string cacheName)
			{
				this.Name = cacheName;
				
				this.Accesses = ulong.Parse (iniFile[this.Name]["Accesses"].Value);
				this.Hits = ulong.Parse (iniFile[this.Name]["Hits"].Value);
				this.Misses = ulong.Parse (iniFile[this.Name]["Misses"].Value);
				this.HitRatio = double.Parse (iniFile[this.Name]["HitRatio"].Value);
				this.Evictions = ulong.Parse (iniFile[this.Name]["Evictions"].Value);
				this.Retries = ulong.Parse (iniFile[this.Name]["Retries"].Value);
				this.ReadRetries = ulong.Parse (iniFile[this.Name]["ReadRetries"].Value);
				this.WriteRetries = ulong.Parse (iniFile[this.Name]["WriteRetries"].Value);
				
				this.NoRetryAccesses = ulong.Parse (iniFile[this.Name]["NoRetryAccesses"].Value);
				this.NoRetryHits = ulong.Parse (iniFile[this.Name]["NoRetryHits"].Value);
				this.NoRetryMisses = ulong.Parse (iniFile[this.Name]["NoRetryMisses"].Value);
				this.NoRetryHitRatio = double.Parse (iniFile[this.Name]["NoRetryHitRatio"].Value);
				this.NoRetryReads = ulong.Parse (iniFile[this.Name]["NoRetryReads"].Value);
				this.NoRetryReadHits = ulong.Parse (iniFile[this.Name]["NoRetryReadHits"].Value);
				this.NoRetryReadMisses = ulong.Parse (iniFile[this.Name]["NoRetryReadMisses"].Value);
				this.NoRetryWrites = ulong.Parse (iniFile[this.Name]["NoRetryWrites"].Value);
				this.NoRetryWriteHits = ulong.Parse (iniFile[this.Name]["NoRetryWriteHits"].Value);
				this.NoRetryWriteMisses = ulong.Parse (iniFile[this.Name]["NoRetryWriteMisses"].Value);
				
				this.Reads = ulong.Parse (iniFile[this.Name]["Reads"].Value);
				this.BlockingReads = ulong.Parse (iniFile[this.Name]["BlockingReads"].Value);
				this.NonBlockingReads = ulong.Parse (iniFile[this.Name]["NonBlockingReads"].Value);
				this.ReadHits = ulong.Parse (iniFile[this.Name]["ReadHits"].Value);
				this.ReadMisses = ulong.Parse (iniFile[this.Name]["ReadMisses"].Value);
				
				this.Writes = ulong.Parse (iniFile[this.Name]["Writes"].Value);
				this.BlockingWrites = ulong.Parse (iniFile[this.Name]["BlockingWrites"].Value);
				this.NonBlockingWrites = ulong.Parse (iniFile[this.Name]["NonBlockingWrites"].Value);
				this.WriteHits = ulong.Parse (iniFile[this.Name]["WriteHits"].Value);
				this.WriteMisses = ulong.Parse (iniFile[this.Name]["WriteMisses"].Value);
			}

			public string Name { get; set; }

			public ulong Accesses { get; set; }
			public ulong Hits { get; set; }
			public ulong Misses { get; set; }
			public double HitRatio { get; set; }
			public ulong Evictions { get; set; }
			public ulong Retries { get; set; }
			public ulong ReadRetries { get; set; }
			public ulong WriteRetries { get; set; }

			public ulong NoRetryAccesses { get; set; }
			public ulong NoRetryHits { get; set; }
			public ulong NoRetryMisses { get; set; }
			public double NoRetryHitRatio { get; set; }
			public ulong NoRetryReads { get; set; }
			public ulong NoRetryReadHits { get; set; }
			public ulong NoRetryReadMisses { get; set; }
			public ulong NoRetryWrites { get; set; }
			public ulong NoRetryWriteHits { get; set; }
			public ulong NoRetryWriteMisses { get; set; }

			public ulong Reads { get; set; }
			public ulong BlockingReads { get; set; }
			public ulong NonBlockingReads { get; set; }
			public ulong ReadHits { get; set; }
			public ulong ReadMisses { get; set; }

			public ulong Writes { get; set; }
			public ulong BlockingWrites { get; set; }
			public ulong NonBlockingWrites { get; set; }
			public ulong WriteHits { get; set; }
			public ulong WriteMisses { get; set; }
		}

		public sealed class TlbReport
		{
			public sealed class Serializer : XmlConfigSerializer<TlbReport>
			{
				public Serializer ()
				{
				}

				public override XmlConfig Save (TlbReport tlbReport)
				{
					XmlConfig xmlConfig = new XmlConfig ("TlbReport");
					xmlConfig["name"] = tlbReport.Name;
					xmlConfig["accesses"] = tlbReport.Accesses + "";
					xmlConfig["hits"] = tlbReport.Hits + "";
					xmlConfig["misses"] = tlbReport.Misses + "";
					xmlConfig["hitRatio"] = tlbReport.HitRatio + "";
					xmlConfig["evictions"] = tlbReport.Evictions + "";
					
					return xmlConfig;
				}

				public override TlbReport Load (XmlConfig xmlConfig)
				{
					string name = xmlConfig["name"];
					ulong accesses = ulong.Parse (xmlConfig["accesses"]);
					ulong hits = ulong.Parse (xmlConfig["hits"]);
					ulong misses = ulong.Parse (xmlConfig["misses"]);
					double hitRatio = double.Parse (xmlConfig["hitRatio"]);
					ulong evictions = ulong.Parse (xmlConfig["evictions"]);
					
					TlbReport tlbReport = new TlbReport ();
					tlbReport.Name = name;
					tlbReport.Accesses = accesses;
					tlbReport.Hits = hits;
					tlbReport.Misses = misses;
					tlbReport.HitRatio = hitRatio;
					tlbReport.Evictions = evictions;
					
					return tlbReport;
				}

				public static Serializer SingleInstance = new Serializer ();
			}

			public TlbReport ()
			{
			}

			public void Load (IniFile iniFile, string tlbName)
			{
				this.Name = tlbName;
				
				this.Accesses = ulong.Parse (iniFile[this.Name]["Accesses"].Value);
				this.Hits = ulong.Parse (iniFile[this.Name]["Hits"].Value);
				this.Misses = ulong.Parse (iniFile[this.Name]["Misses"].Value);
				this.HitRatio = double.Parse (iniFile[this.Name]["HitRatio"].Value);
				this.Evictions = ulong.Parse (iniFile[this.Name]["Evictions"].Value);
			}

			public string Name { get; set; }

			public ulong Accesses { get; set; }
			public ulong Hits { get; set; }
			public ulong Misses { get; set; }
			public double HitRatio { get; set; }
			public ulong Evictions { get; set; }
		}

		public sealed class Serializer : XmlConfigSerializer<CacheSystemReport>
		{
			public Serializer ()
			{
			}

			public override XmlConfig Save (CacheSystemReport cacheSystemReport)
			{
				XmlConfig xmlConfig = new XmlConfig ("CacheSystemReport");
				
				xmlConfig.Entries.Add (SaveStringDictionary<CacheReport> ("Caches", cacheSystemReport.Caches, new SaveEntryDelegate<CacheReport> (CacheReport.Serializer.SingleInstance.Save)));
				xmlConfig.Entries.Add (CacheReport.Serializer.SingleInstance.Save (cacheSystemReport.mainMemory));
				xmlConfig.Entries.Add (SaveStringDictionary<TlbReport> ("Tlbs", cacheSystemReport.Tlbs, new SaveEntryDelegate<TlbReport> (TlbReport.Serializer.SingleInstance.Save)));
				
				return xmlConfig;
			}

			public override CacheSystemReport Load (XmlConfig xmlConfig)
			{
				SortedDictionary<string, CacheReport> caches = LoadStringDictionary<CacheReport> (xmlConfig.Entries[0], new LoadEntryDelegate<CacheReport> (CacheReport.Serializer.SingleInstance.Load), cache => cache.Name);
				CacheReport mainMemory = CacheReport.Serializer.SingleInstance.Load (xmlConfig.Entries[1]);
				SortedDictionary<string, TlbReport> tlbs = LoadStringDictionary<TlbReport> (xmlConfig.Entries[2], new LoadEntryDelegate<TlbReport> (TlbReport.Serializer.SingleInstance.Load), tlb => tlb.Name);
				
				CacheSystemReport cacheSystemReport = new CacheSystemReport ();
				cacheSystemReport.Caches = caches;
				cacheSystemReport.mainMemory = mainMemory;
				cacheSystemReport.Tlbs = tlbs;
				
				return cacheSystemReport;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public CacheSystemReport ()
		{
		}

		public CacheSystemReport (Simulation simulation)
		{
			this.Caches = new SortedDictionary<string, CacheReport> ();
			this.Tlbs = new SortedDictionary<string, TlbReport> ();
			
			IniFile iniFile = new IniFile ();
			iniFile.Load (simulation.Cwd + Path.DirectorySeparatorChar + POSTFIX_CACHE_SYSTEM_REPORT_FILE);
			
			foreach (CacheSystemConfig.CacheConfig cache in simulation.CacheSystemConfig.Caches) {
				this.Caches[cache.Name] = new CacheReport ();
				this.Caches[cache.Name].Load (iniFile, cache.Name);
			}
			
			this.mainMemory = new CacheReport ();
			this.mainMemory.Load (iniFile, "mm");
			
			for (uint core = 0; core < simulation.PipelineConfig.Cores; core++) {
				for (uint thread = 0; thread < simulation.PipelineConfig.Threads; thread++) {
					string itlbName = "itlb." + core + "." + thread;
					string dtlbName = "dtlb." + core + "." + thread;
					
					this.Tlbs[itlbName] = new TlbReport ();
					this.Tlbs[itlbName].Load (iniFile, itlbName);
					this.Tlbs[dtlbName] = new TlbReport ();
					this.Tlbs[dtlbName].Load (iniFile, dtlbName);
				}
			}
		}

		public SortedDictionary<string, CacheReport> Caches { get; set; }
		public CacheReport mainMemory { get; set; }
		public SortedDictionary<string, TlbReport> Tlbs { get; set; }

		public static string POSTFIX_CACHE_SYSTEM_REPORT_FILE = "report.cache";
	}

	#endregion

	#region Simulation

	public sealed class Simulation
	{
		public sealed class Serializer : XmlConfigFileSerializer<Simulation>
		{
			public Serializer ()
			{
			}

			public override XmlConfigFile Save (Simulation simulation)
			{
				XmlConfigFile xmlConfig = new XmlConfigFile (simulation.FileName, "Simulation");
				
				xmlConfig.Entries.Add (PipelineConfig.Serializer.SingleInstance.Save (simulation.PipelineConfig));
				xmlConfig.Entries.Add (CacheSystemConfig.Serializer.SingleInstance.Save (simulation.CacheSystemConfig));
				xmlConfig.Entries.Add (ContextConfig.Serializer.SingleInstance.Save (simulation.ContextConfig));
				
				xmlConfig.Entries.Add (PipelineReport.Serializer.SingleInstance.Save (simulation.PipelineReport));
				xmlConfig.Entries.Add (CacheSystemReport.Serializer.SingleInstance.Save (simulation.CacheSystemReport));
				
				return xmlConfig;
			}

			public override Simulation Load (XmlConfigFile xmlConfigFile)
			{
				string fileName = xmlConfigFile.FileName;
				
				int numEntries = xmlConfigFile.Entries.Count;
				
				int i = 0;
				
				PipelineConfig pipelineConfig = PipelineConfig.Serializer.SingleInstance.Load (xmlConfigFile.Entries[i++]);
				CacheSystemConfig cacheSystemConfig = CacheSystemConfig.Serializer.SingleInstance.Load (xmlConfigFile.Entries[i++]);
				ContextConfig contextConfig = ContextConfig.Serializer.SingleInstance.Load (xmlConfigFile.Entries[i++]);
				
				PipelineReport pipelineReport = i < numEntries ? PipelineReport.Serializer.SingleInstance.Load (xmlConfigFile.Entries[i++]) : null;
				CacheSystemReport cacheSystemReport = i < numEntries ? CacheSystemReport.Serializer.SingleInstance.Load (xmlConfigFile.Entries[i++]) : null;
				
				Simulation simulation = new Simulation (fileName, pipelineConfig, cacheSystemConfig, contextConfig);
				simulation.PipelineReport = pipelineReport;
				simulation.CacheSystemReport = cacheSystemReport;
				
				return simulation;
			}

			public static Serializer SingleInstance = new Serializer ();
		}

		public Simulation (string fileName, PipelineConfig pipelineConfig, CacheSystemConfig cacheSystemConfig, ContextConfig contextConfig)
		{
			this.FileName = fileName;
			
			this.PipelineConfig = pipelineConfig;
			this.CacheSystemConfig = cacheSystemConfig;
			this.ContextConfig = contextConfig;
		}

		public void Execute (bool dryRun)
		{
			Console.WriteLine ("Simulating {0:s}. ", this.Title);
			
			this.BeforeRun ();
			if (!dryRun) {
				this.Run ();
			}
			this.AfterRun (dryRun);
			
			Console.WriteLine ("\tThis may take some time... done.\n");
		}

		public void BeforeRun ()
		{
			Directory.CreateDirectory (this.Cwd);
			
			this.CacheSystemConfig.WriteTo (this.Cwd + Path.DirectorySeparatorChar + POSTFIX_CACHE_CONFIG_FILE);
			this.ContextConfig.WriteTo (this.Cwd + Path.DirectorySeparatorChar + POSTFIX_CONTEXT_CONFIG_FILE);
			
			this.PipelineConfig.Julie.WriteTo (this.Cwd + Path.DirectorySeparatorChar + PipelineConfig.JulieConfig.CTX_TO_MAPPINGS_FILE_NAME);
			
			string fileNameBootstrap = this.Cwd + Path.DirectorySeparatorChar + BOOTSTRAP_FILE;
			
			StreamWriter sw = new StreamWriter (fileNameBootstrap);
			
			sw.Write ("/home/itecgo/Julie/Multi2Sim/m2s-build/src/m2s");
			sw.Write (" -cacheconfig " + POSTFIX_CACHE_CONFIG_FILE);
			sw.Write (" -ctxconfig " + POSTFIX_CONTEXT_CONFIG_FILE);
			sw.Write (" " + this.PipelineConfig.WriteTo (this));
			sw.Write (" 1>m2s.out 2>m2s.err");
			
			sw.Close ();
			
			Stat sStat;
			if (Syscall.stat (fileNameBootstrap, out sStat) == 0) {
				FilePermissions fp = sStat.st_mode | FilePermissions.S_IXUSR;
				Syscall.chmod (fileNameBootstrap, fp);
			}
		}

		public void Run ()
		{
			string cmdline = "cd " + this.Cwd + ";./" + BOOTSTRAP_FILE;
			
			Process p = new Process ();
			p.StartInfo.UseShellExecute = false;
			p.StartInfo.FileName = "bash";
			p.StartInfo.Arguments = "-c \"" + cmdline + "\"";
			p.Start ();
			p.WaitForExit ();
			
			if (p.ExitCode != 0) {
				throw new Exception (string.Format ("\"{0:s}\" failed with exit code: {1:d}", cmdline, p.ExitCode));
			}
		}

		public void AfterRun (bool dryRun)
		{
			this.PipelineReport = new PipelineReport (this);
			this.CacheSystemReport = new CacheSystemReport (this);
			
			if (!dryRun) {
				Serializer.SingleInstance.SaveXML (this, this.FileName);
			}
		}

		public string Title {
			get { return Path.GetFileNameWithoutExtension (this.FileName); }
		}

		public string Cwd {
			get { return Path.GetDirectoryName (this.FileName) + Path.DirectorySeparatorChar + this.Title; }
		}

		public string FileName { get; set; }

		public PipelineConfig PipelineConfig { get; set; }
		public CacheSystemConfig CacheSystemConfig { get; set; }
		public ContextConfig ContextConfig { get; set; }

		public PipelineReport PipelineReport { get; set; }
		public CacheSystemReport CacheSystemReport { get; set; }

		public static string POSTFIX_CACHE_CONFIG_FILE = "config.cache";
		public static string POSTFIX_CONTEXT_CONFIG_FILE = "config.context";
		public static string BOOTSTRAP_FILE = "bootstrap.sh";
	}

	public sealed class Simulator
	{
		public Simulator ()
		{
			Console.WriteLine ("ImpetusSharp - A driver program written in C#/Mono for Multi2Sim");
			Console.WriteLine ("Copyright (C) 2010 Min Cai <itecgo@163.com>.");
			Console.WriteLine ("");
		}

		public void DoFunctionalSimulation (Workload workload)
		{
			string cmdline = "cd " + workload.Cwd + ";/home/itecgo/Julie/Multi2Sim/m2s-build/src/m2s-fast ./" + workload.Exe + " " + workload.Args;
			
			Process p = Process.Start (cmdline);
			p.WaitForExit ();
			
			if (p.ExitCode != 0) {
				Console.WriteLine ("\"{:s}\" failed with exit code: {:d}", cmdline, p.ExitCode);
			}
		}

		public sealed class MultithreadedExecutor
		{
			private int numBusy;
			private ManualResetEvent doneEvent;

			public List<Simulation> Simulations { get; set; }

			public MultithreadedExecutor (List<Simulation> simulations, bool dryRun)
			{
				this.Simulations = simulations;
				this.DryRun = dryRun;
			}

			public void Execute ()
			{
				int workerThreads, completionPortThreads;
				ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
				
				workerThreads = 4;
				
				ThreadPool.SetMaxThreads (workerThreads, completionPortThreads);
				
				this.doneEvent = new ManualResetEvent (false);
				
				for (int i = 0; i < this.Simulations.Count; i++) {
					ThreadPool.QueueUserWorkItem (new WaitCallback (this.DoSimulation), (object)i);
				}
				
				this.numBusy = this.Simulations.Count;
				
				this.doneEvent.WaitOne ();
			}

			private void DoSimulation (object o)
			{
				int i = (int)o;
				
				Simulation simulation = this.Simulations[i];
				
				simulation.Execute (this.DryRun);
				
				if (Interlocked.Decrement (ref this.numBusy) == 0) {
					this.doneEvent.Set ();
				}
			}

			public bool DryRun { get; set; }
		}

		public void DoBatchExecute (List<Simulation> simulations, bool multithreaded, bool dryRun)
		{
			if (multithreaded) {
				MultithreadedExecutor executor = new MultithreadedExecutor (simulations, dryRun);
				executor.Execute ();
			} else {
				foreach (Simulation simulation in simulations) {
					simulation.Execute (dryRun);
				}
			}
		}
		
//		private void WithQ6600 (string workloadResultsDir, Workload workload, ref List<Simulation> simulations, bool cacheProfilerEnabled)
//		{
//			PipelineConfig pipelineConfig = PipelineConfig.DefaultValue (4, 1, workloadResultsDir + Path.DirectorySeparatorChar + "q6600", cacheProfilerEnabled);
//			
//			CacheSystemConfig cacheConfig = CacheSystemConfig.Q6600 ();
//			
//			ContextConfig contextConfig = new ContextConfig ();
//			contextConfig.Contexts[0] = workload;
//			
//			simulations.Add (new Simulation (workload.Title + "_Q6600", workloadResultsDir + Path.DirectorySeparatorChar + "q6600", pipelineConfig, cacheConfig, contextConfig));
//		}
//
//		private void WithCorei7_930 (string workloadResultsDir, Workload workload, ref List<Simulation> simulations, bool cacheProfilerEnabled)
//		{
//			PipelineConfig pipelineConfig = PipelineConfig.DefaultValue (4, 2, workloadResultsDir + Path.DirectorySeparatorChar + "corei7_930", cacheProfilerEnabled);
//			
//			CacheSystemConfig cacheConfig = CacheSystemConfig.corei7_930 ();
//			
//			ContextConfig contextConfig = new ContextConfig ();
//			contextConfig.Contexts[0] = workload;
//			
//			simulations.Add (new Simulation (workload.Title + "_Corei7_930", workloadResultsDir + Path.DirectorySeparatorChar + "corei7_930", pipelineConfig, cacheConfig, contextConfig));
//		}
//
//		public void DoDetailedSimulation (string resultsDir, WorkloadSet workloadSet, bool cacheProfilerEnabled)
//		{
//			List<Simulation> simulations = new List<Simulation> ();
//			
//			foreach (KeyValuePair<string, Workload> pair in workloadSet.Workloads) {
//				string workloadTitle = pair.Key;
//				Workload workload = pair.Value;
//				
//				string workloadResultsDir = resultsDir + Path.DirectorySeparatorChar + workloadTitle;
//				
//				if (workload.NumThreadsNeeded == 1) {
//					PipelineConfig pipelineConfig = PipelineConfig.DefaultValue (1, 1, workloadResultsDir, cacheProfilerEnabled);
//					
//					CacheSystemConfig cacheConfig = CacheSystemConfig.DefaultValue (1, 1);
//					
//					ContextConfig contextConfig = new ContextConfig ();
//					contextConfig.Contexts[0] = workload;
//					
//					simulations.Add (new Simulation (workload.Title, workloadResultsDir, pipelineConfig, cacheConfig, contextConfig));
//					
//				} else if (workload.NumThreadsNeeded == 2) {
//					WithQ6600 (workloadResultsDir, workload, ref simulations, cacheProfilerEnabled);
//					WithCorei7_930 (workloadResultsDir, workload, ref simulations, cacheProfilerEnabled);
//				} else {
//					throw new Exception (string.Format ("Simulation of the workload ({0:s}) not implemented.", workload.Title));
//				}
//			}
//			
//			DoBatchExecute (simulations, false);
//		}
		
	}
	
	#endregion
}
