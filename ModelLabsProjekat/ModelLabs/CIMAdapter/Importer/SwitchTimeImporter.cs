﻿using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class SwitchTimeImporter
	{
		/// <summary> Singleton </summary>
		private static SwitchTimeImporter ptImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static SwitchTimeImporter Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new SwitchTimeImporter();
							ptImporter.Reset();
						}
					}
				}
				return ptImporter;
			}
		}

		public Delta NMSDelta
		{
			get 
			{
				return delta;
			}
		}
		#endregion Properties


		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					// convert into DMS elements
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}
			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		/// <summary>
		/// Method performs conversion of network elements from CIM based concrete model into DMS model.
		/// </summary>
		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

			//// import all concrete model types (DMSType enum)
			//ImportBaseVoltages();
			//ImportLocations();
			//ImportPowerTransformers();
			//ImportTransformerWindings();
			//ImportWindingTests();

			ImportLoadBreakSwitch();
			ImportBreaker();
			ImportSeason();
			ImportDayType();
			ImportSwitchSchedule();
			ImportRegularTimePoint();
			ImportRecloseSequence();

            LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import
        private void ImportRecloseSequence()
        {
            SortedDictionary<string, object> cimRecloseSequences = concreteModel.GetAllObjectsOfType("FTN.RecloseSequence");
            if (cimRecloseSequences != null)
            {
                foreach (KeyValuePair<string, object> cimRecloseSequencePair in cimRecloseSequences)
                {
                    FTN.RecloseSequence cimRecloseSequence = cimRecloseSequencePair.Value as FTN.RecloseSequence;

                    ResourceDescription rd = CreateRecloseSequenceResourceDescription(cimRecloseSequence);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("RecloseSequence ID = ").Append(cimRecloseSequence.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("RecloseSequence ID = ").Append(cimRecloseSequence.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateRecloseSequenceResourceDescription(FTN.RecloseSequence cimRecloseSequence)
        {
            ResourceDescription rd = null;
            if (cimRecloseSequence != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.RECLOSESEQUENCE, importHelper.CheckOutIndexForDMSType(DMSType.RECLOSESEQUENCE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimRecloseSequence.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateRecloseSequenceProperties(cimRecloseSequence, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportRegularTimePoint()
        {
            SortedDictionary<string, object> cimRegularTimePoints = concreteModel.GetAllObjectsOfType("FTN.RegularTimePoint");
            if (cimRegularTimePoints != null)
            {
                foreach (KeyValuePair<string, object> cimRegularTimePointPair in cimRegularTimePoints)
                {
                    FTN.RegularTimePoint cimRegularTimePoint = cimRegularTimePointPair.Value as FTN.RegularTimePoint;

                    ResourceDescription rd = CreateRegularTimePointResourceDescription(cimRegularTimePoint);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("RegularTimePoint ID = ").Append(cimRegularTimePoint.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("RegularTimePoint ID = ").Append(cimRegularTimePoint.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }

        private ResourceDescription CreateRegularTimePointResourceDescription(FTN.RegularTimePoint cimRegularTimePoint )
        {
            ResourceDescription rd = null;
            if (cimRegularTimePoint != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.REGULARTIMEPOINT, importHelper.CheckOutIndexForDMSType(DMSType.REGULARTIMEPOINT));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimRegularTimePoint.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateRegularTimePointProperties(cimRegularTimePoint, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSwitchSchedule()
        {
            SortedDictionary<string, object> cimSwitchSchedules = concreteModel.GetAllObjectsOfType("FTN.SwitchSchedule");
            if (cimSwitchSchedules != null)
            {
                foreach (KeyValuePair<string, object> cimSwitchSchedulePair in cimSwitchSchedules)
                {
                    FTN.SwitchSchedule cimSwitchSchedule = cimSwitchSchedulePair.Value as FTN.SwitchSchedule;

                    ResourceDescription rd = CreateSwitchScheduleResourceDescription(cimSwitchSchedule);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("SwitchSchedule ID = ").Append(cimSwitchSchedule.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("SwitchSchedule ID = ").Append(cimSwitchSchedule.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateSwitchScheduleResourceDescription(FTN.SwitchSchedule cimSwitchSchedule )
        {
            ResourceDescription rd = null;
            if (cimSwitchSchedule != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SWITCHSCHEDULE, importHelper.CheckOutIndexForDMSType(DMSType.SWITCHSCHEDULE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSwitchSchedule.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateSwitchScheduleProperties(cimSwitchSchedule, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportDayType()
        {
            SortedDictionary<string, object> cimDayTypes = concreteModel.GetAllObjectsOfType("FTN.DayType");
            if (cimDayTypes != null)
            {
                foreach (KeyValuePair<string, object> cimDayTypePair in cimDayTypes)
                {
                    FTN.DayType cimDayType = cimDayTypePair.Value as FTN.DayType;

                    ResourceDescription rd = CreateDayTypeResourceDescription(cimDayType);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("DayType ID = ").Append(cimDayType.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("DayType ID = ").Append(cimDayType.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateDayTypeResourceDescription(FTN.DayType cimDayType)
        {
            ResourceDescription rd = null;
            if (cimDayType != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DAYTYPE, importHelper.CheckOutIndexForDMSType(DMSType.DAYTYPE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDayType.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateDayTypeProperties(cimDayType, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportSeason()
        {
            SortedDictionary<string, object> cimSeasons = concreteModel.GetAllObjectsOfType("FTN.Season");
            if (cimSeasons != null)
            {
                foreach (KeyValuePair<string, object> cimSeasonPair in cimSeasons)
                {
                    FTN.Season cimSeason = cimSeasonPair.Value as FTN.Season;

                    ResourceDescription rd = CreateSeasonResourceDescription(cimSeason);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Season ID = ").Append(cimSeason.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Season ID = ").Append(cimSeason.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateSeasonResourceDescription(FTN.Season cimSeason)
        {
            ResourceDescription rd = null;
            if (cimSeason != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SEASON, importHelper.CheckOutIndexForDMSType(DMSType.SEASON));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimSeason.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateSeasonProperties(cimSeason, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportBreaker()
        {
            SortedDictionary<string, object> cimBreakers = concreteModel.GetAllObjectsOfType("FTN.Breaker");
            if (cimBreakers != null)
            {
                foreach (KeyValuePair<string, object> cimBreakerPair in cimBreakers)
                {
                    FTN.Breaker cimBreaker = cimBreakerPair.Value as FTN.Breaker;

                    ResourceDescription rd = CreateBreakerResourceDescription(cimBreaker);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("Breaker ID = ").Append(cimBreaker.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("Breaker ID = ").Append(cimBreaker.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateBreakerResourceDescription(FTN.Breaker cimBreaker)
        {
            ResourceDescription rd = null;
            if (cimBreaker != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.BREAKER, importHelper.CheckOutIndexForDMSType(DMSType.BREAKER));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimBreaker.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateBreakerProperties(cimBreaker, rd, importHelper, report);
            }
            return rd;
        }

        private void ImportLoadBreakSwitch()
        {
            SortedDictionary<string, object> cimLoadBreakSwitches = concreteModel.GetAllObjectsOfType("FTN.LoadBreakSwitch");
            if (cimLoadBreakSwitches != null)
            {
                foreach (KeyValuePair<string, object> cimLoadBreakSwitchPair in cimLoadBreakSwitches)
                {
                    FTN.LoadBreakSwitch cimLoadBreakSwitch = cimLoadBreakSwitchPair.Value as FTN.LoadBreakSwitch;

                    ResourceDescription rd = CreateLoadBreakSwitchResourceDescription(cimLoadBreakSwitch);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append("LoadBreakSwitch ID = ").Append(cimLoadBreakSwitch.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("LoadBreakSwitch ID = ").Append(cimLoadBreakSwitch.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateLoadBreakSwitchResourceDescription(FTN.LoadBreakSwitch cimLoadBreakSwitch)
        {
            ResourceDescription rd = null;
            if (cimLoadBreakSwitch != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.LOADBREAKSWITCH, importHelper.CheckOutIndexForDMSType(DMSType.LOADBREAKSWITCH));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimLoadBreakSwitch.ID, gid);

                ////populate ResourceDescription
                SwitchTimeConverter.PopulateLoadBreakSwitchProperties(cimLoadBreakSwitch, rd, importHelper, report);
            }
            return rd;
        }
		#endregion Import
	}
}

