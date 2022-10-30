using System.Text.Json;
using System.IO;
namespace genshin_rounding
{
    public class ReliquaryAffixExcelConfigData
    {
        public int id { get; set; }
        public int depotId { get; set; }
        public int groupId { get; set; }
        public string propType { get; set; }
        public float propValue { get; set; }
    }

    class Program
    {
        static readonly string[] flatList = { "FIGHT_PROP_HP", "FIGHT_PROP_ATTACK", "FIGHT_PROP_DEFENSE", "FIGHT_PROP_ELEMENT_MASTERY" };
        static void Main(string[] args)
        {
            var docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dirName = $"{docPath}/GenshinData";
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
                    data.TryAdd(rank, new Dictionary<string, List<float>>());
                    var dict = data[rank];
                    dict.TryAdd(rdata.propType, new List<float>());
                    var list = dict[rdata.propType];
                    list.Add(rdata.propValue);
                }
                string outDir = $"{docPath}/GenshinGenerated";
                if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                File.WriteAllText($"{outDir}/subStatsRolls.json", JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
                var table = new Dictionary<int, Dictionary<string, Dictionary<string, List<List<float>>>>>();
                foreach (var rankDict in data) {
                    var rank = rankDict.Key;
                    var dict = rankDict.Value;
                    table.TryAdd(rank, new Dictionary<string, Dictionary<string, List<List<float>>>>());
                    var tableRank = table[rank];
                    foreach (var propTypeList in dict) {
                        var propType = propTypeList.Key;
                        var list = propTypeList.Value;
                        tableRank.TryAdd(propType, new Dictionary<string, List<List<float>>>());
                        var tableProp = tableRank[propType];
                        var numUpgrades = rank == 2 ? 2 : rank + 1;
                        var combs = getAllCombsAndPerms(list, numUpgrades);
                        foreach (var rolls in combs) {
                            var sum = rolls.Sum();
                            var rounded = flatList.Contains(propType) ? string.Format("{0:#,0}", sum) : string.Format("{0:0.0}", sum * 100);
                            tableProp.TryAdd(rounded, new List<List<float>>());
                            var roundedList = tableProp[rounded];
                            roundedList.Add(rolls.ToList());
                        }
                    }
                }
                File.WriteAllText($"{outDir}/rollTable.json", JsonSerializer.Serialize(table, new JsonSerializerOptions { WriteIndented = true }));
            } else Console.WriteLine($@"Expect Directory at '{dirName}'");
        }
        static IEnumerable<IEnumerable<T>> getAllCombsAndPerms<T>(IEnumerable<T> list, int length) where T : IComparable
        {
            IEnumerable<IEnumerable<T>> values = new IEnumerable<T>[] { };
            IEnumerable<IEnumerable<T>> GetKCombsWithRept<T>(IEnumerable<T> list, int length, ref IEnumerable<IEnumerable<T>> all) where T : IComparable
            {
                if (length == 1) return list.Select(t => new T[] { t });
                var combs = GetKCombsWithRept(list, length - 1, ref all);
                all = all.Concat(combs);
                return combs.SelectMany(t => list.Where(o => o.CompareTo(t.Last()) >= 0), (t1, t2) => t1.Concat(new T[] { t2 }));
            }
            var combs = GetKCombsWithRept(list, length, ref values);
            values = values.Concat(combs);

            return values;
        }
    }
}

