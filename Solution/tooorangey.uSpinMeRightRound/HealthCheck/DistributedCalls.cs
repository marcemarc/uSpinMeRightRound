using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Config
{
    [HealthCheck("2520a89d-02e6-4b07-8867-0602d8a9cce8", "Distributed Calls",
  Description = "Check if traditional load balancing is enabled and configured correctly",
  Group = "SEO")]
    public class DistributedCalls : Umbraco.Web.HealthCheck.HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        protected DistributedCalls(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            return new[] { CheckDistributedCallsStatus() };
        }
        private HealthCheckStatus CheckDistributedCallsStatus()
        {
            //look in umbracoSettings
            // is Distributed Calls turned off
            // if off return 'you are using flexible load balancing message'
            // if on
            // are there any server elements listed
            // if none, then return error message, please specify some servers
            // check servers, does the primary server have a servername or appid
            // if not return error message, please specify servername or appid

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