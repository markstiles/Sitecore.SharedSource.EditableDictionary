using System;
using Sitecore.Globalization;
using Sitecore.SharedSource.EditableDictionary.Services;

namespace Sitecore.SharedSource.EditableDictionary.Statics
{
    public static class EditableTranslate
    {
        #region Text

        public static string Text(string key)
        {
            var text = Translate.Text(key);

            var dService = new DictionaryService();

            return dService.RenderTextDisplay(text, key);
        }

        public static string Text(string key, params object[] parameters)
        {
            var text = Translate.Text(key, parameters);

            var dService = new DictionaryService();

            return dService.RenderTextDisplay(text, key);
        }

        #endregion

        #region Text By Domain

        public static string TextByDomain(string dictionaryDomain, string key)
        {
            var text = Translate.TextByDomain(dictionaryDomain, key);

            var dService = new DictionaryService();

            return dService.RenderTextDisplay(text, dictionaryDomain, key);
        }

        public static string TextByDomain(string dictionaryDomain, string key, params object[] parameters)
        {
            var text = Translate.TextByDomain(dictionaryDomain, key, parameters);

            var dService = new DictionaryService();

            return dService.RenderTextDisplay(text, dictionaryDomain, key);
        }

        public static string TextByDomain(string dictionaryDomain, TranslateOptions options, string key, params object[] parameters)
        {
            var text = Translate.TextByDomain(dictionaryDomain, options, key, parameters);

            var dService = new DictionaryService();

            return dService.RenderTextDisplay(text, dictionaryDomain, key);
        }

        #endregion
    }
}