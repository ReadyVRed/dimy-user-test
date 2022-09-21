using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;

namespace dimy_user_test.Services
{
    public static class DbHelper
    {
        static readonly string _connectionString = "Server=localhost;Port=3306;Database=test;Uid=root;Pwd=123456;Allow User Variables=True";
        public static DataSet ExecuteDataset(string commandString, Dictionary<string, string> parameters)
        {
            try
            {
                using MySqlConnection myConnection = new(_connectionString);
                myConnection.Open();
                using MySqlCommand myCommand = new(commandString, myConnection);
                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters.ElementAt(i);
                    myCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                using MySqlDataAdapter da = new(myCommand);
                using DataSet ds = new();
                da.Fill(ds);
                return ds;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static int ExecuteNonQuery(string commandString, Dictionary<string, string> parameters)
        {
            try
            {
                using MySqlConnection myConnection = new(_connectionString);
                using MySqlTransaction myTrans = myConnection.BeginTransaction();
                using MySqlCommand myCommand = myConnection.CreateCommand();
                myCommand.Connection = myConnection;
                myCommand.Transaction = myTrans;
                myConnection.Open();
                try
                {
                    myCommand.CommandText = commandString;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var parameter = parameters.ElementAt(i);
                        myCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                    int rowAffected = myCommand.ExecuteNonQuery();
                    myTrans.Commit();
                    return rowAffected;
                }
                catch (Exception)
                {
                    myTrans.Rollback();
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static T GetColumnValue<T>(DataSet ds, string column, int row = 0, T nullValue = default)
        {
            try
            {
                var value = ds.Tables[0].Rows[row][column];
                var type = default(T);
                if (type is bool)
                {
                    if (value.Equals(1))
                        value = true;
                    else
                        value = false;
                }
                if (Convert.IsDBNull(value))
                    return nullValue;
                else
                    return (T)Convert.ChangeType(value, typeof(T), CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static bool CheckDataset(DataSet ds, int rowCount = 0, int table = 0)
        {
            try
            {
                if (ds.Tables[table].Rows.Count > rowCount)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
