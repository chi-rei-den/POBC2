using ReLogic.Peripherals.RGB;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
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
		internal static Action<string> Log;
		private StreamWriter sw;

		#region Info
		public override string Name => "PBOC";

		public override string Author => "欲情,冲冲";

		public override string Description => "DPS 获取货币系统.";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
		public string ConfigPath => Path.Combine(TShock.SavePath, "POBC.json");
		public POBCConfig Config = new POBCConfig();
		//public string time = DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");    yuqing : 日志整体移动到DB Sheet
		#endregion

		#region Initialize

		public Data data;
		public override void Initialize()
		{
			File();
			Db.Connect();
			data = new Data(d => (d * 147 / 100) / 2 * Config.Multiple);
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.NpcStrike.Register(this, Dps);
			ServerApi.Hooks.NpcKilled.Register(this, KillID);
			ServerApi.Hooks.NetGetData.Register(this, GetData);//抓包
			PlayerHooks.PlayerPostLogin += Login;
			PlayerHooks.PlayerLogout += UserOut;

		}


		private void UserOut(PlayerLogoutEventArgs e)
		{
			string n = e.Player.Name;
		}

		private void GetData(GetDataEventArgs args)
		{
			if (!Config.PlayerKillFine)
			{
				return;
			}
			if (args.MsgID == PacketTypes.PlayerDeathV2)
			{
				//args.Msg.reader.BaseStream.Position = args.Index;  yuqing： 不知道干啥的 看上去没用直接//
				int playerID = args.Msg.whoAmI;
				//	var p = Main.player[playerID];                    yuqing： 取消了P 变量
				string n = Main.player[playerID].name;
				if (n != null)
				{
					data.ClearPlayer(playerID);
					//Data.Data.DelUser(n);                        yuqing： 冲冲 说玩家死亡保留玩家缓存伤害 
					var c = Math.Round(Db.QueryCurrency(n) * Config.DeductionPercentage);
					Db.DownC(n, (int)c, "您因死亡被而被扣除");
					TShock.Players[playerID].SendWarningMessage(" 您因死亡被而被扣除：" + (int)c + " 货币");
				}




			}
		}

		private void Login(PlayerPostLoginEventArgs e)
		{
			data.ClearPlayer(e.Player.Index); //clear the previous player data in case the damage count is inherited to other players.
			//throw new NotImplementedException();
		}

		public void KillID(NpcKilledEventArgs args)
		{
			if(!Config.IsIgnored(args.npc.netID))
			{
				data.SettleNPC(args.npc.whoAmI,Config.infodisplay,Config.displaytext);
			}
		}

		private void Dps(NpcStrikeEventArgs args)
		{
			Log($"player `{args.Player.name}` dealt {args.Damage} to `{args.Npc.FullName}`");

			int ply = args.Player.whoAmI;
			//var n = TShock.Players[ply].Name;
			if (!TShock.Players[ply].Group.HasPermission("pobc.c"))
			{
				//TShock.Players[ply].SendWarningMessage(" 你没有权限!");   yuqing： 取消打怪权限提示
				return;
			}

			string name = args.Player.name;
			int id = args.Npc.netID;
			int id2 = args.Npc.whoAmI;
			//int damage = Math.Min(args.Damage, args.Npc.realLife == -1 ? args.Npc.life : Main.npc[args.Npc.realLife].life);
			int damage = args.Damage;
			var life = args.Npc.realLife == -1 ? args.Npc.life : Main.npc[args.Npc.realLife].life;
			// 存在BUG  后面再来摸                yuqing： 取消打怪权限提示
			// overflow damage may cause to unexpected result.

			if (life > 0) damage = Math.Min(damage, life);

			//int BB = Config.Pobcs[0].IgnoreNpc[1];
			if (!Config.IsIgnored(id))
			{
				//todo: 没必要刀刀都记
				/*System.IO.Directory.CreateDirectory(TShock.SavePath + $"\\POBC\\");                     yuqing : 移动至 DB Sheet
				System.IO.File.AppendAllText(TShock.SavePath + $"\\POBC\\{time}.txt", $" \r\n {name}击败的NPC {id} 获得经验：{damage}");
				//	TShock.Utils.Broadcast(" 你击败的NPC :" + id + "获得经验：" + c, 255, 255, 255);     yuqing： 这里应该是每刀的数据 缓存*/
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
				sw.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion

		public POBCSystem(Main game)
			: base(game)
		{
			Order = 10;
		}

		private void OnInitialize(EventArgs args) //添加命令
		{

			Commands.ChatCommands.Add(new Command("pobc.query", Query, "查询")
			{
				HelpText = "查询您当前拥有的货币数量."
			});
			Commands.ChatCommands.Add(new Command("pobc.up", Pobcup, "pobcup", "给钱")
			{
				HelpText = "给与指定玩家一定数量货币."
			});
			Commands.ChatCommands.Add(new Command("pobc.Down", PobcDown, "pobcDown", "扣钱")
			{
				HelpText = "减少玩家一定数量货币."
			});
			Commands.ChatCommands.Add(new Command("pobc.Pay", Pay, "pay", "支付")
			{
				HelpText = "玩家支付给玩家货币."
			});

			Directory.CreateDirectory(Path.Combine(TShock.SavePath, "POBC"));

			sw = new StreamWriter(new FileStream(Path.Combine(TShock.SavePath, "POBC", $"{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.txt"), FileMode.Append));

			Log = s =>
			{
				var st = new StackTrace();
				var method = st.GetFrame(1).GetMethod();
				sw.WriteLine($"[{DateTime.Now:hh-mm-ss}]\t[{method.DeclaringType.FullName}::{method.Name}]\t{s}");
				sw.Flush();
			};
		}

        private void Pay(CommandArgs args)
        {
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("语法错误，正确语法：/支付  玩家名 货币值");
				return;
			}
			if (!Db.Queryuser(args.Parameters[0]))
			{
				args.Player.SendErrorMessage("未能在POBC用户数据中查找到该玩家:" + args.Parameters[0] + "! 请确认玩家名");
				return;
			}
			if (int.Parse(args.Parameters[1]) <1)
			{
				args.Player.SendErrorMessage("支付的货币不能小于 1" );
				return;
			}
			if (int.Parse(args.Parameters[1]) > Db.QueryCurrency(args.Player.Name))
			{
				args.Player.SendErrorMessage("您拥有的货币不够你要支付的货币数，拥有货币数：" + Db.QueryCurrency(args.Player.Name));
				return;
			}
			Db.DownC(args.Player.Name, int.Parse(args.Parameters[1]), "支付货币给与玩家");
			Db.UpC(args.Parameters[0], int.Parse(args.Parameters[1]), "玩家支付货币给与您");
			args.Player.SendErrorMessage($"您支付了{args.Parameters[1]}货币给与玩家 {args.Parameters[0]}，当前拥有货币数：" + Db.QueryCurrency(args.Player.Name));
			for (int i = 0; i <TShock.Utils.GetActivePlayerCount() ; i++)
			{
				if (TShock.Players[i].Name == args.Parameters[0])
                {
					TShock.Players[i].SendErrorMessage($"玩家{args.Player.Name}支付了{args.Parameters[1]}货币给与您，当前拥有货币数：" + Db.QueryCurrency(TShock.Players[i].Name));
					break;
				}
            }
           
		}

        private void PobcDown(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("语法错误，正确语法：/扣钱  玩家名 货币值");
				return;
			}
			if (!Db.Queryuser(args.Parameters[0]))
			{
				args.Player.SendErrorMessage("未能在POBC用户数据中查找到该玩家:" + args.Parameters[0] + "! 请确认玩家名");
				return;
			}
			if (int.Parse(args.Parameters[1]) > Db.QueryCurrency(args.Parameters[0]))
			{
				args.Player.SendErrorMessage("玩家拥有的货币不够你要减去的货币数，玩家拥有货币数：" + Db.QueryCurrency(args.Parameters[0]));
				return;
			}
			Db.DownC(args.Parameters[0], int.Parse(args.Parameters[1]),"管理员扣除了您的货币");
		}

		private void Pobcup(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("语法错误，正确语法：/给钱 玩家名 货币值");
				return;
			}
			if (!Db.Queryuser(args.Parameters[0]))
			{
				args.Player.SendErrorMessage("未能在POBC用户数据中查找到该玩家:" + args.Parameters[0] + "! 请确认玩家名");
				return;
			}
			if (int.Parse(args.Parameters[1]) < 0)
			{
				args.Player.SendErrorMessage("不能给与玩家负值货币值");
				return;
			}
			Db.UpC(args.Parameters[0], int.Parse(args.Parameters[1]),"管理员增加了您的货币");
		}

		private void Query(CommandArgs args)
		{
			int a = Db.QueryCurrency(args.Player.Name);
			args.Player.SendWarningMessage(" 您当前拥有货币数： " + a);

		}

		public void File()
		{
			try
			{
				Config = POBCConfig.Read(ConfigPath).Write(ConfigPath);
			}
			catch (Exception ex)
			{
				Config = new POBCConfig();
				TShock.Log.ConsoleError("[POBC] 读取配置文件发生错误!\n{0}".SFormat(ex.ToString()));
			}
		}


	}
}

