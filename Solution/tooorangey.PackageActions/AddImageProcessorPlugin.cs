using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using umbraco.interfaces;
using System.Xml;
using umbraco.cms.businesslogic.packager.standardPackageActions;

namespace tooorangey.PackageActions
{
    public class AddImageProcessorPlugin : IPackageAction
    {
        public string Alias()
        {
            return "tooorangey.AddImageProcessorPlugin";
        }
        public bool Execute(string packageName, System.Xml.XmlNode xmlData)
        {
            bool result = false;

            //Get attribute values
            string pluginName = XmlHelper.GetAttributeValueFromNode(xmlData, "pluginName");
            string pluginType = XmlHelper.GetAttributeValueFromNode(xmlData, "pluginType");
            string pluginEnabled = XmlHelper.GetAttributeValueFromNode(xmlData, "pluginEnabled").ToLower() == "true" ? "True" : "False";

            //Open the Image Processor Security Settings config file
            XmlDocument imageProcessorConfigFile = Umbraco.Core.XmlHelper.OpenAsXmlDocument("/config/imageprocessor/processing.config");

            //Select plugins  node from the file
            XmlNode pluginsRootNode = imageProcessorConfigFile.SelectSingleNode("//plugins");

            //check if plugin already exists
            XmlNode existingPluginNode = pluginsRootNode.SelectSingleNode("//plugin[@name = '" + pluginName + "']");

            if (existingPluginNode == null)
            {
                //Create a new plugin node 
                XmlNode pluginNode = (XmlNode)imageProcessorConfigFile.CreateElement("plugin");

                //Append addributes
                pluginNode.Attributes.Append(Umbraco.Core.XmlHelper.AddAttribute(imageProcessorConfigFile, "name", pluginName));
                pluginNode.Attributes.Append(Umbraco.Core.XmlHelper.AddAttribute(imageProcessorConfigFile, "type", pluginType));
                pluginNode.Attributes.Append(Umbraco.Core.XmlHelper.AddAttribute(imageProcessorConfigFile, "enabled", pluginEnabled));

                //Append the new plugin to the image processor processor config file
                pluginsRootNode.AppendChild(pluginNode);

                //Save the config file with the new plugin
                imageProcessorConfigFile.Save(System.Web.HttpContext.Current.Server.MapPath("/config/imageprocessor/processing.config"));
            }
            //No errors so the result is true
            result = true;

            return result;
        }
        /// <summary>
        /// Sample xml
        /// </summary>
        public System.Xml.XmlNode SampleXml()
        {
            string sample = "<Action runat=\"install\" undo=\"true/false\" alias=\"tooorangey.AddImageProcessorPlugin\" pluginName=\"Pirouette\" pluginType=\"tooorangey.uSpinMeRightRound.Processors.Pirouette, tooorangey.uSpinMeRightRound\" pluginEnabled=\"true\" ></Action>";
            return helper.parseStringToXmlNode(sample);
        }
        public bool Undo(string packageName, System.Xml.XmlNode xmlData)
        {
            bool result = false;
            //Get name to remove
            string pluginName = XmlHelper.GetAttributeValueFromNode(xmlData, "pluginName");

            //Open the image processor config file
            XmlDocument umbracoSettingsFile = Umbraco.Core.XmlHelper.OpenAsXmlDocument("/config/imageprocessor/processing.config");

            //Select plugins root node from config file
            XmlNode pluginsRootNode = umbracoSettingsFile.SelectSingleNode("//plugins");

            //Get the child node with the plugin name we want to remove
            
            XmlNode pluginNode = pluginsRootNode.SelectSingleNode("//plugin[@name = '" + pluginName + "']");

            if (pluginNode != null)
            {
                //Child node is not null, remove it
                pluginsRootNode.RemoveChild(pluginNode);

                //Save the modified configuration file
                umbracoSettingsFile.Save(System.Web.HttpContext.Current.Server.MapPath("/config/imageprocessor/processing.config"));
            }

            return result;
        }

    }
    public class XmlHelper
    {
        /// <summary>
        /// Gets the value from an attribute or returns an empty string if it wasn't specified
        /// </summary>
        public static string GetAttributeValueFromNode(XmlNode node, string attributeName)
        {
            return GetAttributeValueFromNode<string>(node, attributeName, string.Empty);
        }

        /// <summary>
        /// Gets the value from an attribute or returns defaultValue if it wasn't specified
        /// </summary>
        public static string GetAttributeValueFromNode(XmlNode node, string attributeName, string defaultValue)
        {
            return GetAttributeValueFromNode<string>(node, attributeName, defaultValue);
        }

        /// <summary>
        /// Gets the value from an attribute, if no value or empty, it returns your default value (everything converted to the right type).
        /// </summary>
        public static T GetAttributeValueFromNode<T>(XmlNode node, string attributeName, T defaultValue)
        {
            if (node.Attributes[attributeName] != null)
            {
                string result = node.Attributes[attributeName].InnerText;
                if (string.IsNullOrEmpty(result))
                    return defaultValue;

                return (T)Convert.ChangeType(result, typeof(T));
            }
            return defaultValue;
        }


    }
}
