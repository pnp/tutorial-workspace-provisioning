/**
 * Add your local configuration schema here so you can use manipulate it easily in your code per env
 */
export interface IFunctionSettings {
    appId: string;
    tenant: string;
    resource: string;
    certificateThumbPrint: string;
    certificatePath: string;
    spListId: string;
    spWebUrl: string;
    webhookSubscriptionIdentifier: string;
    webhookNotificationUrl: string;
    webhookTargetEndpointUrl: string;
    azStorageConnectionString: string;
    azTableName: string;
}