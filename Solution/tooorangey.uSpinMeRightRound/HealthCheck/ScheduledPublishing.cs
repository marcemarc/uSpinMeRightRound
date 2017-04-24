using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck;

namespace tooorangey.uSpinMeRightRound.HealthCheck
{

    [HealthCheck("b2ea5e7a-0235-4c73-b0d9-80500741f252", "Test1",
   Description = "Test1",
   Group = "Configuration")]
    public class ScheduledPublishing : Umbraco.Web.HealthCheck.HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        protected ScheduledPublishing(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            throw new NotImplementedException();
            return new[] { CheckScheduledPublishingStatus() };
        }
        private HealthCheckStatus CheckScheduledPublishingStatus()
        {
            //look in umbracoSettings
            //read umbracoapplicationurl
            // if it begins with https
            // check if umbracoUseSSL is turned on
            // check if server status is 'unknown' eg distributed calls settings
            // make a webrequest to scheduled publishing url?

         
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

    }
}