# Tutorial - Create an end-to-end Office 365 groups provisioning solution #

## Why a provisiong solution for Office 365 groups?

This solution was first designed to avoid SharePoint sites sprawling resulting of an uncontrolled Teams deployment inside an organisation. By 'uncontrolled' we mainly refer to Teams containing organisation reference or critical documents without any kind structure or metadata making hard for users to find them outside this specific Teams (data isolation).

A provision solution can, of course, also serve many other purposes. You can also refer to this article to know more about this topic: https://laurakokkarinen.com/teams-and-sharepoint-provisioning-what-why-and-how/

This tutorial only provides the very minimum building blocks to create a working provisioning solution for Office 365 groups. Feel free to adapt it according to your context and requirements.

![PnP Worskpaces Demo](./images/demo.gif)

Here are the covered topics during this tutorial:

- Use Microsfot Graph and SharePoint APIs to create and configure Office 365 groups.
- Use Azure back end services to run the provisioning logic.
- Create and use SharePoint web hooks.
- Create a SharePoint search experience for created groups.

## Prerequisites

- A subscription to [Office 365 developer tenant](https://developer.microsoft.com/en-us/office/dev-program)
- An Azure subscription ([Free Trial](https://azure.microsoft.com/en-us/free/)).
- Azure following permissions
    - Create AD applications and resource groups
- Office 365
    - Create term groups and term sets
    - Create site collections
- [Node.js 10](https://nodejs.org/dist/latest-v10.x/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [Postman](https://www.getpostman.com/)
- A modern browser (pick one)
    - [The new Edge](https://www.microsoftedgeinsider.com/en-us/download/)
    - [Google Chrome](https://www.google.com/chrome/index.html)
    - [FireFox](https://www.mozilla.org/en-US/firefox/new/)
    - [Brave](https://brave.com/bra043)
- A modern command line tool (pick one)
    - [New Windows Terminal](https://www.microsoft.com/store/productId/9N0DX20HK701)
    - [Cmder](https://cmder.net/)
- [SharePoint Online Client Components SDK](https://www.microsoft.com/en-us/download/details.aspx?id=42038) + [PnP PowerShell for SharePoint Online](https://github.com/SharePoint/PnP-PowerShell/releases)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
- [Dot Net Core SDK 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) (installer version)
- (Optional) Azure Functions Core tools 2.X : `npm i -g azure-functions-core-tools@2.X`

## Tutorial steps

- [Part 1 - Setup Azure AD application and workspace requests SharePoint site](./docs/PART1.md)
- [Part 2 - Setup Azure back-end services](./docs/PART2.md)
- [Part 3 - Create and register SharePoint Webhook](./docs/PART3.md)
- [Part 4 - Test the end-to-end Office 365 group creation](./docs/PART4.md)
- [Part 5 - Setup end-user experience](./docs/PART5.md)
- [Part 6 - Setup Teams Configuration](./docs/PART6.md)



