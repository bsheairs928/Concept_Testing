using System.Data;
using System.Data.SqlClient;

namespace Concept_Testing.DAL
{
    public class DAL_args
    {
        private SqlCommand _cmdUpdate;

        public SqlCommand cmdUpdate
        {
            get { return _cmdUpdate; }
            set { _cmdUpdate = value; }
        }

        private SqlCommand _cmdInsert;

        public SqlCommand cmdInsert
        {
            get { return _cmdInsert; }
            set { _cmdInsert = value; }
        }

        private SqlCommand _cmdDelete;

        public SqlCommand cmdDelete
        {
            get { return _cmdDelete; }
            set { _cmdDelete = value; }
        }

        private DataTable _dt;

        public DataTable dt
        {
            get { return _dt; }
            set { _dt = value; }
        }

        private bool _capturePrimaryKey;

        public bool capturePrimaryKey
        {
            get { return _capturePrimaryKey; }
            set { _capturePrimaryKey = value; }
        }

        private SqlCommand _cmdExecuteSQL;

        public SqlCommand cmdExecuteSQL
        {
            get { return _cmdExecuteSQL; }
            set { _cmdExecuteSQL = value; }
        }

        internal DAL_args(DataTable nDT, SqlCommand nCmdUp)
        {
            SetDT(ref nDT, true);
            _cmdUpdate = nCmdUp;
            _cmdInsert = null;
            _cmdDelete = null;
            _cmdExecuteSQL = null;
            _capturePrimaryKey = false;
        }

        internal DAL_args(SqlCommand nCmdExec)
        {
            _cmdUpdate = null;
            _cmdInsert = null;
            _cmdDelete = null;
            _cmdExecuteSQL = nCmdExec;
            _capturePrimaryKey = false;
        }

        internal DAL_args(DataTable nDT, SqlCommand nCmdUp, bool idByRef)
        {
            SetDT(ref nDT, idByRef);
            _cmdUpdate = nCmdUp;
            _cmdInsert = null;
            _cmdDelete = null;
            _cmdExecuteSQL = null;
            _capturePrimaryKey = false;
        }

        internal DAL_args(DataTable nDT, SqlCommand nCmdUp, SqlCommand nCmdIns, 
                            SqlCommand nCmdDel, bool nCapPriKey, bool idByRef)
        {
            SetDT(ref nDT, idByRef);
            _cmdUpdate = nCmdUp;
            _cmdInsert = nCmdIns;
            _cmdDelete = nCmdDel;
            _cmdExecuteSQL = null;
            _capturePrimaryKey = nCapPriKey;
        }

        internal DAL_args(DataTable nDT, SqlCommand nCmdUp, SqlCommand nCmdIns,
                         SqlCommand nCmdDel, bool nCapPriKey)
        {
            SetDT(ref nDT, true);
            _cmdUpdate = nCmdUp;
            _cmdInsert = nCmdIns;
            _cmdDelete = nCmdDel;
            _cmdExecuteSQL = null;
            _capturePrimaryKey = nCapPriKey;
        }
        internal DAL_args(DataTable nDT, SqlCommand nCmdUp, SqlCommand nCmdIns,
                 SqlCommand nCmdDel)
        {
            SetDT(ref nDT, true);
            _cmdUpdate = nCmdUp;
            _cmdInsert = nCmdIns;
            _cmdDelete = nCmdDel;
            _cmdExecuteSQL = null;
            _capturePrimaryKey = false;
        }


        private void SetDT(ref DataTable nDT, bool isByRef)
        {
            if (isByRef)
            {
                _dt = nDT;
            }
            else
            {
                // Have to Clear & Merge or else same instance is updated/overwritted 
                // when multiple instances of this class are in a list
                _dt.Clear();
                _dt.AcceptChanges();
                _dt.Merge(nDT);
            }
        }


    }
}