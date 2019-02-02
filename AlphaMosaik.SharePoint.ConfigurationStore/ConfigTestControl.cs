namespace AlphaMosaik.SharePoint.ConfigurationStore
{
	using System.Collections.Generic;
	using System.Web.UI;

	/// <summary>
	/// Control to show usage of config store solution. Illustrates calling both an individual value and multiple 
	/// values.
	/// </summary>
	[ToolboxData("<{0}:ConfigTestControl  runat=server></{0}:ConfigTestControl>")]
	public class ConfigTestControl : Control
	{
		public ConfigTestControl() { }

		protected override void Render(HtmlTextWriter writer)
		{
			writer.Write("<br />1. Calling GetValue():<br /><br />");

			try
			{
				string sAdminEmail = ConfigStore.GetValue("MyApplication", "AdminEmail");
				writer.Write(string.Format("Retrieved '{0}' for AdminEmail <br />", sAdminEmail));
			}
			catch (InvalidConfigurationException e)
			{
				writer.Write(string.Format("Exception occurred : {0}", e));
			}

			writer.Write("<br /><br />2. Now calling GetMultipleValues():<br /><br />");

			List<ConfigIdentifier> configIds = new List<ConfigIdentifier>();
			ConfigIdentifier adminEmail = new ConfigIdentifier("MyApplication", "AdminEmail");
			ConfigIdentifier workflowEmails = new ConfigIdentifier("MyApplication", "SendWorkflowEmails");

			configIds.Add(adminEmail);
			configIds.Add(workflowEmails);
			try
			{
				Dictionary<ConfigIdentifier, string> configItems = ConfigStore.GetMultipleValues(configIds);

				string sAdminEmails = ConfigStoreHelper.ReadDictionaryValue(configItems, adminEmail, string.Empty);
				string sWorkflowEmails = ConfigStoreHelper.ReadDictionaryValue(configItems, workflowEmails, string.Empty);

				writer.Write(string.Format("Retrieved '{0}' for AdminEmail <br />", sAdminEmails));
				writer.Write(string.Format("Retrieved '{0}' for SendWorkflowEmails <br />", sWorkflowEmails));
			}
			catch (InvalidConfigurationException e)
			{
				writer.Write(string.Format("Exception occurred: {0}", e));
			}
		}
	}
}
