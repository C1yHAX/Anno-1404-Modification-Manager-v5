using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows.Data;

namespace AnnoModificationManager5.Language.DictionarySystem
{
    [MarkupExtensionReturnType(typeof(string))]
    public class LanguageMarkupExtension : MarkupExtension
    {
        [ConstructorArgument("Dictionary")]
        public string Dictionary { get; set; }
        [ConstructorArgument("Key")]
        public string Key { get; set; }

        public LanguageMarkupExtension()
        {
        }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return LanguageDictionary.CollectedData[Dictionary][Key].Get;
            }
            catch (Exception)
            {
                return "<Dummy>";
            }
        }
    }
}
