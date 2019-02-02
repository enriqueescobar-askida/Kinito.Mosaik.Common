// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Installer.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the Installer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace InstallHelper
{
    public class Installer
    {
        private Guid _solutionId = Guid.Empty;
        
        public Installer()
        {
            SolutionFilename = null;
        }

        public Installer(string wspFilename)
        {
            SolutionFilename = wspFilename;
        }

        public static bool HasInstallationPermissions
        {
            get
            {
                try
                {
                    if (SPFarm.Local.CurrentUserIsAdministrator())
                    {
                        return true;
                    }
                }
                catch (NullReferenceException)
                {
                    return false;
                }
                catch (Exception ee)
                {
                    throw new Exception(ee.Message, ee);
                }

                return false;
            }
        }

        public string SolutionFilename
        {
            get;
            set;
        }

        public Guid SolutionId
        {
            get
            {
                return _solutionId;
            }

            set
            {
                _solutionId = value;
            }
        }

        /// <summary>
        /// Gets used to tell sharepoint timer service when to perform operations requested of it
        /// </summary>
        private static DateTime Immediately
        {
            get
            {
                return DateTime.Now - TimeSpan.FromDays(1);
            }
        }

        /// <summary>
        /// returns true if the solution is already installed in the sharepoint server
        /// </summary>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if installed</returns>
        public static bool IsAlreadyInstalledById(string solutionId)
        {
            return IsAlreadyInstalled(new Guid(solutionId));
        }

        /// <summary>
        /// returns true if the solution is already installed in the sharepoint server
        /// </summary>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if installed</returns>
        public static bool IsAlreadyInstalled(Guid solutionId)
        {
            try
            {
                SPSolution sln = SPFarm.Local.Solutions[solutionId];
                if (sln != null)
                {
                    return true;
                }

                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception(ee.Message, ee);
            }
        }

        /// <summary>
        /// Does a preflight check to make sure the basic services are running
        /// </summary>
        /// <returns>An array of strings with all of the errors detected.  one string for each error</returns>
        public static string[] GetPreFlightErrors()
        {
            var list = new List<string>();

            if (!HasInstallationPermissions) list.Add("User " + Environment.UserName + " does not have installation permissions on " + Environment.MachineName);

            return list.ToArray();
        }

        /// <summary>
        /// returns true if the solution is already installed in the sharepoint server
        /// </summary>
        /// <returns>true if installed</returns>
        public bool IsAlreadyInstalled()
        {
            return IsAlreadyInstalled(SolutionId);
        }

        public bool InstallSolution()
        {
            return InstallSolution(SolutionFilename, SolutionId);
        }

        public bool InstallSolution(string solutionFile, Guid solutionId)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);
                if (solution != null && solution.JobExists)
                {
                    KillRunningJobs(solution);

                    // force add
                    solution = null;
                }

                // does not exist so add it
                if (solution == null)
                {
                    SPFarm.Local.Solutions.Add(solutionFile);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to install solution", ee);
            }

            return true;
        }

        public bool InstallSolution(string solutionFile, Guid solutionId, Guid featureId, bool upgradeDeployed)
        {
            try
            {
                var deployedWebApplications = new Collection<SPWebApplication>();
                var activatedFeatureWebApplications = new Collection<SPWebApplication>();

                SPSolution solution = LocateInFarm(solutionId);
                if (solution != null)
                {
                    if (solution.JobExists)
                        KillRunningJobs(solution);

                    if (upgradeDeployed && solution.Deployed)
                    {
                        if (solution.ContainsWebApplicationResource)
                        {
                            AddAllConfiguredWebApplications(solution, deployedWebApplications);

                            foreach (SPWebApplication deployedWebApplication in deployedWebApplications)
                            {
                                foreach (SPFeature feature in deployedWebApplication.Features)
                                {
                                    if (feature.DefinitionId.Equals(featureId))
                                    {
                                        deployedWebApplication.Features.Remove(featureId, true);

                                        activatedFeatureWebApplications.Add(deployedWebApplication);
                                        break;
                                    }
                                }
                            }

                            solution.Retract(Immediately, deployedWebApplications);
                        }
                        else
                        {
                            solution.Retract(Immediately);
                        }

                        // Wait for the retract job to finish
                        WaitForJobToFinish(solution);
                    }

                    solution.Delete();
                }

                // does not exist so add it
                solution = SPFarm.Local.Solutions.Add(solutionFile);

                // Wait for the retract job to finish
                WaitForJobToFinish(solution);

                if (deployedWebApplications.Count > 0)
                {
                    solution.Deploy(Immediately, true, deployedWebApplications, true);

                    // Wait for the retract job to finish
                    WaitForJobToFinish(solution);

                    foreach (SPWebApplication deployedWebApplication in activatedFeatureWebApplications)
                    {
                        deployedWebApplication.Features.Add(featureId, true);
                    }
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to install solution", ee);
            }

            return true;
        }

        public bool UninstallSolution()
        {
            return UninstallSolution(SolutionId);
        }

        public bool UninstallSolution(Guid solutionId)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);

                if (RetractSolution(solutionId))
                {
                    // Wait for it to end.
                    WaitForJobToFinish(solution);

                    SPFarm.Local.Solutions.Remove(solutionId);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to uninstall solution", ee);
            }

            return true;
        }

        public bool UninstallSolution(Guid solutionId, Guid featureId)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);

                if (RetractSolution(solutionId, featureId))
                {
                    // Wait for it to end.
                    WaitForJobToFinish(solution);

                    SPFarm.Local.Solutions.Remove(solutionId);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to uninstall solution", ee);
            }

            return true;
        }

        public bool DeploySolution()
        {
            return DeploySolution(SolutionFilename, SolutionId, true, true);
        }

        /// <summary>
        /// Deploys a solution .wsp file to sharepoint server
        /// </summary>
        /// <param name="solutionFile">the full path to the solution package file</param>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if it was able to deploy</returns>
        public bool DeploySolution(string solutionFile, string solutionId)
        {
            return DeploySolution(solutionFile, new Guid(solutionId), true, true);
        }

        /// <summary>
        /// Deploys a solution .wsp file to sharepoint server
        /// </summary>
        /// <param name="solutionFile">the full path to the solution package file</param>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <param name="deployToAllWebApplications">true if you want to register package with all of the WebApplications on sharepoint server</param>
        /// <param name="deployToAllContentWebApplications">true if you want to register package with all of the content WebApplications on the sharepoint server</param>
        /// <returns>true if it was able to deploy</returns>
        public bool DeploySolution(string solutionFile, Guid solutionId, bool deployToAllWebApplications, bool deployToAllContentWebApplications)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);
                if (solution != null && solution.JobExists)
                {
                    KillRunningJobs(solution);

                    // force add
                    solution = null;
                }

                // does not exist so add it
                if (solution == null)
                {
                    Trace.WriteLine("WSP package installing.");
                    solution = SPFarm.Local.Solutions.Add(solutionFile);
                }

                // check to see if this has web application stuff
                if (solution.ContainsWebApplicationResource)
                {
                    var webApplications = new Collection<SPWebApplication>();

                    // add request web applications
                    if (deployToAllWebApplications) AddAllWebApplications(webApplications);
                    if (deployToAllContentWebApplications) AddAllContentWebApplications(webApplications);

                    // try to make sure we have at least one
                    if (webApplications.Count == 0)
                    {
                        SPWebApplication app = SPWebService.AdministrationService.WebApplications.GetEnumerator().Current ??
                                               SPWebService.ContentService.WebApplications.GetEnumerator().Current;

                        if (app == null)
                        {
                            Trace.WriteLine("No web app found");
                        }
                    }

                    // deploy it
                    Trace.WriteLine("WSP package deploying to " + webApplications.Count + " webapplication(s).");
                    solution.Deploy(Immediately, true, webApplications, true);
                }
                else
                {
                    Trace.WriteLine("WSP package deploying global.");

                    // deploy it without web app stuff
                    solution.Deploy(Immediately, true, true);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to deploy solution", ee);
            }

            return true;
        }

        public bool UpgradeSolution()
        {
            return UpgradeSolution(SolutionFilename, SolutionId);
        }

        /// <summary>
        /// Upgrades a solution using a .wsp file to sharepoint server
        /// </summary>
        /// <param name="solutionFile">the full path to the solution package file</param>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if it was able to upgrade the solution</returns>
        public bool UpgradeSolution(string solutionFile, string solutionId)
        {
            return UpgradeSolution(solutionFile, new Guid(solutionId));
        }

        /// <summary>
        /// Upgrades a solution using a .wsp file to sharepoint server
        /// </summary>
        /// <param name="solutionFile">the full path to the solution package file</param>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if it was able to upgrade the solution</returns>
        public bool UpgradeSolution(string solutionFile, Guid solutionId)
        {
            try
            {
                if (string.IsNullOrEmpty(solutionFile))
                {
                    throw new Exception("No solution file specified.");
                }

                if (!File.Exists(solutionFile))
                {
                    throw new Exception("Solution file not found.");
                }

                SPSolution solution = SPFarm.Local.Solutions[solutionId];
                KillRunningJobs(solution);

                solution.Upgrade(solutionFile, Immediately);
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return DeploySolution(solutionFile, solutionId.ToString());
            }
            catch (Exception eee)
            {
                throw new Exception("Unable to upgrade solution.", eee);
            }

            return true;
        }

        public bool RetractSolution()
        {
            return RetractSolution(SolutionId);
        }

        /// <summary>
        /// Retracts a solution using a .wsp file from a sharepoint server
        /// </summary>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if it was able to retract the solution</returns>
        public bool RetractSolution(string solutionId)
        {
            return RetractSolution(solutionId);
        }

        /// <summary>
        /// Retracts a solution using a .wsp file from a sharepoint server
        /// </summary>
        /// <param name="solutionId">the SolutionId (GUID) of the solution</param>
        /// <returns>true if it was able to retract the solution</returns>
        public bool RetractSolution(Guid solutionId)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);
                if (solution == null)
                    throw new Exception("Solution currently not deployed to server.  Can not retract.");

                KillRunningJobs(solution);

                if (solution.Deployed)
                {
                    if (solution.ContainsWebApplicationResource)
                    {
                        var deployedWebApplications = new Collection<SPWebApplication>();
                        AddAllConfiguredWebApplications(solution, deployedWebApplications);
                        solution.Retract(Immediately, deployedWebApplications);
                    }
                    else
                    {
                        solution.Retract(Immediately);
                    }

                    // Wait for the retract job to finish
                    WaitForJobToFinish(solution);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to retract solution", ee);
            }

            return true;
        }

        public bool RetractSolution(Guid solutionId, Guid featureId)
        {
            try
            {
                SPSolution solution = LocateInFarm(solutionId);
                if (solution == null)
                    throw new Exception("Solution currently not deployed to server.  Can not retract.");

                KillRunningJobs(solution);

                if (solution.Deployed)
                {
                    if (solution.ContainsWebApplicationResource)
                    {
                        var deployedWebApplications = new Collection<SPWebApplication>();
                        AddAllConfiguredWebApplications(solution, deployedWebApplications);

                        foreach (SPWebApplication deployedWebApplication in deployedWebApplications)
                        {
                            foreach (SPFeature feature in deployedWebApplication.Features)
                            {
                                if (feature.DefinitionId.Equals(featureId))
                                {
                                    deployedWebApplication.Features.Remove(featureId, true);
                                    break;
                                }
                            }
                        }

                        solution.Retract(Immediately, deployedWebApplications);
                    }
                    else
                    {
                        solution.Retract(Immediately);
                    }

                    // Wait for the retract job to finish
                    WaitForJobToFinish(solution);
                }
            }
            catch (NullReferenceException)
            {
                return false;
            }
            catch (Exception ee)
            {
                throw new Exception("Unable to retract solution", ee);
            }

            return true;
        }

        /// <summary>
        /// If a job is running on the solution, this method waits it to finish.
        /// </summary>
        /// <param name="solution">solution object</param>
        private static void WaitForJobToFinish(SPSolution solution)
        {
            if (solution == null) return;

            try
            {
                while (solution.JobExists
                    && (solution.JobStatus == SPRunningJobStatus.Initialized
                    || solution.JobStatus == SPRunningJobStatus.Scheduled))
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ee)
            {
                throw new Exception("Error while waiting to finish running jobs.", ee);
            }
        }

        /// <summary>
        /// Kills previously scheduled jobs for a solution
        /// </summary>
        /// <param name="solution">solution to kill jobs on</param>
        private static void KillRunningJobs(SPSolution solution)
        {
            if (solution == null) return;

            try
            {
                if (solution.JobExists)
                {
                    // is the job already running
                    if (solution.JobStatus == SPRunningJobStatus.Initialized)
                    {
                        throw new Exception("A deployment job already running for this solution.");
                    }

                    // find the running job
                    SPJobDefinition definition = null;
                    foreach (SPJobDefinition jobdefs in SPFarm.Local.TimerService.JobDefinitions)
                    {
                        if ((jobdefs.Title != null) && jobdefs.Title.Contains(solution.Name))
                        {
                            definition = jobdefs;
                            break;
                        }
                    }

                    if (definition != null)
                    {
                        definition.Delete();    // kill if it was found
                        Thread.Sleep(1000);     // give it time to delete
                    }
                }
            }
            catch (Exception ee)
            {
                throw new Exception("Error while trying to kill running jobs.", ee);
            }
        }

        /// <summary>
        /// gets all of the currently configured applications for a solution
        /// </summary>
        /// <param name="solution">solution get get configured applications for</param>
        /// <param name="applications">the collection to add it to</param>
        private static void AddAllConfiguredWebApplications(SPSolution solution, Collection<SPWebApplication> applications)
        {
            foreach (SPWebApplication app in solution.DeployedWebApplications)
            {
                applications.Add(app);
            }
        }

        /// <summary>
        /// gets all of the currently configured applications on the current sharepoint server
        /// </summary>
        /// <param name="applications">the collection to add it to</param>
        private static void AddAllWebApplications(Collection<SPWebApplication> applications)
        {
            foreach (SPWebApplication app1 in SPWebService.AdministrationService.WebApplications)
                applications.Add(app1);
        }

        /// <summary>
        ///  gets all of the currently configured applications on the current sharepoint content server
        /// </summary>
        /// <param name="applications">the collection to add it to</param>
        private static void AddAllContentWebApplications(Collection<SPWebApplication> applications)
        {
            foreach (SPWebApplication app2 in SPWebService.ContentService.WebApplications)
                applications.Add(app2);
        }

        /// <summary>
        /// Finds a solution in the SharePoint farm
        /// created because it sometimes would lie to me when passing it a solution GUID, wierd
        /// </summary>
        /// <param name="solutionId">the SolutionID (GUID) of the solution to </param>
        /// <returns>returns the soltion if found, and null if not found</returns>
        private static SPSolution LocateInFarm(Guid solutionId)
        {
            string id = solutionId.ToString();

            // move through all of the solutions we can find and see if 
            // we can locate the one we are looking for
            foreach (SPSolution sol in SPFarm.Local.Solutions)
            {
                if (string.Equals(sol.SolutionId.ToString(), id, StringComparison.InvariantCultureIgnoreCase))
                    return sol;
            }

            return null;
        }
    }
}

