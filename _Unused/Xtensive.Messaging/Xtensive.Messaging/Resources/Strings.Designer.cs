﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Xtensive.Messaging.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Xtensive.Messaging.Resources.Strings", typeof(Strings).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument out of range. Possible value must be within [{0}, {1}] interval..
        /// </summary>
        internal static string ExArgumentOutOfRange {
            get {
                return ResourceManager.GetString("ExArgumentOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Response did not received in specified period of time..
        /// </summary>
        internal static string ExAskTimeout {
            get {
                return ResourceManager.GetString("ExAskTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Index can&apos;t be less than zero..
        /// </summary>
        internal static string ExCollectionArrayIndexOutOfRange {
            get {
                return ResourceManager.GetString("ExCollectionArrayIndexOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unble to copy to multidimensial array..
        /// </summary>
        internal static string ExCollectionArrayMultidimensial {
            get {
                return ResourceManager.GetString("ExCollectionArrayMultidimensial", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to One of awaited messages missed..
        /// </summary>
        internal static string ExCollectionMessageMissing {
            get {
                return ResourceManager.GetString("ExCollectionMessageMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incoming message contains incorrect Sequence value..
        /// </summary>
        internal static string ExCollectionSequenceOutOfRange {
            get {
                return ResourceManager.GetString("ExCollectionSequenceOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeout occured while waiting for data..
        /// </summary>
        internal static string ExCollectionTimeout {
            get {
                return ResourceManager.GetString("ExCollectionTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to DebugInfo is not available..
        /// </summary>
        internal static string ExDebugInfoIsNotAvailable {
            get {
                return ResourceManager.GetString("ExDebugInfoIsNotAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate default message processor definition found. Please check MessageProcessorAttribute of types {0} and {1}..
        /// </summary>
        internal static string ExDuplicateDefaultProcessor {
            get {
                return ResourceManager.GetString("ExDuplicateDefaultProcessor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate message processor definition found for message type {0}. Please check MessageProcessorAttribute of types {1} and {2}..
        /// </summary>
        internal static string ExDuplicateProcessor {
            get {
                return ResourceManager.GetString("ExDuplicateProcessor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message type should implement IQueryMessage interface..
        /// </summary>
        internal static string ExInvalidProcessorMessageType {
            get {
                return ResourceManager.GetString("ExInvalidProcessorMessageType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processor type should implement IMessageProcessor interface..
        /// </summary>
        internal static string ExInvalidProcessorType {
            get {
                return ResourceManager.GetString("ExInvalidProcessorType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message body is not available yet..
        /// </summary>
        internal static string ExMessageBodyIsNotAvailableYet {
            get {
                return ResourceManager.GetString("ExMessageBodyIsNotAvailableYet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message header is longer than buffer..
        /// </summary>
        internal static string ExMessageHeaderBufferTooSmall {
            get {
                return ResourceManager.GetString("ExMessageHeaderBufferTooSmall", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message header is not available yet..
        /// </summary>
        internal static string ExMessageHeaderIsNotAvailableYet {
            get {
                return ResourceManager.GetString("ExMessageHeaderIsNotAvailableYet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message is already read. Use MessagerReader.Clear() to prepare the reader for reading the next message..
        /// </summary>
        internal static string ExMessageIsAlreadyRead {
            get {
                return ResourceManager.GetString("ExMessageIsAlreadyRead", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message is incomplete..
        /// </summary>
        internal static string ExMessageIsIncomplete {
            get {
                return ResourceManager.GetString("ExMessageIsIncomplete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown error while using Xtensive.Messaging..
        /// </summary>
        internal static string ExMessagingException {
            get {
                return ResourceManager.GetString("ExMessagingException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find plugin for protocol {0} at path {1}..
        /// </summary>
        internal static string ExPluginForProtocolNotFound {
            get {
                return ResourceManager.GetString("ExPluginForProtocolNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while create new instance of {0} messaging processor. Please see inner exception for details..
        /// </summary>
        internal static string ExProcessorCreationError {
            get {
                return ResourceManager.GetString("ExProcessorCreationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message processor {0} must have default constructor..
        /// </summary>
        internal static string ExProcessorDefaultConstructorMissing {
            get {
                return ResourceManager.GetString("ExProcessorDefaultConstructorMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find message processor for type {0}..
        /// </summary>
        internal static string ExProcessorNotFound {
            get {
                return ResourceManager.GetString("ExProcessorNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ory..
        /// </summary>
        internal static string ExProcessorWasNotAdded {
            get {
                return ResourceManager.GetString("ExProcessorWasNotAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while sending data to provider..
        /// </summary>
        internal static string ExProviderDataSending {
            get {
                return ResourceManager.GetString("ExProviderDataSending", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to recreate sending connection after send error. Please look at inner exception for details..
        /// </summary>
        internal static string ExProviderRecreateError {
            get {
                return ResourceManager.GetString("ExProviderRecreateError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Assertion. Receiver got data from unknown connection..
        /// </summary>
        internal static string ExReceiverGotDataFromUnknownConnection {
            get {
                return ResourceManager.GetString("ExReceiverGotDataFromUnknownConnection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Receiving connection already exists..
        /// </summary>
        internal static string ExReceivingConnectionAlreadyExists {
            get {
                return ResourceManager.GetString("ExReceivingConnectionAlreadyExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Response receiver is not initialized..
        /// </summary>
        internal static string ExResponseReceiverIsNotInitialized {
            get {
                return ResourceManager.GetString("ExResponseReceiverIsNotInitialized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to send message. Please look at inner exception for details..
        /// </summary>
        internal static string ExSendError {
            get {
                return ResourceManager.GetString("ExSendError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to add processor while receiver started. Please stop receiver before add processor..
        /// </summary>
        internal static string ExUnableToAddProcessorWhileReceiverStarted {
            get {
                return ResourceManager.GetString("ExUnableToAddProcessorWhileReceiverStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sender can&apos;t create receiver to get Ask responses. It&apos;s possible that connection is not IBidirectionalConnection..
        /// </summary>
        internal static string ExUnableToCreateInternalReceiver {
            get {
                return ResourceManager.GetString("ExUnableToCreateInternalReceiver", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to modify processor context while receiver started. Please stop receiver before modify context or add/remove processors..
        /// </summary>
        internal static string ExUnableToModifyContextWhileReceiverStarted {
            get {
                return ResourceManager.GetString("ExUnableToModifyContextWhileReceiverStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to remove processor while receiver started. Please stop receiver before remove processor..
        /// </summary>
        internal static string ExUnableToRemoveProcessorWhileReceiverStarted {
            get {
                return ResourceManager.GetString("ExUnableToRemoveProcessorWhileReceiverStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to release processor because it wasn&apos;t created by ProcessorFactory..
        /// </summary>
        internal static string ExUnknownProcessorToRelease {
            get {
                return ResourceManager.GetString("ExUnknownProcessorToRelease", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Processor type is unknown..
        /// </summary>
        internal static string ExUnknownProcessorType {
            get {
                return ResourceManager.GetString("ExUnknownProcessorType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error while attempt to deserialize message..
        /// </summary>
        internal static string LogMessageDeserializeError {
            get {
                return ResourceManager.GetString("LogMessageDeserializeError", resourceCulture);
            }
        }
    }
}