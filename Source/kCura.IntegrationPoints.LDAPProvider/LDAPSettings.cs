using System.DirectoryServices;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.LDAPProvider
{
    public class LDAPSettings
    {
        #region Constructor

        public const string FILTER_DEFAULT = "(objectClass=*)";
        public const char MULTIVALUEDELIMITER_DEFAULT = ';';
        public const int PAGESIZE_DEFAULT = 1000;
        public const int GETPROPERTIESITEMSEARCHLIMIT_DEFAULT = 100;


        public LDAPSettings()
        {
            ConnectionPath = null;
            ConnectionAuthenticationType = AuthenticationTypesEnum.Secure;// - dotNet default
            Filter = FILTER_DEFAULT;// - dotNet default
            ImportNested = false;
            //PageSize = 0;// - dotNet default
            PageSize = PAGESIZE_DEFAULT;
            SizeLimit = 0;// - dotNet default
            PropertyNamesOnly = false;// - dotNet default
            ProviderReferralChasing = ReferralChasingOption.External;
            ProviderExtendedDN = ExtendedDNEnum.None;
            MultiValueDelimiter = MULTIVALUEDELIMITER_DEFAULT;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating the node in the Active Directory Domain Services hierarchy where the search starts.
        /// If ConnectionPath is a empty (null) reference, the search root is set to the root of the domain that your server is currently using.
        /// </summary>
        public string ConnectionPath { get; set; }

        /// <summary>
        /// if true, will not check for presence of provider - "LDAP://" "WinNT://" "IIS://"
        /// </summary>
        public bool IgnorePathValidation { get; set; }

        /// <summary>
        /// One of the AuthenticationTypes values. 
        /// </summary>
        public AuthenticationTypesEnum ConnectionAuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the page size in a paged search.
        /// The maximum number of objects the server can return in a paged search. 
        /// When you do a paged search (PageSize>0), the SizeLimit is ignored. 
        /// The default is zero, which means do not do a paged search.
        /// After the server has found the number of objects that are specified by the PageSize property, it will stop searching and return the results to the client. When the client requests more data, the server will restart the search where it left off.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the maximum number of objects that the server returns in a search.
        /// The maximum number of objects that the server returns in a search. 
        /// When you do a paged search (PageSize>0), the SizeLimit is ignored. 
        /// The default value is zero, which means to use the server-determined default size limit of 1000 entries.
        /// The server stops searching after the size limit is reached and returns the results accumulated up to that point.
        /// If you set SizeLimit to a value that is larger than the server-determined default of 1000 entries, the server-determined default is used.
        /// </summary>
        public int SizeLimit { get; set; }

        /// <summary>
        /// number of items to query to retrieve property list
        /// </summary>
        public int GetPropertiesItemSearchLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the scope of the search that is observed by the server.
        /// One of the SearchScope values. The default is Subtree.
        /// </summary>
        public bool ImportNested { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the search retrieves only the names of attributes to which values have been assigned.
        /// true if the search obtains only the names of attributes to which values have been assigned; 
        /// false if the search obtains the names and values for all the requested attributes. 
        /// The default value is false.
        /// </summary>
        public bool PropertyNamesOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how referrals are chased.
        /// One of the ReferralChasingOption values. The default is External.
        /// If the root search is not specified in the naming context of the server or when the search results cross a naming context 
        /// (for example, when you have child domains and search in the parent domain), 
        /// the server sends a referral message to the client that the client can either ignore or chase.
        /// </summary>
        public ReferralChasingOption ProviderReferralChasing { get; set; }

        /// <summary>
        /// Gets or sets the LDAP display name of the distinguished name attribute to search in. Only one attribute can be used for this type of search.
        /// The LDAP display name of the attribute to perform the search against, or an empty string of no attribute scope query is set.
        /// The search is performed against the objects that are identified by the distinguished name that is specified in the attribute of the base object. 
        /// For example, if the base object is an adschema group class and the AttributeScopeQuery is set to "member," then the search will be performed against all objects that are members of the group. For more information, see the adschema "Group" class topic in the MSDN Library at http://msdn.microsoft.com/library.
        /// When the AttributeScopeQuery property is used, the SearchScope property must be set to Base. If the SearchScope property is set to any other value, setting the AttributeScopeQuery property will throw an ArgumentException.
        /// </summary>
        public string AttributeScopeQuery { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the format of the distinguished names.
        /// </summary>
        public ExtendedDNEnum ProviderExtendedDN { get; set; }


        /// <summary>
        /// Gets or sets a value indicating the Lightweight Directory Access Protocol (LDAP) format filter string.
        /// The search filter string in LDAP format, such as "(objectClass=user)". 
        /// The default is "(objectClass=*)", which retrieves all objects.
        /// For more information: http://msdn.microsoft.com/en-us/library/system.directoryservices.directorysearcher.filter(v=vs.110).aspx
        /// For more information about the LDAP search string format, see "Search Filter Syntax" in the MSDN Library at http://msdn.microsoft.com/library. 
        /// </summary>
        private string _filter;
        public string Filter
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_filter))
                {
                    _filter = FILTER_DEFAULT;
                }
                return _filter;
            }
            set { _filter = value; }
        }


        /// <summary>
        /// If set, will convert original multi-value entry into a single-value 
        /// by concatinating all values and separating them with specified delimiter 
        /// </summary>
        public char? MultiValueDelimiter { get; set; }

        #endregion

        #region Internal Properties
        [JsonIgnore]
        internal System.DirectoryServices.AuthenticationTypes AuthenticationType
        {
            get { return (System.DirectoryServices.AuthenticationTypes)this.ConnectionAuthenticationType; }
        }
        [JsonIgnore]
        internal string Path
        {
            get
            {
                string localPath = this.ConnectionPath;
                if (!this.IgnorePathValidation && !string.IsNullOrEmpty(localPath) && !localPath.Contains("://"))
                {
                    //if no source provider prefix specified in settings, default to LDAP
                    localPath = string.Format("LDAP://{0}", localPath);
                }
                return localPath;
            }
        }
        [JsonIgnore]
        internal SearchScope SearchScope
        {
            get
            {
                return this.ImportNested ? SearchScope.Subtree : SearchScope.OneLevel;
            }
        }
        [JsonIgnore]
        internal ReferralChasingOption ReferralChasing { get { return (ReferralChasingOption)this.ProviderReferralChasing; } }
        [JsonIgnore]
        internal ExtendedDN ExtendedDN { get { return (ExtendedDN)this.ProviderExtendedDN; } }
        #endregion

    }
}
