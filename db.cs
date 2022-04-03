using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using TShockAPI;
using TShockAPI.DB;

namespace POBC2
{
    //fixme: defend against the sql injection
    public static class Db
    {
  
        
        public static void Connect() //连接属性
        {


            SqlTableCreator sqlcreator = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("POBC",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 7, AutoIncrement = true },
                new SqlColumn("UserName", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("Currency", MySqlDbType.Int32) { Length = 255 }));
        }
        
        public static bool Queryuser(string user)
        {
            bool u;
          //  string query = "SELECT * FROM POBC WHERE UserName = @user";
            using (QueryResult reader = TShock.DB.QueryReader("SELECT * FROM POBC WHERE UserName = @0",user))
            {
                if (reader.Read())
                {
                    u = true;
                }
                else
                {
                    u = false;
                }
                return u;
            }
        }

        public static void UpC(string user, int data,string str="未填写原因")
        {
            // string query = $"UPDATE POBC SET Currency = Currency + {data} WHERE UserName = '{user};";
            TShock.DB.Query("UPDATE POBC SET Currency = Currency + @0 WHERE UserName = @1",data,user);
            POBCSystem.Log($"\r\n{user}增加了{data}货币 原因:{str}");
        }

        public static void DownC(string user, int data,string str = "未填写原因")
        {
            //  string query = $"UPDATE POBC SET Currency = Currency - {data} WHERE UserName = '{user}';";
            TShock.DB.Query("UPDATE POBC SET Currency = Currency - @0 WHERE UserName = @1",data,user);
            Directory.CreateDirectory(TShock.SavePath + $"\\POBC\\");
            POBCSystem.Log($"\r\n{user}扣除了{data}货币 原因:{str}");
        }
        public static void Adduser(string user, int data ,string str="未填写原因")
        {
            //  string query = $"INSERT INTO POBC (UserName,Currency) VALUES ('{user}','{data}');";

            TShock.DB.Query("INSERT INTO POBC (UserName,Currency) VALUES (@0,@1)",user,data);

            Directory.CreateDirectory(TShock.SavePath + $"\\POBC\\");
            POBCSystem.Log($"\r\n{user}增加了{data}货币 原因:{str}");
        }

        public static int QueryCurrency(string user)
        {
            int u;
            //string query = $"SELECT Currency FROM POBC WHERE UserName = '{user}'";
            using (QueryResult reader = TShock.DB.QueryReader("SELECT Currency FROM POBC WHERE UserName = @0", user))
            {
                if (reader.Read())
                {
                    u = reader.Get<int>("Currency");
                }
                else
                {
                    u = 0;
                }
            }
            return u;
        }
    }
}