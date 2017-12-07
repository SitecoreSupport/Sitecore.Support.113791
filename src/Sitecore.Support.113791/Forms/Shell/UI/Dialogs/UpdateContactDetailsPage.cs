using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Core.Data.Helpers;
using Sitecore.Forms.Shell.UI.Dialogs;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.WFFM.Abstractions.Data;
using Sitecore.WFFM.Abstractions.Dependencies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Sitecore.Support.Forms.Shell.UI.Dialogs
{
    public class UpdateContactDetailsPage : EditorBase
    {
        private string ContactDetailsColumnHeader;
        protected PlaceHolder Content;
        private string FormFieldColumnHeader;
        protected HtmlInputHidden MappedFields;
        private const string MappingKey = "Mapping";
        protected Checkbox OverwriteContact;
        protected Groupbox UserProfileGroupbox;

        protected string GetFieldsData(string restrictedTypes = "")
        {
            IEnumerable<string> values = new FormItem(this.CurrentDatabase.GetItem(this.CurrentID, this.CurrentLanguage)).Fields.Where<IFieldItem>(delegate (IFieldItem property) {
                if (!string.IsNullOrEmpty(restrictedTypes))
                {
                    return !restrictedTypes.Contains(property.TypeID.ToString());
                }
                return true;
            }).ToDictionary<IFieldItem, string, string>(property => property.ID.ToString(), property => property.Title).Select<KeyValuePair<string, string>, string>(delegate (KeyValuePair<string, string> d) {
                string[] textArray1 = new string[5];
                textArray1[0] = "{\"id\":\"";
                char[] trimChars = new char[] { '{', '}' };
                textArray1[1] = d.Key.Trim(trimChars);
                textArray1[2] = "\",\"title\":\"";
                textArray1[3] = HttpUtility.JavaScriptStringEncode(d.Value).Replace("\"", "\\\"");
                textArray1[4] = "\"}";
                return string.Concat(textArray1);
            });
            return ("[" + string.Join(",", values) + "]");
        }

        protected override void Localize()
        {
            base.Header = DependenciesManager.ResourceManager.Localize("UPDATE_CONTACT_HEADER");
            base.Text = DependenciesManager.ResourceManager.Localize("UPDATE_CONTACT_DESCRIPTION");
            this.FormFieldColumnHeader = DependenciesManager.ResourceManager.Localize("FORM_FIELD");
            this.ContactDetailsColumnHeader = DependenciesManager.ResourceManager.Localize("CONTACT_DETAILS");
        }

        protected override void OnInit(EventArgs e)
        {
            this.MappedFields.Value = base.GetValueByKey(MappingKey);
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" if (typeof ($scw) === \"undefined\") {");
                stringBuilder.Append(" window.$scw = jQuery.noConflict(true); }");
                stringBuilder.Append(" $scw(document).ready(function () {");
                stringBuilder.Append(string.Format(" var treeData = $scw.parseJSON('{0}');", ContactFacetsHelper.GetContactFacetsXmlTree()));
                stringBuilder.Append(string.Format(" var fieldsData = $scw.parseJSON('{0}');", this.GetFieldsData(this.RestrictedFieldTypes)));
                stringBuilder.Append(string.Format(" var selectedDataValue = $scw(\"#{0}\").val();", this.MappedFields.ClientID));
                stringBuilder.Append(" var selectedData = [];");
                stringBuilder.Append(" if(selectedDataValue) {");
                stringBuilder.Append(" selectedData = $scw.parseJSON(selectedDataValue); }");
                stringBuilder.Append(" $scw(\"#treeMap\").droptreemap({");
                stringBuilder.Append(" treeData: treeData.Top,");
                stringBuilder.Append(" selected: selectedData,");
                stringBuilder.Append(" listData: fieldsData,");
                stringBuilder.Append(string.Format(" fieldsHeader: \"{0}\",", this.FormFieldColumnHeader));
                stringBuilder.Append(string.Format(" mappedKeysHeader: \"{0}\",", this.ContactDetailsColumnHeader));
                stringBuilder.Append(string.Format(" addFieldTitle: \"{0}\",", DependenciesManager.ResourceManager.Localize("ADD_FIELD")));
                stringBuilder.Append(" change: function (value) {");
                stringBuilder.Append(string.Format(" $scw(\"#{0}\").val(value);", this.MappedFields.ClientID));
                stringBuilder.Append("} });   });");
                this.Page.ClientScript.RegisterClientScriptBlock(base.GetType(), "sc_wffm_update_contact" + this.ClientID, stringBuilder.ToString(), true);
            }
        }

        protected override void SaveValues()
        {
            base.SaveValues();
            base.SetValue(MappingKey, this.MappedFields.Value);
        }

        public string RestrictedFieldTypes
        {
            get
            {
                return WebUtil.GetQueryString("RestrictedFieldTypes", "{1F09D460-200C-4C94-9673-488667FF75D1}|{1AD5CA6E-8A92-49F0-889C-D082F2849FBD}|{7FB270BE-FEFC-49C3-8CB4-947878C099E5}");
            }
        }
    }
}