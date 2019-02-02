// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OceanikCQWP.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the OceanikCQWP type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint.Publishing.WebControls;
using Microsoft.SharePoint.WebPartPages;
using Translator.Common.Library;

namespace Oceanik.CQWP.WebPart
{
    using System.Collections;
    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Text;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Publishing;
    using Microsoft.SharePoint.Utilities;

    [ToolboxItemAttribute(false)]
    public class OceanikCQWP : ContentByQueryWebPart
    {
        private static string _audienceFieldColumnId = "{" + FieldId.AudienceTargeting + "}";

        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private string _currentLanguage = "EN";

        [WebBrowsable(true),
        Personalizable(PersonalizationScope.User),
        WebDescription("Specify if the items have to be filtered by language display"),
        Category("Oceanik Multilinguism"),
        WebDisplayName("Apply multilingual filtering by language display")]
        public bool IsMultilingual { get; set; }

        public override ToolPart[] GetToolParts()
        {
            var res = new List<ToolPart>(base.GetToolParts());
            res.Insert(1, new CustomPropertyToolPart { Title = "IsMultilingual" });
            return res.ToArray();
        }

        protected override System.Xml.XPath.XPathNavigator GetXPathNavigator(string viewPath)
        {
            if (this.DesignMode || !IsMultilingual)
            {
                return base.GetXPathNavigator(viewPath);
            }

            using (new SPMonitoredScope("CBQ Query and Process Data", 100, new ISPScopedPerformanceMonitor[] { new SPSqlQueryCounter(), new SPRequestUsageCounter(), new SPExecutionTimeCounter() }))
            {
                Type contentByQueryWebPart = typeof(ContentByQueryWebPart);

                this.IssueQuery();

                MethodInfo addContentQueryDataMethod = contentByQueryWebPart.GetMethod("AddContentQueryData",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);
                addContentQueryDataMethod.Invoke(this, null);

                Assembly design = Assembly.Load("Microsoft.SharePoint.Publishing, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
                Type designHost = design.GetType("Microsoft.SharePoint.Publishing.StringDataTableXPathNavigator");

                object[] parameters = { this.Data };

                FieldInfo field = contentByQueryWebPart.GetField("navigator", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(this, Activator.CreateInstance(designHost, parameters));
                    return (System.Xml.XPath.XPathNavigator)field.GetValue(this);
                }

                return base.GetXPathNavigator(viewPath);
            }
        }

        protected override void CreateChildControls()
        {
            if (IsMultilingual)
            {
                try
                {
                    try
                    {
                        SetLanguage();
                    }
                    catch (FileNotFoundException e)
                    {
                        Controls.Add(new LiteralControl("This Content Query Web Part Oceanik is usable only with the SharePoint multilingual module Oceanik installed on your site - " + e.Message));
                    }
                }
                catch (Exception e)
                {
                    Utilities.LogException(e.Message);
                }
            }

            base.CreateChildControls();

            EnableViewState = false;
        }

        private void SetLanguage()
        {
            if (HttpContext.Current != null)
                _currentLanguage = Utilities.GetLanguageCode(HttpContext.Current);
        }

        private void IssueQuery()
        {
            Type contentByQueryWebPart = typeof(ContentByQueryWebPart);
            FieldInfo fieldResults = contentByQueryWebPart.GetField("results", BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldResults != null)
            {
                if (fieldResults.GetValue(this) == null)
                {
                    MethodInfo isConfiguredMethod = contentByQueryWebPart.GetMethod("IsConfigured",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                    if (!(bool)isConfiguredMethod.Invoke(this, null))
                    {
                        var table = new DataTable
                            {
                                Locale = CultureInfo.InvariantCulture
                            };

                        fieldResults.SetValue(this, new SiteDataResults(table, false));
                    }
                    else
                    {
                        PropertyInfo errorTextInfo = contentByQueryWebPart.GetProperty("ErrorText",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

                        try
                        {
                            CrossListQueryInfo queryCacheInfo = this.BuildCrossListQueryInfo();
                            string logInfo = Resources.GetString("CbqLogWebPartTitle") + this.Title;
                            fieldResults.SetValue(this, new CrossListQueryCache(queryCacheInfo, logInfo).GetSiteDataResults(SPContext.Current.Site, true));
                            if (this.ProcessDataDelegate != null)
                            {
                                this.Data = this.ProcessDataDelegate(this.Data);
                            }

                            PropertyInfo isEditModeInfo = contentByQueryWebPart.GetProperty("isEditMode",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

                            if ((((SiteDataResults)fieldResults.GetValue(this)).Partial && (bool)isEditModeInfo.GetValue(this, null)) && SPContext.Current.Web.DoesUserHavePermissions(SPBasePermissions.EmptyMask | SPBasePermissions.AddAndCustomizePages))
                            {
                                Assembly server = Assembly.Load("Microsoft.Office.Server, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c");
                                Type indexedListQueryExecutionContextType = server.GetType("Microsoft.Office.Server.Utilities.IndexedListQueryExecutionContext");

                                object indexedListQueryExecutionContextObject = Activator.CreateInstance(indexedListQueryExecutionContextType, null);

                                MethodInfo createSimplePartialResutlsWarningControlMethod = indexedListQueryExecutionContextType.GetMethod("CreateSimplePartialResutlsWarningControl",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                                this.Controls.Add((Control)createSimplePartialResutlsWarningControlMethod.Invoke(indexedListQueryExecutionContextObject, null));
                            }
                        }
                        catch (SPQueryThrottledException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorThrottled"), null);
                        }
                        catch (WebPartPageUserException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorMalformedQuery"), null);
                        }
                        catch (SPException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorMalformedQuery"), null);
                        }
                        catch (ArgumentException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorMalformedQuery"), null);
                        }
                        catch (InvalidOperationException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorMalformedQuery"), null);
                        }
                        catch (FileNotFoundException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorMalformedQuery"), null);
                        }
                        catch (SqlException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorBackend"), null);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            errorTextInfo.SetValue(this, Resources.GetString("CbqErrorBackend"), null);
                        }
                        finally
                        {
                            if (fieldResults.GetValue(this) == null)
                            {
                                var table2 = new DataTable
                                    {
                                        Locale = CultureInfo.InvariantCulture
                                    };

                                fieldResults.SetValue(this, new SiteDataResults(table2, true));
                            }
                        }
                    }
                }
            }
        }

        private CrossListQueryInfo BuildCrossListQueryInfo()
        {
            var info = new CrossListQueryInfo
            {
                FilterByAudience = this.FilterByAudience,
                GroupByAudience = this.GroupBy == _audienceFieldColumnId,
                GroupByAscending = this.GroupByDirection == SortDirection.Asc,
                ShowUntargetedItems = this.ShowUntargetedItems,
                UseCache = this.UseCache
            };

            Type crossListQueryInfoType = typeof(CrossListQueryInfo);

            PropertyInfo requestThrottleOverrideInfo = crossListQueryInfoType.GetProperty("RequestThrottleOverride",
                                                                     BindingFlags.NonPublic |
                                                                     BindingFlags.Instance);

            requestThrottleOverrideInfo.SetValue(info, this.UseCache, null);

            if (this.ItemLimit >= 0)
            {
                info.RowLimit = !this.FilterByAudience ? ((uint)this.ItemLimit) : ((uint)(this.ItemLimit * 5));
            }
            else
            {
                info.RowLimit = uint.MaxValue;
            }

            Type contentByQueryWebPart = typeof(ContentByQueryWebPart);
            MethodInfo buildWebsElementMethod = contentByQueryWebPart.GetMethod("buildWebsElement",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            info.Webs = string.IsNullOrEmpty(this.WebsOverride) ? (string)buildWebsElementMethod.Invoke(this, null) : this.WebsOverride;

            if (string.IsNullOrEmpty(this.ListsOverride))
            {
                MethodInfo buildListsElementMethod = contentByQueryWebPart.GetMethod("buildListsElement",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                info.Lists = (string)buildListsElementMethod.Invoke(this, null);
            }
            else
            {
                MethodInfo getListIdMethod = contentByQueryWebPart.GetMethod("getListId",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                string str = string.Format(CultureInfo.InvariantCulture, this.ListsOverride, new[] { getListIdMethod.Invoke(this, null) });
                info.Lists = str;
            }

            info.Query = string.IsNullOrEmpty(this.QueryOverride) ? this.BuildQueryElements() : this.QueryOverride;

            if (string.IsNullOrEmpty(this.ViewFieldsOverride))
            {
                MethodInfo buildViewFieldsElementsMethod = contentByQueryWebPart.GetMethod("buildViewFieldsElements",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                info.ViewFields = (string)buildViewFieldsElementsMethod.Invoke(this, null);
                if (info.FilterByAudience || info.GroupByAudience)
                {
                    MethodInfo getCorrectFormatForFieldRefMethod = contentByQueryWebPart.GetMethod("getCorrectFormatForFieldRef",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                    object[] parameters = { _audienceFieldColumnId };

                    info.AudienceFieldId = (string)getCorrectFormatForFieldRefMethod.Invoke(this, parameters);
                }
            }
            else
            {
                info.ViewFields = this.ViewFieldsOverride;

                FieldInfo fieldaddedViewFields = contentByQueryWebPart.GetField("addedViewFields", BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldaddedViewFields != null)
                {
                    if (((Hashtable)fieldaddedViewFields.GetValue(this)).Count == 0)
                    {
                        MethodInfo addViewFieldsFromOverrideMethod = contentByQueryWebPart.GetMethod("AddViewFieldsFromOverride",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                        addViewFieldsFromOverrideMethod.Invoke(this, null);
                    }
                }
            }

            MethodInfo makeServerRelUrlMethod = contentByQueryWebPart.GetMethod("MakeServerRelUrl",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            object[] makeServerParameters = { this.WebUrl };

            info.WebUrl = ((string)makeServerRelUrlMethod.Invoke(this, makeServerParameters)).TrimEnd(new[] { '/' });
            return info;
        }

        private string BuildQueryElements()
        {
            var sb = new StringBuilder();
            this.BuildWhereClause(sb);

            Type contentByQueryWebPart = typeof(ContentByQueryWebPart);
            MethodInfo buildOrderByClauseMethod = contentByQueryWebPart.GetMethod("buildOrderByClause",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            object[] parameters = { sb };

            buildOrderByClauseMethod.Invoke(this, parameters);

            return sb.ToString();
        }

        private void BuildWhereClause(StringBuilder sb)
        {
            if ((!string.IsNullOrEmpty(this.ContentTypeName) || !string.IsNullOrEmpty(this.ContentTypeBeginsWithId)) || !string.IsNullOrEmpty(this.FilterField1))
            {
                string str = this.BuildChainingFilterClauses();
                if (!string.IsNullOrEmpty(str))
                {
                    sb.Append("<Where>");
                    sb.Append(str);
                    sb.Append("</Where>");
                }
            }
        }

        private string BuildChainingFilterClauses()
        {
            Type contentByQueryWebPart = typeof(ContentByQueryWebPart);

            var builder = new StringBuilder();
            StringBuilder builder2;
            StringBuilder builder4 = null;
            StringBuilder builder5 = null;

            MethodInfo fixO12ContentTypeFilteringMethod = contentByQueryWebPart.GetMethod("fixO12ContentTypeFiltering",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            fixO12ContentTypeFilteringMethod.Invoke(this, null);

            MethodInfo buildFilterClauseMethod = contentByQueryWebPart.GetMethod("buildFilterClause",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

            if (!string.IsNullOrEmpty(this.ContentTypeBeginsWithId))
            {
                object[] parameters = { "ContentTypeId", FilterFieldQueryOperator.BeginsWith, this.ContentTypeBeginsWithId, "ContentTypeId", false, false };

                builder = (StringBuilder)buildFilterClauseMethod.Invoke(this, parameters);
            }
            else if (!string.IsNullOrEmpty(this.ContentTypeName))
            {
                object[] parameters = { "ContentType", FilterFieldQueryOperator.Eq, this.ContentTypeName, "Text", false, false };
                builder = (StringBuilder)buildFilterClauseMethod.Invoke(this, parameters);
            }

            if (!string.IsNullOrEmpty(this.FilterField1))
            {
                StringBuilder builder3;
                try
                {
                    object[] parameters = { this.FilterField1, this.FilterOperator1, this.FilterValue1, this.FilterType1, this.Filter1IsCustomValue, this.FilterIncludeChildren1 };
                    builder3 = (StringBuilder)buildFilterClauseMethod.Invoke(this, parameters);
                }
                catch (ContentByQueryWebPartException)
                {
                    builder3 = null;
                }

                if (!string.IsNullOrEmpty(this.FilterField2))
                {
                    try
                    {
                        object[] parameters = { this.FilterField2, this.FilterOperator2, this.FilterValue2, this.FilterType2, this.Filter2IsCustomValue, this.FilterIncludeChildren2 };
                        builder4 = (StringBuilder)buildFilterClauseMethod.Invoke(this, parameters);
                    }
                    catch (ContentByQueryWebPartException)
                    {
                        builder4 = null;
                    }

                    if (!string.IsNullOrEmpty(this.FilterField3))
                    {
                        try
                        {
                            object[] parameters = { this.FilterField3, this.FilterOperator3, this.FilterValue3, this.FilterType3, this.Filter3IsCustomValue, this.FilterIncludeChildren3 };
                            builder5 = (StringBuilder)buildFilterClauseMethod.Invoke(this, parameters);
                        }
                        catch (ContentByQueryWebPartException)
                        {
                            builder5 = null;
                        }
                    }
                }

                object[] parametersBuilderCurrentLanguage = { "SharePoint_Item_Language", FilterFieldQueryOperator.Eq, "SPS_LNG_" + _currentLanguage, "Text", false, false };
                var builderCurrentLanguage = (StringBuilder)buildFilterClauseMethod.Invoke(this, parametersBuilderCurrentLanguage);

                object[] parametersBuilderLanguageAll = { "SharePoint_Item_Language", FilterFieldQueryOperator.Eq, "(SPS_LNG_ALL)", "Text", false, false };
                var builderLanguageAll = (StringBuilder)buildFilterClauseMethod.Invoke(this, parametersBuilderLanguageAll);

                object[] parametersBuilderLanguageEmpty = { "SharePoint_Item_Language", FilterFieldQueryOperator.Eq, string.Empty, "Text", false, false };
                var builderLanguageEmpty = (StringBuilder)buildFilterClauseMethod.Invoke(this, parametersBuilderLanguageEmpty);

                MethodInfo buildAndOrClauseMethod = contentByQueryWebPart.GetMethod("buildAndOrClause",
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.InvokeMethod);

                object[] parametersLanguage = { builderCurrentLanguage, builderLanguageAll, FilterChainingOperator.Or };
                var builderLanguage = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parametersLanguage);

                object[] parametersLanguage1 = { builderLanguage, builderLanguageEmpty, FilterChainingOperator.Or };
                builderLanguage = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parametersLanguage1);

                if (builder3 != null)
                {
                    if (builder4 != null)
                    {
                        object[] parameters = { builder3, builder4, this.Filter1ChainingOperator };
                        builder2 = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parameters);

                        if (builder5 != null)
                        {
                            object[] parameters1 = { builder2, builder5, this.Filter2ChainingOperator };
                            builder2 = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parameters1);
                        }
                    }
                    else if (builder5 != null)
                    {
                        object[] parameters1 = { builder3, builder5, this.Filter1ChainingOperator };
                        builder2 = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parameters1);
                    }
                    else
                    {
                        builder2 = builder3;
                    }
                }
                else if (builder4 != null)
                {
                    if (builder5 != null)
                    {
                        object[] parameters1 = { builder4, builder5, this.Filter2ChainingOperator };
                        builder2 = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parameters1);
                    }
                    else
                    {
                        builder2 = builder4;
                    }
                }
                else if (builder5 != null)
                {
                    builder2 = builder5;
                }
                else
                {
                    builder2 = new StringBuilder();
                }

                object[] parametersLang = { builder2, builderLanguage, FilterChainingOperator.And };
                builder2 = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parametersLang);

                if (string.IsNullOrEmpty(this.ContentTypeBeginsWithId) && string.IsNullOrEmpty(this.ContentTypeName))
                {
                    return builder2.ToString();
                }

                object[] parameters2 = { builder, builder2, FilterChainingOperator.And };
                builder = (StringBuilder)buildAndOrClauseMethod.Invoke(this, parameters2);
            }

            return builder.ToString();
        }
    }
}
