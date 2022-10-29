using System.Text.Json;
using System.IO;
namespace genshin_rounding
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class ReliquaryAffixExcelConfigData
    {
        public int id { get; set; }
        public int depotId { get; set; }
        public int groupId { get; set; }
        public string propType { get; set; }
        public float propValue { get; set; }
    }

    public class MainStatRank
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dirName = $"{docPath}/GenshinData";
            //Console.WriteLine($@"Checking dir '{dirName}'");
            if (Directory.Exists(dirName)) {
                string fileName = $"{dirName}/ExcelBinOutput/ReliquaryAffixExcelConfigData.json";
                string fileStr = File.ReadAllText(fileName);

                var reliquaryAffixExcelConfigDataArr = JsonSerializer.Deserialize<List<ReliquaryAffixExcelConfigData>>(fileStr);
                if (reliquaryAffixExcelConfigDataArr == null) {
                    return;
                }
                var data = new Dictionary<int, Dictionary<string, List<float>>>();
                foreach (var rdata in reliquaryAffixExcelConfigDataArr) {
                    int rank = rdata.depotId / 100;
                    if (rank > 5) continue;
                    System.Console.WriteLine($"{rank} {rdata.propType} {rdata.propValue}");
                    data.TryAdd(rank, new Dictionary<string, List<float>>());
                    var dict = data[rank];
                    dict.TryAdd(rdata.propType, new List<float>());
                    var list = dict[rdata.propType];
                    list.Add(rdata.propValue);
                }
                var jstring = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jstring);
                string outDir = $"{docPath}/GenshinParsed";
                // If directory does not exist, create it
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                File.WriteAllText($"{outDir}/subStatsRolls.json", jstring);
            } else {
                Console.WriteLine($@"Expect Directory at '{dirName}'");
            }
        }
    }
}
