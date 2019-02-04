using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class DataController
{
    public readonly char delimiter = ',';
    public readonly string[] separator = { "," };

    public abstract class ListBase
    {
        public abstract bool TryLoadTable(string[] str);
	}
	
	public abstract class DicBase
	{
        public abstract bool TryLoadTable(string[] str, out int key);
	}

    public abstract class MixedBase
    {
        public abstract bool TryLoadTable(string[] str, out int key);
    }
    
	public void ParsingTableList<T>(TextAsset txt, ref List<T> list) where T : ListBase, new()
	{
		string[] strArray = ParsingData(txt);
        list.Clear();

        for (int i = 0, max = strArray.Length; i < max; i++)
        {
            if (i == 0)
            {
                continue;
            }
            string[] strElement = strArray[i].Split(delimiter);

            T t = new T();
            bool bLoadeSuccess = t.TryLoadTable(strElement);
            if (bLoadeSuccess)
            {
                list.Add(t);
            }
        }
    }

    public void ParsingTableDic<T>(TextAsset txt, ref Dictionary<int, T> dic) where T : DicBase, new()
    {
        string[] strArray = ParsingData(txt);
        dic.Clear();

        for (int i = 0, max = strArray.Length; i < max; i++)
        {
            if (i == 0)
            {
                continue;
            }
            string[] strElement = strArray[i].Split(delimiter);

            T t = new T();
            int key;
            bool bLoadeSuccess = t.TryLoadTable(strElement, out key);
            if (bLoadeSuccess)
            {
                dic.Add(key, t);
            }
        }
    }

    public void ParsingTableMixed<T>(TextAsset txt, ref Dictionary<int, List<T>> mix) where T : MixedBase, new()
    {
        string[] strArray = ParsingData(txt);
        mix.Clear();

        for (int i = 0, max = strArray.Length; i < max; i++)
        {
            if (i == 0)
            {
                continue;
            }
            string[] strElement = strArray[i].Split(delimiter);

            T t = new T();
            int key;
            bool bLoadeSuccess = t.TryLoadTable(strElement, out key);
            if (bLoadeSuccess)
            {
                if (!mix.ContainsKey(key))
                    mix.Add(key, new List<T>());

                mix[key].Add(t);
            }
        }
    }

    public string[] ParsingData(TextAsset txt)
	{
		return ParsingData(txt.text);
	}

	public string[] ParsingData(string data)
	{
		string dataText = data.TrimEnd('\n');
		string[] strArray = dataText.Split('\n');
		for(int i=0, max=strArray.Length; i<max; i++)
		{
			strArray[i] = strArray[i].Trim('\r');
		}
		return strArray;
	}
	
	public string ReadFile(string path)
	{
		if(File.Exists(path) == false)
		{
			return null;
		}
		
		byte[] b;
		FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		int length = (int)fs.Length;
		b = new byte[ length ];
		int count;
		int sum = 0;	    
		while((count = fs.Read(b, sum, length - sum) ) > 0)
		{
			sum += count;
		}
		fs.Close();
		return Encoding.UTF8.GetString(b);
	}

	public void WriteFile(string path, string data)
	{
		byte[] b = null;
		b = Encoding.UTF8.GetBytes( data );
		FileStream fs = new FileStream(path, FileMode.Create);
		fs.Write(b, 0, b.Length);
		fs.Close();
	}

    public string[] GetStringElements(string str) { return str.Split(delimiter); }

    public void SaveData(string key, string data)
    {
        //추후 암호화. //
        ObscuredPrefs.SetString(key, data);
    }

    public string LoadData(string key)
    {
        //추후 복호화. //
        if (ObscuredPrefs.HasKey(key))
            return ObscuredPrefs.GetString(key);
        return null;
    }

    public void ResetData()
    {
        ObscuredPrefs.DeleteAll();
    }
}
