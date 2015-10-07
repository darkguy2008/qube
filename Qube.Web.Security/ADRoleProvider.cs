using System;
using System.Collections.Generic;
using System.Web.Security;
using System.DirectoryServices;
using System.Web.Configuration;

namespace Italcambio.Web.Security
{
    public class ADRoleProvider : RoleProvider
    {
        #region Not Implemented
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override string[] GetRolesForUser(string username)
        {
            try
            {
                List<string> rv = new List<string>();
                DirectoryEntry deGetCN = new DirectoryEntry(WebConfigurationManager.ConnectionStrings["LDAP"].ConnectionString);
                DirectorySearcher dsGet = new DirectorySearcher(deGetCN);
                dsGet.Filter = "(SAMAccountName=" + username + ")";
                dsGet.ServerTimeLimit = new TimeSpan(0, 0, 5);
                dsGet.ClientTimeout = new TimeSpan(0, 0, 5);
                dsGet.PropertiesToLoad.Add("memberOf");

                SearchResult sr = dsGet.FindOne();
                if (sr == null)
                    throw new ArgumentNullException("Invalid user");
                else
                {
                    for (int i = 0; i < sr.Properties["memberOf"].Count; i++)
                    {
                        string result = (string)sr.Properties["memberOf"][i];
                        int ixEqual = result.IndexOf("=", 1);
                        int ixComma = result.IndexOf(",", 1);
                        if (ixEqual == -1)
                            return null;
                        rv.Add(result.Substring(ixEqual + 1, (ixComma - ixEqual) - 1));
                    }
                    return rv.ToArray();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
