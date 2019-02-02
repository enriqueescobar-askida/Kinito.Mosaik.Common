using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Alphamosaik.Oceanik.Sdk
{
    public class TranslationUserAccount
    {
        private string _Login;
        private string _Password;
        private string _Url;
        private List<string> _Profiles;

        public string Login
        {
            get
            {
                return _Login;
            }
            set
            {   
                _Login = value;                
            }
        }

        public string Password
        {
            get
            {
                return _Password;
            }
            set
            {   
                _Password = value;                
            }
        }

        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;
            }
        }

        public List<string> Profiles
        {
            get
            {
                return _Profiles;
            }
            set
            {
                _Profiles = value;
            }
        }
    }
}
