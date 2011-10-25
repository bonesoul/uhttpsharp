/*  
 * uhttpsharp - A very lightweight & simple embedded http server for c# - http://code.google.com/p/uhttpsharp/
 * 
 * Copyright (c) 2010, Hüseyin Uslu
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *  
 *   Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * 
 *   Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
 *   in the documentation and/or other materials provided with the distribution.
 *   
 *   Neither the name of the uhttpsharp nor the names of its contributors may be used to endorse or promote products derived from 
 *   this software without specific prior written permission.
 *   
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT 
 * NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
 * THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;

namespace uhttpsharp.Embedded
{
    public sealed class HttpResponse
    {
        private Dictionary<int, string> _responseTexts = new Dictionary<int, string>()
        {
            {200,"OK"},
            {302,"Found"},
            {303,"See Other"},
            {400,"BadRequest"},
            {404,"Not Found"},
            {502,"Server Busy"},
            {500,"Internal Server Error"}
        };

        public string Protocol { get; private set; }
        public string ContentType { get; private set; }
        public bool CloseConnection { get; private set; }
        public ResponseCode Code { get; private set; }        
        public string Content {get;private set;}                
        public string Response {get;private set;}


        public HttpResponse(ResponseCode code, string content)
        {
            this.Protocol = "HTTP/1.1";
            this.ContentType = "text/html";
            this.CloseConnection = true;

            this.Code=code;
            this.Content=content;
            this.ForgeResponse();
        }

        private void ForgeResponse()
        {
            this.Response = string.Format("{0} {1} {2}\r\nDate: {3}\r\nServer: {4}\r\nConnection: {5}\r\nContent-Type: {6}\r\nContent-Lenght: {7}\r\n\r\n{8}",
                Protocol,
                (int)Code,
                this._responseTexts[(int)this.Code],
                DateTime.Now.ToString("R"),
                HttpServer.Instance.Banner,
                this.CloseConnection ? "close":"Keep-Alive" ,
                this.ContentType,
                this.Content.Length,
                this.Content);                               
        }

        public enum ResponseCode
        {
            OK = 200,
            Found = 302,
            SeeOther = 303,
            BadRequest = 400,
            NotFound = 404,
            InternalServerError = 500,
            ServerBusy = 502,
        }
    }
}
