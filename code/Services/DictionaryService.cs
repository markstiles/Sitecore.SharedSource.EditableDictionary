using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using Sitecore.Data.Items;
using Sitecore;
using Sitecore.Data.Managers;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Data;
using System.Linq.Expressions;
using System.Web;
using System.Web.Caching;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Caching;

namespace Sitecore.SharedSource.EditableDictionary.Services
{
    public class DictionaryService
    {
        public string RenderTextDisplay(string text, string key)
        {
            return RenderTextDisplay(text, "", key);
        }

        public string RenderTextDisplay(string text, string dictionaryDomain, string key)
        {
            if (!Context.PageMode.IsExperienceEditorEditing)
                return text;

            Item currentItem = Context.Database.GetItem(GetDictionaryEntries(dictionaryDomain)[key]);
            if (currentItem == null)
                return text;

            var d = new Sitecore.Mvc.Common.EditFrame(currentItem.Paths.FullPath, "Dictionary", "Dictionary Phrase", "Edit Phrase", null, null);
            var t = new StringWriter();
            var h = new HtmlTextWriter(t);
            d.RenderFirstPart(h);
            h.Write(text);
            d.RenderLastPart(h);
            var showIcon = Registry.GetValue("/Current_User/Page Editor/Show/DictionaryItems") == "on";
            var visibility = showIcon ? "" : "visibility: hidden;";
            h.Write($"<span class='dictionary-item' style='{visibility}'>{ThemeManager.GetImage("People/16x16/book_red.png", 16, 16)}</span>");

            return t.ToString();
        }
        
        public Dictionary<string, string> GetDictionaryItems()
        {
            return GetDictionaryEntries("");
        }
        
        public Dictionary<string, string> GetDictionaryEntries(string dictionaryDomain)
        {
            var entryKey = $"DictionaryEntryKey-{dictionaryDomain}-{Context.Database.Name}";
            var isInCache = HttpRuntime.Cache[entryKey] != null;
            if (isInCache)
                return (Dictionary<string, string>)HttpRuntime.Cache[entryKey];

            var domains = GetDictionaryDomains();
            var predicate = PredicateBuilder.False<SearchResultItem>();
            
            //if domain is specified find only items in that domain or fallbacks
            if (!string.IsNullOrWhiteSpace(dictionaryDomain) && domains.ContainsKey(dictionaryDomain))
            {
                var domain = domains[dictionaryDomain];
                foreach(var d in domain)
                {
                    predicate = predicate.Or(a => a.Paths.Contains(d));
                }
            }                
            else // else default to built in dictionary
            {
                var defaultDictionaryId = new ID("{504AE189-9F36-4C62-9767-66D73D6C3084}");
                predicate = predicate.Or(a => a.Paths.Contains(defaultDictionaryId));
            }

            var entryItems = GetSearchItems(predicate);
            var entryDictionary = new Dictionary<string, string>();
            foreach (var entry in entryItems)
            {
                var key = entry.Fields["Key"]?.Value;
                if (string.IsNullOrWhiteSpace(key) || entryDictionary.ContainsKey(key))
                    continue;

                entryDictionary.Add(key, entry.ID.Guid.ToString());
            }

            HttpRuntime.Cache.Add(entryKey, entryDictionary, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);

            return entryDictionary;
        }
                
        public Dictionary<string, List<ID>> GetDictionaryDomains()
        {
            var domainKey = $"DictionaryDomainKey-{Context.Database.Name}";
            var isInCache = HttpRuntime.Cache[domainKey] != null;
            if (isInCache)
                return (Dictionary<string, List<ID>>)HttpRuntime.Cache[domainKey];

            var predicate = PredicateBuilder.False<SearchResultItem>();
            predicate = predicate.Or(item => item.TemplateName == "Dictionary Domain");
            var domainItems = GetSearchItems(predicate);

            var d = new Dictionary<string, List<ID>>();
            foreach (var dom in domainItems)
            {
                var list = GetDomainFallbacks(dom);
                d.Add(dom.DisplayName, list);
            }

            HttpRuntime.Cache.Add(domainKey, d, null, DateTime.UtcNow.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);

            return d;
        }

        protected List<ID> GetDomainFallbacks(Item dictionaryDomain)
        {
            var list = new List<ID> { dictionaryDomain.ID };
            var fallbackField = dictionaryDomain.Fields["Fallback Domain"];
            if (fallbackField == null)
                return list;

            var fallbackId = fallbackField.Value;
            if (!ID.IsID(fallbackId))
                return list;

            var fallbackItem = dictionaryDomain.Database.GetItem(new ID(fallbackId));
            if (fallbackItem == null)
                return list;

            var ids = GetDomainFallbacks(fallbackItem);
            list.AddRange(ids);

            return list;
        }

        protected List<Item> GetSearchItems(Expression<Func<SearchResultItem, bool>> predicate)
        {
            var indexName = $"sitecore_{Context.Database.Name}_index";
            var index = ContentSearchManager.GetIndex(indexName);
            using (var context = index.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                return context.GetQueryable<SearchResultItem>()
                    .Where(predicate)
                    .Select(a => a.GetItem())
                    .ToList();
            }
        }
    }
}