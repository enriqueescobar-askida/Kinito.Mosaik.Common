//------------------------------------------------------------------------------------------//
// PAGE COMPONENT : GESTION DU COMPORTEMENT DU BOUTON D'OUVERTURE DE POPUP MANAGE SETTINGS  //
//------------------------------------------------------------------------------------------//

Type.registerNamespace('Translator.UI');

Translator.UI.PageComponent = function () {
    Translator.UI.PageComponent.initializeBase(this);
}

Translator.UI.PageComponent.initialize = function () {
    ExecuteOrDelayUntilScriptLoaded(Function.createDelegate(null, Translator.UI.PageComponent.initializeComponent), 'SP.Ribbon.js');
}

Translator.UI.PageComponent.initializeComponent = function () {
    var ribbonManager = SP.Ribbon.PageManager.get_instance();
    if (null !== ribbonManager) {
        ribbonManager.addPageComponent(Translator.UI.PageComponent.instance);
    }
}

Translator.UI.PageComponent.refreshRibbonStatus = function () {
    SP.Ribbon.PageManager.get_instance().get_commandDispatcher().executeCommand(Commands.CommandIds.ApplicationStateChanged, null);
}

Translator.UI.PageComponent.prototype = {
    init: function () {
    },
    getId: function () {
        return 'TranslatorPageComponent'; // Non déterministe
    },
    getFocusedCommands: function () {
        return [];
    },
    getGlobalCommands: function () {
        return ['OpenDialog'];
    },
    isFocusable: function () {
        return true;
    },
    receiveFocus: function () {
        return true;
    },
    yieldFocus: function () {
        return true;
    },
    canHandleCommand: function (commandId) {
        return true;
    },
    handleCommand: function (commandId, properties, sequence) {
        if (commandId == 'OpenDialog') {

            //if (this.listGuid == undefined || this.listGuid == null) {
            //    this.GetPageLibrary();
            //}
            //else {
            Translator.UI.PageComponent.openSettingsDialog(SP.ListOperation.Selection.getSelectedList());
            //}
        }
        return true;
    }
}

Translator.UI.PageComponent.registerClass('Translator.UI.PageComponent', CUI.Page.PageComponent);
Translator.UI.PageComponent.instance = new Translator.UI.PageComponent();

NotifyScriptLoadedAndExecuteWaitingJobs('Alphamosaik.Translator.UI.js');

//-----------------------------------------------//
// GESTION DE LA MODALDIALOG ET DE SON OUVERTURE //
//-----------------------------------------------//

// Méthode d'ouverture de la fenêtre
Translator.UI.PageComponent.openSettingsDialog = function (listId) {
    var options = SP.UI.$create_DialogOptions();
    options.url = SP.Utilities.Utility.getLayoutsPageUrl('Alphamosaik.Translator/Pages/ManageTranslatorSettings.aspx?listId=' + listId); // Permet de récupérer la page layout relative au Web en cours (dans un site [serverurl]/sites/wiki on obtiendra [serverurl]/sites/wiki/_layouts/AlphaMosaik.Translator/ManageTranslatorSettings.aspx par exemple)
    options.title = 'Oceanik Settings';
    options.allowMaximize = true;
    options.showClose = true;
    options.width = 500;
    options.height = 160;
    options.dialogReturnValueCallback = Function.createDelegate(null, Translator.UI.PageComponent.settingsDialogCallBack); // Méthode de callback

    SP.UI.ModalDialog.showModalDialog(options);
}

// Méthode de callback appelée lors de la fermeture de la dialog
Translator.UI.PageComponent.settingsDialogCallBack = function (dialogResult, returnValue) {
    if (dialogResult == SP.UI.DialogResult.OK) {

        // Activation des fonctionnalités translator sur la liste
        if (returnValue == 'EnableItemTradFromList') {
            SP.UI.Notify.addNotification('Oceanik features activation in progess. Please wait during page refresh', true);

            window.location.search = '?listForItemLanguageId=' + SP.ListOperation.Selection.getSelectedList() + '&SPS_Trans_Code=EnableItemTrad';
        }

        // Désactivation des fonctionnalités translator sur la liste
        if (returnValue == 'DisableItemTradFromList') {
            SP.UI.Notify.addNotification('Oceanik features deactivation in progress. Please wait during page refresh', true);
            window.location.search = '?listForItemLanguageId=' + SP.ListOperation.Selection.getSelectedList() + '&SPS_Trans_Code=DisableItemTrad';
        }
    }
    else {
        SP.UI.Notify.addNotification('Operation cancelled');
    }
}


// Wiki Page Library Workaround
//Translator.UI.PageComponent.prototype.GetPageLibrary = function () {
//    this.ctx = SP.ClientContext.get_current();
//    this.cWeb = this.ctx.get_web();

//    this.ctx.load(this.cWeb);
//    this.ctx.executeQueryAsync(Function.createDelegate(this, this.GetPageLibrarySuceeded), Function.createDelegate(this, this.GetLibraryFailed));
//}

//Translator.UI.PageComponent.prototype.GetPageLibrarySuceeded = function () {
//    var currentFile = this.cWeb.getFileByServerRelativeUrl('/wiki/Pages/Home.aspx');
//    var currentItem = currentFile.get_listItemAllFields();
//    this.currentList = currentItem.get_parentList();

//    this.ctx.load(currentFile);
//    this.ctx.load(currentItem);
//    this.ctx.load(this.currentList);

//    this.ctx.executeQueryAsync(Function.createDelegate(this, this.GetCurrentLibrarySuceeded), Function.createDelegate(this, this.GetCurrentLibraryFailed));
//}


//Translator.UI.PageComponent.prototype.GetCurrentLibrarySuceeded = function () {
//    this.listGuid = this.currentList.get_id();

//    Translator.UI.PageComponent.openSettingsDialog(this.listGuid.toString());
//}

//Translator.UI.PageComponent.prototype.GetCurrentLibraryFailed = function (sender, args) {
//    alert(args.get_message());
//}