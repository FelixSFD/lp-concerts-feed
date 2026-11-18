# Agent instructions for LPshows / LP Concert Calendar

## General Instructions

- try to follow the general structure of the existing project. This includes folder structure and naming conventions
- when writing code, make sure to properly document it with comments. At least add comments to public functions and classes, and any complex logic that may not be immediately clear to someone reading the code for the first time.
- The application is a web application that allows users to view and manage Linkin Park concerts. The main features include viewing upcoming concerts, adding new concerts, viewing setlists and subscribing to an iCal feed
- The frontend is built using Angular
- The backend is split into different parts
  - The newest API is a Docker container. The API routes for that are prefixed with `/v3`
  - The backend container also includes periodic jobs using Quartz
  - Some features are using AWS. However, most of the Lambda functions will be replaced with the new API in the future
- both the API and the frontend are deployed behind CloudFront distributions
- The AWS configuration is done using SAM templates. The templates are located in the `aws` folder. The templates are used to create the necessary resources in AWS, such as Lambda functions, API Gateway endpoints, and DynamoDB tables.

## Testing instructions

- avoid writing unit tests for the frontend for now. Instead, the frontend can be tested using the Safari MCP server
- The backend should be well tested
  - Repository classes should use DbIntegratedTests
  - Service classes should be tested with unit tests. The repositories must be mocked for that
