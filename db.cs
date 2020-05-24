using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
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
        private static IDbConnection db;

        public static void Connect() //连接属性
        {
            switch (TShock.Config.StorageType.ToLower())
            {
                case "mysql":
                    string[] dbHost = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection()
                    {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                            dbHost[0],
                            dbHost.Length == 1 ? "3306" : dbHost[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)

                    };
                    break;

                case "sqlite":
                    string sql = Path.Combine(TShock.SavePath, "tshock.sqlite");
                    db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
                    break;

            }

            SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            sqlcreator.EnsureTableStructure(new SqlTable("POBC",
                new SqlColumn("ID", MySqlDbType.Int32) { Primary = true, Unique = true, Length = 7, AutoIncrement = true },
                new SqlColumn("UserName", MySqlDbType.Text) { Length = 500 },
                new SqlColumn("Currency", MySqlDbType.Int32) { Length = 255 }));
        }

        public static bool Queryuser(string user)
        {
            bool u;
          //  string query = "SELECT * FROM POBC WHERE UserName = @user";
            using (QueryResult reader = db.QueryReader("SELECT * FROM POBC WHERE UserName = @0",user))
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
        public static void UpC(string user, int data)
        {
           // string query = $"UPDATE POBC SET Currency = Currency + {data} WHERE UserName = '{user};";
            db.Query("UPDATE POBC SET Currency = Currency + @0 WHERE UserName = @1",data,user);
        }

        public static void DownC(string user, int data)
        {
          //  string query = $"UPDATE POBC SET Currency = Currency - {data} WHERE UserName = '{user}';";
            db.Query("UPDATE POBC SET Currency = Currency - @0 WHERE UserName = @1",data,user);
        }
        public static void Adduser(string user, int data)
        {
          //  string query = $"INSERT INTO POBC (UserName,Currency) VALUES ('{user}','{data}');";

            db.Query("INSERT INTO POBC (UserName,Currency) VALUES (@0,@1)",user,data);
        }

        public static int QueryCurrency(string user)
        {
            int u;
            //string query = $"SELECT Currency FROM POBC WHERE UserName = '{user}'";
            using (QueryResult reader = db.QueryReader("SELECT Currency FROM POBC WHERE UserName = @0", user))
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