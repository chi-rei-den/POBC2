using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace POBC2
{
	public class POBCConfig
	{
		public int[] IgnoreNpc = new int[0];
		public int infodisplay = 1;
		public string displaytext = "你因击败了{Name}而获得{coin}经验";
		public int Multiple = 1;
		public bool PlayerKillFine = true;
		public double DeductionPercentage = 0.1;

		//todo: implement with hashset
		public bool IsIgnored(int npctype)
		{
			return IgnoreNpc.Contains(npctype);
		}
    
		public POBCConfig Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static POBCConfig Read(string file)
		{
			if (!File.Exists(file))
			{
				return new POBCConfig();
			}
			return JsonConvert.DeserializeObject<POBCConfig>(File.ReadAllText(file));
		}
	}
}