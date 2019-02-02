using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Text;

namespace Alphamosaik.Translator.ApplicationFeatures.Layouts.Alphamosaik.Translator.Pages
{
    public partial class ManageTranslatorSettings : LayoutsPageBase
    {
        private Guid listId;
        private SPList selectedList;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["listId"] != null)
            {
                try
                {
                    if (ViewState["listId"] != null)
                    {
                        listId = (Guid)ViewState["listId"];
                    }
                    else
                    {
                        listId = new Guid(Request.QueryString["listId"]);
                        ViewState["listId"] = listId;
                    }

                    if (listId != null)
                    {
                        try
                        {
                            selectedList = SPContext.Current.Web.Lists[listId];
                        }
                        catch (Exception)
                        {
                        }

                        if (selectedList == null)
                        {
                            foreach (SPWeb spWeb in SPContext.Current.Web.Webs)
                            {
                                try
                                {
                                    selectedList = spWeb.Lists[listId];
                                }
                                catch (Exception)
                                {
                                }
                                finally
                                {
                                    if (spWeb != null)
                                        spWeb.Dispose();
                                }

                                if (selectedList != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    RadioButtonTranslatorDisabled.Disabled = true;
                    RadioButtonTranslatorEnabled.Disabled = true;

                    // TODO : gestion erreur
                }
            }
        }

        protected override void CreateChildControls()
        {
            // Label
            if (selectedList != null)
            {
                bool isTranslationEnabled = IsTranslationEnabled(selectedList);
                if (isTranslationEnabled)
                {
                    RadioButtonTranslatorEnabled.Checked = true;
                }
                else
                {
                    RadioButtonTranslatorDisabled.Checked = true;
                }

                if (!IsTranslationEnablingAllowed(selectedList))
                {
                    RadioButtonTranslatorDisabled.Disabled = true;
                    RadioButtonTranslatorEnabled.Disabled = true;
                }

                // Enregistrement des scripts
                StringBuilder globalVars = new StringBuilder();
                globalVars.Append("var radioButtonEnabledId = '" + RadioButtonTranslatorEnabled.ClientID + "';");
                globalVars.Append("var itemTradFromListEnabled = " + (isTranslationEnabled ? "true" : "false") + ";");

                if (!ClientScript.IsStartupScriptRegistered("GlobalVars"))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "GlobalVars", globalVars.ToString(), true);
                }
            }
            base.CreateChildControls();

        }

        private bool IsTranslationEnabled(SPList list)
        {
            if (list.Fields.ContainsField("SharePoint_Item_Language"))
            {
                return true;
            }
            else
            {
                return false;
            }

            // TODO : modifier cette méthode NE PAS LAISSER COMME CA
            // Il faut faire un nouveau projet Library et y mettre un Helper
            // Ce Helper doit être public et fournir des méthodes simples pour accéder aux paramètres généraux du module.
            // Il doit également permettre de récupérer des informations publiques du style de cette méthode de test de l'activation du module sur une liste.

            // Cette méthode en question doit être plus complète
            // Elle doit tester TOUS les champs afin de détecter les listes corrompues.
            // Dans le cas d'une liste corrompue elle doit retourner une exception pour que le code appelant puisse éventuellement la réparer.
            // La méthode doit vérifier que TOUTES les fonctionnalités sont opérationnelles
        }

        private bool IsTranslationEnablingAllowed(SPList list)
        {
            if ((list.Title == "TranslationContents") || (list.Title == "PagesTranslations") || (list.Title == "LoadBalancingServers") || (list.Title == "LanguagesVisibility")
                || (list.Title == "Configuration Store") || (list.Title == "Troubleshooting Store") || (list.Title == "TranslationContentsSub"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}

