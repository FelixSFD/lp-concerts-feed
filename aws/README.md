# How to deploy to AWS

The deployment is split into different stacks that depend on each other. The number in the filename defines the order.
We don't have to update all stacks every time, so not all files will be used when deploying via GitHub.

## Prerequisites:

Before deploying the first stack, a Hosted Zone for the domain name needs to be added in Route53 manually.
This domain name is the `BaseDomainName` for the stacks. At the moment, 2 DNS names will be used:

* `[BaseDomainName].`: This is where the frontend will be available
* `api.[BaseDomainName]`: The domain to connect to the API

After creating the hosted zone, copy the ID, as this will be used in some of the following stacks.

The stacks use a parameter called `BaseProductName`. This will be tre prefix of exported resources and should be the same for all stacks of one single installation.


## General process

To deploy a stack, first build the deployment using this command: `sam build -t [Deployment file]`

After that, run this command to deploy it and enter the parameters for the first time: `sam deploy --guided`

If you saved the configuration, use this command for _further updates_ to the stack: `sam deploy --config-file [saved toml file]`

## Deployments

### SSL Certificates

The first stack that needs to be deployed is `01-certificates-deployment.yaml`. This stack **must be deployed in _us-east-1_!**
It contains the certificate used by the API Gateway and CloudFront.

After deploying this, make sure to copy the ARN of the generated certificate.


### Backend

`05-backend-deployment.yaml` contains the stack for the backend. This includes database, Cognito, the API gateway and Lambda functions.

_This stack will be deployed automatically in `develop` and `main` branches._


### Frontend

The frontend is an Angular application which will be hosted in a S3 bucket behind CloudFront.
The deployment `10-frontend-deployment.yaml` creates this bucket.
File upload is not handled by this and must be done after the deployment!

Example upload: `aws s3 sync ./dist/lp-calendar-web/browser s3://lpshows-test-web-frontend-bucket`


### CloudFront

The whole application runs behind two CloudFront distributions. One for S3 and one in front of the API Gateway.
With the deployment `20-cloudfront-deployment.yaml`, both distributions are created.

You will need the ARN of the previously generated certificate for this deployment. Make sure you deployed the certificate in `us-east-1`!
However, the CloudFront doesn't have to be in that zone. The restriction is just for the certificate.


### DNS Records

The hosted zone that was created manually doesn't contain the DNS entries to CloudFront yet as this requires CF and S3 to already be created.
With the deployment `30-dns-deployment.yaml`, all remaining DNS records are created and the application should now be accessible.
