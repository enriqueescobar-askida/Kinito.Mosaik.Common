using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.ApplicationPages;
using AlphaMosaik.Logger.Storage;
using Microsoft.SharePoint.Administration;
using AlphaMosaik.Logger.Administration.WebControls;
using System.Text.RegularExpressions;

namespace AlphaMosaik.Logger.Administration.ApplicationPages
{
    public class ConfigureSqlLogProviderPage : OperationsPage
    {
        protected SqlProviderSection SqlProviderSection;
        protected Label LabelErrorMessage;

        private SPWebApplication selectedWebApplication;
        private StorageManager storageManager;
        private SqlLogStorageProvider provider;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Request.QueryString.Get("WebApplicationId") != null)
            {
                Guid webAppId = new Guid(Request.QueryString.Get("WebApplicationId"));
                selectedWebApplication = SPFarm.Local.GetObject(webAppId) as SPWebApplication;

                if (selectedWebApplication != null)
                {
                    storageManager = StorageManager.Lookup(selectedWebApplication);
                    provider = (from p in storageManager.StorageProviders
                                where p.Name == "SqlLogStorageProvider"         // Le nom correspond à celui de la définition du StorageProvider
                                select p).First() as SqlLogStorageProvider;     // TODO : trouver un meilleur moyen d'accès au provider (par le type?)
                }
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (!IsPostBack)
            {
                EnsureChildControls();
                SqlProviderSection.IsProviderEnabled = provider.Enabled;
                
                // On va récupérer la connection string en passant par le ConfigurationManager et non par le provider
                // en effet si le provider est désactivé, la chaine n'est pas chargée. Par le configuration manager on est sur 
                // de toujours récupérer la connection string quelque soit l'état du provider.
                ConnectionStringSettings settings = null;
                try
                {
                    settings = Configuration.ConfManager.GetConnectionString(
                        selectedWebApplication,
                        SqlLogStorageProvider.ConnectionStringName);
                }
                catch (ArgumentException)
                {
                    SqlProviderSection.IsProviderEnabled = false;
                }
                
                if(SqlProviderSection.IsProviderEnabled && settings != null)
                {
                    string connectionString = settings.ConnectionString;
                    string serverName = string.Empty;
                    string databaseName = string.Empty;
                    string username = string.Empty;
                    string password = string.Empty;
                    bool flagValid = true;

                    Regex serverRegex = new Regex("Data Source=(?<server>.+?);");
                    Match serverMatch = serverRegex.Match(connectionString);

                    if (serverMatch.Groups["server"].Success)
                    {
                        serverName = serverMatch.Groups["server"].Value;
                    }
                    else
                    {
                        flagValid = false;
                    }

                    Regex databaseRegex = new Regex("Initial Catalog=(?<database>.+?);");
                    Match databaseMatch = databaseRegex.Match(connectionString);

                    if (databaseMatch.Groups["database"].Success)
                    {
                        databaseName = databaseMatch.Groups["database"].Value;
                    }
                    else
                    {
                        flagValid = false;
                    }

                    Regex userRegex = new Regex("User Id=(?<username>.+?);");
                    Match userMatch = userRegex.Match(connectionString);

                    if (userMatch.Groups["username"].Success)
                    {
                        username = userMatch.Groups["username"].Value;
                    }

                    Regex passwordRegex = new Regex("Password=(?<password>.+?);");
                    Match passwordMatch = passwordRegex.Match(connectionString);

                    if (passwordMatch.Groups["password"].Success)
                    {
                        password = passwordMatch.Groups["password"].Value;
                    }

                    if (username == string.Empty && password == string.Empty)
                    {
                        SqlProviderSection.Authentication = SqlProviderSection.AuthenticationMethod.Windows;
                        SqlProviderSection.Username = string.Empty;
                        SqlProviderSection.Password = string.Empty;
                    }
                    else if (username != string.Empty && password != string.Empty)
                    {
                        SqlProviderSection.Authentication = SqlProviderSection.AuthenticationMethod.Sql;
                        SqlProviderSection.Username = username;
                        SqlProviderSection.Password = password;
                    }
                    else
                    {
                        LabelErrorMessage.Text = "The connection string is invalid."; // TODO : resx
                        flagValid = false;
                    }

                    if(flagValid)
                    {
                        SqlProviderSection.DatabaseServer = serverName;
                        SqlProviderSection.DatabaseName = databaseName;
                        if(username != string.Empty)
                        {
                            SqlProviderSection.Authentication = SqlProviderSection.AuthenticationMethod.Sql;
                            SqlProviderSection.Username = username;
                            SqlProviderSection.Password = password;
                        }
                        else
                        {
                            SqlProviderSection.Authentication = SqlProviderSection.AuthenticationMethod.Windows;
                            SqlProviderSection.Username = string.Empty;
                            SqlProviderSection.Password = string.Empty;
                        }
                    }
                }
            }
        }

        protected void BtSubmitClick(object sender, EventArgs e)
        {
            ConnectionStringSettings settings = null;
            if (SqlProviderSection.Authentication == SqlProviderSection.AuthenticationMethod.Windows)
            {
                settings = BuildConnectionString(SqlProviderSection.DatabaseServer, SqlProviderSection.DatabaseName);
            }
            else if (SqlProviderSection.Authentication == SqlProviderSection.AuthenticationMethod.Sql)
            {
                settings = BuildConnectionString(SqlProviderSection.DatabaseServer, SqlProviderSection.DatabaseName, SqlProviderSection.Username, SqlProviderSection.Password);
            }

            if (settings != null)
            {
                provider.Enabled = SqlProviderSection.IsProviderEnabled;
                provider.SaveSettings(selectedWebApplication);

                Configuration.ConfManager.SetConnectionString(selectedWebApplication, settings);
                Configuration.ConfManager.SaveSettings(selectedWebApplication);
            }

            Response.Redirect("~/_admin/AlphaMosaik.Logger/Configure.aspx");
        }

        private ConnectionStringSettings BuildConnectionString(string server, string database)
        {
            ConnectionStringSettings settings = new ConnectionStringSettings();
            settings.Name = SqlLogStorageProvider.ConnectionStringName;
            settings.ProviderName = "System.Data.SqlClient";
            settings.ConnectionString = "Data Source=" + server + ";Initial Catalog=" + database + ";Integrated Security=True";

            return settings;
        }

        private ConnectionStringSettings BuildConnectionString(string server, string database, string username, string password)
        {
            ConnectionStringSettings settings = new ConnectionStringSettings();
            settings.Name = SqlLogStorageProvider.ConnectionStringName;
            settings.ProviderName = "System.Data.SqlClient";
            settings.ConnectionString = "Data Source=" + server + ";Initial Catalog=" + database + ";User Id=" + username + ";Password=" + password + ";";

            return settings;
        }
    }
}
