#!/bin/bash

echo "Start generating models and API..."

docker run --rm \
  -v ${PWD}:/local openapitools/openapi-generator-cli generate \
  -i /local/openapi_v3.yaml \
  -g aspnetcore \
  -o /local/tmp/generated_code \
  --additional-properties=packageName=Common.Contracts.Generated,modelNameSuffix=Dto,buildTarget=library \
  --additional-properties=operationModifier=abstract,operationResultTask=true,useNewtonsoft=false,useSwashbuckle=false,useDateTimeOffset=true \
  --additional-properties=nullableReferenceTypes=true,useDataAnnotations=false \
    --global-property=models,apis,supportingFiles \
    --enable-post-process-file


echo "Finished generating models and API."

rm -R ${PWD}/Generated
mv ${PWD}/tmp/generated_code/src/Common.Contracts.Generated/ ${PWD}/Generated
rm -R ${PWD}/tmp

#mv ${PWD}/tmp/generated_code/src/Common.Contracts.Generated/Models ${PWD}/Common.Contracts/Generated
#mv ${PWD}/tmp/generated_code/src/Server.Api.Controllers.Generated/Controllers ${PWD}/Server.Api/Controllers/Generated