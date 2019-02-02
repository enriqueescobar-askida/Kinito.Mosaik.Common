var ItemsToAdd = new Array();
var ItemsToAddIndex = 0;
var ItemsAdded = new Array();
var ItemsAddedIndex = 0;
var separator = "|ALPHA_SEP|";
var oReq = new XMLHttpRequest();

function showPleaseWaitDialog() {
    document.all.pleasewaitScreen.style.pixelTop = (document.body.scrollTop + 100);
    document.all.pleasewaitScreen.style.visibility = "visible";
}

function closePleaseWaitDialog() {
    document.all.pleasewaitScreen.style.visibility = "hidden";
}

function AddAllItemToDictionary() {
    // On affiche un dialogue "Please Wait..." 
    // pendant le temps de traitement des tags SPAN (qui prends 5-10 sec)
    showPleaseWaitDialog();

    // Cette passe-passe permet de mettre le GUI a jour
    window.setTimeout('AddAllItemToDictionaryPrivate()', 1);
}

function AddAllItemToDictionaryPrivate() {

    var allItems = document.getElementsByTagName("span");
    
    var goodItems = new Array();
    var goodItemIndex = 0;
    var itemList = ";";
    
    for (var i = 0; i < allItems.length; i++) 
    {
        var item = allItems[i];
        
        var innerTextTrimmed = item.innerText.replace(/^\s+|\s+$/g, '');
        
        if (item.children.length == 0
        && item.getAttribute("alphaSpan") != null
        && isVisible(item)
        && innerTextTrimmed != "" 
        && itemList.indexOf(";" + item.innerText + ";") == -1) {
            var r = new Array();

            r[0] = '<input type="checkbox" checked/>';
            r[1] = item.innerText;
            itemList += item.innerText + ";";
            goodItems[goodItemIndex] = r;
            goodItemIndex++;
        }
    }
    
    closePleaseWaitDialog();

    Jq('#DoneAddItemToDictionary').hide();

    Jq('#ExecuteAddItemToDictionary').show();

    Jq('#CancelAddItemToDictionary').show();

    var dTable = Jq('#TableAddItemtoDictionary').dataTable({
        "bJQueryUI": true,
        "bFilter": false,
        "bInfo": false,
        "bPaginate": false,
        "bScrollInfinite": false,
        "bScrollCollapse": true,
        "sScrollY": "200px",
        "bSort": false,
        "aoColumns": [{ "sWidth": "10%" }, { "sTitle": "Text", "sWidth": "90%"}],
        "bDestroy": true
    });
    
    dTable.fnClearTable();
      
    dTable.fnAddData(goodItems);
    Jq('#TableAddItemtoDictionary').css("width", "100%");

    var aTrs = dTable.fnGetNodes();

    for (var i = 0; i < aTrs.length; i++) {
        if (!Jq(aTrs[i]).hasClass('datahighlight')) {
            Jq(aTrs[i]).addClass('datahighlight');
            Jq(aTrs[i]).css("background-color", "#ffdc87");
        }
    }
    
    Jq('#TableAddItemtoDictionary tr').click(function () {
        if (Jq(this).hasClass('datahighlight')) {
            Jq(this).removeClass('datahighlight');
            Jq(this).css("background-color", "transparent");
            this.children[0].children[0].checked = false;
        }
        else {
            Jq(this).addClass('datahighlight');
            Jq(this).css("background-color", "#ffdc87");
            this.children[0].children[0].checked = true;
        }
    });

    Jq('#ExecuteAddItemToDictionary').click(function () {
        Jq.unblockUI({ fadeOut: 0 });
        Jq.blockUI({ message: Jq('<div id="counter_alpha"></div>'), fadeIn: 0 });
        Jq('#counter_alpha').html('Please wait');
        var aTrs = dTable.fnGetNodes();

        var stReturn = separator;

        ItemsToAdd = new Array();
        ItemsToAddIndex = 0;
        ItemsAdded = new Array();
        ItemsAddedIndex = 0;

        for (var i = 0; i < aTrs.length; i++) {
            if (aTrs[i].children[0].children[0].checked) {
                ItemsToAdd[ItemsToAddIndex] = aTrs[i].innerText;
                ItemsToAddIndex++;
            }
        }

        if (ItemsToAddIndex == 0) {
            Jq.unblockUI({ fadeOut: 0 });
        }
        else {
            SendTermToServer(ItemsToAdd[0]);
        }

        
        return true;
    });

    Jq('#CancelAddItemToDictionary').click(function () {
        Jq.unblockUI({ fadeOut: 0 });
        return false;
    });

    Jq('#DoneAddItemToDictionary').click(function () {
        Jq.unblockUI({ fadeOut: 0 });
        window.location.reload();
        return false;
    });

    Jq.blockUI({ message: Jq('#DivAddItemtoDictionary') });
    Jq('#TableAddItemtoDictionary').focus();
}
function isVisible(obj) {
    if (obj == document) return true;

    if (!obj) return false;
    if (!obj.parentNode) return false;
    if (obj.outerHTML == '') return false;
    if (obj.style) {
        if (obj.style.display == 'none') return false;
        if (obj.style.visibility == 'hidden') return false;
    }

    //Try the computed style in a standard way
    if (window.getComputedStyle) {
        var style = window.getComputedStyle(obj, "")
        if (style.display == 'none') return false;
        if (style.visibility == 'hidden') return false;
    }

    //Or get the computed style using IE's silly proprietary way
    var style = obj.currentStyle
    if (style) {
        if (style['display'] == 'none') return false;
        if (style['visibility'] == 'hidden') return false;
    }

    return isVisible(obj.parentNode)
    
}

function SendTermToServer(term) {
    Jq('#counter_alpha').html('Adding item ' + (ItemsAddedIndex + 1) + ' of ' +  ItemsToAddIndex + '.');
    Jq.ajax({ 
        url: window.location.protocol + '//' + window.location.hostname + window.location.pathname,
        data: ({ SPS_Trans_Code: "AddTerm", SPS_Default_Lang: CurrentLcCode, SPS_Dest_Lang: Lang, SPS_Term: EncodeToUri(term) }),
        dataType: "text",
        success: function (ItemAdded) {
            ItemsAdded[ItemsAddedIndex] = decodeURIComponent(ItemAdded).split(separator);
            if (ItemsAddedIndex == ItemsToAddIndex - 1) {
                ReloadCache();
            }
            else {
                ItemsAddedIndex++;
                SendTermToServer(ItemsToAdd[ItemsAddedIndex]);
            }
        },
        error: function () {
            Jq.unblockUI({ fadeOut: 0 });
        }
    });
}
function ReloadCache() {
    Jq('#counter_alpha').html('Refreshing cache');
    Jq.ajax({
        url: window.location.protocol + '//' + window.location.hostname + window.location.pathname,
        data: ({ SPS_Trans_Code: "SPS_Reload_Dictionary" }),
        dataType: "text",
        success: function () {
            ShowResult();
        },
        error: function () {
            Jq.unblockUI({ fadeOut: 0 });
        }
    });
}

function ShowResult() {
    

    Jq('#DoneAddItemToDictionary').show();

    Jq('#ExecuteAddItemToDictionary').hide();

    Jq('#CancelAddItemToDictionary').hide();

    var dTable = Jq('#TableAddItemtoDictionary').dataTable({
        "bJQueryUI": true,
        "bFilter": false,
        "bInfo": false,
        "bPaginate": false,
        "bScrollInfinite": false,
        "bScrollCollapse": true,
        "sScrollY": "200px",
        "bSort": false,
        "aoColumns": [{ "sTitle": "Result", "sWidth": "20%" }, { "sTitle": "Term", "sWidth": "80%"}],
        "bDestroy": true
    });

    dTable.fnClearTable();

    dTable.fnAddData(ItemsAdded);
    Jq('#TableAddItemtoDictionary').css("width", "100%");

    Jq.unblockUI({ fadeOut: 0 });

    Jq.blockUI({ message: Jq('#DivAddItemtoDictionary'), fadeIn: 0 });
    Jq('#TableAddItemtoDictionary').focus();
}

function EncodeToUri(term) {
    
    var strSample = term;
    
    strSample = strSample.replace(/^\s*((?:[\S\s]*\S)?)\s*$/, '$1');
    
    strSample = encodeURIComponent(strSample);
    
    var encodedCharacters = new Array('~', '!', '*', '(', ')', '\'');
    var decodedCharacters = new Array('%7E', '%21', '%2A', '%28', '%29', '%60');
    for (var i = 0; i < encodedCharacters.length; i++) {
        strSample =
       strSample.replace(encodedCharacters[i], decodedCharacters[i]);
    }
    return strSample;
}

function GoCompletingMode(value) {
    var today = new Date();
    var oneYear = new Date(today.getTime() + 365 * 24 * 60 * 60 * 1000);
    var url = window.location.href;
    document.cookie = "lcid=" + value + ";path=/;expires=" + oneYear.toGMTString();
    window.location.search = changeLocation(changeLocation(changeLocation(window.location.search, 'InitLanguage', Lang), 'SPS_Trans_Code', 'Completing_Dictionary_Mode_Process1'), 'SPSLanguage', CurrentLcCode);
}

