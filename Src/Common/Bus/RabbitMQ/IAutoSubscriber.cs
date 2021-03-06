﻿using RabbitMQ.Client;
using System;
using System.Reflection;

namespace Common.Bus.RabbitMQ
{
    public interface IAutoSubscriber : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();

        void Disconnect();
        void Subscribe(Assembly getExecutingAssembly);
        void SubscribeAsync(Assembly getExecutingAssembly);
    }
}
