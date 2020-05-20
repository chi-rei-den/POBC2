using Newtonsoft.Json;
using POBC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace Bank
{
	[ApiVersion(2, 1)]
	public class POBCSystem : TerrariaPlugin
	{

		#region Info
		public override string Name => "PBOC";

		public override string Author => "欲情";

		public override string Description => "DPS 获取货币系统.";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public string ConfigPath { get { return Path.Combine(TShock.SavePath, "POBC.json"); } }
		public POBCConfin Config = new POBCConfin();
		public string time = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
		#endregion

		#region Initialize
		public override void Initialize()
		{
			pobcc.Db.Connect();
			Data.Data.CreateDataTable();
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.NpcStrike.Register(this, Dps);
			ServerApi.Hooks.NpcKilled.Register(this, KillID);
			ServerApi.Hooks.NetGetData.Register(this, GetData);//抓包
			TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += Login;
			PlayerHooks.PlayerLogout += UserOut;
			File();
		}


		private void UserOut(PlayerLogoutEventArgs e)
		{
			string n = e.Player.Name;
			Data.Data.DelUser(n);
		}

		private void GetData(GetDataEventArgs args)
		{
			if (args.MsgID == PacketTypes.PlayerDeathV2)
			{
				args.Msg.reader.BaseStream.Position = args.Index;
				int playerID = args.Msg.whoAmI;
			//	var p = Main.player[playerID];
				var n = Main.player[playerID].name;
				if (n != null)
				{
					Data.Data.DelUser(n);
				}
				



			}
		}

		private void Login(PlayerPostLoginEventArgs e)
		{
			//throw new NotImplementedException();
		}

		public void KillID(NpcKilledEventArgs args)
		{
			if (Array.IndexOf(Config.Pobcs[0].IgnoreNpc, args.npc.netID) == -1)
			{
				Data.Data.DelNpc(args.npc.whoAmI);
			}
		
		}

		private void Dps(NpcStrikeEventArgs args)
		{
			var ply = args.Player.whoAmI;
			//var n = TShock.Players[ply].Name;
			if (!TShock.Players[ply].Group.HasPermission("pobc.c"))
			{
				TShock.Players[ply].SendWarningMessage(" 你没有权限!");
				return;
			}
			var c = (args.Damage * 147 / 100) / 2 * Config.Pobcs[0].Multiple;
			string name = args.Player.name;
			var id =args.Npc.netID;
			var id2 = args.Npc.whoAmI;
			// 存在BUG  后面再来摸
			int B = Array.IndexOf(Config.Pobcs[0].IgnoreNpc, id); // 这里的1就是你要查找的值
			//int BB = Config.Pobcs[0].IgnoreNpc[1];
			if (B == -1)
			{	
				System.IO.Directory.CreateDirectory(TShock.SavePath + $"\\POBC\\");
				System.IO.File.AppendAllText(TShock.SavePath + $"\\POBC\\{time}.txt",$" \r\n {name}击败的NPC {id} 获得经验：{ c}");
			//	TShock.Utils.Broadcast(" 你击败的NPC :" + id + "获得经验：" + c, 255, 255, 255);
				Data.Data.Add(name, id2, c, id);
			}
			else
			{

			//	TShock.Utils.Broadcast(" 你击败的NPC 已被屏蔽 NPC ID :" + id, 255, 255, 255);
			}

				


		}
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.NpcStrike.Deregister(this, Dps);
				ServerApi.Hooks.NpcKilled.Deregister(this, KillID);
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);//抓包
				PlayerHooks.PlayerPostLogin -= Login;
				PlayerHooks.PlayerLogout -= UserOut;

			}
			base.Dispose(disposing);
		}
		#endregion

		public POBCSystem(Main game)
			: base(game)
		{
			Order = 10;
		}

		void OnInitialize(EventArgs args) //添加命令
		{

			Commands.ChatCommands.Add(new Command("pobc.query", Query, "查询")
			{
				HelpText = "查询您当前拥有的货币数量."
			});
		}

		void Query(CommandArgs args)
		{	
	
			var a = pobcc.Db.QueryCurrency(args.Player.Name);
			args.Player.SendWarningMessage(" 您当前拥有货币数： " + a );



			}

		public void File()
		{
			try
			{
				Config = POBCConfin.Read(ConfigPath).Write(ConfigPath);
			}
			catch (Exception ex)
			{
				Config = new POBCConfin();
				TShock.Log.ConsoleError("[POBC] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
			}

			
		


		}




	}



}

