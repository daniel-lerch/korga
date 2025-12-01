# Mailist email processing pipeline architecture

## Design goals

**Exactly once** semantics in case of

- SMTP rate limiting
- Graceful shutdown
- No internet connection

**At least once** semantics in case of

- Application crash
- No connection to database

## Processing pipeline

Sections are divided semantically for better abstraction.
Each responsibility is idempotent as a whole.
This means that each step can be executed repeatedly without affecting the result.

### 1. IMAP section

#### Responsibility: Retrieve emails via IMAP and write to database

`ImapReceiverService.FetchAsync()`

- Mark emails as _seen_ if already in database

### 2. Validation section

Applies to: `InboxEmails.Where(email => email.ProcessingCompletedTime == default)`

#### Responsibility: context-free verification (Receiver, max. size)

#### Responsibility: context-based verification (Alias, sender permission)

#### Responsibility: Fetch recipients

`DistributionListService.GetRecipients(DistributionList distributionList)`

#### Responsibility: Prepare email for delivery (Resent headers)

`MimeMessageCreationService.PrepareForResentTo(long emailId, string mailAddress)`

- Adds `Resent` headers and returns a `MimeMessage`

`EmailDeliveryService.Enqueue(MimeMessage message, long? emailId)`

- Add to email queue
- (optional) link to received email

### 3. SMTP section

Applies to: `OutboxEmails.Where(email => email.DeliveryTime == default)`

#### Responsibility: Deliver emails (not idempotent on abort)

`EmailDeliveryJobController.ExecuteJob`

- Fetch mailqueue and send
- Might use a decreasing priority with every error in the future
- Could be replaced by other services for MailChimp, MailJet, MailGun, etc.
- Resent headers might work with SMTP but especially marketing APIs work with templates and batch processing instead of inidividual emails
