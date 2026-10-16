using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement_WPF_Core.Model
{
    public class DataProvider
    {
        private static DataProvider _instance;
        public static DataProvider Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DataProvider();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
        public SalesManagementContext Data { get; set; }
        private DataProvider()
        {
            Data = new SalesManagementContext();
        }
    }
}
