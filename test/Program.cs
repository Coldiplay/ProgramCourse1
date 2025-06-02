namespace test
{
    public class Program
    {
        static void Main(string[] args)
        {
            /*
            WebClient client = new();
            HttpClient client2 = new();
            
            var xml = client.DownloadString("https://www.cbr-xml-daily.ru/daily.xml");
            xml = client2.GetStringAsync();
            XDocument xdoc = XDocument.Parse(xml);
            var el = xdoc.Element("valcurs").Elements("valute");
            string dollar = el.Where(x => x.Attribute("id").Value == "r01235").Select(x => x.Element("value").Value).FirstOrDefault();
            string eur = el.Where(x => x.Attribute("id").Value == "r01239").Select(x => x.Element("value").Value).FirstOrDefault();
            Console.WriteLine(($"евро: {eur} доллар: {dollar}"));
            */
            
        }

    }
}
