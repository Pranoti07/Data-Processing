# Data-Processing
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.IO.Compression;
/// <summary>
/// Summary description for DataProcessingClass
/// </summary>
public class DataProcessingClass
{
    static DBClass db = new DBClass();
    static Settings obj = new Settings();
    public static RSAPSS obj1 = new RSAPSS();
	public static void Initialize()
	{
		//
		// TODO: Add constructor logic here
		//
        DataTable dt = db.GetTable("select * from Rules");
        if (dt.Rows.Count > 0)
        {
            obj.Threeshold = dt.Rows[0][1].ToString();
            obj.TimeInterval = dt.Rows[0][2].ToString();
            obj.MinRange = dt.Rows[0][3].ToString();
            obj.MaxRange = dt.Rows[0][4].ToString();
        }
        else
        {
            obj = null;
        }


	}
    public static void DataMonitoring()
    {
        DataTable dt = db.GetTable("select * from temp");
        if (dt.Rows.Count > 0)
        {
            if (dt.Rows.Count > int.Parse(obj.Threeshold))
            {
                db.Execute("Insert into Log (msg,ldatetime) )values('Data Monitoring Alert: Exceeded Limit...','"+DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt")+"')");
            }

        }
        

    }
    public static void DataFiltering()
    {
        DataTable dt = db.GetTable("select * from temp");
        for (int i = 0; i < dt.Rows.Count; i++)
        {
            
            if(!(double.Parse(dt.Rows[i][1].ToString())>double.Parse(obj.MinRange) &&
                double.Parse(dt.Rows[i][1].ToString()) < double.Parse(obj.MaxRange)))
            {
                db.Execute("delete from temp where t1=" + dt.Rows[i][0].ToString() + " and t2=" + dt.Rows[i][0].ToString());
                db.Execute("Insert into Log (msg,ldatetime) values('" + dt.Rows[i][0] + " exceeded range...','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "')");
            }

        }       

    }
    public static void DataCompression()
    {
        String lines = "";
        DataTable dt = db.GetTable("select * from temp");
        foreach (DataRow dr in dt.Rows)
        {
            lines += dr[1].ToString() + "," + dr[2].ToString() + "\r\n";
        }
        String path=HttpContext.Current.Server.MapPath("~/data1.txt");
        String path1 = HttpContext.Current.Server.MapPath("~/data1.cmp");
        File.WriteAllText(path, lines);

        //Compress data using Huffman coding and LZ77 for Loss less compression
        FileStream os = File.OpenRead(path);
        FileStream cs = File.Create(path1);
        DeflateStream ds = new DeflateStream(cs, CompressionMode.Compress);
        os.CopyTo(ds);
        ds.Close();
        os.Close();
        cs.Close();
        FileInfo f1 = new FileInfo(path);
        FileInfo f2 = new FileInfo(path1);
        db.Execute("Insert into Log values('Compressed Data1.txt from " + f1.Length+ " To "+f2.Length +" ..','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "')");
 
    }
    public static void DataDecompression()
    {
        String path = HttpContext.Current.Server.MapPath("~/data1.cmp");
        String path1 = HttpContext.Current.Server.MapPath("~/data2.txt");
        

        //DeCompress data using Huffman coding and LZ77 for Loss less compression
        FileStream os = File.OpenRead(path);
        FileStream cs = File.Create(path1);
        DeflateStream ds = new DeflateStream(os, CompressionMode.Decompress);
        ds.CopyTo(cs);
        ds.Close();
        os.Close();
        cs.Close();
        FileInfo f1 = new FileInfo(path);
        FileInfo f2 = new FileInfo(path1);
        db.Execute("Insert into Log (msg,ldatetime) values('Decompressed Data1.cmp from " + f1.Length + " To " + f2.Length + " ..','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "')");

    }

    public static byte[] SignData()
    {
        String path1 = HttpContext.Current.Server.MapPath("~/data2.txt");
        byte[] b = File.ReadAllBytes(path1);

        
        obj1.SignData(b);

        db.Execute("Insert into Log (msg,ldatetime) values('Data is signed ...','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "')");
        return obj1.signed;

    }
    public static bool VerifySignData(byte []signed)
    {
        String path1 = HttpContext.Current.Server.MapPath("~/data2.txt");
        byte[] b = File.ReadAllBytes(path1);

        db.Execute("Insert into Log values('Data is verified using sign ...','" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "')");
        return obj1.VerifyDataUsingSign(b, signed);

    }
}
