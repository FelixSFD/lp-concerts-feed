# Service.SharePics

This service project is responsible for generating share-pics for users.
It uses Scriban to fill HTML templates and passes it to Gotenberg to generate the image files from the filled templates.


## How to create a new share-pic template

1. Create a new directory in `Templates` with the name of your template.
2. add an index.html file to that directory. This file will be used as the template for generating the share-pic.
3. Add any additional assets (like images, CSS files, etc.) to the same directory. These assets will be copied to the output directory when generating the share-pic.

