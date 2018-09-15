﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PullRequestMonitor.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PullRequestMonitor.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Unable to authenticate against the server.
        ///Have you added valid credentials to Windows credential manager? .
        /// </summary>
        public static string AuthorisationErrorMessage {
            get {
                return ResourceManager.GetString("AuthorisationErrorMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Awaiting first update....
        /// </summary>
        public static string AwaitingFirstUpdateMessage {
            get {
                return ResourceManager.GetString("AwaitingFirstUpdateMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not reach the server..
        /// </summary>
        public static string CouldNotReachServerMessage {
            get {
                return ResourceManager.GetString("CouldNotReachServerMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Check that you can open the link below in a web browser..
        /// </summary>
        public static string CouldNotReachServerSuggestion {
            get {
                return ResourceManager.GetString("CouldNotReachServerSuggestion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No projects are configured..
        /// </summary>
        public static string NoProjectsMessage {
            get {
                return ResourceManager.GetString("NoProjectsMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to completed.
        /// </summary>
        public static string PullRequestCompletionActionPastTense {
            get {
                return ResourceManager.GetString("PullRequestCompletionActionPastTense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:d} unapproved
        ///{1:d} approved.
        /// </summary>
        public static string PullRequestCountTooltipFormatString {
            get {
                return ResourceManager.GetString("PullRequestCountTooltipFormatString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1 active pull request.
        /// </summary>
        public static string PullRequestCountTooltipSingular {
            get {
                return ResourceManager.GetString("PullRequestCountTooltipSingular", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to opened.
        /// </summary>
        public static string PullRequestCreationActionPastTense {
            get {
                return ResourceManager.GetString("PullRequestCreationActionPastTense", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to https://github.com/nickform/pull-request-monitor.
        /// </summary>
        public static string SquirrelUrlOrPath {
            get {
                return ResourceManager.GetString("SquirrelUrlOrPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Int32 similar to 25.
        /// </summary>
        public static int TfsServer_Timeout_Seconds {
            get {
                object obj = ResourceManager.GetObject("TfsServer_Timeout_Seconds", resourceCulture);
                return ((int)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem with the last update..
        /// </summary>
        public static string UnrecognisedErrorMessage {
            get {
                return ResourceManager.GetString("UnrecognisedErrorMessage", resourceCulture);
            }
        }
    }
}
