using Newtonsoft.Json;
using POBC2;
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

namespace POBC2
{
	[ApiVersion(2, 1)]
	public class POBCSystem : TerrariaPlugin
	{
		#region Info
		public override string Name => "PBOC";

		public override string Author => "欲情";

		public override string Description => "DPS 获取货币系统.";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public string ConfigPath => Path.Combine(TShock.SavePath, "POBC.json");
		public POBCConfin Config = new POBCConfin();
		public string time = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
		#endregion

		#region Initialize

		public Data data;
		public override void Initialize()
		{
			Db.Connect();
			data = new Data(d => (d * 147 / 100) / 2 * Config.Multiple);
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.NpcStrike.Register(this, Dps);
			ServerApi.Hooks.NpcKilled.Register(this, KillID);
			ServerApi.Hooks.NetGetData.Register(this, GetData);//抓包
			PlayerHooks.PlayerPostLogin += Login;
			PlayerHooks.PlayerLogout += UserOut;
			File();
		}


		private void UserOut(PlayerLogoutEventArgs e)
		{
			string n = e.Player.Name;
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
					data.ClearPlayer(playerID);
					//Data.Data.DelUser(n);
				}
			}
		}

		private void Login(PlayerPostLoginEventArgs e)
		{
			//throw new NotImplementedException();
		}

		public void KillID(NpcKilledEventArgs args)
		{
			if (!Config.IsIgnored(args.npc.netID))
			{
				data.SettleNPC(args.npc.whoAmI);
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

			string name = args.Player.name;
			var id = args.Npc.netID;
			var id2 = args.Npc.whoAmI;
			var damage = Math.Min(args.Damage, args.Npc.realLife == -1 ? args.Npc.life : Main.npc[args.Npc.realLife].life);
			// 存在BUG  后面再来摸

			//int BB = Config.Pobcs[0].IgnoreNpc[1];
			if (!Config.IsIgnored(id))
			{
				//todo: 没必要刀刀都记
				System.IO.Directory.CreateDirectory(TShock.SavePath + $"\\POBC\\");
				System.IO.File.AppendAllText(TShock.SavePath + $"\\POBC\\{time}.txt", $" \r\n {name}击败的NPC {id} 获得经验：{damage}");
				//	TShock.Utils.Broadcast(" 你击败的NPC :" + id + "获得经验：" + c, 255, 255, 255);
				data.AddDamage(args.Npc.whoAmI, args.Player.whoAmI, damage);
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
			Commands.ChatCommands.Add(new Command("pobc.up", Pobcup, "pobcup","给钱")
			{
				HelpText = "给与指定玩家一定数量货币."
			});
			Commands.ChatCommands.Add(new Command("pobc.Down", PobcDown, "pobcup", "扣钱")
			{
				HelpText = "减少玩家一定数量货币."
			});
		}

		private void PobcDown(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("语法错误，正确语法：/扣钱  玩家名 货币值");
				return;
			}
			if (!Db.Queryuser(args.Parameters[1]))
			{
				args.Player.SendErrorMessage("未能在POBC用户数据中查找到该玩家:" + args.Parameters[1] + "! 请确认玩家名");
				return;
			}
			if (int.Parse(args.Parameters[2]) < Db.QueryCurrency(args.Parameters[1]))
			{
				args.Player.SendErrorMessage("玩家拥有的货币不够你要减去的货币数，玩家拥有货币数："+Db.QueryCurrency(args.Parameters[1]));
				return;
			}
			Db.DownC(args.Parameters[1], int.Parse(args.Parameters[2]));
		}

		private void Pobcup(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("语法错误，正确语法：/给钱  玩家名 货币值");
				return;
			}
			if (!Db.Queryuser(args.Parameters[1]))
			{
				args.Player.SendErrorMessage("未能在POBC用户数据中查找到该玩家:"+ args.Parameters[1]+"! 请确认玩家名");
				return;
			}
			if (int.Parse(args.Parameters[2])>0)
			{
				args.Player.SendErrorMessage("不能给与玩家负值货币值");
				return;
			}
			Db.UpC(args.Parameters[1], int.Parse(args.Parameters[2]));
		}

		void Query(CommandArgs args)
		{	
	
			var a = Db.QueryCurrency(args.Player.Name);
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

