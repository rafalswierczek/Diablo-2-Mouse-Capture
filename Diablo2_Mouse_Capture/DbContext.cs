using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace Diablo2_Mouse_Capture
{
    public class DbContext
    {
        private readonly SQLiteConnection _db;

        public DbContext()
        {
            try
            {
                _db = new SQLiteConnection($@"Data Source={Directory.GetCurrentDirectory()}\D2MC.db; Version = 3; New = True; Compress = True;");
                _db.Open();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | DbContext() | {e.Message}{Environment.NewLine}");
            }
        }

        public void CreateTable()
        {
            try
            {
                SQLiteCommand cmd = _db.CreateCommand();
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Handles(hwnd VARCHAR(20))";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | CreateTable() | {e.Message}{Environment.NewLine}");
            }
        }

        public void InsertData(IntPtr hwnd)
        {
            try
            {
                SQLiteCommand cmd = _db.CreateCommand();
                cmd.CommandText = $"INSERT INTO Handles(hwnd) VALUES({hwnd.ToString()})";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | InsertData() | {e.Message}{Environment.NewLine}");
            }
        }

        public void DeleteData(IntPtr hwnd)
        {
            try
            {
                SQLiteCommand cmd = _db.CreateCommand();
                cmd.CommandText = $"DELETE FROM Handles WHERE hwnd = {hwnd.ToString()}";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | DeleteData() | {e.Message}{Environment.NewLine}");
            }
        }

        public void DeleteAllData()
        {
            try
            {
                SQLiteCommand cmd = _db.CreateCommand();
                cmd.CommandText = $"DELETE FROM Handles";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | DeleteAllData() | {e.Message}{Environment.NewLine}");
            }
        }

        public List<IntPtr> FetchData()
        {
            try
            {
                SQLiteCommand cmd = _db.CreateCommand();
                cmd.CommandText = "SELECT * FROM Handles";

                SQLiteDataReader reader = cmd.ExecuteReader();
                List<IntPtr> output = new List<IntPtr>();
                while (reader.Read())
                {
                    output.Add(new IntPtr(Convert.ToInt32(reader["hwnd"].ToString())));
                }

                return output;
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | FetchData() | {e.Message}{Environment.NewLine}");
                return null;
            }
        }

        public void Dispose()
        {
            try
            {
                _db.Dispose();
            }
            catch (Exception e)
            {
                File.AppendAllText("D2MC_log.log", $"{DateTime.Now} | Dispose() | {e.Message}{Environment.NewLine}");
            }
}
    }
}
