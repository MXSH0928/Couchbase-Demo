using System;
using System.Collections.Generic;
using System.Text;

namespace CouchbaseDemo
{
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string TagLine { get; set; }
        public string Type => "User";
    }
}
