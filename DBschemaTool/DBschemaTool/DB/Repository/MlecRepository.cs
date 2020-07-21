using Dapper;
using DBschemaTool.Repository.Common.Interface;
using System;
using System.Collections.Generic;
using System.Data;

namespace DBschemaTool.Repository
{
    /// <summary>
    /// Pchome資料Repository
    /// </summary>
    public class MlecRepository : IDisposable
    {
        private IDatabaseConnectionHelper _DatabaseConnection { get; }

        internal MlecRepository(IDatabaseConnectionHelper databaseConnectionHelper)
        {
            this._DatabaseConnection = databaseConnectionHelper;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    return;
                }
                disposedValue = true;
            }
        }
        ~MlecRepository()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        public List<string> GetTable()
        {
            return null;

        }
        public List<string> GetView()
        {
            return null;

        }
        



    }
}
