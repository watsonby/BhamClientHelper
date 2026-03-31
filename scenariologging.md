# Gallagher API Scenarios with NLog

This document provides BizTalk Expression-shape examples for Scenario 1 (add membership if missing) and Scenario 2 (remove membership if present), with NLog logging enabled.

## Prerequisites

- Add `NLog.config` to the BizTalk host process folder (for example from `samples/NLog.config.sample`).
- Ensure your helper assembly references NLog and includes `GallagherLoggingHelper`.
- Use the following orchestration string variables:
  - `strGallagherBaseUrl`
  - `strApiKey`
  - `strCertThumbprint`
  - `strPdfFieldId`
  - `strPdfFieldKey`
  - `strCardholderId`
  - `strAccessGroupName`
  - `strAccessGroupHref`
  - `strFromDate`
  - `strUntilDate`
  - `strResponse`
  - `strGallagherCardholderId`
  - `strMembershipHref`

## Extend GallagherLoggingHelper for full workflow

If your `GallagherLoggingHelper` does not already include these wrappers, add them first so all Scenario 1 and 2 calls are logged through NLog.

```csharp
public static string GetCardholdersByPdfValueWithNLog(
    string baseUrl,
    string apiKeyHeaderName,
    string apiKeyHeaderValue,
    string cardholderId,
    string pdfFieldKey,
    string certThumbprint = null,
    int timeoutSeconds = 100)
{
    return GallagherApiFacade.GetCardholdersByPdfValue(
        baseUrl,
        apiKeyHeaderName,
        apiKeyHeaderValue,
        cardholderId,
        pdfFieldKey,
        certThumbprint,
        System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
        System.Security.Cryptography.X509Certificates.StoreName.My,
        timeoutSeconds,
        CreateNLogLogger("GetCardholdersByPdfValue"));
}

public static string ResolveAccessGroupHrefByNameWithNLog(
    string baseUrl,
    string apiKeyHeaderName,
    string apiKeyHeaderValue,
    string accessGroupName,
    string certThumbprint = null,
    int timeoutSeconds = 100)
{
    return GallagherApiFacade.ResolveAccessGroupHrefByName(
        baseUrl,
        apiKeyHeaderName,
        apiKeyHeaderValue,
        accessGroupName,
        certThumbprint,
        System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
        System.Security.Cryptography.X509Certificates.StoreName.My,
        timeoutSeconds,
        CreateNLogLogger("ResolveAccessGroupHrefByName"));
}

public static string ResolveAccessGroupMembershipHrefWithNLog(
    string baseUrl,
    string apiKeyHeaderName,
    string apiKeyHeaderValue,
    string accessGroupHref,
    string cardholderId,
    string certThumbprint = null,
    int timeoutSeconds = 100)
{
    return GallagherApiFacade.ResolveAccessGroupMembershipHref(
        baseUrl,
        apiKeyHeaderName,
        apiKeyHeaderValue,
        accessGroupHref,
        cardholderId,
        certThumbprint,
        System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
        System.Security.Cryptography.X509Certificates.StoreName.My,
        timeoutSeconds,
        CreateNLogLogger("ResolveAccessGroupMembershipHref"));
}

public static string RemoveCardholderFromAccessGroupWithNLog(
    string baseUrl,
    string apiKeyHeaderName,
    string apiKeyHeaderValue,
    string cardholderId,
    string membershipHref,
    string certThumbprint = null,
    int timeoutSeconds = 100)
{
    return GallagherApiFacade.RemoveCardholderFromAccessGroup(
        baseUrl,
        apiKeyHeaderName,
        apiKeyHeaderValue,
        cardholderId,
        membershipHref,
        certThumbprint,
        System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
        System.Security.Cryptography.X509Certificates.StoreName.My,
        timeoutSeconds,
        CreateNLogLogger("RemoveCardholderFromAccessGroup"));
}
```

## Scenario 1: Add Cardholder to Access Group with Date Range (with NLog)

### Expression Shape A (Initialize Variables)

```csharp
strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
strApiKey = "YOUR_API_KEY";
strCertThumbprint = "YOUR_CERT_THUMBPRINT";
strPdfFieldId = "629";
strPdfFieldKey = "pdf_" + strPdfFieldId;
strCardholderId = "IDCARD.2953599";
strAccessGroupName = "6090-MASON-114-02";
strFromDate = "2025-09-12T05:00:00Z";
strUntilDate = "2026-05-20T10:00:00Z";
```

### Expression Shape B (Find Cardholder by PDF Value with NLog)

```csharp
strResponse =
    GallagherLoggingHelper.GetCardholdersByPdfValueWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strCardholderId,
        strPdfFieldKey,
        strCertThumbprint,
        100);
```

### Expression Shape C (Extract Gallagher Cardholder ID)

```csharp
strGallagherCardholderId =
    Bham.BizTalk.Rest.GallagherApiResponseParser.GetFirstEntityId(strResponse);
```

### Expression Shape C.5 (Resolve Access Group href by Name with NLog)

```csharp
strAccessGroupHref =
    GallagherLoggingHelper.ResolveAccessGroupHrefByNameWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strAccessGroupName,
        strCertThumbprint,
        100);
```

### Expression Shape D (Check if Membership Already Exists with NLog)

```csharp
strMembershipHref =
    GallagherLoggingHelper.ResolveAccessGroupMembershipHrefWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strAccessGroupHref,
        strGallagherCardholderId,
        strCertThumbprint,
        100);
```

### Decision Shape (Check if Membership Exists)

- Rule: `strMembershipHref != ""`
- True: Skip adding (already exists)
- False: Proceed to add membership

### Expression Shape E (Add Cardholder to Access Group with NLog)

```csharp
strResponse =
    GallagherLoggingHelper.AddAccessGroupToCardholderWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strGallagherCardholderId,
        strAccessGroupHref,
        strFromDate,
        strUntilDate,
        strCertThumbprint,
        100);
```

## Scenario 2: Remove Cardholder from Access Group (with NLog)

### Expression Shape A (Initialize Variables)

```csharp
strGallagherBaseUrl = "https://its-d-cdx-01.adf.bham.ac.uk:8904/api";
strApiKey = "YOUR_API_KEY";
strCertThumbprint = "YOUR_CERT_THUMBPRINT";
strPdfFieldId = "629";
strPdfFieldKey = "pdf_" + strPdfFieldId;
strCardholderId = "IDCARD.2953599";
strAccessGroupName = "6090-MASON-114-02";
```

### Expression Shape B (Find Cardholder by PDF Value with NLog)

```csharp
strResponse =
    GallagherLoggingHelper.GetCardholdersByPdfValueWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strCardholderId,
        strPdfFieldKey,
        strCertThumbprint,
        100);
```

### Expression Shape C (Extract Gallagher Cardholder ID)

```csharp
strGallagherCardholderId =
    Bham.BizTalk.Rest.GallagherApiResponseParser.GetFirstEntityId(strResponse);
```

### Expression Shape C.5 (Resolve Access Group href by Name with NLog)

```csharp
strAccessGroupHref =
    GallagherLoggingHelper.ResolveAccessGroupHrefByNameWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strAccessGroupName,
        strCertThumbprint,
        100);
```

### Expression Shape D (Resolve Membership Href with NLog)

```csharp
strMembershipHref =
    GallagherLoggingHelper.ResolveAccessGroupMembershipHrefWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strAccessGroupHref,
        strGallagherCardholderId,
        strCertThumbprint,
        100);
```

### Decision Shape (Check if Membership Exists)

- Rule: `strMembershipHref != ""`
- True: Proceed to remove membership
- False: Skip removing (does not exist)

### Expression Shape E (Remove Cardholder from Access Group with NLog)

```csharp
strResponse =
    GallagherLoggingHelper.RemoveCardholderFromAccessGroupWithNLog(
        strGallagherBaseUrl,
        "Authorization",
        strApiKey,
        strGallagherCardholderId,
        strMembershipHref,
        strCertThumbprint,
        100);
```

## Logging Notes

- `ResolveAccessGroupMembershipHref` returns an empty string when no membership exists, and also when access group lookup returns 404.
- `strAccessGroupHref` should contain the full Gallagher href (for example `https://its-d-cdx-01.adf.bham.ac.uk:8904/api/access_groups/659`).
- NLog file target and retention can be configured via `NLog.config`.
