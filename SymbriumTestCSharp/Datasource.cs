using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SymbriumTestCSharp
{
    // Class where we retrieve the data from the database file
    class Datasource
    {
        public const string TABLE_MEASUREMENTS = "Measurements";
        public SQLiteConnection conn;

        // Method to open the data source
        // Path for the database is obtained as input from the Console
        public bool open()
        {
            try
            {
                Console.WriteLine("Enter the Data Source");
                String source = Console.ReadLine();
                conn = new SQLiteConnection("Data Source=" + source);
                conn.Open();
                return true;
            } 
            catch (SQLiteException e)
            {
                Console.Write("Couldn't connect to database: " + e.Message);
                return false;
            }
        }

        // Method to close the data source when finished
        public void close()
        {
            try
            {
                if(conn != null)
                {
                    conn.Close();
                }
            } catch(SQLiteException e)
            {
                Console.Write("Couldn't close connection: " + e.Message);
            }
        }


        // Query the database for all the data of the measurements
        // and save into a list of Measurement objects
        // X and Y are not truly needed, but we are saving them anyway
        public List<Measurement> queryMeasurements()
        {
            List<Measurement> measurements = new List<Measurement>();

            try
            {
                string sql = "SELECT * FROM " + TABLE_MEASUREMENTS;
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    double x = reader.GetDouble(1);
                    Measurement measurement = new Measurement(reader.GetInt16(0), reader.GetInt16(1),
                        reader.GetFloat(2), reader.GetFloat(3), reader.GetDouble(4));
                    measurements.Add(measurement);
                }

            }
            catch (SQLiteException e)
            {
                Console.WriteLine("Query failed: " + e.Message);
            }
            return measurements;
        }
    }
}
