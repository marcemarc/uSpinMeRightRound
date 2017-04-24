using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using umbraco.IO;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.Checks.Config;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using ClientDependency.Core;

using System.Net.Http;
using System.Threading;

using Umbraco.Core.Sync;
using Umbraco.Web.Mvc;

namespace tooorangey.uSpinMeRightRound.HealthCheck
{

    [HealthCheck("b2ea5e7a-0235-4c73-b0d9-80500741f252", "Scheduled Publishing",
   Description = "Check to see if the necessary config is in place to enabled Scheduled Publishing to work",
   Group = "Configuration")]
    public class ScheduledPublishingHealthCheck : Umbraco.Web.HealthCheck.HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly string _umbracoSettingsFilePath;
        public ScheduledPublishingHealthCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            // for localisation
            _textService = healthCheckContext.ApplicationContext.Services.TextService;

            //for config setting
            var filePath = "~/Config/umbracoSettings.config";
            _umbracoSettingsFilePath = Umbraco.Core.IO.IOHelper.MapPath(filePath);
           
        }
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckScheduledPublishingStatus() };
        }
        private HealthCheckStatus CheckScheduledPublishingStatus()
        {
            //look in umbracoSettings for distributed Calls setting
            //if distributed calls is on then for scheduled publishign to work
            // there must be servers configured, with the first server in the list having a servername or app id
            var configIssues = new List<string>();
            var enabledXPath = "/settings/content/distributedCall/@enable";          
            var distributedCallsOn = new ConfigurationService(_umbracoSettingsFilePath, enabledXPath).GetConfigurationValue();
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(_umbracoSettingsFilePath);
            if (distributedCallsOn.Success)
            {
                //check for servers
          
                var serversXPath = "/settings/content/distributedCall/servers/server";
                var xmlNodes = xmlDocument.SelectNodes(serversXPath);
                if (xmlNodes == null || xmlNodes.Count == 0)
                {
                    // there are no servers and stuff won't work
                }
                else
                {
                    // get primary server, first in list and look for servername
                    var primaryServerXmlNode = xmlNodes[0];
                    if (primaryServerXmlNode.Attributes == null)
                    {
                        // no attributes we have a problem
                    }
                    else
                    {
                        var serverName = primaryServerXmlNode.Attributes["serverName"] != null ? primaryServerXmlNode.Attributes["serverName"].Value : String.Empty;
                        var appId = primaryServerXmlNode.Attributes["appId"] != null ? primaryServerXmlNode.Attributes["appId"].Value : String.Empty;
                        if (String.IsNullOrWhiteSpace(appId) && String.IsNullOrWhiteSpace(serverName))
                        {
                            // server role is unknown and scheduled publising wong'work
                        }
                        else
                        {
                            // is this machine the master, scheduled publishing wont' run on a slave
                            var isCurrentServerMaster = IsCurrentServer(appId, serverName);
                            if (!isCurrentServerMaster)
                            {
                                //this server won't run scheduled publishing
                            }
                        } 
                    }
                }
                // should we check user 0 exists here ?
            }
            //read umbracoapplicationurl
            // should I be able to read this off of the ApplicationContext?
            // from config:
            var umbracoApplicationXPath = "/settings/web.routing/@umbracoApplicationUrl";
            var applicationPathNode = xmlDocument.SelectSingleNode(umbracoApplicationXPath);

            var umbracoApplication = ApplicationContext.Current; 
         //   var url = umbracoApplication.UmbracoApplicationUrl;
            // if it begins with https
            // check if umbracoUseSSL is turned on
            // check if server status is 'unknown' eg distributed calls settings
            // make a webrequest to scheduled publishing url?
            //is the UmbracoApplicationUrl set
            // is the url currently being used by UmbracoApplicationUrl(guessed at startup) beggining with https
            // if so is umbracoUseSSL turned on in web.config


            var success = true;
            var message = success
                ? _textService.Localize("healthcheck/seoRobotsCheckSuccess")
                : _textService.Localize("healthcheck/seoRobotsCheckFailed");

            var actions = new List<HealthCheckAction>();

            //if (success == false)
            //    actions.Add(new HealthCheckAction("addDefaultRobotsTxtFile", Id)
            //    // Override the "Rectify" button name and describe what this action will do
            //    {
            //        Name = _textService.Localize("healthcheck/seoRobotsRectifyButtonName"),
            //        Description = _textService.Localize("healthcheck/seoRobotsRectifyDescription")
            //    });

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }
        private static bool IsCurrentServer(string appId, string serverName)
        {
            // match by appId or computer name
            return (String.IsNullOrWhiteSpace(appId) == false && appId.Trim().InvariantEquals(HttpRuntime.AppDomainAppId))
                || (serverName.IsNullOrWhiteSpace() == false && serverName.Trim().InvariantEquals(NetworkHelper.MachineName));
        }
    }
}
