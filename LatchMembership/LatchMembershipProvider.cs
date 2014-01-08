/*
    Latch C# Membership Provider - Manages the membership extension of an inner provider with Latch functionality
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
using System.Web.Security;
using System.Web.Profile;
using System.Web;
using System.IO;

namespace LatchMembership
{
    public class LatchMembershipProvider : MembershipProvider
    {
        public delegate void SecondFactorRequiredEventHandler(object sender, SecondFactorEventArgs e);
        public event SecondFactorRequiredEventHandler SecondFactorRequired;

        private string innerMembershipProviderName;
        private MembershipProvider innerMembershipProvider;

        private String appId;
        private String appSecret;
        private String apiHost = LatchSDK.Latch.API_HOST;
        private String loginOperation;
        private LatchSDK.Latch latchAPI;

        private string defaultStorageXmlFile;
        LatchDefaultStorage defaultStorage;

        private LatchSDK.Latch LatchAPI
        {
            get
            {
                if (this.latchAPI == null)
                {
                    throw new InvalidOperationException("Invalid or empty API settings for Latch");
                }
                return this.latchAPI;
            }
        }

        public MembershipProvider InnerMembershipProvider
        {
            get
            {
                if (innerMembershipProvider == null)
                {
                    innerMembershipProvider = Membership.Providers[innerMembershipProviderName];
                    if (innerMembershipProvider == null)
                    {
                        throw new ArgumentException("Empty name or invalid inner membership provider");
                    }
                    else if (innerMembershipProvider is LatchMembershipProvider || innerMembershipProvider.GetType().IsSubclassOf(typeof(LatchMembershipProvider)))
                    {
                        throw new ArgumentException("The inner membership provider cannot be of the same type");
                    }
                }
                return innerMembershipProvider;
            }
        }

        public override string Description
        {
            get
            {
                return innerMembershipProvider.Description + " extended with Latch";
            }
        }

        #region  Properties and methods directly delegated to the inner provider

        public override string ApplicationName
        {
            get
            {
                return InnerMembershipProvider.ApplicationName;
            }
            set
            {
                InnerMembershipProvider.ApplicationName = value;
            }
        }

        public override bool EnablePasswordReset
        {
            get { return InnerMembershipProvider.EnablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return InnerMembershipProvider.EnablePasswordRetrieval; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return InnerMembershipProvider.MaxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return InnerMembershipProvider.MinRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return InnerMembershipProvider.MinRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return InnerMembershipProvider.PasswordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return InnerMembershipProvider.PasswordFormat; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return InnerMembershipProvider.PasswordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return InnerMembershipProvider.RequiresQuestionAndAnswer; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return InnerMembershipProvider.RequiresUniqueEmail; }
        }



        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return InnerMembershipProvider.ChangePassword(username, oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            return InnerMembershipProvider.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            return InnerMembershipProvider.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return InnerMembershipProvider.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return InnerMembershipProvider.FindUsersByEmail(usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            return InnerMembershipProvider.GetAllUsers(pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            return InnerMembershipProvider.GetNumberOfUsersOnline();
        }

        public override string GetPassword(string username, string answer)
        {
            return InnerMembershipProvider.GetPassword(username, answer);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            return InnerMembershipProvider.GetUser(providerUserKey, userIsOnline);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return InnerMembershipProvider.GetUser(username, userIsOnline);
        }

        public override string GetUserNameByEmail(string email)
        {
            return InnerMembershipProvider.GetUserNameByEmail(email);
        }

        public override string ResetPassword(string username, string answer)
        {
            return InnerMembershipProvider.ResetPassword(username, answer);
        }

        public override bool UnlockUser(string userName)
        {
            return InnerMembershipProvider.UnlockUser(userName);
        }

        public override void UpdateUser(MembershipUser user)
        {
            InnerMembershipProvider.UpdateUser(user);
        }


        #endregion

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            var latchConfig = (LatchSettingsSection)System.Configuration.ConfigurationManager.GetSection("latchSettings");

            this.innerMembershipProviderName = latchConfig.InnerMembershipProvider;
            this.appId = latchConfig.AppId;
            this.appSecret = latchConfig.AppSecret;

            if (!string.IsNullOrEmpty(latchConfig.ApiHost))
            {
                this.apiHost = latchConfig.ApiHost;
            }

            this.loginOperation = string.IsNullOrEmpty(latchConfig.LoginOperation) ? appId : latchConfig.LoginOperation;
            
            this.defaultStorageXmlFile = Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, latchConfig.DefaultStorageXmlFile);

            this.latchAPI = new LatchSDK.Latch(this.appId, this.appSecret);
            LatchSDK.Latch.SetHost(this.apiHost);

            this.defaultStorage = new LatchDefaultStorage();
            if (File.Exists(this.defaultStorageXmlFile))
            {
                try
                {
                    this.defaultStorage.ReadXml(this.defaultStorageXmlFile);
                }
                catch (Exception)
                {
                }
            }
        }


        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                UnpairAccount(username);
            }
            catch { }
            return InnerMembershipProvider.DeleteUser(username, deleteAllRelatedData);
        }


        #region Default local storage management

        /// <summary>
        /// Returns the Latch account ID associated with a membership user
        /// </summary>
        /// <param name="user">The membership user to query the account ID</param>
        /// <returns>the Latch account ID</returns>
        /// <remarks>A default XML storage file is used. You can override this method if this does not fit your specific needs</remarks>
        protected string GetUserAccountId(MembershipUser user)
        {
            if (user == null) return null;

            var pairingInfo = this.defaultStorage.PairingInfo.FindByUsername(user.UserName);
            return pairingInfo == null ? String.Empty : pairingInfo.AccountId;
        }


        /// <summary>
        /// Returns the Latch account ID associated with a membership username
        /// </summary>
        /// <param name="username">The membership username to query the account ID</param>
        /// <returns>the Latch account ID</returns>
        public string GetUserAccountId(string username)
        {
            var user = InnerMembershipProvider.GetUser(username, false);
            if (user == null)
            {
                throw new ArgumentException("Invalid username");
            }
            return GetUserAccountId(user);
        }

        /// <summary>
        /// Associates the account ID with a membership user
        /// </summary>
        /// <param name="user">The membership user to query the account ID</param>
        /// <param name="accountId">The Latch account ID</param>
        /// <remarks>A default XML storage file is used. You can override this method if this does not fit your specific needs</remarks>
        protected void SetUserAccountId(MembershipUser user, string accountId)
        {
            if (user == null) return;

            var pairingInfo = this.defaultStorage.PairingInfo.FindByUsername(user.UserName);

            if (pairingInfo == null)
            {
                this.defaultStorage.PairingInfo.AddPairingInfoRow(user.UserName, accountId);
            }
            else
            {
                pairingInfo.AccountId = accountId;
            }
            this.defaultStorage.AcceptChanges();
            this.defaultStorage.WriteXml(this.defaultStorageXmlFile);
        }
        #endregion

        #region  Latch specific functionality

        /// <summary>
        /// Pairs (associates) the username account with Latch
        /// </summary>
        /// <param name="username">The membership username to pair</param>
        /// <param name="pairingToken">The pairing authentication token generated by Latch and received by the user in its mobile device</param>
        /// <returns>the account ID returned by the server</returns>
        public string PairAccount(string username, string pairingToken)
        {
            var user = InnerMembershipProvider.GetUser(username, false);
            if (user == null)
            {
                throw new ArgumentException("Invalid username");
            }

            var pairResponse = LatchAPI.Pair(pairingToken);
            if (pairResponse.Data != null && pairResponse.Data.ContainsKey("accountId"))
            {
                string accountId = pairResponse.Data["accountId"].ToString();
                SetUserAccountId(user, accountId);
                return accountId;
            }
            else if (pairResponse.Error != null)
            {
                throw new ApplicationException(pairResponse.Error.ToString());
            }
            return null;
        }

        /// <summary>
        /// Unpairs (deassociates) the username account with Latch
        /// </summary>
        /// <param name="username">the membership username to unpair</param>
        public void UnpairAccount(string username)
        {
            var user = InnerMembershipProvider.GetUser(username, false);
            string accountId = GetUserAccountId(user);

            if (this.latchAPI != null && user != null && !string.IsNullOrEmpty(accountId))
            {
                var unpairResponse = this.latchAPI.Unpair(accountId);
                SetUserAccountId(user, string.Empty);
                if (unpairResponse.Error != null)
                {
                    throw new ApplicationException(unpairResponse.Error.ToString());
                }
            }
        }


        /// <summary>
        /// Verifies that the specified user name and password are valid in the inner membership provider and then performs a Latch verification
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user</param>
        /// <returns>true if the specified username and password are valid and the account is not disabled in Latch; otherwise, false</returns>
        public override bool ValidateUser(string username, string password)
        {
            if (InnerMembershipProvider.ValidateUser(username, password))
            {
                var user = InnerMembershipProvider.GetUser(username, false);
                string accountId = GetUserAccountId(user);

                if (this.latchAPI != null && !string.IsNullOrEmpty(accountId))
                {
                    var statusResponse = this.latchAPI.OperationStatus(accountId, this.loginOperation);

                    if (statusResponse.Data != null && statusResponse.Data.ContainsKey("operations"))
                    {
                        var operations = (Dictionary<string, object>)statusResponse.Data["operations"];
                        if (operations.ContainsKey(this.loginOperation))
                        {
                            var app = (Dictionary<string, object>)operations[this.loginOperation];
                            if (app.ContainsKey("status"))
                            {
                                string status = app["status"].ToString().ToLower();

                                if (status == "on")
                                {
                                    if (app.ContainsKey("two_factor"))
                                    {
                                        var twoFactor = (Dictionary<string, object>)app["two_factor"];
                                        string token = ((Dictionary<string, object>)twoFactor)["token"].ToString();
                                        if (!string.IsNullOrEmpty(token))
                                        {
                                            OnSecondFactorRequired(new SecondFactorEventArgs(username, token));
                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Raises the SecondFactorRequired event if an event handler has been defined
        /// </summary>
        /// <param name="e">Event arguments containing the username and expected token</param>
        protected void OnSecondFactorRequired(SecondFactorEventArgs e)
        {
            if (SecondFactorRequired != null)
            {
                SecondFactorRequired(this, e);
            }
        }

        #endregion



    }
}
