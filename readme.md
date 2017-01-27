# Sitecore.Support.107730.107856
Sometimes the search index may not be updated under the following conditions:
- the `Publishing.PublishEmptyItems` setting is set to `true`
- additional system languages are used
- the published item has a `Shared` field
This patch overrides the standard Sitec`oreItemCrawler to resolve these issues.

## License  
This patch is licensed under the [Sitecore Corporation A/S License for GitHub](https://github.com/sitecoresupport/Sitecore.Support.107730.107856/blob/master/LICENSE).  

## Download  
Downloads are available via [GitHub Releases](https://github.com/sitecoresupport/Sitecore.Support.107730.107856/releases).  
