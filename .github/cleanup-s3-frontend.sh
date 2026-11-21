
#!/bin/sh

set -eu

if [ "$#" -ne 2 ]; then
    echo "Usage: $0 <bucket-name> <github-build-number>" >&2
    echo "Only files tagged with the build number will remain in the bucket. Makre sure the files are properly tagged with the GitHubBuildNumber metadata before running this script."
    exit 1
fi

BUCKET="$1"
BUILD_NUMBER="$2"

echo "Cleaning up s3://$BUCKET (keeping build $BUILD_NUMBER)"

aws s3api list-objects-v2 \
    --bucket "$BUCKET" \
    --query 'Contents[].Key' \
    --output text |
tr '\t' '\n' |
while IFS= read -r key; do
    [ -z "$key" ] && continue

    METADATA=$(aws s3api head-object \
        --bucket "$BUCKET" \
        --key "$key" \
        --query 'Metadata.GitHubBuildNumber' \
        --output text)

    if [ "$METADATA" != "$BUILD_NUMBER" ]; then
        echo "Deleting: $key (GitHubBuildNumber: $METADATA)"
        aws s3 rm "s3://$BUCKET/$key"
    fi
done

