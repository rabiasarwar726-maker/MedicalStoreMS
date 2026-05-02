// DataAccess/AuditRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace MedicalStoreMS.DataAccess
{
    public class AuditRepository
    {
        public DataTable GetRecent(int top = 200)
            => DatabaseHelper.ExecuteQuery($@"
                SELECT TOP {top} a.LogID, u.Username, a.Action, a.Details, a.Timestamp
                FROM   AuditLog a
                LEFT JOIN Users u ON a.UserID = u.UserID
                ORDER BY a.Timestamp DESC");
    }
}
