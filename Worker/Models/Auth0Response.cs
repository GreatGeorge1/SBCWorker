using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Worker.Host.Models
{
    public class Auth0Response
    {
        [DeserializeAs(Name = "access_token")]
        public string AccessToken { get; set; }
        [DeserializeAs(Name = "token_type")]
        public string TokenType { get; set; }
    }
}
