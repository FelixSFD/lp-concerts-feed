
export const environment = {
  apiCachedBaseUrl: "https://api.ROOT_DOMAIN",
  apiNoCacheBaseUrl: "https://api.ROOT_DOMAIN",
  cognitoBaseUrl: "https://COGNITO_URL",
  cognitoClientId: "COGNITO_CLIENT_ID",
  cognitoRedirectUrl: "https://ROOT_DOMAIN",
  cognitoLogoutUrl: "https://COGNITO_AUTH_SERVER/logout?client_id=COGNITO_CLIENT_ID&logout_uri=https://" + window.document.location.host,
  build: "BUILD_NUMBER"
};
