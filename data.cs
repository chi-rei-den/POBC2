
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using TShockAPI;

namespace Data
{




    public static class Data
    {

        public static DataTable dt = new DataTable("NewDt");


        public static DataTable CreateDataTable()
        {
            //创建DataTable
         //   DataTable dt = new DataTable("NewDt");

            //创建自增长的ID列
            DataColumn dc = dt.Columns.Add("ID", Type.GetType("System.Int32"));
            dc.AutoIncrement = true;   //自动增加
            dc.AutoIncrementSeed = 1;  //起始为1
            dc.AutoIncrementStep = 1;  //步长为1
            dc.AllowDBNull = false;    //非空

            //创建其它列表
            dt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("NpcID", Type.GetType("System.Int32")));
            dt.Columns.Add(new DataColumn("D", Type.GetType("System.Int32")));
            dt.Columns.Add(new DataColumn("NetID", Type.GetType("System.Int32")));
            dt.Columns.Add(new DataColumn("CreateTime", Type.GetType("System.DateTime")));
            dt.CaseSensitive = true;


            DataRow dr = dt.NewRow();
            dr["Name"] = "占位符-*/56+40.6+-*";
            dr["NpcID"] = "99999999";
            dr["D"] = "0";
            dr["NetID"] = "99999999";
            dr["CreateTime"] = DateTime.Now;
            dt.Rows.Add(dr);

            return dt;
        }
        public static void Add(string name,int npcid ,int d,int netid)
        {

            DataRow dr = dt.NewRow();
            dr["Name"] = name;
            dr["NpcID"] = npcid;
            dr["D"] = d;
            dr["NetID"] = netid;
            dr["CreateTime"] = DateTime.Now;
            dt.Rows.Add(dr);


        }
        public static void DelUser(string Name)
        {
            if (dt.Rows.Count>=0)
            {
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    if (Convert.ToString(dt.Rows[i]["Name"]) == Name)
                        dt.Rows.RemoveAt(i);
                }
            }


        }
       public static void DelNpc(int NpcID)		

        {
            string Name;
            int D = 0;
            int NetID;

            for (int i = dt.Rows.Count -1; i >= 0; i--)
            {
                if (Convert.ToInt32(dt.Rows[i]["NpcID"]) == NpcID)
                {
                    Name = Convert.ToString(dt.Rows[i]["Name"]);
                    NetID = Convert.ToInt32(dt.Rows[i]["NetID"]);
                   var CreateTime = Convert.ToDateTime(dt.Rows[i]["CreateTime"]);

                    for (int i2 = dt.Rows.Count - 1; i2 >= 0; i2--)
                    {
                        if ((Convert.ToInt32(dt.Rows[i2]["NpcID"]) == NpcID) && (Convert.ToString(dt.Rows[i2]["Name"]) == Name) && dt.Rows.Count >= 0)
                        {
                            D = D + Convert.ToInt32(dt.Rows[i2]["D"]);
                            dt.Rows.RemoveAt(i2);
                            i = i2;

            


                        }
                        else
                        {

                            if (pobcc.Db.Queryuser(Name))
                            {
                                pobcc.Db.UpC(Name, D);
                                //   PobcLog(Name + "在"  + " 击杀了" + NetID + "获得了" + " " + D + " 货币" + CreateTime);
                                D = 0;
                            }
                            else
                            {
                                pobcc.Db.Adduser(Name, D);
                                //  PobcLog(Name + "在"  + " 击杀了" + NetID + "获得了" + " " + D + " 货币" + CreateTime);
                                D = 0;
                            }
                        }
                    }
                }
            }
        }



    }

}
       