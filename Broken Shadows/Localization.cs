using System.Collections.Generic;
using System.Xml;

namespace Broken_Shadows
{
    public class Localization : Patterns.Singleton<Localization>
    {
        private Dictionary<string, string> translations = new Dictionary<string, string>();

        public void Start(string locFilename)
        {
            translations.Clear();

            using (XmlTextReader reader = new XmlTextReader(locFilename))
            {
                while (reader.Read())
                {
                    if (reader.Name == "text" && reader.NodeType == XmlNodeType.Element)
                    {
                        reader.MoveToAttribute("id");
                        string key = reader.Value;
                        reader.MoveToElement();
                        string value = reader.ReadInnerXml();
                        translations.Add(key, value);
                    }
                }
            }
        }

        public string Text(string Key)
        {
            return translations[Key];
        }
    }
}
