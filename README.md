===============================================
=  LATCH MEMBERSHIP PROVIDER AND UI CONTROLS  =
===============================================

This DLL provides an implementation of Latch membership provider and extended
login and pairing controls.

To use it in your application, you will have to register first your account 
and add an application in the Latch website. The server will provide you a
unique Application ID string and a Secret Key.

You must also specify an internal provider (e.g: SqlMembershipProvider), 
because Latch will only provide you an additional authorization layer, 
and it will not perform authentication or membership management.

Once you have all the required data, you have to enter this information in the
config file adding the following sections:

<configuration>
  <configSections>
    <section name="latchSettings" type="LatchMembership.LatchSettingsSection, LatchMembership, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
  </configSections>

  <latchSettings innerMembershipProvider="MyMembershipProvider" appId="12345678901234567890" appSecret="1234567890123456789012345678901234567890" />

  <system.web>
    <membership defaultProvider="LatchMembershipProvider">
      <providers>
        <add name="MyMembershipProvider" type="..." connectionStringName="..."    ... />
        <add name="LatchMembershipProvider" type="LatchMembership.LatchMembershipProvider, LatchMembership, Version=1.0.0.0, Culture=neutral" />
      </providers>
    </membership>
  <system.web>
</configuration>


If your application uses the Latch second factor, you can replace the standard
ASP.NET Login control with the LatchLogin control, or alternatively, capture the
Authenticate event and manage it by yourself. The LatchMembershipProvider will
raise an event when a second factor token is needed for a specific account.

You can use the PairingControl to display and manage the pairing status of an
account in a basic and simple way. Feel free to extend it or use your own
control if the existing functionality does not fulfill your requirements.
