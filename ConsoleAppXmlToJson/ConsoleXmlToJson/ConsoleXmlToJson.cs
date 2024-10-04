using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;
using System.Xml;

namespace ConsoleApp1
{
    public class DictionaryBaseType
    {
        [Display(Name = "Код записи")]
        public int IDCZ { get; set; }

        [Display(Name = "Начало")] 
        public DateTime BeginDate { get; set; }

        [Display(Name = "Окончание")] 
        public DateTime EndDate { get; set; }
        
        [Display(Name = "Тип состояния")]
        public string N_CZ { get; set; } = string.Empty;
    }

    public class DictionaryItem : DictionaryBaseType{}

    public interface IDictionaryXmlReader<T> where T : DictionaryBaseType
    {
        List<T> ReadFromXml(string filePath);
    }

    public interface IDictionaryJsonWriter<T> where T : DictionaryBaseType
    {
        bool WriteToJson(List<T> dictionaryItems, string outputPath);
    }

    public class DictionaryXmlReader : IDictionaryXmlReader<DictionaryItem>
    {
        public List<DictionaryItem> ReadFromXml(string filePath)
        {
            var items = new List<DictionaryItem>();
            
            try
            {
                using (var reader = XmlReader.Create(filePath))
                {
                    var xdoc = XDocument.Load(reader);
                    foreach (var element in xdoc.Descendants("zap"))
                    {
                        var item = new DictionaryItem
                        {
                            IDCZ = int.Parse(element.Element("IDCZ")?.Value ?? "0"),
                            
                            N_CZ = element.Element("N_CZ")?.Value ?? string.Empty ,
                            
                            BeginDate = DateTime.Parse(element.Element("DATEBEG")?.Value ?? string.Empty),
                            
                            EndDate = string.IsNullOrEmpty(element.Element("DATEEND")?.Value) 
                                ? DateTime.MaxValue
                                : DateTime.Parse(element.Element("DATEEND")?.Value ?? string.Empty)
                        };
                        items.Add(item);
                    }
                    
                }
            }

            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при чтении XML файла: {e.Message}");
            }

            return items;
        }
    }

    public class DictionaryJsonWriter : IDictionaryJsonWriter<DictionaryItem>
    {
        public bool WriteToJson(List<DictionaryItem> dictionaryItems, string outputPath)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(dictionaryItems, Newtonsoft.Json.Formatting.Indented);
                
                File.WriteAllText(outputPath, jsonData);
                Console.WriteLine($"Данные успешно записаны в файл {outputPath}");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка при записи JSON файла: {e.Message}");
                return false;
            }
        }
    }

    class Program
    {
        static void Main()
        {

            string xmlFilePath = @"C:\Users\nabiu\RiderProjects\ConsoleAppXmlToJson\ConsoleXmlToJson\V027.xml";
            string jsonFilePath = @"C:\Users\nabiu\RiderProjects\ConsoleAppXmlToJson\ConsoleXmlToJson\testJson.json";

            var xmlReader = new DictionaryXmlReader();
            var jsonWriter = new DictionaryJsonWriter();

            var dictionaryItems = xmlReader.ReadFromXml(xmlFilePath);

            if (dictionaryItems.Count == 0)
            {
                Console.WriteLine("List of elements is empty");
                return;
            }

            jsonWriter.WriteToJson(dictionaryItems, jsonFilePath);
        }
    }
}
