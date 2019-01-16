﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AgentR.Client.SignalR;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace AgentR.Client
{
    public interface IAgentClient
    {
        Task StartAsync();
        Task StopAsync();
        bool IsConnected { get; }
        void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse>;
        Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken));
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken)) where TNotification : INotification;
    }

    public static class AgentClientExtensions
    {
        public static async Task TryConnect(this IAgentClient client, Func<int, Exception, TimeSpan> callback = null)
        {
            callback = callback ?? defaultCallback;
            int i = 0;

            while (!client.IsConnected)
            {
                try
                {
                    await client.StartAsync();
                } catch (Exception ex)
                {
                    await Task.Delay(callback(i, ex));
                }
            }


            TimeSpan defaultCallback(int n, Exception ex) => TimeSpan.FromSeconds(1 + n % 10);
        }
    }

    public class AgentClient : IAgentClient
    {
        private readonly HubConnection connection;
        private readonly IMediator mediator;

        public AgentClient(HubConnection connection, IMediator mediator)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(HubConnection));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(IMediator));
        }

        public void HandleRequest<TRequest, TResponse>() where TRequest : IRequest<TResponse> => connection.HandleRequest<TRequest, TResponse>(this.mediator);

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return connection.SendNotification(notification, cancellationToken);
        }

        public Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return this.connection.SendRequest<TResponse>(request, cancellationToken);
        }

        public Task StartAsync() => connection.StartAsync();

        public Task StopAsync() => connection.StopAsync();

        public bool IsConnected => connection.State == HubConnectionState.Connected;
    }
}
