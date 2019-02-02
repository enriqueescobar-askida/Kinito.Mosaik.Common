// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserObject.cs" company="AlphaMosaik">
//   Copyright (c) AlphaMosaik. All rights reserved.
// </copyright>
// <summary>
//   Defines the UserObject type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Translator.Common.Library
{
    public class UserObject
    {
        public UserObject(string userName,
                          string email,
                          string name,
                          string notes,
                          RoleDefinition roleDefinition,
                          string groupName)
        {
            UserName = userName;
            Email = email;
            Name = name;
            Notes = notes;
            Role = roleDefinition;
            GroupName = groupName;
        }

        public UserObject(string userName)
        {
            GroupName = string.Empty;
            Role = RoleDefinition.LimitedAccess;
            Notes = string.Empty;
            Name = string.Empty;
            Email = string.Empty;
            UserName = userName;
        }

        public UserObject(string userName, string email)
        {
            GroupName = string.Empty;
            Role = RoleDefinition.LimitedAccess;
            Notes = string.Empty;
            Name = string.Empty;
            UserName = userName;
            Email = email;
        }

        public UserObject(string userName, string email, string name)
        {
            GroupName = string.Empty;
            Role = RoleDefinition.LimitedAccess;
            Notes = string.Empty;
            UserName = userName;
            Email = email;
            Name = name;
        }

        public UserObject(string userName, string email, string name, string notes)
        {
            GroupName = string.Empty;
            Role = RoleDefinition.LimitedAccess;
            UserName = userName;
            Email = email;
            Name = name;
            Notes = notes;
        }

        public UserObject(string userName, string email, string name, string notes, RoleDefinition roleDefinition)
        {
            GroupName = string.Empty;
            UserName = userName;
            Email = email;
            Name = name;
            Notes = notes;
            Role = roleDefinition;
        }

        public enum RoleDefinition
        {
            FullControl,
            Design,
            ManageHierarchy,
            Approve,
            Contribute,
            Read,
            RestrictedRead,
            LimitedAccess,
            ViewOnly,
            RecordsCenterSubmissionCompletion
        }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Notes { get; set; }

        public RoleDefinition Role { get; set; }

        public string GroupName { get; set; }
    }
}