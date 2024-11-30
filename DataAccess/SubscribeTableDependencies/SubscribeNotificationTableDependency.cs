using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableDependency.SqlClient;

namespace DataAccess.SubscribeTableDependencies
{
    public class SubscribeNotificationTableDependency : ISubscribeTableDependency
    {
        SqlTableDependency<Notification> sqlTableDependency;

        public SubscribeNotificationTableDependency(Notification)
        public void SubscribeTableDependency(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}
