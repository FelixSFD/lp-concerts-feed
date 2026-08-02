#!/bin/bash

echo "Start generating models and API..."

docker run --rm \
  -v ${PWD}:/local:rw openapitools/openapi-generator-cli generate \
  -i /local/openapi_v3.yaml \
  -g aspnetcore \
  -o /local/tmp/generated_code \
  --additional-properties=packageName=Common.Contracts.Generated,modelNameSuffix=Dto \
  --additional-properties=operationModifier=abstract,classModifier=abstract,operationResultTask=true,operationIsAsync=false,useNewtonsoft=false,useSwashbuckle=false,useDateTimeOffset=true \
  --additional-properties=nullableReferenceTypes=true,useDataAnnotations=false \
    --global-property=models,supportingFiles \
    --enable-post-process-file


echo "Finished generating models and API."

rm -R ${PWD}/Generated
mkdir "Generated"
mv ${PWD}/tmp/generated_code/src/Common.Contracts.Generated/Attributes ${PWD}/Generated/Attributes
mv ${PWD}/tmp/generated_code/src/Common.Contracts.Generated/Converters ${PWD}/Generated/Converters
mv ${PWD}/tmp/generated_code/src/Common.Contracts.Generated/Models ${PWD}/Generated/Models
rm -R ${PWD}/tmp
