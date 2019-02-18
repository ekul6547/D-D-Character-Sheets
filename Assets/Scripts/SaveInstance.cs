using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Data.Sqlite;
using System.Data;

namespace SheetCreator
{

    public class SaveInstance : MonoBehaviour
    {

        public static SaveInstance active;

        public static string[] separator = new string[] { "[|]" };

        static string conn;
        IDbConnection data;

        public string settingsPath;
        public string dbFileName = "CharacterData.db";

        public void Awake()
        {
            settingsPath = Application.streamingAssetsPath + "/Settings/";
            active = this;
            conn = "URI=file:" + settingsPath + dbFileName;
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(settingsPath);
            }
        }
        void OnApplicationQuit()
        {
            Close();
        }

        public void LoadDB()
        {
            if (!File.Exists(conn))
            {
                SqliteConnection.CreateFile(conn);
            }
            try
            {
                data = (IDbConnection) new SqliteConnection(conn);
                Debug.Log(data.State.ToString());
                data.Open();
                Create();
            }
            catch (SqliteException e)
            {
                Debug.LogError(e);
                ContentManager.active.AddMessage("Database error");
            }
        }

        void Create()
        {
            IDbCommand command = data.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS characters (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,name TEXT UNIQUE,isnpc INTEGER,password TEXT,canview INTEGER)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS strings (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,variablename TEXT,charid INTEGER,value TEXT)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS floats (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,variablename TEXT,charid INTEGER,value REAL)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS bools (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,variablename TEXT,charid INTEGER,value INTEGER)";
            command.ExecuteNonQuery();
            command.CommandText = "CREATE TABLE IF NOT EXISTS stats (id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,variablename TEXT,charid INTEGER,value_a INTEGER,value_b INTEGER)";
            command.ExecuteNonQuery();

            command.Dispose();
        }

        public void Close()
        {
            Debug.Log("Attempting to Close Database");
            if (data == null)
            {
                Debug.Log("Database Null - not initiated");
                return;
            }
            if (data.State != ConnectionState.Closed)
            {
                data.Close();
                Debug.Log("Database closed successfully");
            }
            else
            {
                Debug.Log("Database already closed or an error has occured");
            }
        }

        public void NewCharacter(int CharID, string characterName,bool npc,string password="", bool canViewWithoutPassword=false)
        {
            int id = CharID;
            IDbCommand cmd = data.CreateCommand();
            if (id == -1)
            {
                cmd.CommandText = String.Format("INSERT INTO characters VALUES (NULL,'{0}',{1},{2},{3})",
                    characterName,
                    BoolToInt(npc),
                    password,
                    BoolToInt(password == "" ? true : canViewWithoutPassword));
                cmd.ExecuteNonQuery();
            }
            else
            {
                cmd.CommandText = String.Format("UPDATE characters SET name='{1}', isnpc={2}, password='{3}', canview={4} WHERE id={0}",
                    id,
                    characterName,
                    BoolToInt(npc),
                    password,
                    BoolToInt(password == "" ? true : canViewWithoutPassword));
                cmd.ExecuteNonQuery();
            }
            cmd.Dispose();
        }

        public int getIdFromName(string characterName)
        {
            int ret = 0;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT id FROM characters WHERE name LIKE '{0}' COLLATE NOCASE LIMIT 1", characterName);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ret = reader.GetInt32(0);
            }
            cmd.Dispose();
            return ret;
        }
        public string getNameFromID(int charID)
        {
            string ret = "";
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT name FROM characters WHERE id = '{0}' LIMIT 1", charID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ret = reader.GetString(0);
            }
            cmd.Dispose();
            return ret;
        }
        public bool getNPCFromID(int charID)
        {
            bool ret = true;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT isnpc FROM characters WHERE id = '{0}' LIMIT 1", charID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ret = IntToBool(reader.GetInt16(0));
            }
            cmd.Dispose();
            return ret;
        }

        //Can be viewed with given password
        public bool CanView(int CharID, string checkPassword)
        {
            string password = "";
            bool view = false;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT password,canview FROM characters WHERE id={0} LIMIT 1", CharID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) //Is password null?
                    return true;
                password = reader.GetString(0);
                view = IntToBool(reader.GetInt16(1));
            }
            if (view || password == "") //Is password blank? OR canViewWithoutPassword is true
                return true;
            return password == checkPassword; //Are the passwords the same
        }
        //Can be saved with given password
        public bool CanSave(int CharID, string checkPassword)
        {
            string password = "";
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT password FROM characters WHERE id={0} LIMIT 1", CharID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) //Is password null?
                    return true;
                password = reader.GetString(0);
            }
            if (password == "") //Is password blank?
                return true;
            return password == checkPassword; //Are the passwords the same
        }
        //Requires a password to load
        public bool NeedsPasswordForLoad(int CharID)
        {
            string password = "";
            bool view = false;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT password,canview FROM characters WHERE id={0} LIMIT 1", CharID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) //Is password null?
                    return false;
                password = reader.GetString(0);
                view = IntToBool(reader.GetInt16(1));
            }
            if (view || password == "") //Is password blank?
                return false;
            return true; //Password can't be blank or null, must have a password
        }
        //Requires a password to save
        public bool NeedsPasswordForSave(int CharID)
        {
            string password = "";
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = String.Format("SELECT password FROM characters WHERE id={0} LIMIT 1", CharID);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.IsDBNull(0)) //Is password null?
                    return false;
                password = reader.GetString(0);
            }
            if (password == "") //Is password blank?
                return false;
            return true; //Password can't be blank or null, must have a password
        }

        public string GetString(int characterid, string variableName)
        {
            string ret = "";
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT value FROM strings WHERE charid={0} AND variablename='{1}'", characterid, variableName);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.GetValue(0) == DBNull.Value)
                return null;
            while (reader.Read())
            {
                ret = reader.GetString(0);
            }
            cmd.Dispose();
            return ret;
        }
        public void CreateString(int characterid, string variableName)
        {
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO strings VALUES (NULL,'{1}',{0},NULL)", characterid, variableName);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        public void SaveString(int characterid, string variableName, string value)
        {
            if(GetString(characterid,variableName) == null)
            {
                CreateString(characterid, variableName);
            }
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("UPDATE strings SET value='{2}' WHERE charid={0} AND variablename='{1}'", characterid, variableName, value);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public float? GetFloat(int characterid, string variableName)
        {
            float ret = 0.0f;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT value FROM floats WHERE charid={0} AND variablename='{1}'", characterid, variableName);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.GetValue(0) == DBNull.Value)
                return null;
            while (reader.Read())
            {
                ret = reader.GetFloat(0);
            }
            cmd.Dispose();
            return ret;
        }
        public void CreateFloat(int characterid, string variableName)
        {
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO floats VALUES (NULL,'{1}',{0},NULL)", characterid, variableName);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        public void SaveFloat(int characterid, string variableName, float value)
        {
            if (GetFloat(characterid, variableName) == null)
            {
                CreateFloat(characterid, variableName);
            }
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("UPDATE floats SET value='{2}' WHERE charid={0} AND variablename='{1}'", characterid, variableName, value);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public bool? GetBool(int characterid, string variableName)
        {
            bool ret = false;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT value FROM bools WHERE charid={0} AND variablename='{1}'", characterid, variableName);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.GetValue(0) == DBNull.Value)
                return null;
            while (reader.Read())
            {
                ret = reader.GetInt32(0) == 1 ? true : false;
            }
            cmd.Dispose();
            return ret;
        }
        public void CreateBool(int characterid, string variableName)
        {
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO bools VALUES (NULL,'{1}',{0},NULL)", characterid, variableName);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        public void SaveBool(int characterid, string variableName, bool value)
        {
            if (GetBool(characterid, variableName) == null)
            {
                CreateBool(characterid, variableName);
            }
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("UPDATE bools SET value='{2}' WHERE charid={0} AND variablename='{1}'", characterid, variableName, value ? 1 : 0);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        public Vector2? GetStat(int characterid, string variableName)
        {
            Vector2 ret = Vector2.zero;
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT value_a,value_b FROM stats WHERE charid={0} AND variablename='{1}'", characterid, variableName);
            IDataReader reader = cmd.ExecuteReader();
            if (reader.GetValue(0) == DBNull.Value)
                return null;
            while (reader.Read())
            {
                ret = new Vector2(reader.GetInt32(0), reader.GetInt32(0));
            }
            cmd.Dispose();
            return ret;
        }
        public void CreateStat(int characterid, string variableName)
        {
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO stats VALUES (NULL,'{1}',{0},NULL,NULL)", characterid, variableName);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        public void SaveStat(int characterid, string variableName, int value_a,int value_b)
        {
            if (GetStat(characterid, variableName) == null)
            {
                CreateStat(characterid, variableName);
            }
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("UPDATE stats SET value_a='{2}',value_b='{3}' WHERE charid={0} AND variablename='{1}'", characterid, variableName, value_a,value_b);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }
        
        public string[] getAllStrings(int characterid)
        {
            List<string> strings = new List<string>();
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT variablename,value FROM strings WHERE charid={0}",characterid);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                strings.Add(reader.GetString(0) + separator[0] + reader.GetString(1));
            }
            cmd.Dispose();
            return strings.ToArray();
        }
        public string[] getAllFloats(int characterid)
        {
            List<string> strings = new List<string>();
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT variablename,value FROM floats WHERE charid={0}", characterid);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                strings.Add(reader.GetString(0) + separator[0] + reader.GetFloat(1).ToString());
            }
            cmd.Dispose();
            return strings.ToArray();
        }
        public string[] getAllBools(int characterid)
        {
            List<string> strings = new List<string>();
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT variablename,value FROM bools WHERE charid={0}", characterid);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                strings.Add(reader.GetString(0) + separator[0] + reader.GetInt32(1).ToString());
            }
            cmd.Dispose();
            return strings.ToArray();
        }
        public string[] getAllStats(int characterid)
        {
            List<string> strings = new List<string>();
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = string.Format("SELECT variablename,value_a,value_b FROM stats WHERE charid={0}", characterid);
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                strings.Add(reader.GetString(0) + separator[0] + reader.GetInt32(1).ToString() + separator[0] + reader.GetInt32(2).ToString());
            }
            cmd.Dispose();
            return strings.ToArray();
        }
        public struct data_struct
        {
            public string name;
            public bool isNPC;
            public string[] strings;
            public string[] floats;
            public string[] bools;
            public string[] stats;
        }

        public string getAllData(int characterid)
        {
            data_struct dt;
            dt.name = getNameFromID(characterid);
            dt.isNPC = getNPCFromID(characterid);
            dt.strings = getAllStrings(characterid);
            dt.floats = getAllFloats(characterid);
            dt.bools = getAllBools(characterid);
            dt.stats = getAllStats(characterid);
            
            return TurnToJson(dt,false);
        }


        public string GetJsonFromFile(string fileName, string Folder)
        {
            string path = Application.dataPath + "/Settings/" + Folder + "/" + fileName + ".cfg";
            if (File.Exists(path))
            {
                string F = File.ReadAllText(path);
                Debug.Log("Loaded to json at " + path + " with data: " + F);
                return F;
            }
            return "";
        }
        public string TurnToJson(object Instance, bool neat)
        {
            string json = null;
            try
            {
                json = JsonUtility.ToJson(Instance, neat);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error with loading settings file: " + e);
            }
            return json;
        }
        public void WriteToFile(string json, string FolderPath, string FileName)
        {
            var filePath = FolderPath + "/" + FileName;
            Debug.Log("Save settings file at " + filePath + " with data: " + json);
            if (Directory.Exists(FolderPath) == false)
            {
                Directory.CreateDirectory(FolderPath);
            }
            using (var fs = File.CreateText(filePath))
            {
                fs.Write(' ');
            }
            File.WriteAllText(filePath, json);
        }
        public int[] FindAllIds(string FolderPath)
        {
            List<int> ids = new List<int>();
            IDbCommand cmd = data.CreateCommand();
            cmd.CommandText = "SELECT id FROM characters ORDER BY isnpc DESC";
            IDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            cmd.Dispose();
            return ids.ToArray();
        }

        public static string Format(object[] string_data)
        {
            string ret = string_data[0].ToString();
            for(int i = 1; i < string_data.Length; i ++)
            {
                ret += separator[0] + string_data[i].ToString();
            }
            return ret;
        }

        bool IntToBool(int i)
        {
            return i > 0;
        }
        int BoolToInt(bool b)
        {
            return b ? 1 : 0;
        }
    }
}