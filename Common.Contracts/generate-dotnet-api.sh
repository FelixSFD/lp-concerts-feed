#!/bin/bash

if stat -c '%u:%g' . >/dev/null 2>&1; then
    owner=$(stat -c '%u:%g' .)
else
    owner=$(stat -f '%u:%g' .)
fi

echo "Current directory owner: $owner"

echo "Start generating models and API..."

mkdir -p tmp/generated_code 

docker run --rm \
  --user "$owner" \
  -v ./:/local:rw openapitools/openapi-generator-cli generate \
  -i /local/openapi_v3.yaml \
  -g aspnetcore \
  -o /local/tmp/generated_code \
  --additional-properties=packageName=Common.Contracts.Generated,modelNameSuffix=Dto \
  --additional-properties=operationModifier=abstract,classModifier=abstract,operationResultTask=true,operationIsAsync=false,useNewtonsoft=false,useSwashbuckle=false,useDateTimeOffset=true \
  --additional-properties=nullableReferenceTypes=true,useDataAnnotations=false \
    --global-property=models,supportingFiles \
    --enable-post-process-file


echo "Finished generating models and API."

rm -R ./Generated
mkdir "Generated"

mv ./tmp/generated_code/src/Common.Contracts.Generated/Attributes ./Generated/Attributes
mv ./tmp/generated_code/src/Common.Contracts.Generated/Converters ./Generated/Converters
mv ./tmp/generated_code/src/Common.Contracts.Generated/Models ./Generated/Models

rm -R ./tmp
