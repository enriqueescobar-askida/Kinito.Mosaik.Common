using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using Microsoft.TeamFoundation.Client;
using System.Net;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.ComponentModel.Design;

namespace ReviewCode
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private CommandBar commandBar;
        private CommandBars commandBars;
        private Command command;
        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                commandBars = (CommandBars)_applicationObject.CommandBars;
                try
                {
                    commandBar = (CommandBar)commandBars["Code Window"];

                    //Add a command to the Commands collection:
                    command = commands.AddNamedCommand2(_addInInstance, "ReviewCode", "ReviewCode", "Executes the command for ReviewCode", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Command command2 = commands.AddNamedCommand2(_addInInstance,"Review Code","MyaddIn2","Execute the process for review code", true,59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((command != null) && (commandBar != null))
                    {
                        //command.AddControl(toolsPopup.CommandBar, 1);
                        command.AddControl(commandBar, 1);
                    }
                    Buisness.InteractWindow.OpenPendingChangesWindow((CommandBars)_applicationObject.CommandBars);
                }
                catch (System.ArgumentException)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can 
                    //  safely ignore the exception.
                }
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            Buisness.InteractWindow.OpenPendingChangesWindow((CommandBars)_applicationObject.CommandBars);
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "ReviewCode.Connect.ReviewCode")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "ReviewCode.Connect.ReviewCode")
                {
                    handled = true;
                    Buisness.ReviewCode.CallFormCreateWorkItem(_applicationObject);
                    if (Buisness.ReviewCode.thread != null)
                    {
                        Buisness.ReviewCode.thread.Join();
                    }
                  //  WorkItemCollection queryResult = Buisness.ReviewCode.CreateQueryWorkItemCodeReviewType(_applicationObject);
                    /*
                    Buisness.ReviewCode.WorkItemID = 2058;
                    if (Buisness.ReviewCode.WorkItemID != 0)
                    {
                        //Open Pending Changes Windows
                        _applicationObject.StatusBar.Progress(true,"Perform Action",0,100);
                        Buisness.InteractWindow.OpenPendingChangesWindow((CommandBars)_applicationObject.CommandBars);
                        _applicationObject.StatusBar.Progress(true, "Perform Action", 20, 100);
                        //Get The ToolStripBar to switch view
                        System.Windows.Forms.ToolStrip toolstripLeft = Buisness.ManageWindows.GetWorkItemToolStripLeft(_applicationObject);
                        //Test Create Query
                       // WorkItemCollection queryResult = Buisness.ReviewCode.CreateQueryWorkItemCodeReviewType(_applicationObject);
                        //Open Work Item View
                        _applicationObject.StatusBar.Progress(true, "Perform Action", 40, 100);
                        Buisness.InteractWindow.OpenWorkItemView(toolstripLeft);
                        //Get DatagridView existing on this view
                        System.Windows.Forms.DataGridView datagridview = Buisness.ManageWindows.GetVisualStudioGridViewPendingChanges(_applicationObject);
                        //Get The ToolStripBar to Refresh the view
                        _applicationObject.StatusBar.Progress(true, "Perform Action", 60, 100);
                        System.Windows.Forms.ToolStrip toolstripTop = Buisness.ManageWindows.GetWorkItemToolStripTop(_applicationObject);
                        //Refreshing the view Because of the last Code Review Added
                        //Buisness.InteractWindow.RefreshWorkItemView(toolstripTop);
                        //Select The Last Code Review Added
                        _applicationObject.StatusBar.Progress(true, "Perform Action", 80, 100);
                        Buisness.InteractWindow.GetListBoxToolStripTop(toolstripTop);
                       // Buisness.InteractWindow.ModifyGridViewWorkItemTest(datagridview, Buisness.ReviewCode.WorkItemID, queryResult);
                       /*
                        Buisness.InteractWindow.ModifyGridViewWorkItem(datagridview, Buisness.ReviewCode.WorkItemID);
                        _applicationObject.StatusBar.Progress(true, "Perform Action", 100, 100);
                        //Refreshing the view for fun 
                        Buisness.InteractWindow.RefreshWorkItemView(toolstripTop);
 
                        _applicationObject.StatusBar.Progress(false);
                    }
                */
                    return;
                }
            }
        }
    }
}