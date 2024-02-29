using System;

namespace account_api.Utils
{
    public class AccountData
    {
        public string AccountNumber { get; set; }        
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }        
        public string Address { get; set; }        
        public double SpaceArea { get; set; }
    }
}
