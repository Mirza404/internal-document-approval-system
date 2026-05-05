import {
  EventType,
  InteractionRequiredAuthError,
  PublicClientApplication,
  type AccountInfo,
  type AuthenticationResult,
  type Configuration,
} from "@azure/msal-browser";

const clientId = import.meta.env.VITE_MICROSOFT_CLIENT_ID;
const tenantId = import.meta.env.VITE_MICROSOFT_TENANT_ID;
const redirectUri =
  import.meta.env.VITE_MICROSOFT_REDIRECT_URI || window.location.origin;
const apiScope = import.meta.env.VITE_MICROSOFT_API_SCOPE;

if (!clientId || !tenantId || !apiScope) {
  throw new Error(
    "Microsoft sign-in is not configured. Set VITE_MICROSOFT_CLIENT_ID, VITE_MICROSOFT_TENANT_ID, and VITE_MICROSOFT_API_SCOPE.",
  );
}

export const loginRequest = {
  scopes: [apiScope],
};

const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri,
    postLogoutRedirectUri: redirectUri,
  },
  cache: {
    cacheLocation: "localStorage",
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);

export const initializeMsal = async () => {
  await msalInstance.initialize();

  const response = await msalInstance.handleRedirectPromise();
  if (response?.account) {
    msalInstance.setActiveAccount(response.account);
  }

  const activeAccount = msalInstance.getActiveAccount();
  if (!activeAccount) {
    const [firstAccount] = msalInstance.getAllAccounts();
    if (firstAccount) {
      msalInstance.setActiveAccount(firstAccount);
    }
  }

  msalInstance.addEventCallback((event) => {
    if (event.eventType !== EventType.LOGIN_SUCCESS) {
      return;
    }

    const result = event.payload as AuthenticationResult;
    if (result.account) {
      msalInstance.setActiveAccount(result.account);
    }
  });
};

const getRequestAccount = (): AccountInfo | undefined =>
  msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0];

export const getApiAccessToken = async () => {
  const account = getRequestAccount();
  if (!account) {
    return null;
  }

  try {
    const response = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account,
    });
    return response.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect({
        ...loginRequest,
        account,
      });
    }

    throw error;
  }
};
