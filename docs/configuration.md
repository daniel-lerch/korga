# Korga server configuration

Configuration can be set as enviroment variables or by creating a custom config file.
I recommend to use environment variables and will explain them in the following sections.
However, if you prefer a config file, copy the default [appsettings.json](../server/Korga/appsettings.json), edit it as required, and mount it at `/app/appsettings.json`.

## List of config properties

### `Database__ConnectionString`
Contains all important information to connect to a MariaDB/MySQL database. See [Connection Strings](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings?tabs=dotnet-core-cli) for more information.

Default: `Server=localhost;Port=3306;Database=korga;User=root;Password=root;`  

### `Database__MigrateOnStartup`
Automatically migrates the database on application startup to the latest migration. If you disable this feature, you must run `./Korga database migrate` each time after updating Korga.

Default: `true`

### `Hosting__PathBase`
Configure Korga's URI path part if it is not hosted on its own (sub)domain. Your reverse proxy should not rewrite the request URI.

Example: `/korga`

### `Hosting__AllowProxies`
Enable this if you run Korga behind a reverse proxy like NGINX. Make sure Korga is only accessible via reverse proxy via network configuration, firewall, etc.

Default: `false`

### `OAuth__AuthorizationEndpoint`
The endpoint used to initiate the OAuth authorization process.

Example: `https://demo.church.tools/oauth/authorize`

### `OAuth__TokenEndpoint`
The endpoint used to obtain the OAuth access token.

Example: `https://demo.church.tools/oauth/access_token`

### `OAuth__UserInformationEndpoint`
The endpoint used to retrieve user information after obtaining the access token.

Example: `https://demo.church.tools/oauth/userinfo`

### `OAuth__UsePkce`
Indicates whether to use Proof Key for Code Exchange (PKCE) during the OAuth authorization process.

Default: `true`

### `OAuth__ClientId`
The client identifier issued to the client during the registration process.

Example: `2807de0033284e12bab29389b8bd1ea3acc0352b6bdc4ce8936aa854a555391d`

### `OAuth__ClientSecret`
The client secret issued to the client during the registration process. This configuration is not required for ChurchTools as identity provider.

Default: `undefined`

### `ChurchTools__EnableSync`
Enable or disable synchronization with ChurchTools.

Default: `false`

### `ChurchTools__Host`
The domain of your ChurchTools instance.

Example: `demo.church.tools`

### `ChurchTools__LoginToken`
The login token used to authenticate with the ChurchTools API.

Example: `gMNSXgU8j4pUlYz1RZwpJMsG0uwM70CmQcJihDaQ4TjorKKQBllnQV6eMDPa7QAT8BxvzzPUN7VTmitRIZtXNK8kBUhGcXEw0eJke8xWKNcho28VegF7dXBbSe1A4YKfn3FZysxSbe7A3ZmSrtLfqIRYagmkaU9TvWgo9iTGRbtS4tkRrsUj36gGjIGWL4UALNJaCj8syxanjxr7oPbXdbaRdx8uNiOYXmLwMx0CQHs9yT2hA8O1pnPcM4XuACTl`

### `ChurchTools__SyncIntervalInMinutes`
The interval in minutes at which Korga synchronizes people, groups and memberships from ChurchTools.

Default: `5.0`

### `EmailDelivery__Enable`
Enable or disable email delivery.

Default: `false`

### `EmailDelivery__SenderName`
The name that will appear as the sender of the emails. You may want to set this to the name of your church or organization.

Default: `Korga`

### `EmailDelivery__SenderAddress`
The email address that will appear as the sender of the emails.

Example: `noreply@example.org`

### `EmailDelivery__SmtpHost`
The SMTP server host used for sending emails.

Default: `smtp.strato.de`

### `EmailDelivery__SmtpPort`
The port used to connect to the SMTP server.

Default: `465`

### `EmailDelivery__SmtpUseSsl`
Indicates whether to use SSL for the SMTP connection.

Default: `true`

### `EmailDelivery__SmtpUsername`
The username used to authenticate with the SMTP server.

Example: `noreply@example.org`

### `EmailDelivery__SmtpPassword`
The password used to authenticate with the SMTP server.

Example: `dehxo3ql5uke`

### `EmailRelay__Enable`
Enable or disable email relay.

Default: `false`

### `EmailRelay__ImapHost`
The IMAP server host used for retrieving emails.

Default: `imap.strato.de`

### `EmailRelay__ImapPort`
The port used to connect to the IMAP server.

Default: `993`

### `EmailRelay__ImapUseSsl`
Indicates whether to use SSL for the IMAP connection.

Default: `true`

### `EmailRelay__ImapUsername`
The username used to authenticate with the IMAP server. This should be a catchall inbox that receives all emails on that domain for which no mailbox exists.

Example: `catchall@example.org`

### `EmailRelay__ImapPassword`
The password used to authenticate with the IMAP server.

Example: `dehxo3ql5uke`

### `EmailRelay__RetrievalIntervalInMinutes`
The interval in minutes at which emails are retrieved from the IMAP server.

Default: `2.0`  
Min value: `0.0` (no delay between retrievals)  
Max value: `1440.0` (once per day)

### `EmailRelay__ImapRetentionIntervalInDays`
The number of days to retain emails on the IMAP server before deletion.

Default: `1.0`  
Min value: `0.0` (delete emails immediately after processing)  
Max value: `87600.0` (keep emails for 10 years)

### `EmailRelay__MaxHeaderSizeInKilobytes`
The maximum size of email headers in kilobytes.

Default: `64` (64 KiB)  
Min value: `16` (16 KiB, a usual email SPF, DKIM and DMARC headers is not much smaller)  
Max value: `1024` (1 MiB headers)

### `EmailRelay__MaxBodySizeInKilobytes`
The maximum size of email bodies in kilobytes.

Default: `12288` (12 MiB)  
Min value: `64` (64 KiB)  
Max value: `131072` (128 MiB, almost no email server will accept such a large message)
