﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace kCura.IntegrationPoints.Email.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("kCura.IntegrationPoints.Email.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to retrieve email notification configuration. Please verify kCura.Notification&apos;s settings to enable emailing functionality..
        /// </summary>
        public static string Invalid_SMTP_Settings {
            get {
                return ResourceManager.GetString("Invalid_SMTP_Settings", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SMTP port was not specified..
        /// </summary>
        public static string SMTP_Port_Missing {
            get {
                return ResourceManager.GetString("SMTP_Port_Missing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to UseSsl SMTP config value was not specified..
        /// </summary>
        public static string SMTP_Requires_IsSSL {
            get {
                return ResourceManager.GetString("SMTP_Requires_IsSSL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SMTP host was not specified..
        /// </summary>
        public static string SMTP_Requires_SMTP_Domain {
            get {
                return ResourceManager.GetString("SMTP_Requires_SMTP_Domain", resourceCulture);
            }
        }
    }
}
