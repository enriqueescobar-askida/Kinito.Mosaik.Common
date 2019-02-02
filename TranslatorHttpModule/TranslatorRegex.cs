// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslatorRegex.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the TranslatorRegex type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace TranslatorHttpHandler
{
    public static class TranslatorRegex
    {
        public static readonly Regex WelcomeRegex = new Regex("(serverclientid=\"zz[0-9]_Menu\">)( )*([^<]*)( )*<", RegexOptions.IgnoreCase);
        public static readonly Regex CurrentUserIdRegex = new Regex("ctx.CurrentUserId = [0-9]+;", RegexOptions.IgnoreCase);
        public static readonly Regex UserIdRegex = new Regex("var _spUserId=[0-9]+;", RegexOptions.IgnoreCase);
        public static readonly Regex ImnTypeSmtpRegex = new Regex("id=('|\")imn(_|\\{)(?<id>([0-9A-Z]+(-[0-9A-Z]+)*))\\}?,type=s((mt)|(i))p('|\")", RegexOptions.IgnoreCase);  // OLD: "id='imn_(?<id>([0-9]+)),type=smtp'"
        public static readonly Regex DispExRegex = new Regex("(?<return>return DispEx\\([^,]*,)(?<param2_11>([^,]*,){10})(?<tild>')(?<userid>[0-9]+)(?<tild2>',)(?<param13_14>([^,]*,){2})(?<param15>[^\\)]*\\))", RegexOptions.IgnoreCase);
        public static readonly Regex CtxIdRegex = new Regex("(?<constant>(      ctx\\.ctxId = ))(?<ctxId>([0-9]+));", RegexOptions.IgnoreCase);
        public static readonly Regex FilteringChangeLanguageRegex = new Regex("onclick=\"window\\.location\\.search = changeLocation\\(window\\.location\\.search\\,'SPSLanguage'\\,'(?<language>([A-Z][A-Z]))'\\);\"");
        public static readonly Regex SelectedViewRegex = new Regex("serverclientid=\"zz[0-9]+_ViewSelectorMenu\">(?<viewName>([^<]+))<", RegexOptions.IgnoreCase);
        public static readonly Regex GroupByInViewAsHtmlRegex = new Regex("<TBODY id=\"titl[0-9]+\\-[0-9]+_\" ", RegexOptions.IgnoreCase);
        public static readonly Regex UrlPageToReplaceRegex = new Regex(" HREF=\"[^\"]+/\\?", RegexOptions.IgnoreCase);
        public static readonly Regex SendExtractorTranslationsToDictionaryRegex = new Regex("(<menu type='ServerMenu' )[^°]+(onMenuClick=\"window.location =)[^<]+(/_layouts/listedit.aspx)[^<]+(</ie:menuitem>)", RegexOptions.IgnoreCase);
        public static readonly Regex HasSharepointGroupLanguageFieldRegex = new Regex(@"<!-- FieldName=""SharePoint_Group_Language""", RegexOptions.IgnoreCase);
        public static readonly Regex HasAutoTranslationFieldRegex = new Regex(@"<!-- FieldName=""AutoTranslation""", RegexOptions.IgnoreCase);
        public static readonly Regex HasItemsAutoCreationFieldRegex = new Regex(@"<!-- FieldName=""ItemsAutoCreation""", RegexOptions.IgnoreCase);
        public static readonly Regex HasMetadataToDuplicateFieldRegex = new Regex(@"<!-- FieldName=""MetadataToDuplicate""", RegexOptions.IgnoreCase);
        public static readonly Regex HasSharepointItemLanguageFieldRegex = new Regex(@"<!-- FieldName=""SharePoint_Item_Language""", RegexOptions.IgnoreCase);
        public static readonly Regex HasBodyEndTagRegex = new Regex(@"</body>", RegexOptions.IgnoreCase);
        public static readonly Regex HasEmptyFieldNameRegex = new Regex(@"<!-- FieldName=""""", RegexOptions.IgnoreCase);
        public static readonly Regex BlockAjaxRegex = new Regex("(^(?<length>([0-9]+))(?<constant>(\\|updatePanel\\|[^\\|]*\\|)))" + "|" + "(\\|(?<length>([0-9]+))(?<constant>(\\|updatePanel\\|[^\\|]*\\|)))");
        public static readonly Regex QuickLaunchRegEx = new Regex("(?<MenuDiv><div id=\".*?_V4QuickLaunchMenu\" class=\".*?\">).*?(?<BeginInnerDiv><div .*?>)(?<Content>.*?)(?<EndInnerDiv></div>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public static readonly Regex QuickLaunchRegExV2 = new Regex("(?<MenuDiv><div id=\".*?_CurrentNav\"[^>]*>).*?(?<BeginInnerDiv><div .*?>)(?<Content>.*?)(?<EndInnerDiv></div>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public static readonly Regex TopNavigationBarRegex = new Regex("(?<MenuDiv><div id=\".*?_TopNavigationMenuV4\" class=\".*?\">).*?(?<BeginInnerDiv><div .*?>)(?<Content>.*?)(?<EndInnerDiv></div>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public static readonly Regex RegexOpeningDiv = new Regex("(<DIV)|(<DIV>)", RegexOptions.IgnoreCase);
        public static readonly Regex RegexClosingDiv = new Regex("</DIV>", RegexOptions.IgnoreCase);
        public static readonly Regex RegexPostDate = new Regex("(<h3 class=\"ms-PostDate\">)+[^<]+(</h3>)+", RegexOptions.IgnoreCase);
        public static readonly Regex GreaterThanSmallerThanRegex = new Regex(">[^<>]+<");
        public static readonly Regex TextRegex = new Regex("(?<title> text=| TEXT=|{Text:)\"(?<value>([^\"]+))\"");
        public static readonly Regex TitleRegex = new Regex(" (title|TITLE)=\"[^<>\"]+\"");
        public static readonly Regex AltRegex = new Regex(" (alt|ALT)=\"[^<>\"]+\"");
        public static readonly Regex DiidSort = new Regex("(<a id=\"diidSort[^>]*>)([^<]*)(<img)", RegexOptions.IgnoreCase);
        public static readonly Regex ImageSrcRect = new Regex("(<img src=\"/_layouts/images/rect.gif\")[^°]+</a>", RegexOptions.IgnoreCase);
        public static readonly Regex SpsUrlRegex = new Regex("(\\$\\$SPS_URL:)[^\\$]+(\\$\\$)");
        public static readonly Regex EndHtmlFileRegex = new Regex(@"(</html>(((\s)[^<]*)|([\s]*(<span></span>)*( )*$)))", RegexOptions.IgnoreCase);
        public static readonly Regex EndFileAjaxRegex = new Regex("(\\|hiddenField\\|ms-rtedirtybit\\|)|(\\|0\\|hiddenField\\|_wzSelected\\|\\|)", RegexOptions.IgnoreCase);
        public static readonly Regex AsciiCodeRegex = new Regex("(&#)[0-9]+;");
        public static readonly Regex TagMainRegex = new Regex(">(\\s)*(([^\\s<>]*)[^<>]+([^\\s]))((\\s)*<)");
        public static readonly Regex JavascriptAreasBeginRegex = new Regex("<script[^>]*>", RegexOptions.IgnoreCase);
        public static readonly Regex SpanRegex = new Regex("<span alphaSpan=\"true\" style=\"border:solid 1px black; color:black; background:" + "((yellow)|(#E68DC3))" + "; \" >");
        public static readonly Regex MsPickerFooterRegex = new Regex("class=\"ms-picker-footer\".*?</td>");
        public static readonly Regex StringsRegex = new Regex(@"\""Strings\"":\[\"".*?\""\]\}\}\]\)");
        public static readonly Regex ArgumentBetweenQuotesRegex = new Regex("(?<begin>(\"))(?<value>([^\"]+))(?<end>(\"))");
        public static readonly Regex LangViewRegex = new Regex("(?<urlBase>(.*))((?<lang>([A-Z][A-Z]))\\.aspx)$");
        public static readonly Regex LangViewRegex2 = new Regex("(?<urlBase>(.*))(\\.aspx)$");
        public static readonly Regex TagDateInSharepointListRegex = new Regex("<td class=\"ms-vb2\"><nobr>(?<date>(.*?))</nobr>");
        public static readonly Regex LayoutInfoRegex = new Regex(@"LayoutInfo\(\'.*?\'\)");
        public static readonly Regex NavigationNodeRegex = new Regex(@"NavigationNode\(\'.*?\'\)");
        public static readonly Regex DescriptionRegex = new Regex(" description=\"[^<>\"]+\"");
        public static readonly Regex TagTitleRegex = new Regex(" (title|Title)=( )?\"[^<>\"]+\"");
        public static readonly Regex TagAltRegex = new Regex(" (alt|ALT)=\"(?<value>([^<>\"]+))\"");
        public static readonly Regex DfwpListRegex = new Regex("<ul class=\"dfwp-list\"(.+?)</ul>");
        public static readonly Regex TypeButtonSubmitResetRegex = new Regex("(type|Type|TYPE)=\"(button|Button|BUTTON|submit|Submit|SUBMIT|reset|Reset|RESET)\"[^<>]+((value|Value|VALUE)=\")[^<>\"]+\" ");
        public static readonly Regex InnerHtmlRegex = new Regex("(.innerHTML = \")+[^(\")]+(\";)");
        public static readonly Regex InputSbPlainTagRegex = new Regex("<input [^<]*class=\"ms-sbplain\"[^<]*/>", RegexOptions.IgnoreCase);
        public static readonly Regex DayHeaderRegex = new Regex("<th scope=\"col\" class=ms-picker-dayheader nowrap><ABBR title= \"(?<day>([^\"]+))\" >&nbsp;(?<header>([^&]))&nbsp;</ABBR></th>");
        public static readonly Regex TagRolloverRegex = new Regex("(;ShowListInformation\\('[^']+'\\,')[^']+('\\,)", RegexOptions.IgnoreCase);
        public static readonly Regex ServerMenuRegex = new Regex("(<menu type='ServerMenu' )[^°]+(onMenuClick=\"javascript:LoginAsAnother)[^°]+(/_layouts/SignOut.aspx')[^<]+(</ie:menuitem>)", RegexOptions.IgnoreCase);
        public static readonly Regex EndBodyRegex = new Regex("(</body>)", RegexOptions.IgnoreCase);
        public static readonly Regex EnableItemTradFromListRegex = new Regex("(<menu type='ServerMenu' )[^°]+(onMenuClick=\"window.location =)[^<]+(/_layouts/listedit.aspx)[^<]+(</ie:menuitem>)", RegexOptions.IgnoreCase);
        public static readonly Regex AutoCompletingModeButtonRegex = new Regex(" onMenuClick=\\\"STSNavigate2\\(event,'(/[^/]+)*/_layouts/settings.aspx'\\);\\\"", RegexOptions.IgnoreCase);
        public static readonly Regex ValueInputRegex = new Regex("value=\"(?<value>([^\"]+))\"", RegexOptions.IgnoreCase);
        public static readonly Regex BlurRegex = new Regex("{this.value='(?<thisvalue>([^']+))';", RegexOptions.IgnoreCase);
        public static readonly Regex ValueButtonRegex = new Regex(" (value|Value|VALUE)=\"[^<>\"]+\"");
        public static readonly Regex ArgumentBetweenQuotes = new Regex(@"\'.*?\'");
        public static readonly Regex TagForRolloverRegex = new Regex("('\\,')[^']+'");
        public static readonly Regex DefinitionResetRegex = new Regex("function ResetPageInformation\\(\\)[^}]+document\\.getElementById\\(\"idItemHoverTitle\"\\)\\.childNodes\\[0\\]\\.nodeValue = \"(?<value>([^\"]+))\";[\\s]*document\\.getElementById\\(\"idItemHoverDescription\"\\)\\.childNodes\\[0\\]\\.nodeValue= \"(?<value2>([^\"]+))\"");
        public static readonly Regex HidDescriptionRegex = new Regex("<input type=\"hidden\" name=\"HidDescription[0-9]\" id=\"HidDescription[0-9]\" value=\"(?<value>([^\"]+))\"");
        public static readonly Regex CallBackRegex = new Regex("#(?<value>([^\\?#]+))\\?");
        public static readonly Regex PatternLiRegex = new Regex("<li class(.+?)</li>", RegexOptions.IgnoreCase);
        public static readonly Regex PatternHrefRegex = new Regex("<a.*?href=[\"'](?<url>.*?)[\"'].*?>(?<name>.*?)</a>", RegexOptions.IgnoreCase);
        public static readonly Regex MultilookupPickerRegex = new Regex(@"<input [^<]*\$MultiLookupPicker\$data[^<]*</input>");
        public static readonly Regex MultilookupPickerInitialRegex = new Regex(@"<input [^<]*\$MultiLookupPicker\$initial[^<]*</input>");
		public static readonly Regex MoveToDateRegex = new Regex(@"javascript:MoveToDate\(\'.*?\'");
    }
}
