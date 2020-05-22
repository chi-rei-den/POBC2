using System;
using Terraria;
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
                damage[i, Main.maxPlayers] = damage[i, index];
                damage[i, index] = 0;
            }
        }

        public void SettleNPC(int index)
        {
            int s = 0;
            for (int i = 0; i < Main.maxPlayers + 1; ++i)
                s += damage[index, i];
            for (int i = 0; i < Main.maxPlayers; ++i)
            {
                string account = TShock.Players[i]?.Account?.Name;
                int value = damage[index, i];
                damage[index, i] = 0;
                if (string.IsNullOrEmpty(account)) continue;

                int coin = calcExp(value);

                TShock.Players[i].SendInfoMessage($"你因击败了{Main.npc[index].GivenOrTypeName}而获得{coin}经验");

                if (Db.Queryuser(account))
                    Db.UpC(account, coin);
                else
                    Db.Adduser(account, coin);
            }
        }

        public void AddDamage(int npc, int player, int damage)
        {
            this.damage[npc, player] += damage;
        }
    }
}
