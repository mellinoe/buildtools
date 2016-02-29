﻿using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using GenFacades;
using Microsoft.Cci.Extensions;
using System.Linq;
using System.Diagnostics;

namespace Microsoft.DotNet.Build.Tasks
{
    /// <summary>
    /// Runs GenFacades In-Proc.
    /// </summary>
    public sealed class GenFacadesTask : Task
    {
        [Required]
        public string Seeds { get; set; }

        [Required]
        public string Contracts { get; set; }

        [Required]
        public string FacadePath { get; set; }

        public Version AssemblyFileVersion { get; set; }

        public bool ClearBuildAndRevision { get; set; }

        public bool IgnoreMissingTypes { get; set; }

        public bool BuildDesignTimeFacades { get; set; }

        public string InclusionContracts { get; set; }

        public string SeedLoadErrorTreatment { get; set; }

        public string ContractLoadErrorTreatment { get; set; }

        public ITaskItem[] SeedTypePreferencesUnsplit { get; set; }

        public bool ForceZeroVersionSeeds { get; set; }

        public bool ProducePdb { get; set; } = true;

        public string PartialFacadeAssemblyPath { get; set; }

        public override bool Execute()
        {
            TraceLogger logger = new TraceLogger(Log);

            try
            {
                Trace.Listeners.Add(logger);
                ErrorTreatment seedLoadErrorTreatment = ErrorTreatment.Default;
                ErrorTreatment contractLoadErrorTreatment = ErrorTreatment.Default;

                if (SeedLoadErrorTreatment != null)
                {
                    seedLoadErrorTreatment = (ErrorTreatment)Enum.Parse(typeof(ErrorTreatment), SeedLoadErrorTreatment);
                }
                if (ContractLoadErrorTreatment != null)
                {
                    contractLoadErrorTreatment = (ErrorTreatment)Enum.Parse(typeof(ErrorTreatment), ContractLoadErrorTreatment);
                }

                string[] seedTypePreferencesUnsplit = null;
                if (SeedTypePreferencesUnsplit != null)
                {
                    seedTypePreferencesUnsplit = SeedTypePreferencesUnsplit.Select(iti => iti.ItemSpec).ToArray();
                }

                Generator.GenerateFacades(
                    Seeds, Contracts, FacadePath, AssemblyFileVersion, ClearBuildAndRevision,
                    IgnoreMissingTypes, BuildDesignTimeFacades, InclusionContracts, seedLoadErrorTreatment,
                    contractLoadErrorTreatment, seedTypePreferencesUnsplit, ForceZeroVersionSeeds,
                    ProducePdb, PartialFacadeAssemblyPath);

                return true;
            }
            catch (Exception e)
            {
                Trace.Listeners.Remove(logger);
                Log.LogErrorFromException(e, showStackTrace: true);
                return false;
            }
        }
    }

    // Trace listener which writes to the MSBuild log.
    public class TraceLogger : TraceListener
    {
        private readonly TaskLoggingHelper _log;

        public TraceLogger(TaskLoggingHelper log)
        {
            _log = log;
        }

        public override void Write(string message)
        {
            _log.LogMessage(message);
        }

        public override void WriteLine(string message)
        {
            _log.LogMessage(message + Environment.NewLine);
        }
    }
}
