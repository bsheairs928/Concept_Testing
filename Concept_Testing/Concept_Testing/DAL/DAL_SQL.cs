using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Concept_Testing.DAL
{
    /// <summary>
    /// This is a Singleton class.  There will only ever be one instance of this class.
    /// Information about this design http://csharpindepth.com/Articles/General/Singleton.aspx this is the 4th Version
    /// </summary>
    class DAL_SQL
    {
        private string connstrMSSQL;
        private SqlConnection connMSSQL;
        private SqlTransaction trans;
        #region Singleton Properties      
        private static readonly DAL_SQL inst = new DAL_SQL();
        static DAL_SQL()
        {
        }

        private DAL_SQL()
        {
        }

        public static DAL_SQL Inst
        {
            get
            {
                return inst;
            }
        }

        #endregion

        /// <summary>
        /// Used to 
        /// </summary>
        /// <param name="ConnString"></param>
        public void InitConnStr(string ConnString)
        {
            connstrMSSQL = ConnString;
        }

        /// <summary>
        /// This is used to prevent multiple calls to the DB from causing each other to fail
        /// </summary>
        private object lockConn = new Object();


        #region Update Database

        /// <summary>
        /// Method to update multiple tables in the database all within one transaction.  if one update fails all updates are rolled back
        /// </summary>
        /// <param name="lstArgs">A list of args classes, one class for each table that is being updated</param>
        /// <returns>Returns wheather or not the command modified any rows across all commands run</returns>
        public bool PerformUpdate(List<DAL_args> lstArgs)
        {
            lock (lockConn)
            {
                bool isSuccess = true;
                int intEffectivedRowCount = 0;
                if (connMSSQL == null)
                {
                    connMSSQL = new SqlConnection(connstrMSSQL);
                }
                using (connMSSQL)
                {
                    if (connMSSQL.State != ConnectionState.Open)
                    {
                        connMSSQL.ConnectionString = connstrMSSQL;
                        connMSSQL.Open();
                    }
                    try
                    {
                        trans = connMSSQL.BeginTransaction();
                        SqlDataAdapter da = new SqlDataAdapter("", connMSSQL);
                        foreach (DAL_args Arg in lstArgs)
                        {
                            if (Arg.cmdExecuteSQL == null)
                            {
                                if (isSuccess)
                                {
                                    if (Arg.cmdUpdate != null)
                                    {
                                        Arg.cmdUpdate.Connection = connMSSQL;
                                        Arg.cmdUpdate.Transaction = trans;
                                        da.UpdateCommand = Arg.cmdUpdate;
                                    }

                                    if (Arg.cmdInsert != null)
                                    {
                                        Arg.cmdInsert.Connection = connMSSQL;
                                        Arg.cmdInsert.Transaction = trans;
                                        da.InsertCommand = Arg.cmdInsert;
                                    }

                                    if (Arg.cmdDelete != null)
                                    {
                                        Arg.cmdDelete.Connection = connMSSQL;
                                        Arg.cmdDelete.Transaction = trans;
                                        da.DeleteCommand = Arg.cmdDelete;
                                    }

                                    //\\\\\\\\\\\\\\\\\\\ Update DB
                                    if (Arg.capturePrimaryKey) da.RowUpdated += OnRowUpdated;

                                    intEffectivedRowCount = da.Update(Arg.dt);
                                    isSuccess = (intEffectivedRowCount > 0 & isSuccess);
                                    if (Arg.capturePrimaryKey) da.RowUpdated -= OnRowUpdated;
                                }
                            }
                            else
                            {
                                Arg.cmdExecuteSQL.Connection = connMSSQL;
                                Arg.cmdExecuteSQL.Transaction = trans;
                                intEffectivedRowCount = Arg.cmdExecuteSQL.ExecuteNonQuery();
                                if (!Arg.cmdExecuteSQL.CommandText.ToUpper().StartsWith("DELETE FROM"))
                                {
                                    isSuccess = (intEffectivedRowCount > 0 & isSuccess);
                                }

                            }

                        }

                        if (trans != null && (trans.Connection != null) && trans.Connection.State == ConnectionState.Open)
                        {
                            if (isSuccess) trans.Commit(); else trans.Rollback(); ;
                        }

                    }
                    catch (Exception ex)
                    {
                        if (trans != null && (trans.Connection != null) && (trans.Connection.State == ConnectionState.Open)) trans.Rollback();
                        isSuccess = false;
                        throw ex;
                    }
                    finally
                    {
                        if (connMSSQL.State != ConnectionState.Closed) connMSSQL.Close();
                    }
                }
                return isSuccess;
            }
        }

        /// <summary>
        /// Overload method to allow a single update 
        /// </summary>
        /// <param name="Arg">Class instance with all information loaded in order to update the database.</param>
        /// <returns>Returns wheather or not the command modified any rows across all commands run</returns>
        public bool PerformUpdate(DAL_args Arg)
        {
            List<DAL_args> lstArgs = new List<DAL_args>();
            lstArgs.Add(Arg);
            return PerformUpdate(lstArgs);
        }

        /// <summary>
        /// Method used to handle if a Row needs to reflect changes made by the Database, ie. Getting the Auto Number back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnRowUpdated(Object sender, SqlRowUpdatedEventArgs args)
        {
            // If the Statement is an Insert for now we only want to get the Autonumber ID that the Database added to the row.
            if (args.StatementType == StatementType.Insert)
            {
                int newID = 0;
                string strSelect = "SELECT @@IDENTITY";
                SqlCommand idCMD = new SqlCommand(strSelect, connMSSQL, trans);
                var results = idCMD.ExecuteScalar();
                newID = Convert.ToInt32(results);
                args.Row[0] = newID;
            }
        }

        /// <summary>
        /// Used to executing SQL strings that do not return a value
        /// </summary>
        /// <param name="cSQL">SQL string needed to Execute</param>
        /// <returns>Returns wheather or not the command modified any rows</returns>
        public bool ExecuteSQL(string cSQL)
        {
            lock (lockConn)
            {
                using (SqlConnection cn = new SqlConnection(connstrMSSQL))
                {
                    SqlCommand cmd = new SqlCommand(cSQL, cn);
                    try
                    {
                        if (cn.State != ConnectionState.Open) { cn.Open(); }
                        return (cmd.ExecuteNonQuery() > 0);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (cn.State != ConnectionState.Closed) { cn.Close(); }
                    }

                }
            }
        }

        public DataTable GetTable(string cSQL)
        {
            lock (lockConn)
            {
                using (SqlConnection cn = new SqlConnection(connstrMSSQL))
                {
                    SqlCommand cmd = new SqlCommand(cSQL, cn);
                    try
                    {
                        DataTable ret = new DataTable();
                        if (cn.State != ConnectionState.Open) cn.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(ret);
                        return ret;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (cn.State != ConnectionState.Closed) cn.Close();
                    }
                }
            }        
        }
    #endregion

    }
}
