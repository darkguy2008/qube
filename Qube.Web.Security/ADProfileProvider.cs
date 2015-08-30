using System;
using System.Web.Profile;
using System.Collections.Specialized;

namespace Italcambio.Web.Security
{
    public class ADProfileProvider : ProfileProvider
    {
        #region Not Implemented
        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
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

        public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection collection)
        {
            throw new NotImplementedException();
        }

        public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection collection)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override void Initialize(string name, NameValueCollection config)
        {
            _name = name;
        }

        private string _name;
        public override string Name
        {
            get { return _name; }
        }
    }
}
