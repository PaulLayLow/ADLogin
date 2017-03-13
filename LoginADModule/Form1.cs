using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Security.Permissions;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Runtime.InteropServices;

namespace LoginADModule
{
    public partial class Form1 : Form
    {
        // Variables
        public bool authenticated = false;
        public string currentUser;

        // Used for bool to get groups
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        // Get the domain name, just in case domain name changes, didn't want to hard code
        private string GetSystemDomain()
        {
            try
            {
                return Domain.GetComputerDomain().ToString().ToLower();
            }
            catch (Exception e)
            {
                e.Message.ToString();
                return string.Empty;
            }
        }

        // Search if user/pass combo exists
        private bool GetDirectorySearcher(string usr, string pwd, string dom)
        {
            DirectoryEntry entry = new
                DirectoryEntry("LDAP://" + dom, usr, pwd);
            try
            {
                object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + usr + ")";
                search.PropertiesToLoad.Add("cn");
            }
            catch (DirectoryServicesCOMException e)
            {
                e.ToString();
                return false;
            }
            return true;
        }

        // Authenticate
        public bool GetUserInformation(string user, string pass)
        {
            if (user.Trim().Length != 0 && pass.Trim().Length != 0)
            {
                string dom = GetSystemDomain();
                if (GetDirectorySearcher(user, pass, dom))
                {
                    authenticated = true;
                    currentUser = user;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // Return Username
        public string getUsername()
        {
            return currentUser;
        }

        // Return which group they belong to
        public string getGroup()
        {
            return getAuthorizationGrps(currentUser);
        }

        // Get which group they belong to
        public string getAuthorizationGrps(string userName)
        {
            List<string> grps = new List<string>();
            string domain_name = GetSystemDomain();
            PrincipalContext context = new PrincipalContext(ContextType.Domain, domain_name); ;

            try
            {
                var currentUser = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);
                RevertToSelf();
                PrincipalSearchResult<Principal> groups = currentUser.GetGroups();
                IEnumerable<string> groupNames = groups.Select(x => x.SamAccountName);
                foreach (var name in groupNames)
                {
                    grps.Add(name.ToString());
                }
                string groupLists = string.Join(", ", grps);
                return groupLists;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return error;
            }
        }
    }
}
