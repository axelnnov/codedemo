namespace Companynamehere.Project.UI.Services.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Hosting;
    using Companynamehere.Common.BaseComponents;
    using Companynamehere.Project.Services.Shared.BusinessServices.Response;
    using Companynamehere.Project.UI.CrossCutting;
    using Companynamehere.Outsourcing.Shared.ApplicationEvents;
    using Companynamehere.Outsourcing.Shared.Entities.ParticipantObjectModel.BusinessObjects;
    using Companynamehere.Outsourcing.Shared.Entities.ParticipantObjectModel.Interfaces;
    using Companynamehere.Outsourcing.Shared.SharedLibrary;

    using Newtonsoft.Json;

    public static class FeatureFileName
    {
        public const string CalcRequestResult = "CalcRequestResult";

        public const string EstimateRequestResult = "EstimateRequestResult";
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public static class MockFileName
    {
        public const string CalcModel = "DBCalcWebModel";

        public const string GlobalModel = "GlobalModel";

        public const string PayModel = "PayWebModel";

        public const string PlanModel = "PlanData";

        public const string DbParticipantModel = "DbParticipant";

        public const string BeneficiaryModel = "BeneficiaryWebModel";

        public const string MegaNavModel = "MegaNav";

        public const string CommencementPersonalInfoModel = "CommencementPersonalInfoWebModel";

        public const string CommencementWebModel = "CommencementWebModel";

        public const string CommencementOverviewModel = "GetCommencementOverviewWebModel";

        public const string DashboardDataDetails = "GetDashboardDataDetails";

        public const string KnowledgeCenterData = "KnowledgeCenterData";

        public const string ParticipantPersonalizations = "ParticipantPersonalizations";

        public const string Client = "Client";

        public const string Participant = "Participant";

        public const string ParticipantProfile = "ParticipantProfile";

        public const string ParticipantProfileLookUpData = "ParticipantProfileLookUpData";

        public const string ParticipantDomainData = "ParticipantDomainData";

        public const string PortalDashboardInfo = "PortalDashboardInfo";

        public const string AllSiteCoreContent = "AllSiteCoreContent";
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class FileMockDataService : IMockDataService
    {
        private readonly string mockDataPath;

        private readonly bool _emulateResponseLatency;
        private readonly int _responseLatency;

        private readonly List<string> _featureFileList = new List<string> { FeatureFileName.CalcRequestResult, FeatureFileName.EstimateRequestResult };

        public FileMockDataService(string dataDirectory, string responseLatency)
        {
            try
            {
                mockDataPath = HostingEnvironment.MapPath("~/" + dataDirectory);
                Directory.CreateDirectory(mockDataPath ?? dataDirectory);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            _emulateResponseLatency = false;
            if (int.TryParse(responseLatency, out _responseLatency))
            {
                _emulateResponseLatency = _responseLatency > 0;
            }
        }

        public void WriteObject<TObject>(TObject instance, string filename = null, ICallerContext callerContext = null)
        {
            try
            {
                if (instance is ServiceWebModel<IDBParticipant> && filename != MockFileName.GlobalModel)
                {
                    SetDom(instance, callerContext);
                }

                filename = GetJsonFileFullName(filename, callerContext);
                using (var writer = new StreamWriter(filename))
                {
                    var result = JsonConvert.SerializeObject(instance, Formatting.Indented);
                    writer.Write(result);
                }

                LogMessage("Wrote mock data to {0}", filename);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        public TObject ReadObject<TObject>(Func<TObject> job, string filename = null, ICallerContext callerContext = null)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }

            if (_emulateResponseLatency)
            {
                Thread.Sleep(_responseLatency);
            }

            TObject item = default(TObject);
            if (_featureFileList.Contains(filename))
            {
                try
                {
                    item = ReadObject<TObject>(filename, callerContext);
                }
                catch
                {
                }

                if (Equals(item, default(TObject)))
                {
                    item = job.Invoke();
                    WriteObject(item, filename);

                    LogMessage("Wrote feature data to {0}", filename);
                }
            }
            else
            {
                try
                {
                    item = ReadObject<TObject>(filename, callerContext);
                }
                catch (Exception ex)
                {
                    var ssn = CrossCuttingUtilities.GetParticipantIdFromContext(callerContext);
                    throw new FileLoadException(string.Format("Error while processing json mock file. ssn={0}, modelFile={1}", ssn, filename), filename, ex);
                }

                if (Equals(item, default(TObject)))
                {
                    var ssn = CrossCuttingUtilities.GetParticipantIdFromContext(callerContext);
                    throw new FileNotFoundException(string.Format("Error while processing json mock file. ssn={0}", ssn), filename);
                }
            }

            if (item is ServiceWebModel<IDBParticipant> && filename != MockFileName.GlobalModel)
            {
                item = GetDom(item, callerContext);
            }

            return item;
        }

        public TObject DeserializeObject<TObject>(string jsonString)
        {
            return JsonConvert.DeserializeObject<TObject>(jsonString, MockUtils.GetDepersonalizationSettings());
        }

        public TObject ReadObject<TObject>(string filename = null, ICallerContext callerContext = null)
        {
            try
            {
                filename = GetJsonFileFullName(filename, callerContext);
                var jsonString = File.ReadAllText(filename);

                var obj = JsonConvert.DeserializeObject<TObject>(jsonString, MockUtils.GetDepersonalizationSettings());
                LogMessage("Read mock data from {0}", filename);
                return obj;
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }

            ////return default(TObject);
        }

        private static void LogError(Exception ex)
        {
            ServicesApplicationContext.Instance.Logging.LogException(null, null, ex, ex.ToString());
        }

        private static void LogMessage(string format, params object[] args)
        {
            string temp = string.Format(format, args);
            ServicesApplicationContext.Instance.Logging.LogMessage(null, null, LoggingLevel.Verbose, temp);
        }

        private static string GetMappedFileName(string methodName)
        {
            switch (methodName)
            {
                case MockFileName.CalcModel:
                    return "DBCalcWebModelService.GetCalcWebModel";
                case MockFileName.GlobalModel:
                    return "DBGlobalWebModelService.GetGlobalWebModel";
                case MockFileName.PayModel:
                    return "DBPayWebModelService.GetPayWebModel";
                case MockFileName.PlanModel:
                case MockFileName.DbParticipantModel:
                    return "DBPlanWebModelService.GetPlanWebModel";
                case MockFileName.BeneficiaryModel:
                    return "DBBeneficiaryWebModelService.GetBenficiaryWebModel";
                case MockFileName.MegaNavModel:
                    return "MegaNavService.GetMegaNav";
                case MockFileName.CommencementWebModel:
                    return "DBCommencementWebModelService.GetCommencementWebModel";
                case MockFileName.CommencementPersonalInfoModel:
                    return "DBCommencementWebModelService.GetCommencementPersonalInfoWebModel";
                case MockFileName.CommencementOverviewModel:
                    return "DBCommencementWebModelService.GetCommencementOverviewWebModel";
                case MockFileName.DashboardDataDetails:
                    return "PortalDashboardService.GetPortalDashboardInfo";
                case MockFileName.KnowledgeCenterData:
                    return "ParticipantService.GetKnowledgeCenterData";
                case MockFileName.ParticipantPersonalizations:
                    return "ParticipantService.GetParticipantPersonalizations";
                case MockFileName.Client:
                    return "ClientService.GetClient";
                case MockFileName.Participant:
                    return "ParticipantService.GetParticipant";
                case MockFileName.ParticipantProfile:
                    return "ParticipantProfileService.GetParticipantProfile";
                case MockFileName.ParticipantProfileLookUpData:
                    return "ParticipantProfileService.GetParticipantProfileLookUpData";
                case MockFileName.ParticipantDomainData:
                    return "ParticipantService.GetParticipantDomainData";
                case MockFileName.PortalDashboardInfo:
                    return "PortalDashboardService.GetPortalDashboardInfo";
                case MockFileName.AllSiteCoreContent:
                    return "AllSiteCoreContent";
                default:
                    return null;
            }
        }

        private string GetJsonFileFullName(string methodName = null, ICallerContext callerContext = null)
        {
            if (methodName != null && callerContext != null)
            {
                var mappedMethodName = GetMappedFileName(methodName);
                if (mappedMethodName != null)
                {
                    var ssn = CrossCuttingUtilities.GetParticipantIdFromContext(callerContext);
                    try
                    {
                        var files = Directory.GetFiles(mockDataPath, "*.json");
                        var targetFile = files.FirstOrDefault(item => item.Contains(mappedMethodName) && item.Contains(ssn));
                        if (targetFile != null)
                        {
                            return targetFile;
                        }

                        LogMessage(string.Format("There is no service responce file for ssn={0} methodName={1} mappedMethodName={2}", ssn, methodName, mappedMethodName));
                    }
                    catch (Exception ex)
                    {
                        LogMessage(string.Format("Error occurred while getting ervice responce file for ssn={0} methodName={1} mappedMethodName={2}{3}{4}", ssn, methodName, mappedMethodName, Environment.NewLine, ex));
                    }
                }
            }

            return string.Format("{0}\\{1}.json", mockDataPath, methodName ?? MockUtils.GetCallerMethodName());
        }

        private void SetDom<TObject>(TObject targetData, ICallerContext callerContext)
        {
            var webModel = targetData as ServiceWebModel<IDBParticipant>;
            if (webModel != null)
            {
                var global = ReadObject(() => default(ServiceWebModel<IDBParticipant>), MockFileName.GlobalModel, callerContext);
                if (!Equals(global, default(ServiceWebModel<IDBParticipant>)))
                {
                    ((DBParticipant)webModel.DomainObject).PlansNotInPay = null;
                    global.DomainObject = webModel.DomainObject;
                    WriteObject(global, MockFileName.GlobalModel, callerContext);
                }
                else
                {
                    LogMessage(string.Format("SetDom -> There is no GlobalModel.json file"));
                }
            }
        }

        private TObject GetDom<TObject>(TObject targetData, ICallerContext callerContext)
        {
            var webModel = targetData as ServiceWebModel<IDBParticipant>;
            if (webModel != null)
            {
                var global = ReadObject(() => default(ServiceWebModel<IDBParticipant>), MockFileName.GlobalModel, callerContext);
                if (!Equals(global, default(ServiceWebModel<IDBParticipant>)))
                {
                    webModel.DomainObject = global.DomainObject;
                }
                else
                {
                    LogMessage(string.Format("GetDom -> There is no GlobalModel.json file"));
                }
            }

            return targetData;
        }
    }
}
