using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Broken_Shadows
{
    public class Localization : Patterns.Singleton<Localization>
    {
        Dictionary<string, string> _translations = new Dictionary<string, string>();

        public void Start(string locFilename)
        {
            _translations.Clear();

            using (XmlTextReader reader = new XmlTextReader(locFilename))
            {
                while (reader.Read())
                {
                    if (reader.Name == "text" && reader.NodeType == XmlNodeType.Element)
                    {
                        reader.MoveToAttribute("id");
                        string key = reader.Value;
                        reader.MoveToElement();
                        // Grab everything from this element INCLUDING any other markup
                        // so it can be parsed later on
                        string value = reader.ReadInnerXml();
                        _translations.Add(key, value);
                    }
                }
            }
        }

        public string Text(string Key)
        {
            return _translations[Key];
        }
    }
}
