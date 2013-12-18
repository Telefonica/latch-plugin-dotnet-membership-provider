/*
    Latch C# Membership Provider Settings - Represents the Latch settings section in the configuration file.
    Copyright (C) 2013 Eleven Paths
 
    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.
 
    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.
 
    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LatchMembership
{
    public class LatchSettingsSection : ConfigurationSection
    {

        /// <summary>
        /// The inner membership provider name used to authenticate and manage all membership functionality
        /// </summary>
        [ConfigurationProperty("innerMembershipProvider", IsRequired = true)]
        public string InnerMembershipProvider
        {
            get
            {
                return (string)this["innerMembershipProvider"];
            }
            set
            {
                this["innerMembershipProvider"] = value;
            }
        }

        /// <summary>
        /// The application ID provided by Latch when registering your application account
        /// </summary>
        [ConfigurationProperty("appId", DefaultValue="12345679801234567980", IsRequired = true)]
        [StringValidator(MinLength = 20, MaxLength = 20)]
        public string AppId
        {
            get
            {
                return (string)this["appId"];
            }
            set
            {
                this["appId"] = value;
            }
        }

        /// <summary>
        /// The application secret key provided by Latch when registering your application account
        /// </summary>
        [ConfigurationProperty("appSecret", DefaultValue = "1234567980123456798012345679801234567980", IsRequired = true)]
        [StringValidator(MinLength = 40, MaxLength = 40)]
        public string AppSecret
        {
            get
            {
                return (string)this["appSecret"];
            }
            set
            {
                this["appSecret"] = value;
            }
        }

        /// <summary>
        /// The Url of Latch service
        /// </summary>
        [ConfigurationProperty("apiHost", DefaultValue = "", IsRequired = false)]
        public string ApiHost
        {
            get
            {
                return (string)this["apiHost"];
            }
            set
            {
                this["apiHost"] = value;
            }
        }

        /// <summary>
        /// The identifier for login operation (by default is appID)
        /// </summary>
        [ConfigurationProperty("loginOperation", DefaultValue = "", IsRequired = false)]
        [StringValidator(MaxLength = 20)]
        public string LoginOperation
        {
            get
            {
                return (string)this["loginOperation"];
            }
            set
            {
                this["loginOperation"] = value;
            }
        }

        /// <summary>
        /// The relative path and file name of the XML default storage to handle the association with Latch user account identifiers.
        /// </summary>
        [ConfigurationProperty("defaultStorageXmlFile", DefaultValue = @"App_Data\LatchAccounts.xml", IsRequired = false)]
        public string DefaultStorageXmlFile
        {
            get
            {
                return (string)this["defaultStorageXmlFile"];
            }
            set
            {
                this["defaultStorageXmlFile"] = value;
            }
        }


        [ConfigurationProperty("operations")]
        public OperationElementCollection Operations
        {
            get
            {
                return this["operations"] as OperationElementCollection;
            }
        }

    }

    public class OperationElementCollection : ConfigurationElementCollection
    {
        public OperationElement this[object key]
        {
            get
            {
                return base.BaseGet(key) as OperationElement;
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "operation";
            }
        }

        protected override bool IsElementName(string elementName)
        {
            bool isName = false;
            if (!String.IsNullOrEmpty(elementName))
                isName = elementName.Equals("operation");
            return isName;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OperationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OperationElement)element).Name;
        }
    }

    public class OperationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get
            {
                return this["name"] as string;
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get
            {
                return this["value"] as string;
            }
            set
            {
                this["value"] = value;
            }
        }
    }
}
