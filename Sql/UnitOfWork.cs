using System;
using System.Data;
using System.Data.SqlClient;

namespace dotnet_api_extensions.Sql
{
    /// <summary>
    /// This class manages the database transactions, and is a must for all repositories.
    /// The transaction is created when first required, and is closed at the end of the request
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SqlDbConnectionFactory _sqlDbConnectionFactory;
        private SqlConnection _sqlConnection;
        private SqlTransaction _sqlTransaction;
        private bool _alreadyCommittedOrRolledback;
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified; //Default value

        //These locks are needed so that multiple threads using same unitofwork do not break connection/transaction pair
        private readonly object _lockObjConn = new object();
        private readonly object _lockObjTran = new object();

        public UnitOfWork(SqlDbConnectionFactory sqlDbConnectionFactory)
        {
            _sqlDbConnectionFactory = sqlDbConnectionFactory;
        }

        public SqlConnection Connection
        {
            get
            {
                CheckIfAlreadyCommittedOrRolledback();
                if (_sqlConnection == null)
                {
                    lock (_lockObjConn)
                    {
                        if (_sqlConnection == null)
                        {
                            _sqlConnection = _sqlDbConnectionFactory.GetDbConnection();
                            //Nullify transaction so that new transaction is created using this transaction
                            _sqlTransaction = null;
                        }
                    }
                }
                return _sqlConnection;
            }
        }

        public void SetIsolationLevel(IsolationLevel isolationLevel)
        {
            _isolationLevel = isolationLevel;
        }

        public SqlTransaction Transaction
        {
            get
            {
                CheckIfAlreadyCommittedOrRolledback();
                if (_sqlTransaction == null)
                {
                    lock (_lockObjTran)
                    {
                        _sqlTransaction ??= Connection.BeginTransaction(_isolationLevel);
                    }
                }
                return _sqlTransaction;
            }
        }

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public void Commit()
        {
            CheckIfAlreadyCommittedOrRolledback();

            try
            {
                _sqlTransaction?.Commit();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while committing transaction", ex);
            }
            finally
            {
                _alreadyCommittedOrRolledback = true;
            }
        }

        /// <summary>
        /// Rollsback the transaction
        /// </summary>
        public void RollBack()
        {
            CheckIfAlreadyCommittedOrRolledback();

            try
            {
                _sqlTransaction?.Rollback();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while rolling back transaction", ex);
            }
            finally
            {
                _alreadyCommittedOrRolledback = true;
            }
        }

        /// <summary>
        /// Implementation for IDisposable
        /// </summary>
        public void Dispose()
        {
            if (!_alreadyCommittedOrRolledback)
            {
                Commit();
            }
            _sqlTransaction?.Dispose();
            _sqlConnection?.Dispose();
        }

        private void CheckIfAlreadyCommittedOrRolledback()
        {
            if (_alreadyCommittedOrRolledback)
                throw new Exception("The transaction is already committed or rolled back. Cannot proceed further.");
        }
    }
}