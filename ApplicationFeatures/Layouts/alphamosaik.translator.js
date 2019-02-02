var selection, selectionText, selectionButton, newRange, urlDictionary;

function alpha_initialize(url) {
    urlDictionary = url;
    document.onclick = handleClick;
}

function handleClick() {
    if (selectionButton) {
        cleanUp();
    }
    selection = getSelectedText();
    selectionText = selection && selection.toString();
    if (selectionText) {
        window.setTimeout(insertButton, 0);
    }
}

function getSelectedText() {
    return Try.these(
      function () { return window.getSelection() },
      function () { return document.getSelection() },
      function () {
          var selection = document.selection && document.selection.createRange();
          selection.toString = function () { return this.text };
          return selection;
      }
    ) || false;
}

function encode_utf8(s) {
    return unescape(encodeURIComponent(s));
}

function insertButton() {
    if (navigator.appName == "Microsoft Internet Explorer") {
        newRange = selection.duplicate();
        newRange.setEndPoint("StartToEnd", selection);
        newRange.pasteHTML("<div id='alphamosaik_selection_button' onClick=\"window.open(urlDictionary + encode_utf8(selectionText), 'Alphamosaik_Add_To_Dictionary', config = 'height=230, width=600, toolbar=no, menubar=no, scrollbars=no, resizable=yes, location=no, directories=no, status=no')\" style='position:absolute; cursor:hand; font-size: 10pt; text-decoration:none;width:1000px' ><img src=\"/_layouts/images/alpha_logo_menu.png\" /><div id=\"Container\" style=\"position:absolute;left:32px;top:0px;z-index:10;\"><div style='color:white; border: solid 2px #990000; padding:3px; font-weight:bold; background-color:blue; position:absolute; text-align:justify;'>Populate dictionary with: </div><div style='border: solid 2px #990000; padding:3px; margin-top:20px; background-color:#E7E8F2; font-weight:normal; color:black;'>" + selectionText + "</div></div></div>");
        selectionButton = document.getElementById('alphamosaik_selection_button');
    }
}

function cleanUp() {
    selection = null;
    newRange && newRange.pasteHTML && newRange.pasteHTML('');
    newRange = null;
    selectionButton.parentNode.removeChild(document.getElementById('alphamosaik_selection_button'));
    selectionButton = null;
    selectionText = '';
}

var Try = {
    these: function () {
        var returnValue;
        for (var i = 0, length = arguments.length; i < length; i++) {
            var lambda = arguments[i];
            try {
                returnValue = lambda();
                break;
            } catch (e) { }
        }
        return returnValue;
    }
};