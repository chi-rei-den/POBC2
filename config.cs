using Newtonsoft.Json;
using System.IO;

namespace POBC
{
	public class POBCConfin
	{
		public Pobccc[] Pobcs = new Pobccc[0];

		public POBCConfin Write(string file)
		{
			File.WriteAllText(file, JsonConvert.SerializeObject(this, Formatting.Indented));
			return this;
		}

		public static POBCConfin Read(string file)
		{
			if (!File.Exists(file))
			{
				WriteExample(file);
			}
			return JsonConvert.DeserializeObject<POBCConfin>(File.ReadAllText(file));
		}

		public static void WriteExample(string file)
		{
			var Ex = new Pobccc()
			{
				Name = "POBC",
				IgnoreNpc = new int[]
				{
					1,-1,
				},
				Multiple = 1
			};
			var Conf = new POBCConfin()
			{
				Pobcs = new Pobccc[] { Ex }
			};
			Conf.Write(file);
		}
	}

	public class Pobccc
	{
		public string Name = string.Empty;
		public int[] IgnoreNpc = new int[0];
		public int Multiple = 0;
	}
}
