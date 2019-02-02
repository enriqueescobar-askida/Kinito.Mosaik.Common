// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the FieldObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Translator.Common.Library
{
    public class FieldObject
    {
        /*Keep in mind to follow the following Rules:
        HashTable Key= Field Name
        HashTable Value= clsField.Value
        */

        public FieldObject(string value, Types type)
        {
            Value = value;
            ValueType = type;
            IsRequired = false;
        }

        public FieldObject()
        {
            Value = string.Empty;
            ValueType = Types.Text;
            IsRequired = false;
        }

        public FieldObject(string value, Types type, bool required)
        {
            Value = value;
            ValueType = type;
            IsRequired = required;
        }

        public enum Types
        {
            AllDayEvent,
            Attachments,
            Boolean,
            Calculated,
            Choice,
            Computed,
            ContentTypeId,
            Counter,
            CrossProjectLink,
            Currency,
            DateTime,
            Error,
            File,
            GridChoice,
            Guid,
            Integer,
            Invalid,
            Lookup,
            MaxItems,
            ModStat,
            MultiChoice,
            Note,
            Number,
            PageSeparator,
            Recurrence,
            Text,
            ThreadIndex,
            Threading,
            Url,
            User,
            WorkflowEventType,
            WorkflowStatus
        }
      
        public string Value { get; set; }

        public Types ValueType { get; set; }

        public bool IsRequired { get; set; }
    }
}