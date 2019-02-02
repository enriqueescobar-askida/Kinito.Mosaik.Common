using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE80;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TeamFoundation.WorkItemTracking;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.Server;
using System.Security;
using System.Net;
using EnvDTE;
using System.Windows.Forms;
using System.Configuration;

namespace ReviewCode.Buisness
{
    public static class ReviewCode
    {
        private static DTE2 _applicationObject { get; set; }
        public static SecureString Password { get; set; }
        public static string Username { get; set; }
        private static TeamFoundationServerExt tfsExt = null;
        private static TfsConfigurationServer tfs = null;
        private static VersionControlServer vcs = null;
        private static TfsTeamProjectCollection tfsColl = null;
        private static WorkItemStore wiStore = null;
        private static DocumentService docServ = null;
        public static TeamFoundationServerExt TfsExtCall { get { return tfsExt; } }
        public static int WorkItemID { get; set; }
        private static Shelveset shelveset = null;
        private static WorkItem workItemReviewCode = null;
        private static Workspace workspace = null;
        private static PendingChange[] changes = null;
        private static bool shelvesetExist = false;
        private static Dictionary<string, string> dicoUser = null;
        private static Dictionary<string, string> dicoUserMail = null;
        private static List<string> ListProjects = null;
        public static string UserDiplayName { get; set; }
        public static List<string> ListChanges { get; set; }
        public static string WorkItemDescription { get; set; }
        public static System.Threading.Thread thread = null;
        private static Properties.Settings settings = Properties.Settings.Default;


        /// <summary>
        /// Initialize connect to tfs, if doesn't exist try to connect 
        /// launch form to create WorkItem
        /// </summary>
        /// <param name="appObject">DTE2 _applicationObject</param>
        public static void CallFormCreateWorkItem(DTE2 appObject)
        {
            _applicationObject = appObject;
            //Initialise connection whith tfs server !
            //Temp 


            tfsExt = (TeamFoundationServerExt)_applicationObject.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt");
            if ((tfsExt == null) || (tfsExt.ActiveProjectContext == null) || (tfsExt.ActiveProjectContext.DomainUri == null) || (tfsExt.ActiveProjectContext.ProjectName == null))
            {

                System.Windows.Forms.MessageBox.Show("Merci de bien vouloir vous connecter au TFS avant d'effectuer cette action", "CodeRivew Add-In");
                // Temp Test Login TFS 
                /*
                if (Password == null)
                            {
                                testFormCredential form = new testFormCredential();
                                System.Windows.Forms.DialogResult result = form.ShowDialog();
                                if (result == System.Windows.Forms.DialogResult.OK)
                                {

                                    try
                                    {
                                        // We create a creditential
                                         NetworkCredential cred = new NetworkCredential(Username, Password, "alpha");
                                         Uri uriTfs = new Uri("https://alphatfs.alphamosaik.com:8088/tfs");
                                         tfs = new TfsConfigurationServer(uriTfs, cred);

                                         tfs.Connect(Microsoft.TeamFoundation.Framework.Common.ConnectOptions.IncludeServices);
                                         tfs.Authenticate();
                                         //Microsoft.TeamFoundation.Client.ICredentialsProvider tmp = cred.
                                         TfsTeamProjectCollection collTFS = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri("https://alphatfs.alphamosaik.com:8088/tfs/tfs2008"));
                                         WorkItemStore project = collTFS.GetService<WorkItemStore>();


                            ListProjects = new List<string>();

                            foreach (Microsoft.TeamFoundation.WorkItemTracking.Client.Project proj in project.Projects)
                            {
                                ListProjects.Add(proj.Name);
                            }


                            //Recupérer le projet courant 

                            //System.Windows.Forms.MessageBox.Show("OK : Connection occured");

                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.Message);
                        }
                    }
                }
                // FIN Test Get Creditential On TFS
                */
            }
            else
            {
                FormWorkItem witem = new FormWorkItem(_applicationObject);
                witem.ShowDialog();
                if (!string.IsNullOrEmpty(UserDiplayName) && witem.DialogResult == DialogResult.OK)
                {
                    FormPendingChanges formPendingChanges = new FormPendingChanges();
                    formPendingChanges.ShowDialog();
                    if (formPendingChanges.DialogResult == DialogResult.OK)
                    {
                        thread = new System.Threading.Thread(new System.Threading.ThreadStart(CreateAWorkItem));
                        //   Buisness.ReviewCode.CreateAWorkItem(UserDiplayName);
                        thread.Start();
                    }
                }
            }
        }

        /// <summary>
        /// Get WorkItemStore  
        /// </summary>
        /// <returns>WorkItemStore</returns>
        public static WorkItemStore GetWorkItemStore()
        {
            if (tfsExt == null)
            {
                tfsExt = (Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt)_applicationObject.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt");
            }
            tfsColl = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsExt.ActiveProjectContext.DomainUri));
            wiStore = tfsColl.GetService<WorkItemStore>();

            return wiStore;
        }

        /// <summary>
        /// Get DocumentService 
        /// </summary>
        /// <returns>Document Service</returns>
        public static DocumentService GetDocumentService()
        {
            if (docServ == null)
            {
                docServ = (DocumentService)_applicationObject.GetObject("Microsoft.VisualStudio.TeamFoundation.WorkItemTracking.DocumentService");
            }
            return docServ;
        }

        /// <summary>
        /// Create Shelveset if she doesn't exist else create a new one
        /// </summary>
        /// <param name="name">Name of the Shelvet</param>
        /// <returns>Return the shelveset</returns>
        public static Shelveset CreateShelveset(string name)
        {
            if (tfsColl == null)
            {
                tfsColl = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsExt.ActiveProjectContext.DomainUri));
            }
            if (vcs == null)
            {
                vcs = (VersionControlServer)tfsColl.GetService<VersionControlServer>();
            }
            string nameShelveset = name.Replace(" ", "");

            Shelveset[] shelves = vcs.QueryShelvesets(nameShelveset, null);

            int countShelveset = shelves.Count<Shelveset>();

            if (countShelveset > 0)
            {
                shelveset = shelves[0];
                shelvesetExist = true;
                return shelveset;
            }
            else
            {
                shelveset = new Shelveset(vcs, name, workspace.OwnerName);
                shelveset.Comment = tfsExt.ActiveProjectContext.ProjectName;
                return shelveset;
            }
        }

        /// <summary>
        /// ListShelveset Permit to list all shelvet to the current Project
        /// </summary>
        /// <returns>String shelvet name and Owner</returns>
        public static string ListShelveset()
        {
            if (tfsColl == null)
            {
                tfsColl = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(tfsExt.ActiveProjectContext.DomainUri));
            }
            if (vcs == null)
            {
                vcs = (VersionControlServer)tfsColl.GetService<VersionControlServer>();
            }

            Shelveset[] shelves = vcs.QueryShelvesets(null, null);
            string temp = string.Empty;
            foreach (Shelveset shelve in shelves)
            {
                temp += shelve.Name.ToString() + " : " + shelve.OwnerName + "\n";
                foreach (WorkItemCheckinInfo wici in shelve.WorkItemInfo)
                {
                    temp += "\t" + wici.WorkItem.ToString() + " \n";
                }
            }
            return temp;
        }

        /// <summary>
        /// GetListWorkItem, Retreive list a work Item Type (bug, task,...)
        /// </summary>
        /// <returns> return a list of string of Existing Work Item Type </returns>
        public static List<string> GetListWorkItemType()
        {

            GetWorkItemStore();
            List<string> listWorkItemType = new List<string>();
            foreach (WorkItemType wit in wiStore.Projects[tfsExt.ActiveProjectContext.ProjectName].WorkItemTypes)
            {
                listWorkItemType.Add(wit.Name);
            }
            return listWorkItemType;
        }

        public static bool CheckIfProjectIsTypeCodeReview()
        {
            bool value = false;
            WorkItemTypeCollection wiTypeColl = wiStore.Projects[tfsExt.ActiveProjectContext.ProjectName].WorkItemTypes;
            foreach (WorkItemType type in wiTypeColl)
            {
                if (type.Name == "CodeReview")
                {
                    value = true;
                }
            }
            return value;
        }

        /// <summary>
        /// Creat a Work Item
        /// </summary>
        /// <param name="userDisplayName">string name of the selected item</param>
        public static void CreateAWorkItem()
        {
            // workItemReviewCode = new WorkItem(wiStore.Projects[tfsExt.ActiveProjectContext.ProjectName].WorkItemTypes[selectedItem]);
            if (CheckIfProjectIsTypeCodeReview())
            {
                workItemReviewCode = new WorkItem(wiStore.Projects[tfsExt.ActiveProjectContext.ProjectName].WorkItemTypes["CodeReview"]);

                workItemReviewCode.Title = settings.WorkItemTitle;

                //Get the project Name
                string projectName = tfsExt.ActiveProjectContext.ProjectName;
                //Get user Domain Name 
                string assignment = GetUserDomainName(UserDiplayName);

                dynamic selection = _applicationObject.ActiveDocument.Selection;
                workItemReviewCode.Description = DateTime.Now + " - Code Review envoyé par :" + UserDiplayName + " pour le projet :" + projectName;

                workItemReviewCode.Fields["System.AssignedTo"].Value = UserDiplayName;

                //Save the new WorkItem to Get it's ID
                workItemReviewCode.Save();

                //Get ID of WorkItem Code Review
                WorkItemID = workItemReviewCode.Id;

                //GetPendingChanges

                changes = SetPendingChanges(ListChanges);
                if (changes.Count<PendingChange>() > 0)
                {
                    //Check If the project name is too long to create shelvset : shelveset name can't be > 64 chars
                    string shelvesetName = string.Empty;
                    // Shelvest Don't assume special character in name like "/^@:#'~
                    DateTime dateNow = DateTime.Now;
                    string dateShelvesetParse = dateNow.Day + "-" + dateNow.Month + "-" + dateNow.Year + " " + dateNow.Hour + "h" + dateNow.Minute;
                    //Set Shelveset Name
                    shelvesetName = "CodeReview-" + projectName + "-" + dateShelvesetParse;
                    //Shelvset have a name limit of 64 Characters
                    if (shelvesetName.Length >= 64)
                    {
                        if (projectName.Length >= 36)
                        {
                            projectName = projectName.Remove(36);
                        }
                        shelvesetName = DateTime.Now + "-CR-" + projectName;
                    }

                    CreateShelveset(shelvesetName);

                    workItemReviewCode.Fields["Shelveset Name"].Value = shelvesetName;
                    workItemReviewCode.Fields["WiDescription"].Value = WorkItemDescription;

                    object[] temp = workItemReviewCode.Validate().ToArray();

                    workItemReviewCode.Save();

                    AssociateWorkItemToShelveset();
                    string currentUserName = vcs.TeamProjectCollection.AuthorizedIdentity.DisplayName;
                    string currentUserEmail = GetUserEmailAddress(vcs.TeamProjectCollection.AuthorizedIdentity.DisplayName);
                    string assignToEmail = GetUserEmailAddress(UserDiplayName);

                    string currentUserAccountADName = GetUserDomainName(vcs.TeamProjectCollection.AuthorizedIdentity.DisplayName);
                    //Generate Link to WebApp TFS sample : https://alphatfs.alphamosaik.com:8088/tfs/web/ss.aspx?ss=CodeReview-ALPHAMOSAIK - SharePoint Translator-14-7-2011 14h23;co079

                    string linkToShelveset = tfsExt.ActiveProjectContext.DomainUri + "/web/ss.aspx?ss=" + shelvesetName + ";" + currentUserAccountADName;
                    string urlToShelveset = "<a href=\"" + linkToShelveset + "\">" + linkToShelveset + "</a>";

                    SendEmail(currentUserEmail, assignToEmail, "<p>Un Code Review vous a été affecté :</p> <br /> Nom du Projet : " + projectName + " <br /> Demandeur de la review : " + currentUserName + " <br /> Description : <br />" + WorkItemDescription + "<br /> Lien vers le Shelveset : <br />" + urlToShelveset);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Aucun changement en cours sur le projet");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Le projet selectionner ne dispose pas de : \r\n Work Item de type Code Review !");
            }
        }
        /// <summary>
        /// Associate Work Item to our shelveset 
        /// </summary>
        public static void AssociateWorkItemToShelveset()
        {
            WorkItemCheckinInfo work = new WorkItemCheckinInfo(workItemReviewCode, WorkItemCheckinAction.Associate);
            work.CheckinAction = WorkItemCheckinAction.Associate;
            List<WorkItemCheckinInfo> workCollection = new List<WorkItemCheckinInfo>();

            workCollection.Add(work);

            shelveset.WorkItemInfo = workCollection.ToArray();
            if (shelvesetExist)
            {
                //TODO Ne pas remplacer le ShelveSet mais Creér un id Unique 
                workspace.Shelve(shelveset, changes, ShelvingOptions.Replace);
            }
            else
            {
                workspace.Shelve(shelveset, changes, ShelvingOptions.None);
                wiStore.SyncToCache();
                wiStore.RefreshCache();
            }
        }

        /// <summary>
        /// Retreive User List of the Project
        /// </summary>
        /// <returns>List<string> of users</string></returns>
        public static List<string> RetreiveUserList()
        {
            List<string> userList = new List<string>();
            dicoUser = new Dictionary<string, string>();
            dicoUserMail = new Dictionary<string, string>();

            //Call to Work Item Store to have access tfscoll and Get Services IGroupSecurityService 
            GetWorkItemStore();

            IGroupSecurityService groupsecurityservice = (IGroupSecurityService)tfsColl.GetService<IGroupSecurityService>();

            if (vcs == null)
            {
                vcs = (VersionControlServer)tfsColl.GetService<VersionControlServer>();
            }
            TeamProject teamProject = vcs.GetTeamProject(tfsExt.ActiveProjectContext.ProjectName);

            Identity[] appGroups = groupsecurityservice.ListApplicationGroups(teamProject.ArtifactUri.AbsoluteUri);

            //Identity idSid = groupsecurityservice.ReadIdentity()
            foreach (Identity group in appGroups)
            {
                Identity[] groupMembers = groupsecurityservice.ReadIdentities(SearchFactor.Sid, new string[] { group.Sid }, QueryMembership.Expanded);

                foreach (Identity member in groupMembers)
                {
                    if (member.Members != null)
                    {

                        foreach (string memberSid in member.Members)
                        {
                            Identity memberInfo = groupsecurityservice.ReadIdentity(SearchFactor.Sid, memberSid, QueryMembership.None);
                            if (userList.Contains(memberInfo.DisplayName))
                                continue;
                            userList.Add(memberInfo.DisplayName);
                            dicoUser.Add(memberInfo.DisplayName, memberInfo.AccountName);
                            dicoUserMail.Add(memberInfo.DisplayName, memberInfo.MailAddress);

                        }
                    }
                }
            }
            userList.Sort();
            return userList;
        }
        /// <summary>
        /// Get a User Domain name 
        /// </summary>
        /// <param name="name">User Display name </param>
        /// <returns>string user domain name</returns>
        public static string GetUserDomainName(string name)
        {
            return dicoUser[name];
        }

        /// <summary>
        /// Get A user email 
        /// </summary>
        /// <param name="name">User Display Name </param>
        /// <returns>String email address </returns>
        public static string GetUserEmailAddress(string name)
        {
            return dicoUserMail[name];
        }

        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="emailFrom">from who</param>
        /// <param name="emailTo">to </param>
        /// <param name="body">Body Message</param>
        public static void SendEmail(string emailFrom, string emailTo, string body)
        {
            try
            {
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(emailFrom, emailTo);
                message.CC.Add(emailFrom);
                message.Subject = "TFS : Code Review demandé";
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Body = body;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;

                System.Net.Mail.SmtpClient mail = new System.Net.Mail.SmtpClient(settings.SmtpHost);
                mail.Credentials = tfsColl.Credentials.GetCredential(new Uri(settings.SmtpURI), AuthenticationSchemes.Digest.ToString());
                mail.SendAsync(message, emailTo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'envoyer l'email \n\r" + ex.Message);
            }
        }
        /// <summary>
        /// List Pending changes into string list 
        /// </summary>
        /// <returns>List string of pending changes</returns>
        public static List<string> GetPendingChanges()
        {
            List<string> listPendingChanges = new List<string>();
            //Get the workspace 
            workspace = vcs.TryGetWorkspace(_applicationObject.ActiveDocument.Path);

            //Get Current Pending Changes on the project 
            changes = workspace.GetPendingChanges();

            foreach (PendingChange change in changes)
            {
                string tmp = change.ServerItem;
                if (tmp.Contains(tfsExt.ActiveProjectContext.ProjectName))
                {
                    listPendingChanges.Add(change.LocalItem);
                }
            }

            return listPendingChanges;
        }

        /// <summary>
        /// Set Pending Changes 
        /// </summary>
        /// <param name="pendingchanges">Get List Selected on form </param>
        /// <returns>List of Pending changes with selected Items</returns>
        public static PendingChange[] SetPendingChanges(List<string> pendingchanges)
        {
            List<PendingChange> listChanges = new List<PendingChange>();
            int iteration = 0;
            int nbIterationMax = pendingchanges.Count - 1;
            foreach (PendingChange change in changes)
            {
                if (pendingchanges[iteration] == change.LocalItem)
                {
                    listChanges.Add(change);
                    iteration++;
                }
                if (iteration > nbIterationMax)
                {
                    break;
                }
            }
            return listChanges.ToArray();

        }


        /// <summary>
        /// Query Code Review Item  ---- Not Use ANYMORE
        /// </summary>
        /// <param name="_appObject">DTE2 Object</param>
        /// <returns>WorkItem Collection </returns>
        public static WorkItemCollection CreateQueryWorkItemCodeReviewType(DTE2 _appObject)
        {
            _applicationObject = _appObject;
            tfsExt = (TeamFoundationServerExt)_applicationObject.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt");
            if (wiStore == null)
            {
                GetWorkItemStore();
            }
            string queryWorkItem = " SELECT [System.Id],[System.Title],[System.AssignedTo],[System.State],[ReviewCode.ShelvesetName],[System.CreatedDate]";
            queryWorkItem += " FROM WorkItems";
            queryWorkItem += " WHERE [System.TeamProject] = '" + tfsExt.ActiveProjectContext.ProjectName + "'";
            queryWorkItem += " AND [System.WorkItemType]='CodeReview'";
            queryWorkItem += " AND [System.State] <> 'Closed'";
            queryWorkItem += " ORDER BY [System.Id]";
            Query workItemQuery = new Query(wiStore, queryWorkItem);
            WorkItemCollection resultQuery = workItemQuery.RunQuery();
            return resultQuery;
        }

    }
}
