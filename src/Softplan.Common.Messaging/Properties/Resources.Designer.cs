﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Softplan.Common.Messaging.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Softplan.Common.Messaging.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Destination cannot be empty..
        /// </summary>
        public static string MessageDestionationIsNull {
            get {
                return ResourceManager.GetString("MessageDestionationIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MQManager already started..
        /// </summary>
        public static string MQManagerAlreadyStarted {
            get {
                return ResourceManager.GetString("MQManagerAlreadyStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while starting MQManager..
        /// </summary>
        public static string MQManagerErrorWhileStarting {
            get {
                return ResourceManager.GetString("MQManagerErrorWhileStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while stopping MQManager..
        /// </summary>
        public static string MQManagerErrorWhileStopping {
            get {
                return ResourceManager.GetString("MQManagerErrorWhileStopping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MQManager is not started..
        /// </summary>
        public static string MQManagerNotStarted {
            get {
                return ResourceManager.GetString("MQManagerNotStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MQManager started..
        /// </summary>
        public static string MQManagerStarted {
            get {
                return ResourceManager.GetString("MQManagerStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starting MQManager..
        /// </summary>
        public static string MQManagerStarting {
            get {
                return ResourceManager.GetString("MQManagerStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MQManager stopped..
        /// </summary>
        public static string MQManagerStopped {
            get {
                return ResourceManager.GetString("MQManagerStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Stopping MQManager..
        /// </summary>
        public static string MQManagerStopping {
            get {
                return ResourceManager.GetString("MQManagerStopping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processor {0} is already registered..
        /// </summary>
        public static string ProcessorAlreadyResgistered {
            get {
                return ResourceManager.GetString("ProcessorAlreadyResgistered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not create a instance of {0} processor..
        /// </summary>
        public static string ProcessorInstanceCouldNotBeCreated {
            get {
                return ResourceManager.GetString("ProcessorInstanceCouldNotBeCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processor {0} is not a valid, public processor type..
        /// </summary>
        public static string ProcessorIsInvalid {
            get {
                return ResourceManager.GetString("ProcessorIsInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processor of {0} was explicitly ignored..
        /// </summary>
        public static string ProcessorWasIgnored {
            get {
                return ResourceManager.GetString("ProcessorWasIgnored", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Erro ao consultar API do RabbitMQ. Status {0} - {1}..
        /// </summary>
        public static string RabbitMQAPIError {
            get {
                return ResourceManager.GetString("RabbitMQAPIError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A resposta da mensagem não foi recebida..
        /// </summary>
        public static string ReplyMessageNotReceived {
            get {
                return ResourceManager.GetString("ReplyMessageNotReceived", resourceCulture);
            }
        }
    }
}
