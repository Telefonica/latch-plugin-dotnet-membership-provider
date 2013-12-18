/*
    Latch C# Membership Provider Pairing Control - Manages the pairing control when required by the provider.
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
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LatchMembership.UI
{
    [ToolboxData("<{0}:PairingControl runat=server></{0}:PairingControl>")]
    [BindableAttribute(false)]
    public class PairingControl : CompositeControl
    {
        private MultiView pairingMultiView;
        private Label accountIdLabel;
        private TextBox pairingTokenTextBox;

        private enum Views
        {
            NotUsingLatch,
            Anonymous,
            Paired,
            NotPaired
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            pairingMultiView = new MultiView() { ActiveViewIndex = 0 };

            View notUsingLatchView = new View() { ID = "notUsingLatchView" };
            notUsingLatchView.Controls.Add(new Literal() { Text = "You are not using the Latch Membership Provider." });
            pairingMultiView.Views.Add(notUsingLatchView);

            View anonymousView = new View() { ID = "AnonymousView" };
            anonymousView.Controls.Add(new Literal() { Text = "You are not logged in. Please login to manage your account pairing." });
            pairingMultiView.Views.Add(anonymousView);

            View pairedView = new View() { ID = "PairedView" };
            pairedView.Controls.Add(new Literal() { Text = "Account ID: " });
            accountIdLabel = new Label() { ID = "AccountIdLabel" };
            accountIdLabel.Font.Bold = true;
            pairedView.Controls.Add(accountIdLabel);
            Button unpairButton = new Button() { ID = "UnpairButton", Text = "Unpair" };
            unpairButton.Click += new EventHandler(UnpairButton_Click);
            pairedView.Controls.Add(new Literal() { Text = " " });
            pairedView.Controls.Add(unpairButton);
            pairingMultiView.Views.Add(pairedView);

            View unpairedView = new View() { ID = "UnpairedView" };
            unpairedView.Controls.Add(new Literal() { Text = "Pairing token: " });
            pairingTokenTextBox = new TextBox() { ID = "PairingTokenTextBox" };
            unpairedView.Controls.Add(pairingTokenTextBox);
            Button pairButton = new Button() { ID = "PairButton", Text = "Pair" };
            pairButton.Click += new EventHandler(PairButton_Click);
            unpairedView.Controls.Add(new Literal() { Text = " " });
            unpairedView.Controls.Add(pairButton);
            pairingMultiView.Views.Add(unpairedView);

            //this.Controls.Add(new Literal() { Text = "<h3>Pairing with Latch</h3>" });
            this.Controls.Add(pairingMultiView);
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
                EnsureChildControls();
                Refresh();
            }
        }


        private void UnpairButton_Click(object sender, EventArgs e)
        {
            var user = HttpContext.Current.User;

            if (user != null)
            {
                (Membership.Provider as LatchMembership.LatchMembershipProvider).UnpairAccount(user.Identity.Name);
            }

            Refresh();
        }


        private void PairButton_Click(object sender, EventArgs e)
        {
            var user = HttpContext.Current.User;
            string token = pairingTokenTextBox.Text.Trim();

            pairingTokenTextBox.Text = string.Empty;

            if (user != null && !string.IsNullOrEmpty(token))
            {
                (Membership.Provider as LatchMembership.LatchMembershipProvider).PairAccount(user.Identity.Name, token);
            }

            Refresh();
        }

        private void Refresh()
        {
            var user = HttpContext.Current.User;

            if (Membership.Provider as LatchMembership.LatchMembershipProvider == null)
            {
                pairingMultiView.ActiveViewIndex = (int)Views.NotUsingLatch;
            }
            else if (user == null || !user.Identity.IsAuthenticated)
            {
                pairingMultiView.ActiveViewIndex = (int)Views.Anonymous;
            }
            else
            {
                var accountId = (Membership.Provider as LatchMembership.LatchMembershipProvider).GetUserAccountId(user.Identity.Name);
                if (string.IsNullOrEmpty(accountId))
                {
                    pairingMultiView.ActiveViewIndex = (int)Views.NotPaired;
                    accountIdLabel.Text = string.Empty;
                    pairingTokenTextBox.Text = string.Empty;
                }
                else
                {
                    pairingMultiView.ActiveViewIndex = (int)Views.Paired;
                    accountIdLabel.Text = accountId;
                    pairingTokenTextBox.Text = string.Empty;
                }
            }
        }


    }
}