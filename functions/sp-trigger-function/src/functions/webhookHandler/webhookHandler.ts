import { Context, HttpMethod, HttpRequest, HttpResponse, HttpStatusCode } from 'azure-functions-ts-essentials';
import * as fs from 'fs';
import * as path from 'path';
import { IFunctionSettings } from '../../models/IFunctionSettings';
import { AuthenticationHelper } from '../../helpers/AuthenticationHelper';
import { SharePointWebHookHelper } from '../../helpers/SharePointWebHookHelper';
import { StorageHelper } from '../../helpers/StorageHelper';
import IWebhookSubcriptionData from '../../models/IWebhookSubcriptionData';
import { IChangeQuery, ChangeType } from '../../models/IChangeQuery';
import fetch from 'node-fetch';

/*
 * Routes the request to the default controller using the relevant method.
 */
export async function run(context: Context, req: HttpRequest): Promise<HttpResponse> { 

  let res: HttpResponse;

  switch (req.method) {
    case HttpMethod.Get:
        break;

    case HttpMethod.Post:

      const validationToken = req.query ? req.query.validationtoken : undefined;

      // Get function settings
      const settingsData: IFunctionSettings = process.env as any;

      // Authentication settings
      const { appId, tenant, resource, certificatePath, certificateThumbPrint } = settingsData;
      const certificate = fs.readFileSync(path.resolve(__dirname, `./${certificatePath}`), { encoding: 'utf8' });

      // SharePoint settings
      const { spWebUrl, spListId } = settingsData;

      // Webhook settings
      const { webhookNotificationUrl, webhookSubscriptionIdentifier , webhookTargetEndpointUrl } = settingsData;

      // Azure Storage settings
      const { azStorageConnectionString, azTableName } = settingsData;
      
      try {

        let response = {};

        if (validationToken) {

          // Reply directly to SharePoint to complete the webhook subscription
          response = validationToken;

        } else {

          const authenticationHelper = new AuthenticationHelper(appId, tenant, resource, certificateThumbPrint, certificate);
          
          // Get an acces token
          const token = await authenticationHelper.getToken();
          const webhookHelper = new SharePointWebHookHelper(spWebUrl, token.accessToken);
            
          // Setting to null corresponds to 180 days expiration by default
          const subscriptionData: IWebhookSubcriptionData = {
            expirationDateTime: null,
            key: webhookSubscriptionIdentifier,
            notificationUrl: webhookNotificationUrl
          };

          await webhookHelper.ensureWebhookSubscription(spListId, subscriptionData);

          // Get last time token
          const storageHelper = new StorageHelper(azStorageConnectionString, azTableName, 'webhook-data');
          let tokenValue = await storageHelper.getPropertyValue('lastChangeToken');
          let changeToken = null;

          if (tokenValue) {            
            changeToken = {
              StringValue: tokenValue
            }
          }

          // Get changes for 'Add' operations only
          const changeRequest: IChangeQuery = {
            query: {
              Add: true,
              Item: true,
              RecursiveAll: true,
              ChangeTokenStart: changeToken
            }
          };
          
          const changes = await webhookHelper.getListItemChanges(spListId, changeRequest);

          // Changes are already sorted from least recent to most recent according to the change token
          const additions = changes.filter(changeItem => {
            return changeItem.ChangeType === ChangeType.Add
          });

          // We don't trigger the target endpoint if there was no change token in order to avoid to deal with list full history since creation.
          if (changeToken) {

            const allPromises = additions.map(addition => {
              const response = fetch(webhookTargetEndpointUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                  itemId: addition.ItemId
                })
              });

              return response;
            });

            await Promise.all(allPromises);
          }

          // Store the last change token for future calls. If no changes to process, set a start token as of 'now'
          const lastChangeToken = additions.length > 0 ? 
            additions[additions.length - 1].ChangeToken.StringValue :
            `1;3;${spListId};${((new Date().getTime() * 10000) + 621355968000000000)};-1`

          await storageHelper.setPropertyValue('lastChangeToken', lastChangeToken);
        }

        res = {
          status: HttpStatusCode.OK,
          body: response
        };

      } catch (error) {
        res = {
          status: HttpStatusCode.InternalServerError,
          body: {
            error: {
              type: 'function_error',
              message: error.message
            }
          }
        };
      }

      break;
    case HttpMethod.Patch:
      break;
    case HttpMethod.Delete:
      break;

    default:
      res = {
              status: HttpStatusCode.MethodNotAllowed,
              body: {
                error: {
                  type: 'not_supported',
                  message: `Method ${req.method} not supported.`
                }
              }
            };
  }

  return res;
}
