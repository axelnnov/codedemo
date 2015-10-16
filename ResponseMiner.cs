namespace Companynamehere.Project.UI.Services.DB
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::DB.Services.ApplicationEvents;
    using Companynamehere.Common.BaseComponents;
    using Companynamehere.Project.BusinessApplicationEvents;
    using Companynamehere.Project.Services.Proxy;
    using Companynamehere.Project.Services.Shared.BusinessServices.Interfaces;
    using Companynamehere.Project.UI.CrossCutting;
    using Companynamehere.Project.UI.Models.Sitecore;
    using Companynamehere.Outsourcing.Shared.SharedLibrary;
    using Companynamehere.Outsourcing.Shared.SharedLibrary.Security;
    using Companynamehere.Outsourcing.Shared.SharedLibrary.Security.Claims;
    using Companynamehere.Outsourcing.Shared.Utilities.Extensions;
    using MetadataService.Interfaces;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class ResponseMiner
    {
        #region Constants

        private const string ClientId = "QASTD1";

        #endregion

        #region Static Fields

        private static readonly string SubFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/RESPONSES");
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        private static readonly string ServiceHandlerPath = "http://host.com/handler/sitecorecontentservice.ashx";
        private static readonly string DefaultPathFragment = "/sitecore/content/Project";
        private static readonly string RootFragment = "/sitecore";
        private static readonly string TargetDb = "master";
        private static readonly string Language = "en";

        private static readonly List<string> ContentPaths = new List<string>()
        {
            "/Default/S/StaticSortBy/StaticSortBy_1",
            "/Default/S/StaticCountryList/StaticCountrylist_1",
            "/Default/S/StaticGender/StaticGender_1",
            "/Default/S/StaticImages",
            "/Default/S/StaticMaritalStatus/StaticMaritalStatus_1",
            "/Default/S/StaticMessageCenterCategories/StaticMessageCenterCategories_1",
            "/Default/S/StaticMessageCenterContactUs/StaticMessageCenterContactUs_1",
            "/Default/S/StaticProxySearch/StaticProxySearch_1",
            "/Default/S/StaticSortBy/StaticSortBy_1",
            "/Default/S/StaticTerms",
            "/Default/S/StaticDcPlanTypes"
        };

        #endregion

        #region Public Methods and Operators

        public static void GetResponces(ICallerContext callerContext)
        {
            if (CrossCuttingUtilities.GetParticipantIdFromContext(callerContext) == "111000001")
            {
                Task.Factory.StartNew(GetFiles);
            }
            else if (CrossCuttingUtilities.GetParticipantIdFromContext(callerContext) == "111000005")
            {
                Task.Factory.StartNew(GetAllFiles);
            }
            else
            {
                GetFilesForParticipant(callerContext);
            }
        }

        public static void GetAllFiles()
        {
            foreach (var pair in GetSsnData())
            {
                Console.WriteLine("Requesting responses for personId={0}", pair.Value);
                var personId = pair.Value;
                GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, personId));
            }
        }

        public static void GetFiles()
        {
            Directory.CreateDirectory(SubFolder);

            var ssn = "111000001";

            var callerContext = SecurityHelper.CreateCallerContext(ClientId, ssn);
            GetSiteCoreContent(callerContext);

            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "111000031";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "555333709";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "555217996";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "777000224";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "777357380";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "111000005";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            ssn = "111000078";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(ClientId, ssn));

            var oneCode = "ProjectROL";
            ssn = "777002322";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));

            ssn = "777008186";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));

            ssn = "777003097";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));

            ssn = "777005305";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));

            ssn = "888561927";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));

            ssn = "777990020";
            GetFilesForParticipant(SecurityHelper.CreateCallerContext(oneCode, ssn));
        }

        public static void GetFilesForParticipant(ICallerContext callerContext)
        {
            var ssn = CrossCuttingUtilities.GetParticipantIdFromContext(callerContext);
            var oneCode = callerContext.ClientInfo.ClientOneCode;
            WrapCall(() => SaveDbParticipantDataServiceResponse(oneCode, ssn));
            WrapCall(() => SavePlanWebModelServiceResponse(oneCode, ssn, "1"));
            WrapCall(() => SavePayWebModelServiceResponse(oneCode, ssn));
            WrapCall(() => SaveGetCalcWebModelServiceResponse(oneCode, ssn, "1"));
            WrapCall(() => SaveGlobalWebModelServiceResponse(oneCode, ssn));
            WrapCall(() => SaveBenficiaryWebModelServiceResponse(oneCode, ssn, "1"));
            WrapCall(() => SaveMegaNavServiceResponse(oneCode, ssn));
            WrapCall(() => SaveCommencementWebModelServiceResponse(oneCode, ssn, "1"));
            WrapCall(() => SaveCommencementPersonalInfoWebModelServiceResponse(oneCode, ssn));
            WrapCall(() => SaveCommencementOverviewWebModelServiceResponse(oneCode, ssn));
            WrapCall(() => SavePortalDashboardInfoResponse(oneCode, ssn));
            WrapCall(() => SaveKnowledgeCenterDataResponse(oneCode, ssn));
            WrapCall(() => SaveParticipantPersonalizationsResponse(oneCode, ssn));
            WrapCall(() => SaveClientResponse(oneCode, ssn));
            WrapCall(() => SaveParticipantResponse(oneCode, ssn));
            WrapCall(() => SaveParticipantDomainDataResponse(oneCode, ssn));
            WrapCall(() => SaveWelcomeInfoResponse(oneCode, ssn));
            WrapCall(() => SaveParticipantProfileResponse(oneCode, ssn));
            WrapCall(() => SaveParticipantProfileLookUpDataResponse(oneCode, ssn));
        }

        public static void GetSiteCoreContent(ICallerContext callerContext)
        {
            var oneCode = callerContext.ClientInfo.ClientOneCode;
            var global = WrapFunk(() => ServicesApplicationContext.Instance.DBServicesConduit().DBGlobalWebModelService.GetGlobalWebModel(oneCode, null, callerContext, true));
            var personId = global != null ? global.DomainObject.PersonData.PersonID : "0";
            var personSsn = global != null ? global.DomainObject.PersonData.SSN : "0";
            var planCode = "1";

            try
            {
                var contentList = GetContent(ContentPaths).ToList();

                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBPlanWebModelService.GetPlanWebModel(oneCode, null, planCode, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBPayWebModelService.GetPayWebModel(oneCode, null, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBCalcWebModelService.GetCalcWebModel(oneCode, null, planCode, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBGlobalWebModelService.GetGlobalWebModel(oneCode, null, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBBeneficiaryWebModelService.GetBenficiaryWebModel(oneCode, null, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().MegaNavService.GetMegaNav(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementWebModel(oneCode, personSsn, planCode, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementPersonalInfoWebModel(oneCode, personSsn, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementOverviewWebModel(oneCode, personSsn, callerContext, true)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().PortalDashboardService.GetPortalDashboardInfo(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetKnowledgeCenterData(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipantPersonalizations(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ClientService.GetClient(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipant(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipantDomainData(callerContext, string.Empty)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().WelcomeService.GetWelcomeInfo(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantProfileService.GetParticipantProfile(callerContext)));
                contentList.AddRange(GetContentCollection(() => ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantProfileService.GetParticipantProfileLookUpData(callerContext)));

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(contentList, Newtonsoft.Json.Formatting.Indented, JsonSettings);

                var filename = string.Format("AllSiteCoreContent.json");

                SaveJson(filename, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Methods

        private static Dictionary<string, string> GetSsnData()
        {
            return new Dictionary<string, string>
                       {
                           { "1", "666000042" }, 
                           { "2", "888000004" }, 
                           { "3", "888000218" }, 
                           { "4", "888000244" }, 
                           { "5", "888000199" }, 
                           { "6", "888000201" }, 
                           { "7", "888000198" }, 
                           { "8", "889000022" }, 
                           { "9", "889000106" }, 
                           { "10", "889000103" }, 
                           { "11", "777916443" }, 
                           { "12", "777926991" }, 
                           { "13", "111000051" }, 
                           { "14", "111000001" }, 
                           { "15", "111000011" }, 
                           { "16", "111000052" }, 
                           { "19", "111000031" }, 
                           { "20", "111000041" }, 
                           { "22", "111010051" }, 
                           { "23", "111010001" }, 
                           { "24", "111010011" }, 
                           { "25", "111010031" }, 
                           { "26", "111010041" }, 
                           { "27", "111020051" }, 
                           { "28", "111020001" }, 
                           { "29", "111020011" }, 
                           { "30", "111020031" }, 
                           { "31", "111020041" }, 
                           { "34", "111030051" }, 
                           { "35", "111030001" }, 
                           { "36", "111030011" }, 
                           { "37", "111030031" }, 
                           { "38", "111030041" }, 
                           { "40", "111030052" }, 
                           { "42", "258475851" }, 
                           { "43", "111111101" }, 
                           { "44", "111111102" }, 
                           { "45", "111111103" }, 
                           { "46", "111111104" }, 
                           { "48", "111000005" }, 
                           { "49", "111000012" }, 
                           { "50", "111000078" }, 
                           { "51", "111111105" }, 
                           { "52", "111111106" }, 
                           { "53", "111111107" }, 
                           { "54", "111111108" }, 
                           { "55", "111111109" }, 
                           { "56", "111111110" }, 
                           { "57", "111111111" }, 
                           { "58", "111111112" }, 
                           { "59", "111111113" }, 
                           { "60", "111111114" }, 
                           { "61", "111111115" }, 
                           { "62", "111111116" }, 
                           { "63", "111111117" }, 
                           { "64", "111111118" }, 
                           { "65", "111111119" }, 
                           { "67", "901000055" }, 
                           { "68", "111000020" }, 
                           { "69", "777000005" }, 
                           { "70", "777000224" }, 
                           { "71", "777748236" }, 
                           { "72", "777540768" }, 
                           { "73", "777486255" }, 
                           { "74", "777569539" }, 
                           { "75", "777645431" }, 
                           { "76", "777357380" }, 
                           { "77", "777723425" }, 
                           { "78", "777471631" }, 
                           { "79", "777320000" }, 
                           { "80", "777000008" }, 
                           { "81", "777990020" }, 
                           { "82", "666000242" }, 
                           { "83", "666000246" }, 
                           { "84", "666421941" }, 
                           { "85", "666489753" }, 
                           { "86", "666800418" }, 
                           { "87", "666560590" }, 
                           { "88", "666568724" }, 
                           { "89", "666000098" }, 
                           { "90", "666000199" }, 
                           { "91", "666000225" }, 
                           { "92", "666000228" }, 
                           { "93", "666044552" }, 
                           { "94", "666340000" }, 
                           { "95", "555606523" }, 
                           { "96", "555217996" }, 
                           { "97", "555333709" }, 
                           { "99", "555000226" }, 
                           { "100", "555001452" }, 
                           { "101", "555100000" }, 
                       };
        }

        private static void SaveParticipantProfileLookUpDataResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantProfileService.GetParticipantProfileLookUpData(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantProfileService.GetParticipantProfileLookUpData.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveParticipantProfileResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantProfileService.GetParticipantProfile(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantProfileService.GetParticipantProfile.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveWelcomeInfoResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().WelcomeService.GetWelcomeInfo(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_WelcomeService.GetWelcomeInfo.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveParticipantDomainDataResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipantDomainData(callerContext, string.Empty);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantService.GetParticipantDomainData.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveParticipantResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipant(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantService.GetParticipant.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveClientResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ClientService.GetClient(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ClientService.GetClient.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveKnowledgeCenterDataResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetKnowledgeCenterData(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantService.GetKnowledgeCenterData.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveParticipantPersonalizationsResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().ParticipantService.GetParticipantPersonalizations(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_ParticipantService.GetParticipantPersonalizations.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SavePortalDashboardInfoResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().PortalDashboardService.GetPortalDashboardInfo(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_PortalDashboardService.GetPortalDashboardInfo.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveCommencementOverviewWebModelServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementOverviewWebModel(clientOneCode, personId, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBCommencementWebModelService.GetCommencementOverviewWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveCommencementPersonalInfoWebModelServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementPersonalInfoWebModel(clientOneCode, personId, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBCommencementWebModelService.GetCommencementPersonalInfoWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveCommencementWebModelServiceResponse(string clientOneCode, string personId, string planCode)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBCommencementWebModelService.GetCommencementWebModel(clientOneCode, personId, planCode, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_{2}_DBCommencementWebModelService.GetCommencementWebModel.json", clientOneCode, personId, planCode);

            SaveJson(filename, json);
        }

        private static void SaveBenficiaryWebModelServiceResponse(string clientOneCode, string personId, string planCode)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBBeneficiaryWebModelService.GetBenficiaryWebModel(clientOneCode, null, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBBeneficiaryWebModelService.GetBenficiaryWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveDbParticipantDataServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBParticipantDataService.GetParticipant(clientOneCode, null, callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBParticipantDataService.GetParticipant.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveGetCalcWebModelServiceResponse(string clientOneCode, string personId, string planCode)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBCalcWebModelService.GetCalcWebModel(clientOneCode, null, planCode, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBCalcWebModelService.GetCalcWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveGlobalWebModelServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBGlobalWebModelService.GetGlobalWebModel(clientOneCode, null, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBGlobalWebModelService.GetGlobalWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SaveJson(string filename, string json)
        {
            filename = Path.Combine(SubFolder, filename);
            System.IO.File.WriteAllText(filename, json);
            Console.WriteLine("Saved: {0}", filename);
        }

        private static void SavePayWebModelServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBPayWebModelService.GetPayWebModel(clientOneCode, null, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_DBPayWebModelService.GetPayWebModel.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void SavePlanWebModelServiceResponse(string clientOneCode, string personId, string planCode)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.DBServicesConduit().DBPlanWebModelService.GetPlanWebModel(clientOneCode, null, planCode, callerContext, true);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

            var filename = string.Format("{0}_{1}_{2}_DBPlanWebModelService.GetPlanWebModel.json", clientOneCode, personId, planCode);

            SaveJson(filename, json);
        }

        private static void SaveMegaNavServiceResponse(string clientOneCode, string personId)
        {
            var callerContext = SecurityHelper.CreateCallerContext(clientOneCode, personId);
            var model = ServicesApplicationContext.Instance.PortalServicesConduit().MegaNavService.GetMegaNav(callerContext);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented, JsonSettings);

            var filename = string.Format("{0}_{1}_MegaNavService.GetMegaNav.json", clientOneCode, personId);

            SaveJson(filename, json);
        }

        private static void WrapCall(Action job)
        {
            try
            {
                job.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static T WrapFunk<T>(Func<T> job)
        {
            try
            {
                return job.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default(T);
            }
        }

        private static IEnumerable<SitecoreContentItem> GetContentCollection<T>(Func<T> job)
        {
            var model = WrapFunk(job);
            if (!model.Equals(default(T)))
            {
                var serviceModel = model as IServiceMetadata;
                if (serviceModel != null)
                {
                    var paths = MineAliasesFromConfiguration(serviceModel.Configuration);
                    paths.AddRange(serviceModel.Content.Select(item => item.Value as string).ToList());

                    if (serviceModel.HierarchicalData != null)
                    {
                        serviceModel.HierarchicalData.ForEach(item => paths.AddRange(MineAliasesFromHierarchicalData(item.Value.Root).ToList()));
                    }

                    return GetContent(paths.Where(item => item != null));
                }
            }

            return new List<SitecoreContentItem>();
        }

        private static IEnumerable<SitecoreContentItem> GetContent(IEnumerable<string> paths)
        {
            try
            {
                var serviceHandlerPath = ServiceHandlerPath + (string.IsNullOrWhiteSpace(TargetDb) ? string.Empty : "?target=" + TargetDb);
                var processedPaths = paths
                            .Select(path =>
                                path.StartsWith(RootFragment, StringComparison.OrdinalIgnoreCase) ?
                                path : (DefaultPathFragment + path));
                using (var client = new WebClient())
                {
                    var bytes = client.UploadValues(serviceHandlerPath, new NameValueCollection
                        {
                            {
                                "paths", processedPaths.Aggregate((agg, curr) => agg + "," + curr)
                            }, { "language", Language }
                        });

                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SitecoreContentItem>>(Encoding.ASCII.GetString(bytes));

                    // Normalizing sitecore paths to match the incoming ones
                    foreach (var item in data)
                    {
                        item.Path = item.Path.Replace(DefaultPathFragment, string.Empty, StringComparison.OrdinalIgnoreCase);
                    }

                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return new List<SitecoreContentItem>();
        }

        private static IList<string> MineAliasesFromConfiguration(IEnumerable<KeyValuePair<string, object>> configuration, IList<string> list = null)
        {
            if (list == null)
            {
                list = new List<string>();
            }

            foreach (var pair in configuration)
            {
                if (pair.Key == "CONTENT_ALIAS")
                {
                    list.Add(pair.Value.ToString());
                }
                else
                {
                    if (pair.Value is IDictionary<string, object>)
                    {
                        list.AddRange(MineAliasesFromConfiguration(pair.Value as IDictionary<string, object>, list));
                    }

                    // FOR JSON MOCK SUPPORT
                    if (pair.Value is JObject)
                    {
                        var dict = pair.Value.ToPropertyDictionary();
                        list.AddRange(MineAliasesFromConfiguration(dict, list));
                    }
                }
            }

            return list;
        }

        private static IEnumerable<string> MineAliasesFromHierarchicalData(IMetadataTreeItem data)
        {
            var aliases = new List<string>();
            if (data == null)
            {
                return aliases;
            }

            object alias;

            if (data.Properties.TryGetValue("CONTENT_ALIAS", out alias))
            {
                var res = alias as string;
                if (!string.IsNullOrWhiteSpace(res))
                {
                    aliases.Add(res);
                }

                var list = alias as List<string>;
                if (list != null && list.AnyAndNotNull())
                {
                    aliases.AddRange(list);
                }
            }

            if (data.Children.AnyAndNotNull())
            {
                var subAliases = data.Children.SelectMany(MineAliasesFromHierarchicalData);

                aliases.AddRange(subAliases);
            }

            return aliases.Distinct();
        }

        #endregion
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public class SecurityHelper
    {
        #region Public Methods and Operators

        public static ICallerContext CreateCallerContext(string clientOnecode, string participantAlias, bool isImpersonation = false)
        {
            var identity = new ClaimsIdentity(AuthenticationType.BasicAuthenticationType);

            identity.AddClaim(new Claim(ClaimTypes.Name, CallerContext.DefaultCallerContext.User.Identity.Name, ClaimValueTypes.String, MembershipServiceClaimTypes.Issuer));

            identity.AddClaim(new Claim(MembershipServiceClaimTypes.ApplicationUserIdentifier, participantAlias, ClaimValueTypes.String, MembershipServiceClaimTypes.Issuer));

            identity.AddClaim(new Claim(MembershipServiceClaimTypes.AuthenticationIdentifier, Guid.NewGuid().ToString(), ClaimValueTypes.String, MembershipServiceClaimTypes.Issuer));

            if (!string.IsNullOrEmpty(clientOnecode))
            {
                identity.AddClaim(new Claim(MembershipServiceClaimTypes.ClientCode, clientOnecode, ClaimValueTypes.String, MembershipServiceClaimTypes.Issuer));
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            var clientInfo = new ClientInfo(clientOnecode);

            var callerContext = new CallerContext(claimsPrincipal, clientInfo);
            callerContext.IsImpersonation = isImpersonation;

            Thread.CurrentPrincipal = claimsPrincipal;
            return callerContext;
        }

        #endregion
    }
}