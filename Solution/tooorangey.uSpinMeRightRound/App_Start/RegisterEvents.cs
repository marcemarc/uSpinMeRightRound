using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Trees;

namespace tooorangey.uSpinMeRightRound.App_Start
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarting(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //register custom menu item in the media tree
            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        }

        private void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (sender.TreeAlias == "media")
            {
                var umbracoHelper = new UmbracoHelper(UmbracoContext.Current);
                var nodeId = e.NodeId;
                var mediaItem = umbracoHelper.TypedMedia(nodeId);
                if (mediaItem!= null && mediaItem.DocumentTypeAlias == "Image")
                {
                    var rotateMenuItem = new Umbraco.Web.Models.Trees.MenuItem("rotateImage", "Rotate");
                    rotateMenuItem.Icon = "axis-rotation";
                    rotateMenuItem.SeperatorBefore = true;
                    rotateMenuItem.AdditionalData.Add("actionView", "/app_plugins/tooorangey.uSpinMeRightRound/selectrotation.html");
                    e.Menu.Items.Insert(4, rotateMenuItem);
                }
            }
        }
    }
}
