# Korga software architecture

## Design goals

**Exactly once** semantics in case of

- SMTP rate limiting
- Graceful shutdown
- No internet connection

**At least once** semantics in case of

- Application crash
- No connection to database

## Processing pipeline

### 1. IMAP section

#### Responsibility: Retrieve emails via IMAP and write to database
#### Responsibility: Mark emails as _seen_ if already in database

### 2. Validation section

#### Responsibility: context-free verification (Receiver, max. size)
#### Responsibility: context-based verification (Alias, sender permission)
#### Responsibility: Fetch recipients
#### Responsibility: Prepare email for delivery (Resent headers)

`EmailDeliveryService.SendAsync(MimeMessage message, long? emailId)`
- Add to email queue
- (optional) link to received email

### 3. SMTP section

#### Responsibility: E-Mails versenden

`EmailDeliveryHostedService.ExecuteOnce`
- Fetch mailqueue and send
- Decreasing priority with every error
