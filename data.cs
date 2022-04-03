using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace POBC2
{
    public class Data
    {
        private readonly Func<int, int> calcExp;

        private readonly int[,] damage = new int[Main.maxNPCs, Main.maxPlayers + 1];

        public Data(Func<int, int> calcExp)
        {
            this.calcExp = calcExp;
        }

        public void ClearPlayer(int index)
        {
            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                damage[i, Main.maxPlayers] += damage[i, index];
                damage[i, index] = 0;
            }
        }

        public void SettleNPC(int index, int infodisplay,string text)
        {
            int s = 0;
            for (int i = 0; i < Main.maxPlayers + 1; ++i)
                s += damage[index, i];

            POBCSystem.Log($"calculatng exp for npc `{Main.npc[index].FullName}` with damages ({string.Join("\t", from i in Enumerable.Range(0, 255) where damage[index, i] != 0 select $"`{Main.player[i].name}`(id={i}) => {damage[index, i]}")})");
            
            for (int i = 0; i < Main.maxPlayers; ++i)
            {
                string account = TShock.Players[i]?.Account?.Name;
                int value = damage[index, i];
                damage[index, i] = 0;
                if (string.IsNullOrEmpty(account)) continue;

                int coin = calcExp(value);
                
                if (coin == 0)
                {
                    continue;
                }
                switch (infodisplay)
                {
                    case 1:
                        if (text.Contains("{Name}"))
                        {
                            text = text.Replace("{Name}", Main.npc[index].GivenOrTypeName);
                        }
                        if (text.Contains("{coin}"))
                        {
                            text = text.Replace("{coin}", coin.ToString());
                        }                     
                        TShock.Players[i].SendWarningMessage(text);
                        break;
                    case 2:
                        if (text.Contains("{Name}"))
                        {
                            text = text.Replace("{Name}", Main.npc[index].GivenOrTypeName);
                        }
                        if (text.Contains("{coin}"))
                        {
                            text = text.Replace("{coin}", coin.ToString());
                        }
                        allmsg(i, text);
                        break;
                    case 3:
                        if (text.Contains("{Name}"))
                        {
                            text = text.Replace("{Name}", Main.npc[index].GivenOrTypeName);
                        }
                        if (text.Contains("{coin}"))
                        {
                            text = text.Replace("{coin}", coin.ToString());
                        }
                        playermsg(i, text);
                        break;
                    default:
                        TShock.Players[i].SendWarningMessage(text);
                        break;
                }
                
                //System.Diagnostics.Debug.WriteLine($"你因击败了{Main.npc[index].GivenOrTypeName}而获得{coin}经验");

                if (Db.Queryuser(account))
                    Db.UpC(account, coin, $"用户击杀了{Main.npc[index].GivenOrTypeName}而获得{coin}经验");
                else
                    Db.Adduser(account, coin,$"创建了POBC 用户:{account} \n ,用户击杀了{Main.npc[index].GivenOrTypeName}而获得{coin}经验");
            }
        }

        public void AddDamage(int npc, int player, int damage)
        {
            this.damage[npc, player] += damage;
        }

        public void allmsg(int id, string msg)
        {
            Random rd = new Random();
            int r = rd.Next(0, 255);
            int g = rd.Next(0, 255);
            int b = rd.Next(0, 255);
            string message = msg;
            Microsoft.Xna.Framework.Color c = new Microsoft.Xna.Framework.Color(r, g, b);
            NetMessage.SendData(119,
    -1, -1, NetworkText.FromLiteral(message), (int)c.PackedValue, TShock.Players[id].X, TShock.Players[id].Y + 50);



        }
        public void playermsg(int id, string msg)
        {
            Random rd = new Random();
            int r = rd.Next(0, 255);
            int g = rd.Next(0, 255);
            int b = rd.Next(0, 255);
            string message = msg;
            Microsoft.Xna.Framework.Color c = new Microsoft.Xna.Framework.Color(r, g, b);

            TShock.Players[id].SendData((PacketTypes)119, message, (int)c.PackedValue, TShock.Players[id].X, TShock.Players[id].Y + 50);



        }
    }
}
