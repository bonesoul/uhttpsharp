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

using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace uhttpsharp.Embedded
{
    public sealed class HttpClient
    {
        private TcpClient _client;
        private Stream _inputStream;
        private StreamWriter _outputStream;

        public HttpClient(TcpClient client)
        {
            this._client = client;
            this._inputStream = new BufferedStream(this._client.GetStream());
            this._outputStream = new StreamWriter(this._client.GetStream());
                
            Thread clientThread = new Thread(() => { this.Process(); }) { IsBackground = true };
            clientThread.Start();
        }

        private void Process()
        {
            while (this._client.Connected)
            {
                HttpRequest request = new HttpRequest(this._inputStream);
                if (request.Valid)
                {
                    HttpResponse response = HttpRequestProxy.Instance.Route(request);
                    if (response != null)
                    {
                        this._outputStream.Write(response.Response);
                        this._outputStream.Flush();
                        if (response.CloseConnection) this._client.Close();
                    }
                }
            }
        }
    }
}
