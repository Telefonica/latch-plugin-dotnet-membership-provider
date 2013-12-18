/*
    Latch C# Membership Provider Login - Provides user interface elements and second factor functionality specific to Latch membership provider.
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace LatchMembership.UI
{
    public class LatchLogin : Login
    {
        /// <summary>
        /// A boolean indicating if the user has already entered correctly the second factor token
        /// </summary>
        private bool tokenValidated;

        /// <summary>
        /// A string storing the expected second factor token returned by the Latch API
        /// </summary>
        private string expectedToken;

        private Panel tokenPanel;
        private TextBox secondFactorTextBox;

        private string ServerTokenKey { get { return "LatchToken-" + this.UserName.ToLowerInvariant(); } }

        /// <summary>
        /// Returns the default or specific LatchMembershipProvider. Returns null if it is a different type of provider.
        /// </summary>
        private LatchMembershipProvider LatchProvider
        {
            get
            {
                if (String.IsNullOrEmpty(this.MembershipProvider))
                {
                    return Membership.Provider as LatchMembershipProvider;
                }
                else
                {
                    return Membership.Providers[this.MembershipProvider] as LatchMembershipProvider;
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        /// <summary>
        /// Overrides the base method and performs Latch authentication if the membership provider is valid
        /// </summary>
        protected override void OnAuthenticate(AuthenticateEventArgs e)
        {
            if (LatchProvider == null)
            {
                base.OnAuthenticate(e);
            }
            else
            {
                if (string.IsNullOrEmpty(this.expectedToken))
                {
                    LatchProvider.SecondFactorRequired += LatchProvider_SecondFactorRequiredEvent;
                    base.OnAuthenticate(e);
                    LatchProvider.SecondFactorRequired -= LatchProvider_SecondFactorRequiredEvent;

                    if (e.Authenticated && !string.IsNullOrEmpty(this.expectedToken))
                    {
                        e.Authenticated = false;
                    }
                }
                else
                {
                    e.Authenticated = this.tokenValidated;

                    this.tokenValidated = false;
                    this.expectedToken = null;
                }
            }
        }


        /// <summary>
        /// Handles the second factor required event if the Latch provider raises it
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments containing the username and expected token</param>
        void LatchProvider_SecondFactorRequiredEvent(object sender, SecondFactorEventArgs e)
        {
            this.expectedToken = e.Token;
            this.Context.Application.Add(this.ServerTokenKey, this.expectedToken);
        }


        /// <summary>
        /// Renders the standard login base control or the second factor specific form form
        /// </summary>
        protected override void RenderChildren(System.Web.UI.HtmlTextWriter writer)
        {
            bool secondFactor = !string.IsNullOrEmpty(this.expectedToken);
            foreach (Control ctrl in this.Controls)
            {
                ctrl.Visible = (secondFactor ? (ctrl == tokenPanel) : (ctrl != tokenPanel));
            }
            base.RenderChildren(writer);
        }


        /// <summary>
        /// Creates the second factor internal controls when needed
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (LatchProvider != null)
            {
                tokenPanel = new Panel();
                tokenPanel.Controls.Add(new Literal() { Text = "Second factor: " });
                this.secondFactorTextBox = new TextBox() { ID = "SecondFactorTextBox", TextMode = TextBoxMode.Password, ValidationGroup = "TokenValidationGroup" };
                tokenPanel.Controls.Add(this.secondFactorTextBox);
                tokenPanel.Controls.Add(new Literal() { Text = " " });
                Button secondFactorButton = new Button() { ID = "SecondFactorButton", Text = base.LoginButtonText, CommandName = Login.LoginButtonCommandName, ValidationGroup = "TokenValidationGroup" };
                secondFactorButton.Click += secondFactorButton_Click;
                tokenPanel.Controls.Add(secondFactorButton);

                this.Controls.Add(tokenPanel);
            }
        }

        /// <summary>
        /// Handles the second factor button click and validates the expected token
        /// </summary>
        private void secondFactorButton_Click(object sender, EventArgs e)
        {
            this.expectedToken = (string)this.Context.Application[this.ServerTokenKey];

            if (!string.IsNullOrEmpty(this.expectedToken) && this.secondFactorTextBox.Text == this.expectedToken)
            {
                this.tokenValidated = true;
            }

            this.Context.Application.Remove(this.ServerTokenKey);
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState is List<string>)
            {
                var state = (List<string>)savedState;
                base.UserName = state[0];
            }
        }

        protected override object SaveControlState()
        {
            var state = new List<string>();
            state.Add(this.UserName);
            return state;
        }


    }
}
