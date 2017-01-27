using Sitecore.Collections;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Pipelines.GetContextIndex;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.LanguageFallback;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Sitecore.Support.ContentSearch
{
    public class SitecoreItemCrawler : Sitecore.ContentSearch.SitecoreItemCrawler
    {
        public override void Update(IProviderUpdateContext context, IIndexableUniqueId indexableUniqueId, IndexEntryOperationContext operationContext, IndexingOptions indexingOptions = IndexingOptions.Default)
        {
            Assert.ArgumentNotNull(indexableUniqueId, "indexableUniqueId");
            if (!base.ShouldStartIndexing(indexingOptions))
            {
                return;
            }
            IDocumentBuilderOptions documentOptions = base.DocumentOptions;
            Assert.IsNotNull(documentOptions, "DocumentOptions");
            if (this.IsExcludedFromIndex(indexableUniqueId, true))
            {
                return;
            }
            if (operationContext != null)
            {
                if (operationContext.NeedUpdateChildren)
                {
                    Item item = Sitecore.Data.Database.GetItem(indexableUniqueId as SitecoreItemUniqueId);
                    if (item != null)
                    {
                        this.UpdateHierarchicalRecursive(context, item, CancellationToken.None);
                        return;
                    }
                }
                if (operationContext.NeedUpdatePreviousVersion)
                {
                    Item item2 = Sitecore.Data.Database.GetItem(indexableUniqueId as SitecoreItemUniqueId);
                    if (item2 != null)
                    {
                        this.UpdatePreviousVersion(item2, context);
                    }
                }
                // Sitecore.Support.107730.107856
                if (operationContext.NeedUpdateAllVersions)
                {
                    var item = Sitecore.Data.Database.GetItem(indexableUniqueId as SitecoreItemUniqueId);
                    if (item != null)
                    {
                        this.DoUpdate(context, item, operationContext);
                        return;
                    }
                }
            }
            SitecoreIndexableItem indexableAndCheckDeletes = this.GetIndexableAndCheckDeletes(indexableUniqueId);
            if (indexableAndCheckDeletes != null)
            {
                this.DoUpdate(context, indexableAndCheckDeletes, operationContext);
                return;
            }
            if (this.GroupShouldBeDeleted(indexableUniqueId.GroupId))
            {
                this.Delete(context, indexableUniqueId.GroupId, IndexingOptions.Default);
                return;
            }
            this.Delete(context, indexableUniqueId, IndexingOptions.Default);
        }

        private void UpdatePreviousVersion(Item item, IProviderUpdateContext context)
        {
            Sitecore.Data.Version[] array;
            using (new WriteCachesDisabler())
            {
                array = (item.Versions.GetVersionNumbers() ?? new Sitecore.Data.Version[0]);
            }
            int num = array.ToList<Sitecore.Data.Version>().FindIndex((Sitecore.Data.Version version) => version.Number == item.Version.Number);
            if (num < 1)
            {
                return;
            }
            Sitecore.Data.Version previousVersion = array[num - 1];
            Sitecore.Data.Version version2 = array.FirstOrDefault((Sitecore.Data.Version version) => version == previousVersion);
            ItemUri uri = new ItemUri(item.ID, item.Language, version2, item.Database.Name);
            Item item2 = Sitecore.Data.Database.GetItem(uri);
            SitecoreIndexableItem sitecoreIndexableItem = item2;
            if (sitecoreIndexableItem != null)
            {
                IIndexableBuiltinFields indexableBuiltinFields = sitecoreIndexableItem;
                indexableBuiltinFields.IsLatestVersion = false;
                sitecoreIndexableItem.IndexFieldStorageValueFormatter = context.Index.Configuration.IndexFieldStorageValueFormatter;
                base.Operations.Update(sitecoreIndexableItem, context, this.index.Configuration);
            }
        }
    }
}