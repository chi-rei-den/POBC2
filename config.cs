using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace POBC2
{
	public class POBCConfin
	{
		public int[] IgnoreNpc = new int[0];
		public int Multiple = 1;

		//todo: implement with hashset
		public bool IsIgnored(int npctype)
		{
			return IgnoreNpc.Contains(npctype);
		}
		public POBCConfin Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static POBCConfin Read(string file)
		{
			if (!File.Exists(file))
			{
				return new POBCConfin();
			}
			return JsonConvert.DeserializeObject<POBCConfin>(File.ReadAllText(file));
		}

	}
}
