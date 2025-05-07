
export const environment = {
  apiCachedBaseUrl: "https://api.ROOT_DOMAIN",
  apiNoCacheBaseUrl: "https://api.ROOT_DOMAIN",
  cognitoBaseUrl: "https://COGNITO_URL",
  cognitoClientId: "COGNITO_CLIENT_ID",
  cognitoRedirectUrl: "https://ROOT_DOMAIN",
  cognitoLogoutUrl: "https://COGNITO_AUTH_SERVER/logout?client_id=COGNITO_CLIENT_ID&logout_uri=https://" + window.document.location.host,
  imageBaseUrl: "IMAGE_BASE_URL",
  build: "BUILD_NUMBER",
  trackingUrl: "TRACKING_URL",
  trackingSiteId: "TRACKING_SITE_ID",
  imprintData: {
    name: "IMPRINT_NAME",
    addressLine1: "IMPRINT_ADDRESS1",
    addressLine2: "IMPRINT_ADDRESS2",
    city: "IMPRINT_CITY",
    country: "IMPRINT_COUNTRY",
    email: "IMPRINT_EMAIL",
  }
};
