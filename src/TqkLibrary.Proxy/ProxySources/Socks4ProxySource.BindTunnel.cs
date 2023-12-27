﻿using System.Net;
using TqkLibrary.Proxy.Enums;
using TqkLibrary.Proxy.Exceptions;
using TqkLibrary.Proxy.Helpers;
using TqkLibrary.Proxy.Interfaces;

namespace TqkLibrary.Proxy.ProxySources
{
    public partial class Socks4ProxySource
    {
        class BindTunnel : BaseTunnel, IBindSource
        {
            internal BindTunnel(Socks4ProxySource proxySource) : base(proxySource)
            {

            }

            Socks4_RequestResponse? _socks4_RequestResponse;
            public async Task<IPEndPoint> BindAsync(CancellationToken cancellationToken = default)
            {
                CheckIsDisposed();
                await _ConnectToSocksServerAsync(cancellationToken);

                Socks4_Request socks4_Request = Socks4_Request.CreateBind(_proxySource.userId);

                byte[] buffer = socks4_Request.GetByteArray();
                await this._stream!.WriteAsync(buffer, 0, buffer.Length, cancellationToken);

                _socks4_RequestResponse = await this._stream.Read_Socks4_RequestResponse_Async(cancellationToken);
                if (_socks4_RequestResponse.REP != Socks4_REP.RequestGranted)
                {
                    throw new InitConnectSourceFailedException($"{nameof(Socks4_REP)}: {_socks4_RequestResponse.REP}");
                }

                return _socks4_RequestResponse.IPEndPoint;
            }

            public Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
            {
                if (_stream is null)
                    throw new InvalidOperationException($"Mustbe run {nameof(BindAsync)} first");
                CheckIsDisposed();

                return Task.FromResult(_stream);
            }
        }
    }
}
